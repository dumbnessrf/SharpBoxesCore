using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpBoxesCore.TaskHelper;

/// <summary>
/// 使用案例见<see cref="ProgramFuncTaskTest"/>
/// </summary>
/// <typeparam name="TTaskId"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class ThreadPoolManager<TTaskId, TResult>
    where TTaskId : IEquatable<TTaskId>
    where TResult : ITaskResult, new()
{
    public ConcurrentDictionary<TTaskId, Task<TResult>> Tasks = new();

    private readonly ConcurrentDictionary<TTaskId, CancellationTokenSource> _taskCtsMap = new();
    private readonly ConcurrentQueue<TaskDefinition<TTaskId, TResult>> _taskQueue = new();
    private readonly ILogger _logger;

    // 👇 动态并发控制核心：使用 volatile + Interlocked.Exchange 支持运行时调整
    private volatile SemaphoreSlim _semaphore;
    private int _maxDegreeOfParallelism;

    /// <summary>
    /// 构造函数，接受一个日志记录器和最大并发数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="maxDegreeOfParallelism">最大并发任务数（<=0 表示使用处理器核心数）</param>
    public ThreadPoolManager(ILogger logger, int maxDegreeOfParallelism = -1)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        SetMaxDegreeOfParallelism(maxDegreeOfParallelism);
    }

    /// <summary>
    /// 动态设置最大并发任务数（线程安全）
    /// </summary>
    /// <param name="newMax">新的最大并发数（<=0 表示使用默认值）</param>
    public void SetMaxDegreeOfParallelism(int newMax)
    {
        newMax = newMax <= 0 ? Environment.ProcessorCount : Math.Max(1, newMax);

        var oldSemaphore = Interlocked.Exchange(ref _semaphore, new SemaphoreSlim(newMax, newMax));

        oldSemaphore?.Dispose(); // 释放旧信号量资源

        _logger.LogInfo($"最大并发数已调整为: {newMax}");
        _maxDegreeOfParallelism = newMax;
    }

    public void RegisterTask(TaskDefinition<TTaskId, TResult> definition)
    {
        _taskQueue.Enqueue(definition);
    }

    public void RegisterAndRunTask(TaskDefinition<TTaskId, TResult> definition)
    {
        RegisterTask(definition);
        _ = RunSingle(definition); // 启动任务，不等待
    }

    /// <summary>
    /// 启动所有已注册的异步任务，并处理超时和进度回调
    /// </summary>
    public async Task StartAllAsync()
    {
        if (_taskQueue.IsEmpty)
        {
            _logger.LogInfo("没有注册任何任务。");
            return;
        }

        var runningTasks = new List<Task>();

        while (_taskQueue.TryDequeue(out var definition))
        {
            var task = RunSingle(definition);
            runningTasks.Add(task);
        }

        await Task.WhenAll(runningTasks);
        _logger.LogInfo("所有任务已完成。");
    }

    private async Task SafeFireAndForget(Func<Task> func)
    {
        try
        {
            await func();
        }
        catch { }
    }

    /// <summary>
    /// 启动单个任务，返回 Task 用于等待
    /// </summary>
    private Task RunSingle(TaskDefinition<TTaskId, TResult> definition)
    {
        var taskId = definition.TaskId;

        var progress = new Progress<ProgressInfo>(info =>
        {
            _logger.LogInfo($"任务 {taskId} 进度: {info}");
        });

        var taskCts = new CancellationTokenSource();
        _taskCtsMap[taskId] = taskCts;

        var taskWithId = Task.Run(
            async () =>
            {
                // 👇 获取当前信号量（支持动态替换）
                var currentSemaphore = _semaphore;
                await currentSemaphore.WaitAsync(taskCts.Token);

                try
                {
                    _logger.LogInfo(
                        $"任务 {taskId} 开始执行（当前最大并发: {_maxDegreeOfParallelism - currentSemaphore.CurrentCount} / {_maxDegreeOfParallelism}）。"
                    );

                    return await ExecuteTaskWithTimeoutAsync(
                        definition,
                        taskId,
                        taskCts.Token,
                        progress
                    );
                }
                finally
                {
                    // 👇 释放获取时的信号量（即使已被替换，也要释放当时的实例）
                    currentSemaphore.Release();
                }
            },
            taskCts.Token
        );

        Tasks[taskId] = taskWithId;
        return taskWithId;
    }

    private async Task<TResult> ExecuteTaskWithTimeoutAsync(
        TaskDefinition<TTaskId, TResult> definition,
        TTaskId taskId,
        CancellationToken token,
        IProgress<ProgressInfo> progress
    )
    {
        try
        {
            var startTime = DateTime.Now;
            bool hasTimeout = definition.Timeout.HasValue;

            var timeoutTask = hasTimeout
                ? Task.Delay(definition.Timeout.Value, token)
                : Task.CompletedTask;

            var resultTask = definition.Action(taskId, token, progress);
            var completedTask = await Task.WhenAny(resultTask, timeoutTask);

            if (completedTask == timeoutTask && hasTimeout)
            {
                var endTime = DateTime.Now;
                var elapsedTime = endTime - startTime;

                _logger.LogWarning(
                    $"任务 {taskId} 超时，已运行 {elapsedTime.TotalMilliseconds:F2} 毫秒。"
                );
                if (definition.OnTimeout != null)
                {
                    SafeFireAndForget(() => definition.OnTimeout(taskId));
                }

                //definition.OnTimeout?.Invoke(taskId);
                token.ThrowIfCancellationRequested(); // 确保取消传播

                return CreateTaskResult(ETaskResultStatus.Timeout, "任务超时", null);
            }

            // 正常完成，等待结果（可能抛异常）
            var temp = await resultTask;
            if (definition.OnTaskCompleted != null)
            {
                await SafeFireAndForget(
                    () => definition.OnTaskCompleted.Invoke(definition.TaskId, temp)
                );
            }

            return temp;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning($"任务 {taskId} 被取消。{ex.Message}");
            return CreateTaskResult(ETaskResultStatus.Cancelled, "任务被取消", ex);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError($"任务 {taskId} 超时异常: {ex.Message}");
            return CreateTaskResult(ETaskResultStatus.Timeout, "任务超时异常", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError($"任务 {taskId} 发生异常: {ex.Message}");
            return CreateTaskResult(ETaskResultStatus.Failed, "任务发生异常", ex);
        }
    }

    /// <summary>
    /// 取消指定任务
    /// </summary>
    public void CancelTask(TTaskId taskId)
    {
        if (!_taskCtsMap.TryGetValue(taskId, out var cts))
        {
            _logger.LogWarning($"未找到ID为 {taskId} 的任务的 CTS。");
            return;
        }

        cts.Cancel();
        _logger.LogInfo($"请求取消任务 {taskId}。");
    }

    /// <summary>
    /// 获取指定任务的结果（阻塞直到完成）
    /// </summary>
    public TResult GetResult(TTaskId taskId)
    {
        if (!Tasks.TryGetValue(taskId, out var task))
        {
            _logger.LogError($"未找到ID为 {taskId} 的任务。");
            throw new KeyNotFoundException($"未找到ID为 {taskId} 的任务。");
        }

        return task.Result;
    }

    /// <summary>
    /// 获取所有任务的结果（包括成功、失败、取消）
    /// </summary>
    public List<(TTaskId Id, TResult Result)> GetAllResults()
    {
        var results = new List<(TTaskId Id, TResult Result)>();

        foreach (var kvp in Tasks)
        {
            try
            {
                results.Add((kvp.Key, kvp.Value.Result));
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    _logger.LogError($"获取结果时出错: {e.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"获取结果时出错: {ex.Message}");
            }
        }

        return results;
    }

    /// <summary>
    /// 获取指定任务的状态信息
    /// </summary>
    public TaskStatus GetTaskStatus(TTaskId taskId)
    {
        if (!Tasks.TryGetValue(taskId, out var task))
        {
            _logger.LogError($"未找到ID为 {taskId} 的任务。");
            throw new KeyNotFoundException($"未找到ID为 {taskId} 的任务。");
        }

        return task.Status;
    }

    /// <summary>
    /// 获取所有任务的状态
    /// </summary>
    public List<(TTaskId Id, TaskStatus Status)> GetAllTaskStatuses()
    {
        return Tasks.Select(kvp => (kvp.Key, kvp.Value.Status)).ToList();
    }

    /// <summary>
    /// 等待所有任务完成
    /// </summary>
    public Task WaitAllAsync() => Task.WhenAll(Tasks.Values);

    public void WaitAll()
    {
        Task.WaitAll(Tasks.Values.ToArray());
    }

    /// <summary>
    /// 创建一个封装了状态和异常信息的结果对象
    /// </summary>
    private TResult CreateTaskResult(ETaskResultStatus status, string message, Exception exception)
    {
        var result = new TResult();
        result.TaskResultStatus = status;
        result.Message = message;
        result.Exception = exception;
        return result;
    }

    /// <summary>
    /// 取消所有任务并等待完成
    /// </summary>
    public async Task CancelAll()
    {
        foreach (var cts in _taskCtsMap.Values)
        {
            cts.Cancel();
        }

        await Task.WhenAll(Tasks.Values);
        _logger.LogInfo("所有任务已取消或完成。");
    }

    public bool ContainsTask(TTaskId taskId)
    {
        return Tasks.ContainsKey(taskId);
    }
}

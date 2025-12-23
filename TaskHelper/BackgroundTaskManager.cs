namespace SharpBoxesCore.TaskHelper;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 使用案例见<see cref="ProgramBackgroundTaskTest"/>
/// </summary>
/// <typeparam name="TTaskId"></typeparam>
public class BackgroundTaskManager<TTaskId>
    where TTaskId : IEquatable<TTaskId>
{
    private readonly ConcurrentDictionary<TTaskId, Task> _tasks = new();
    private readonly ConcurrentDictionary<TTaskId, CancellationTokenSource> _taskCtsMap = new();
    private readonly ConcurrentQueue<TaskDefinition<TTaskId>> _taskQueue = new();
    private readonly ILogger _logger;

    // 👇 改为 volatile，支持动态替换
    private volatile SemaphoreSlim _semaphore;
    private int _maxDegreeOfParallelism;

    /// <summary>
    /// 构造函数，接受一个日志记录器和最大并发数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="maxDegreeOfParallelism">最大并发任务数（默认为 Environment.ProcessorCount）</param>
    public BackgroundTaskManager(ILogger logger, int maxDegreeOfParallelism = -1)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        SetMaxDegreeOfParallelism(maxDegreeOfParallelism); // 👈 初始化信号量
    }

    /// <summary>
    /// 动态设置最大并发任务数（线程安全）
    /// </summary>
    /// <param name="newMax">新的最大并发数（<=0 表示使用默认值）</param>
    public void SetMaxDegreeOfParallelism(int newMax)
    {
        newMax = newMax <= 0 ? Environment.ProcessorCount : Math.Max(1, newMax);

        var oldSemaphore = Interlocked.Exchange(ref _semaphore, new SemaphoreSlim(newMax, newMax));

        oldSemaphore?.Dispose(); // 释放旧资源

        _logger.LogInfo($"最大并发数已调整为: {newMax}");
        _maxDegreeOfParallelism = newMax;
    }

    public void RegisterTask(TaskDefinition<TTaskId> definition)
    {
        _taskQueue.Enqueue(definition);
    }

    public void RegisterAndRunTask(TaskDefinition<TTaskId> definition)
    {
        RegisterTask(definition);
        _ = RunSingle(definition); // 启动任务，不等待
    }

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

    private Task RunSingle(TaskDefinition<TTaskId> definition)
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
                var currentSemaphore = _semaphore; // 👈 获取当前信号量（支持动态替换）
                await currentSemaphore.WaitAsync(taskCts.Token);

                try
                {
                    _logger.LogInfo(
                        $"任务 {taskId} 开始执行（当前并发: {_maxDegreeOfParallelism - currentSemaphore.CurrentCount} / 最大: {_maxDegreeOfParallelism}）。"
                    );

                    var startTime = DateTime.Now;
                    bool hasTimeout = definition.Timeout.HasValue;

                    var timeoutTask = hasTimeout
                        ? Task.Delay(definition.Timeout.Value, taskCts.Token) // 👈 传递 token 使超时可取消
                        : Task.CompletedTask;

                    var resultTask = Task.Run(
                        () => definition.Action(taskId, taskCts.Token, progress),
                        taskCts.Token
                    );

                    var completedTask = await Task.WhenAny(resultTask, timeoutTask);

                    if (completedTask == timeoutTask && hasTimeout)
                    {
                        var endTime = DateTime.Now;
                        var elapsedTime = endTime - startTime;
                        _logger.LogError(
                            $"任务 {taskId} 超时，已运行 {elapsedTime.TotalMilliseconds:F2} 毫秒。"
                        );
                        if (definition.OnTimeout != null)
                        {
                            SafeFireAndForget(() => definition.OnTimeout(taskId));
                        }

                        taskCts.Cancel();
                        throw new TimeoutException($"任务 {taskId} 超时。");
                    }

                    await resultTask; // 👈 确保抛出内部异常
                    if (definition.OnTaskCompleted != null)
                    {
                        await SafeFireAndForget(
                            () => definition.OnTaskCompleted.Invoke(definition.TaskId)
                        );
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning($"任务 {taskId} 被取消。");
                }
                catch (TimeoutException ex)
                {
                    _logger.LogError($"任务 {taskId} 超时异常: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"任务 {taskId} 发生异常: {ex.Message}");
                }
                finally
                {
                    currentSemaphore.Release(); // 👈 释放获取时的信号量实例
                }
            },
            taskCts.Token
        );

        _tasks[taskId] = taskWithId;
        return taskWithId;
    }

    /// <summary>
    /// 取消指定任务
    /// </summary>
    /// <param name="taskId">任务ID</param>
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
    /// 获取指定任务的状态信息（是否完成、是否出错等）
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>任务状态</returns>
    public TaskStatus GetTaskStatus(TTaskId taskId)
    {
        if (!_tasks.TryGetValue(taskId, out var task))
        {
            _logger.LogError($"未找到ID为 {taskId} 的任务。");
            throw new KeyNotFoundException($"未找到ID为 {taskId} 的任务。");
        }

        return task.Status;
    }

    /// <summary>
    /// 获取所有任务的状态
    /// </summary>
    /// <returns>任务ID与状态的元组列表</returns>
    public List<(TTaskId Id, TaskStatus Status)> GetAllTaskStatuses()
    {
        return _tasks.Select(kvp => (kvp.Key, kvp.Value.Status)).ToList();
    }

    /// <summary>
    /// 等待所有任务完成
    /// </summary>
    public Task WaitAllAsync() => Task.WhenAll(_tasks.Values);

    public async Task CancelAll()
    {
        foreach (var cts in _taskCtsMap.Values)
        {
            cts.Cancel();
        }

        await Task.WhenAll(_tasks.Values);
        _logger.LogInfo("所有任务已取消或完成。");
    }

    public bool ContainsTask(TTaskId taskId)
    {
        return _tasks.ContainsKey(taskId);
    }
}

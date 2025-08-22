using System.Linq;

namespace SharpBoxesCore.TaskHelper;

/// <summary>
/// 使用案例见<see cref="ProgramFuncTaskTest"/>
/// </summary>
/// <typeparam name="TTaskId"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class ThreadPoolManager<TTaskId, TResult>
    where TTaskId : IEquatable<TTaskId>
    where TResult : ITaskResult
{
    public ConcurrentDictionary<TTaskId, Task<TResult>> Tasks = new();

    private readonly ConcurrentDictionary<TTaskId, CancellationTokenSource> _taskCtsMap = new();

    private readonly ConcurrentQueue<TaskDefinition<TTaskId, TResult>> _taskQueue = new();

    private readonly ILogger _logger;

    public ThreadPoolManager(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void RegisterTask(TaskDefinition<TTaskId, TResult> definition)
    {
        _taskQueue.Enqueue(definition);
    }

    public void RegisterAndRunTask(TaskDefinition<TTaskId, TResult> definition)
    {
        RegisterTask(definition);
        RunSingle(definition);
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

        while (_taskQueue.TryDequeue(out var definition))
        {
            RunSingle(definition);
        }

        await Task.WhenAll(Tasks.Values);
        _logger.LogInfo("所有任务已完成。");
    }

    private void RunSingle(TaskDefinition<TTaskId, TResult> definition)
    {
        var taskId = definition.TaskId;
        //_logger.LogInfo($"注册任务 ID: {taskId}");

        var progress = new Progress<ProgressInfo>(info =>
        {
            _logger.LogInfo($"任务 {taskId} 进度: {info}");
        });

        // 为每个任务创建独立的 CTS
        var taskCts = new CancellationTokenSource();
        _taskCtsMap[taskId] = taskCts;

        var taskWithId = Task.Run(
            async () =>
            {
                try
                {
                    _logger.LogInfo($"任务 {taskId} 开始执行。");

                    var startTime = DateTime.Now;

                    // 是否设置了超时
                    bool hasTimeout = definition.Timeout.HasValue;

                    // 创建超时任务（如果有的话）
                    var timeoutTask = hasTimeout ? Task.Delay(definition.Timeout.Value) : Task.CompletedTask;

                    // 执行任务逻辑
                    var resultTask = definition.Action(taskId, taskCts.Token, progress);

                    var completedTask = await Task.WhenAny(resultTask, timeoutTask);

                    if (completedTask == timeoutTask && hasTimeout)
                    {
                        var endTime = DateTime.Now;
                        var elapsedTime = endTime - startTime;

                        _logger.LogWarning($"任务 {taskId} 超时，已运行 {elapsedTime.TotalMilliseconds:F2} 毫秒。");

                        // 调用用户的超时回调
                        definition.OnTimeout?.Invoke(taskId);

                        // 取消该任务的CTS
                        taskCts.Cancel();

                        // 返回一个封装了超时信息的结果
                        return CreateTaskResult(ETaskResultStatus.Timeout, "任务超时", null);
                    }

                    // 正常完成
                    var result = await resultTask;
                    //_logger.LogInfo($"任务 {taskId} 正常完成。");
                    return result;
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogWarning($"任务 {taskId} 被取消。{ex.Message}");
                    return CreateTaskResult(ETaskResultStatus.Cancelled, "任务被取消", ex);
                }
                //处理内部抛出的异常
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
            },
            taskCts.Token
        );

        Tasks[taskId] = taskWithId;
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
    /// 获取指定任务的结果
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>任务结果</returns>
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
    /// <returns>任务ID与结果的元组列表</returns>
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
    /// 获取指定任务的状态信息（是否完成、是否出错等）
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
        var statuses = new List<(TTaskId Id, TaskStatus Status)>();

        foreach (var kvp in Tasks)
        {
            statuses.Add((kvp.Key, kvp.Value.Status));
        }

        return statuses;
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
        var result = Activator.CreateInstance<TResult>();
        result.TaskResultStatus = status;
        result.Message = message;
        result.Exception = exception;
        return result;
    }

    public async Task CancelAll()
    {
        foreach (var cts in _taskCtsMap.Values)
        {
            cts.Cancel();
        }
        //_logger.LogInfo("请求取消所有任务。");
        await Task.WhenAll(Tasks.Values);
        //_logger.LogInfo("所有任务已取消。");
    }

    public bool ContainsTask(TTaskId taskId)
    {
        return Tasks.ContainsKey(taskId);
    }
}

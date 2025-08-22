namespace SharpBoxesCore.TaskHelper;
/// <summary>
/// 使用案例见<see cref="ProgramBackgroundTaskTest"/>
/// </summary>
/// <typeparam name="TTaskId"></typeparam>
public class BackgroundTaskManager<TTaskId>
    where TTaskId : IEquatable<TTaskId>
{
    // 存储所有注册的任务及其 Task 对象
    private readonly ConcurrentDictionary<TTaskId, Task> _tasks = new();

    // 存储每个任务对应的 CTS（用于单独取消）
    private readonly ConcurrentDictionary<TTaskId, CancellationTokenSource> _taskCtsMap = new();

    // 任务队列，包含任务定义
    private readonly ConcurrentQueue<TaskDefinition<TTaskId>> _taskQueue = new();

    // 日志记录器
    private readonly ILogger _logger;

    /// <summary>
    /// 构造函数，接受一个日志记录器
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public BackgroundTaskManager(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 注册一个后台任务
    /// </summary>
    /// <param name="definition">任务定义对象</param>
    public void RegisterTask(TaskDefinition<TTaskId> definition)
    {
        _taskQueue.Enqueue(definition);
    }

    /// <summary>
    /// 启动所有已注册的后台任务，并处理超时和进度回调
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
            var taskId = definition.TaskId;
            //_logger.LogInfo($"注册任务 ID: {taskId}");

            var progress = new Progress<ProgressInfo>(info =>
            {
                _logger.LogInfo($"任务 {taskId} 进度: {info}");
            });

            // 为每个任务创建独立的 CTS
            var taskCts = new CancellationTokenSource();
            _taskCtsMap[taskId] = taskCts;

            var taskWithId = Task.Run(async () =>
            {
                try
                {
                    _logger.LogInfo($"任务 {taskId} 开始执行。");

                    var startTime = DateTime.Now;

                    // 是否设置了超时
                    bool hasTimeout = definition.Timeout.HasValue;

                    // 创建超时任务（如果有的话）
                    var timeoutTask = hasTimeout ? Task.Delay(definition.Timeout.Value) : Task.CompletedTask;

                    // 等待任务完成或超时
                    var resultTask = Task.Run(() => definition.Action(taskId, taskCts.Token, progress), taskCts.Token);

                    var completedTask = await Task.WhenAny(resultTask, timeoutTask);

                    if (completedTask == timeoutTask && hasTimeout)
                    {
                        var endTime = DateTime.Now;
                        var elapsedTime = endTime - startTime;

                        _logger.LogError($"任务 {taskId} 超时，已运行 {elapsedTime.TotalMilliseconds:F2} 毫秒。");

                        // 调用用户的超时回调
                        definition.OnTimeout?.Invoke(taskId);

                        // 取消该任务的CTS
                        taskCts.Cancel();

                        throw new TimeoutException($"任务 {taskId} 超时。");
                    }

                    //_logger.LogInfo($"任务 {taskId} 正常完成。");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning($"任务 {taskId} 被取消。");
                    //throw;
                }
                catch (TimeoutException ex)
                {
                    _logger.LogError($"任务 {taskId} 超时异常: {ex.Message}");
                    //throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"任务 {taskId} 发生异常: {ex.Message}");
                    //throw;
                }
            });

            _tasks[taskId] = taskWithId;
        }

        await Task.WhenAll(_tasks.Values);
        _logger.LogInfo("所有任务已完成。");
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
        var statuses = new List<(TTaskId Id, TaskStatus Status)>();

        foreach (var kvp in _tasks)
        {
            statuses.Add((kvp.Key, kvp.Value.Status));
        }

        return statuses;
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
        //_logger.LogInfo("请求取消所有任务。");
        await Task.WhenAll(_tasks.Values);

        //_logger.LogInfo("所有任务已取消。");
    }

    public bool ContainsTask(TTaskId taskId)
    {
        return _tasks.ContainsKey(taskId);
    }
}

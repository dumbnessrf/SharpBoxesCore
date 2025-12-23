namespace SharpBoxesCore.TaskHelper;

public class TaskDefinition<TTaskId>
    where TTaskId : IEquatable<TTaskId>
{
    public TTaskId TaskId { get; }
    public Action<TTaskId, CancellationToken, IProgress<ProgressInfo>> Action { get; }
    public TimeSpan? Timeout { get; } // 使用 null 表示无超时
    public Func<TTaskId, Task> OnTimeout { get; }
    public Func<TTaskId, Task> OnTaskCompleted { get; }
    [DebuggerStepThrough]
    public TaskDefinition(
        TTaskId taskId,
        Action<TTaskId, CancellationToken, IProgress<ProgressInfo>> action,
        TimeSpan? timeout = null,
        Func<TTaskId, Task> onTimeout = null,
        Func<TTaskId, Task> onTaskCompleted = null
    )
    {
        TaskId = taskId;
        Action = action;
        Timeout = timeout;
        OnTimeout = onTimeout;
        OnTaskCompleted = onTaskCompleted;
    }
}

public class TaskDefinition<TTaskId, TResult>
    where TTaskId : IEquatable<TTaskId>
{
    public TTaskId TaskId { get; }
    public Func<TTaskId, CancellationToken, IProgress<ProgressInfo>, Task<TResult>> Action { get; }
    public TimeSpan? Timeout { get; } // null 表示不设置超时
    public Func<TTaskId, Task> OnTimeout { get; }
    public Func<TTaskId, TResult, Task> OnTaskCompleted { get; }
    [DebuggerStepThrough]
    public TaskDefinition(
        TTaskId taskId,
        Func<TTaskId, CancellationToken, IProgress<ProgressInfo>, Task<TResult>> action,
        TimeSpan? timeout = null,
        Func<TTaskId, Task> onTimeout = null,
        Func<TTaskId, TResult, Task> onTaskCompleted = null
    )
    {
        TaskId = taskId;
        Action = action;
        Timeout = timeout;
        OnTimeout = onTimeout;
        OnTaskCompleted = onTaskCompleted;
    }
}

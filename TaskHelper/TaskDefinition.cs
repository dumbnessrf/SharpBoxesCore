namespace SharpBoxesCore.TaskHelper;


public class TaskDefinition<TTaskId>
    where TTaskId : IEquatable<TTaskId>
{
    public TTaskId TaskId { get; }
    public Action<TTaskId,CancellationToken, IProgress<ProgressInfo>> Action { get; }
    public TimeSpan? Timeout { get; } // 使用 null 表示无超时
    public Action<TTaskId> OnTimeout { get; }

    public TaskDefinition(
        TTaskId taskId,
        Action<TTaskId,CancellationToken, IProgress<ProgressInfo>> action,
        TimeSpan? timeout = null,
        Action<TTaskId> onTimeout = null
    )
    {
        TaskId = taskId;
        Action = action;
        Timeout = timeout;
        OnTimeout = onTimeout;
    }
}

public class TaskDefinition<TTaskId, TResult>
    where TTaskId : IEquatable<TTaskId>
{
    public TTaskId TaskId { get; }
    public Func<TTaskId,CancellationToken, IProgress<ProgressInfo>, Task<TResult>> Action { get; }
    public TimeSpan? Timeout { get; } // null 表示不设置超时
    public Action<TTaskId> OnTimeout { get; }

    public TaskDefinition(
        TTaskId taskId,
        Func<TTaskId,CancellationToken, IProgress<ProgressInfo>, Task<TResult>> action,
        TimeSpan? timeout = null,
        Action<TTaskId> onTimeout = null
    )
    {
        TaskId = taskId;
        Action = action;
        Timeout = timeout;
        OnTimeout = onTimeout;
    }
}

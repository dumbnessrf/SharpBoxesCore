

namespace SharpBoxesCore.TaskHelper;

public interface ITaskResult
{
    public ETaskResultStatus TaskResultStatus { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }
}

public enum ETaskResultStatus
{
    Timeout,
    Finished,
    Cancelled,
    Failed,
}

namespace SharpBoxesCore.TaskHelper;

public class ProgressInfo
{
    public int Percentage { get; set; }
    public string Message { get; set; }

    public override string ToString()
    {
        return $"[Progress: {Percentage}%, Message: {Message}]";
    }
}
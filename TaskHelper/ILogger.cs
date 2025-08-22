
namespace SharpBoxesCore.TaskHelper;
public interface ILogger
{
    void LogDebug(string message);
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
    void LogFatal(string message);
}

public class ConsoleLogger : ILogger
{
    public void LogDebug(string message)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"[DEBUG] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARN] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public void LogFatal(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[FATAL] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public void LogInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }
}

public enum ELogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

using System.Runtime.CompilerServices;

namespace SharpBoxesCore.TaskHelper;

public interface ILogger
{
    void LogDebug(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );
    void LogInfo(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );
    void LogWarning(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );
    void LogError(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );
    void LogFatal(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    );
}

public class ConsoleLogger : ILogger
{
    public void LogDebug(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"[DEBUG] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public void LogWarning(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARN] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public void LogFatal(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[FATAL] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public void LogInfo(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public void LogError(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
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
    Fatal,
}

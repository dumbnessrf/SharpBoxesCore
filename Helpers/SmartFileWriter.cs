using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxesCore.Helpers;
public class SmartFileWriter : IDisposable
{
    private class FileLockEntry
    {
        public SemaphoreSlim AsyncLock { get; } = new SemaphoreSlim(1, 1);
        public DateTime LastAccess { get; set; } = DateTime.UtcNow;
    }

    private static readonly ConcurrentDictionary<string, FileLockEntry> _fileLocks =
        new ConcurrentDictionary<string, FileLockEntry>();

    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan LockTimeout = TimeSpan.FromMinutes(5);
    private static Timer _cleanupTimer;
    private static int _activeInstanceCount = 0;
    private static readonly object _timerLock = new object();

    private readonly string _filePath;
    private bool _disposed = false;

    public SmartFileWriter(string filePath)
    {
        _filePath = Path.GetFullPath(filePath);
        StartOrResumeTimer();
    }

    private static void StartOrResumeTimer()
    {
        lock (_timerLock)
        {
            _activeInstanceCount++;
            if (_cleanupTimer == null)
            {
                _cleanupTimer = new Timer(
                    CleanupUnusedLocks,
                    null,
                    CleanupInterval,
                    CleanupInterval
                );
                AppDomain.CurrentDomain.ProcessExit += (s, e) => StopCleanupTimer();
            }
        }
    }

    private static void StopCleanupTimer()
    {
        lock (_timerLock)
        {
            _cleanupTimer?.Dispose();
            _cleanupTimer = null;
        }
    }

    private static void CleanupUnusedLocks(object state)
    {
        var cutoffTime = DateTime.UtcNow - LockTimeout;
        var keysToRemove = new List<string>();

        foreach (var kvp in _fileLocks)
        {
            if (kvp.Value.LastAccess < cutoffTime)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            if (_fileLocks.TryRemove(key, out var entry))
            {
                entry.AsyncLock?.Dispose();
            }
        }
    }

    // ✅ 同步写入（使用 StreamWriter）
    public void WriteData(string data)
    {
        ThrowIfDisposed();
        var entry = _fileLocks.GetOrAdd(_filePath, _ => new FileLockEntry());

        entry.AsyncLock.Wait();
        try
        {
            entry.LastAccess = DateTime.UtcNow;
            using var writer = new StreamWriter(
                _filePath,
                append: true,
                Encoding.UTF8,
                bufferSize: 4096
            );
            writer.WriteLine(data);
        }
        finally
        {
            entry.AsyncLock.Release();
        }
    }

    // ✅ 异步写入（使用 StreamWriter + WriteLineAsync）🌟 核心改造点
    public async Task WriteDataAsync(string data, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        var entry = _fileLocks.GetOrAdd(_filePath, _ => new FileLockEntry());

        await entry.AsyncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            entry.LastAccess = DateTime.UtcNow;

            // 使用 using 异步释放资源
            using var stream = new FileStream(
                _filePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read, // 允许其他进程读取（如日志查看器）
                bufferSize: 4096,
                useAsync: true
            ); // 启用异步 I/O

            using var writer = new StreamWriter(
                stream,
                Encoding.UTF8,
                bufferSize: 1024,
                leaveOpen: false
            ); // 写完自动关闭流

            await writer.WriteLineAsync(data).ConfigureAwait(false);
            // StreamWriter 会在 Dispose 时 Flush，但你也可以手动：
            // await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            entry.AsyncLock.Release();
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SmartFileWriter));
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        lock (_timerLock)
        {
            _activeInstanceCount--;
            if (_activeInstanceCount <= 0)
            {
                StopCleanupTimer();
            }
        }
    }

    public static void ForceCleanup() => CleanupUnusedLocks(null);

    public static int GetLockCount() => _fileLocks.Count;
}

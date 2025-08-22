using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxesCore.Helpers;


/// <summary>
/// 通用事件管理器，支持弱引用订阅，防止内存泄漏。
/// 支持 Action、Func、异步委托，自动清理已回收的监听器。
/// </summary>
public class EventManager : IDisposable
{
    public static EventManager Default { get; } = new EventManager();

    // 存储事件：键 => 事件处理器包装列表
    private readonly ConcurrentDictionary<string, List<EventHandlerWrapper>> _eventHandlers =
        new ConcurrentDictionary<string, List<EventHandlerWrapper>>();

    private Timer _cleanupTimer;
    private readonly object _listLock = new object();
    private bool _disposed = false;

    // 清理间隔（默认 60 秒）
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromSeconds(60);

    public EventManager(TimeSpan? cleanupInterval = null)
    {
        _cleanupInterval = cleanupInterval ?? TimeSpan.FromSeconds(60);

        // 启动后台清理定时器
        _cleanupTimer = new Timer(
            _ => CleanupDeadHandlers(),
            null,
            _cleanupInterval,
            _cleanupInterval
        );
    }

    #region 注册事件（自动包装为弱引用）

    public void On(string eventName, Action handler) => AddHandler(eventName, handler);
    public void On<T>(string eventName, Action<T> handler) => AddHandler(eventName, handler);
    public void On<TResult>(string eventName, Func<TResult> handler) => AddHandler(eventName, handler);
    public void On(string eventName, Func<Task> handler) => AddHandler(eventName, handler);
    public void On<T>(string eventName, Func<T, Task> handler) => AddHandler(eventName, handler);
    public void On<TResult>(string eventName, Func<Task<TResult>> handler) => AddHandler(eventName, handler);

    private void AddHandler(string eventName, Delegate handler)
    {
        if (string.IsNullOrEmpty(eventName)) throw new ArgumentException("Event name cannot be null or empty.", nameof(eventName));
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        if (_disposed) throw new ObjectDisposedException(nameof(EventManager));

        var wrapper = new EventHandlerWrapper(handler);

        _eventHandlers.AddOrUpdate(
            eventName,
            _ => new List<EventHandlerWrapper> { wrapper },
            (key, list) =>
            {
                lock (list)
                {
                    // 避免重复添加
                    if (!list.Any(w => DelegatesEqual(w.Handler, handler)))
                        list.Add(wrapper);
                }
                return list;
            });
    }

    #endregion

    #region 移除事件

    public bool Off(string eventName, Delegate handler)
    {
        if (string.IsNullOrEmpty(eventName) || handler == null || _disposed) return false;

        if (_eventHandlers.TryGetValue(eventName, out var list))
        {
            lock (list)
            {
                var wrapper = list.FirstOrDefault(w => DelegatesEqual(w.Handler, handler));
                if (wrapper != null)
                {
                    list.Remove(wrapper);
                    if (list.Count == 0)
                    {
                        _eventHandlers.TryRemove(eventName, out _);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    public bool OffAll(string eventName)
    {
        return !string.IsNullOrEmpty(eventName) && _eventHandlers.TryRemove(eventName, out _);
    }

    #endregion

    #region 触发事件（Fire）

    public void Fire(string eventName)
    {
        if (string.IsNullOrEmpty(eventName) || _disposed) return;

        if (!_eventHandlers.TryGetValue(eventName, out var handlers)) return;

        var toInvoke = GetAliveHandlers(handlers);
        foreach (var handler in toInvoke)
        {
            switch (handler)
            {
                case Action action:
                    SafeInvoke(action);
                    break;
                case Func<Task> funcTask:
                    _ = SafeFireAndForget(funcTask);
                    break;
            }
        }
    }

    public void Fire<T>(string eventName, T arg)
    {
        if (string.IsNullOrEmpty(eventName) || _disposed) return;

        if (!_eventHandlers.TryGetValue(eventName, out var handlers)) return;

        var toInvoke = GetAliveHandlers(handlers);
        foreach (var handler in toInvoke)
        {
            switch (handler)
            {
                case Action<T> action:
                    SafeInvoke(() => action(arg));
                    break;
                case Func<T, Task> funcTask:
                    _ = SafeFireAndForget(() => funcTask(arg));
                    break;
            }
        }
    }

    public List<TResult> Fire<TResult>(string eventName)
    {
        if (string.IsNullOrEmpty(eventName) || _disposed) return new List<TResult>();

        if (!_eventHandlers.TryGetValue(eventName, out var handlers)) return new List<TResult>();

        var results = new List<TResult>();
        var toInvoke = GetAliveHandlers(handlers);

        foreach (var handler in toInvoke)
        {
            if (handler is Func<TResult> func)
            {
                try { results.Add(func()); }
                catch { }
            }
            else if (handler is Func<Task<TResult>> asyncFunc)
            {
                try { results.Add(asyncFunc().GetAwaiter().GetResult()); }
                catch { }
            }
        }

        return results;
    }

    #endregion

    #region 异步触发

    public async Task FireAsync(string eventName)
    {
        if (string.IsNullOrEmpty(eventName) || _disposed) return;

        if (!_eventHandlers.TryGetValue(eventName, out var handlers)) return;

        var toInvoke = GetAliveHandlers(handlers);
        var tasks = new List<Task>();

        foreach (var handler in toInvoke)
        {
            switch (handler)
            {
                case Action action:
                    tasks.Add(Task.Run(() => SafeInvoke(action)));
                    break;
                case Func<Task> funcTask:
                    tasks.Add(SafeTask(funcTask));
                    break;
            }
        }

        if (tasks.Any()) await Task.WhenAll(tasks);
    }

    public async Task FireAsync<T>(string eventName, T arg)
    {
        if (string.IsNullOrEmpty(eventName) || _disposed) return;

        if (!_eventHandlers.TryGetValue(eventName, out var handlers)) return;

        var toInvoke = GetAliveHandlers(handlers);
        var tasks = new List<Task>();

        foreach (var handler in toInvoke)
        {
            switch (handler)
            {
                case Action<T> action:
                    tasks.Add(Task.Run(() => SafeInvoke(() => action(arg))));
                    break;
                case Func<T, Task> funcTask:
                    tasks.Add(SafeTask(() => funcTask(arg)));
                    break;
            }
        }

        if (tasks.Any()) await Task.WhenAll(tasks);
    }

    public async Task<List<TResult>> FireAsync<TResult>(string eventName)
    {
        if (string.IsNullOrEmpty(eventName) || _disposed) return new List<TResult>();

        if (!_eventHandlers.TryGetValue(eventName, out var handlers)) return new List<TResult>();

        var tasks = new List<Task<TResult>>();
        var toInvoke = GetAliveHandlers(handlers);

        foreach (var handler in toInvoke)
        {
            if (handler is Func<TResult> syncFunc)
            {
                tasks.Add(Task.Run(() =>
                {
                    try { return syncFunc(); }
                    catch { return default(TResult); }
                }));
            }
            else if (handler is Func<Task<TResult>> asyncFunc)
            {
                tasks.Add(asyncFunc());
            }
        }

        if (!tasks.Any()) return new List<TResult>();
        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null).ToList();
    }

    #endregion

    #region 私有帮助方法

    /// <summary>
    /// 获取仍存活的处理器列表（线程安全快照）
    /// </summary>
    private List<Delegate> GetAliveHandlers(List<EventHandlerWrapper> source)
    {
        lock (source)
        {
            return source
                .Where(w => w.IsAlive)
                .Select(w => w.GetAliveHandler())
                .Where(h => h != null)
                .ToList();
        }
    }

    /// <summary>
    /// 清理已死亡的处理器
    /// </summary>
    private void CleanupDeadHandlers()
    {
        if (_disposed) return;

        foreach (var kvp in _eventHandlers)
        {
            var list = kvp.Value;
            lock (list)
            {
                var alive = list.Where(w => w.IsAlive).ToList();
                list.Clear();
                list.AddRange(alive);
            }

            // 如果清空后为空，移除键
            if (list.Count == 0)
            {
                _eventHandlers.TryRemove(kvp.Key, out _);
            }
        }
    }

    /// <summary>
    /// 安全调用
    /// </summary>
    private void SafeInvoke(Action action)
    {
        try { action(); } catch { }
    }

    private async Task SafeFireAndForget(Func<Task> func)
    {
        try { await func(); } catch { }
    }

    private async Task SafeTask(Func<Task> func)
    {
        try { await func(); } catch { }
    }

    /// <summary>
    /// 比较两个委托是否“等价”（方法 + 目标对象）
    /// </summary>
    private static bool DelegatesEqual(Delegate a, Delegate b)
    {
        if (a == b) return true;
        return a.Target == b.Target && a.Method == b.Method;
    }

    #endregion

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cleanupTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                _cleanupTimer?.Dispose();
                _eventHandlers.Clear();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
internal class EventHandlerWrapper
{
    public Delegate Handler { get; }
    public WeakReference TargetRef { get; } // 用于判断实例是否还存活

    public EventHandlerWrapper(Delegate handler)
    {
        Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        // 如果是实例方法，弱引用其目标对象；静态方法则 target 为 null
        TargetRef = handler.Target != null ? new WeakReference(handler.Target) : null;
    }

    /// <summary>
    /// 判断处理器是否仍然有效（目标对象未被回收）
    /// </summary>
    public bool IsAlive => TargetRef == null || TargetRef.IsAlive;

    /// <summary>
    /// 获取当前有效的方法（若已回收则返回 null）
    /// </summary>
    public Delegate GetAliveHandler()
    {
        if (!IsAlive) return null;

        // 对于实例方法，需要重新绑定到当前实例（以防闭包等复杂情况）
        // 但 Delegate.Combine 会保留原始 Target，所以我们直接返回原 Handler
        return Handler;
    }
}

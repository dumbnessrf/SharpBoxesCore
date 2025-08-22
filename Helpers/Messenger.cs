using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SharpBoxesCore.Helpers;

using System;
using System.Collections.Concurrent;
using System.Threading;

/// <summary>
/// 数据中心事件参数
/// </summary>
public class DataCenterEventArgs : EventArgs
{
    public string Key { get; }
    public object Value { get; }

    public DataCenterEventArgs(string key, object value)
    {
        Key = key;
        Value = value;
    }
}

/// <summary>
/// 通用数据中心，支持任意类型的值，键为字符串。
/// 支持可选的生命周期管理（TTL）。在 Remove 或访问时自动清理过期项。
/// 实现了 IDisposable 的对象在移除或过期时会自动 Dispose。
/// 支持后台自动清理和事件通知。
/// </summary>
public class DataCenter : IDisposable
{
    public static DataCenter Default { get; } = new DataCenter();

    internal readonly ConcurrentDictionary<string, DataEntry> _dataStore =
        new ConcurrentDictionary<string, DataEntry>();
    private Timer _cleanupTimer;
    private readonly object _cleanupLock = new object();
    private bool _disposed = false;
    private bool isCleanupEnabled = true;

    // 事件定义
    public event EventHandler<DataCenterEventArgs> OnItemRemoved;
    public event EventHandler<DataCenterEventArgs> OnItemExpired;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="cleanupInterval">自动清理间隔，默认 30 秒</param>
    public DataCenter(TimeSpan? cleanupInterval = null)
    {
        isCleanupEnabled = cleanupInterval.HasValue;

        if (isCleanupEnabled)
        {
            var interval = cleanupInterval.HasValue ? cleanupInterval.Value.TotalMilliseconds : 0;
            if (interval < 100)
                interval = 100; // 最小 100ms
            _cleanupTimer = new Timer(
                state => CleanupExpired(),
                null,
                TimeSpan.Zero, // 立即开始第一次
                TimeSpan.FromMilliseconds(interval)
            );
        }
    }

    /// <summary>
    /// 添加一个键值对。如果键已存在，抛出异常。
    /// </summary>
    /// <param name="key">键，不能为空或空字符串</param>
    /// <param name="value">值，可以为任意类型</param>
    /// <param name="ttl">可选：生命周期时长，null 表示永不过期</param>
    /// <exception cref="ArgumentException">键为空或已存在</exception>
    public void Add(string key, object value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var expiration = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : (DateTime?)null;
        var entry = new DataEntry { Value = value, Expiration = expiration };

        if (!_dataStore.TryAdd(key, entry))
        {
            throw new ArgumentException($"Key '{key}' already exists in the data center.");
        }
    }

    /// <summary>
    /// 获取指定键对应的值，若不存在或已过期则返回 null。
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>对应的值，如果键不存在或已过期返回 null</returns>
    public object Get(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        if (_dataStore.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                TryRemoveEntry(key, entry, isExpired: true);
                return null;
            }

            var value = entry.GetValue();
            if (entry.IsCollected)
            {
                // 对象已被 GC 回收
                TryRemoveEntry(key, entry, isExpired: true); // 视为过期
                return null;
            }

            return value;
        }

        return null;
    }

    /// <summary>
    /// 获取指定键的值并尝试转换为指定类型。
    /// </summary>
    /// <typeparam name="T">期望的类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="value">输出值</param>
    /// <returns>是否成功获取并转换</returns>
    public bool TryGet<T>(string key, out T value)
    {
        value = default;
        var obj = Get(key);
        if (obj is T tValue)
        {
            value = tValue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 移除指定键的值。如果值实现了 IDisposable，则调用其 Dispose 方法。
    /// 触发 OnItemRemoved 事件。
    /// </summary>
    /// <param name="key">要移除的键</param>
    /// <returns>是否成功移除</returns>
    public bool Remove(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        return _dataStore.TryRemove(key, out var entry)
            && TryRemoveEntry(key, entry, isExpired: false);
    }

    /// <summary>
    /// 更新指定键的值。如果键不存在，抛出异常。
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">新值</param>
    /// <param name="ttl">可选：新的生命周期</param>
    /// <exception cref="ArgumentException">键不存在</exception>
    public void Update(string key, object value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        if (!_dataStore.ContainsKey(key))
        {
            throw new ArgumentException($"Key '{key}' does not exist in the data center.");
        }

        var expiration = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : (DateTime?)null;
        var newEntry = new DataEntry { Value = value, Expiration = expiration };

        _dataStore[key] = newEntry;
    }

    /// <summary>
    /// 添加或更新指定键的值。如果键存在则更新（覆盖 TTL），否则添加。
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="ttl">可选：生命周期</param>
    public void AddOrUpdate(string key, object value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var expiration = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : (DateTime?)null;
        var entry = new DataEntry { Value = value, Expiration = expiration };

        _dataStore.AddOrUpdate(
            key,
            entry,
            (k, oldEntry) =>
            {
                TryDisposeEntry(oldEntry);
                return entry;
            }
        );
    }

    /// <summary>
    /// 检查是否包含未过期的指定键。
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>是否存在且未过期</returns>
    public bool ContainsKey(string key)
    {
        return Get(key) != null;
    }

    /// <summary>
    /// 清空所有数据，并释放所有实现了 IDisposable 的对象。
    /// 触发 OnItemRemoved 事件（不触发 OnItemExpired）。
    /// </summary>
    public void Clear()
    {
        foreach (var kvp in _dataStore)
        {
            TryRemoveEntry(kvp.Key, kvp.Value, isExpired: false);
        }
        _dataStore.Clear();
    }

    /// <summary>
    /// 获取当前未过期的条目数量。
    /// </summary>
    public int Count
    {
        get
        {
            int count = 0;
            var now = DateTime.UtcNow;
            foreach (var kvp in _dataStore)
            {
                var entry = kvp.Value;
                if (entry.IsExpired || entry.IsCollected)
                    continue;
                count++;
            }
            return count;
        }
    }

    /// <summary>
    /// 主动清理所有过期项（由定时器或手动调用）
    /// </summary>
    public void CleanupExpired()
    {
        if (_disposed)
            return;

        var keysToRemove = new System.Collections.Generic.List<string>();

        foreach (var kvp in _dataStore)
        {
            var entry = kvp.Value;
            if (entry.IsExpired || entry.IsCollected)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            if (_dataStore.TryRemove(key, out var entry))
            {
                TryRemoveEntry(key, entry, isExpired: entry.IsExpired || entry.IsCollected);
            }
        }
    }

    #region Private Helpers

    /// <summary>
    /// 尝试移除并释放一个条目，同时触发事件
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="entry">条目</param>
    /// <param name="isExpired">是否因过期而移除</param>
    /// <returns>是否成功释放</returns>
    private bool TryRemoveEntry(string key, DataEntry entry, bool isExpired)
    {
        bool disposed = TryDisposeEntry(entry);

        if (isExpired)
        {
            OnItemExpired?.Invoke(this, new DataCenterEventArgs(key, entry.Value));
        }

        OnItemRemoved?.Invoke(this, new DataCenterEventArgs(key, entry.Value));

        return disposed;
    }

    /// <summary>
    /// 释放条目中的值（如果是 IDisposable）
    /// </summary>
    internal bool TryDisposeEntry(DataEntry entry)
    {
        if (entry?.Value is IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                // 可替换为日志系统
                // Console.WriteLine($"Dispose failed for {entry.Value?.GetType()}: {ex.Message}");
            }
            return true;
        }
        return false;
    }

    #endregion

    #region IDisposable Support

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cleanupTimer?.Change(Timeout.Infinite, Timeout.Infinite); // 停止定时器
                _cleanupTimer?.Dispose();

                Clear(); // 释放所有资源
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

    /// <summary>
    /// 以弱引用方式添加对象（适用于大对象缓存）
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">大对象值</param>
    /// <param name="ttl">生命周期</param>
    /// <param name="trackResurrection">是否跟踪终结器（finalizer）</param>
    public void AddWeak(
        string key,
        object value,
        TimeSpan? ttl = null,
        bool trackResurrection = false
    )
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var expiration = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : (DateTime?)null;
        var entry = new DataEntry();
        entry.SetWeakReference(value, trackResurrection);
        entry.Expiration = expiration;

        if (!_dataStore.TryAdd(key, entry))
        {
            throw new ArgumentException($"Key '{key}' already exists.");
        }
    }
}

/// <summary>
/// 存储带过期时间的数据项，支持弱引用
/// </summary>
internal class DataEntry
{
    public object Value { get; set; }
    public WeakReference WeakRef { get; set; } // 用于大对象弱引用存储
    public DateTime? Expiration { get; set; } // null 表示永不过期

    public bool IsExpired => Expiration.HasValue && DateTime.UtcNow > Expiration.Value;

    /// <summary>
    /// 获取实际值（考虑弱引用）
    /// </summary>
    public object GetValue()
    {
        if (WeakRef != null)
        {
            return WeakRef.IsAlive ? WeakRef.Target : null;
        }
        return Value;
    }

    /// <summary>
    /// 设置为弱引用模式
    /// </summary>
    public void SetWeakReference(object value, bool trackResurrection = false)
    {
        Value = null;
        WeakRef = new WeakReference(value, trackResurrection);
    }

    /// <summary>
    /// 是否为弱引用且已回收
    /// </summary>
    public bool IsCollected => WeakRef != null && !WeakRef.IsAlive;
}

/// <summary>
/// DataCenter 扩展方法
/// </summary>
public static class DataCenterExtensions
{
    /// <summary>
    /// 获取值，若不存在则通过工厂添加（线程安全）
    /// </summary>
    public static T GetOrAdd<T>(
        this DataCenter dc,
        string key,
        Func<string, T> factory,
        TimeSpan? ttl = null
    )
    {
        if (dc.TryGet<T>(key, out var value))
        {
            return value;
        }

        value = factory(key);
        dc.AddOrUpdate(key, value, ttl);
        return value;
    }

    /// <summary>
    /// 尝试更新值，仅当当前值等于预期旧值时才更新（类似 CAS）
    /// </summary>
    /// <returns>是否更新成功</returns>
    public static bool TryUpdate<T>(
        this DataCenter dc,
        string key,
        T oldValue,
        T newValue,
        TimeSpan? ttl = null
    )
    {
        if (string.IsNullOrEmpty(key))
            return false;

        return dc
                ._dataStore.AddOrUpdate(
                    key,
                    new DataEntry
                    {
                        Value = newValue,
                        Expiration = ttl.HasValue
                            ? DateTime.UtcNow.Add(ttl.Value)
                            : (DateTime?)null,
                    },
                    (k, existingEntry) =>
                    {
                        var currentVal = existingEntry.GetValue();
                        if (Equals(currentVal, oldValue))
                        {
                            // 更新为新值
                            var newEntry = new DataEntry
                            {
                                Value = newValue,
                                Expiration = ttl.HasValue
                                    ? DateTime.UtcNow.Add(ttl.Value)
                                    : (DateTime?)null,
                            };
                            dc.TryDisposeEntry(existingEntry);
                            return newEntry;
                        }
                        return existingEntry; // 保持原值
                    }
                )
                .GetValue()
                is T result
            && Equals(result, newValue);
    }
}

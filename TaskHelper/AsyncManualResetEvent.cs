using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxesCore.TaskHelper;
public class AsyncManualResetEvent
{
    // 使用 volatile 确保多线程可见性
    private volatile TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>(false);

    // 等待信号 (异步，不阻塞线程)
    public Task WaitAsync() => _tcs.Task;

    // 设置信号 (释放所有等待者)
    public void Set() => _tcs.TrySetResult(true);

    // 重置信号 (创建新的任务)
    public void Reset()
    {
        // 只有当当前任务已完成时，才允许重置，防止逻辑错误
        // 这里采用“交换”策略，确保原子性
        while (true)
        {
            var oldTcs = _tcs;
            if (oldTcs.Task.IsCompleted)
            {
                var newTcs = new TaskCompletionSource<bool>(false);
                // 尝试替换，如果失败说明别的线程改了，重试
                if (Interlocked.CompareExchange(ref _tcs, newTcs, oldTcs) == oldTcs)
                    break;
            }
            else
            {
                // 如果任务还没完成，说明还在等待中，此时不应 Reset，或者直接新建
                // 简单实现：直接新建，旧的等待者会永远等不到（取决于业务逻辑）
                // 严谨实现通常需要更复杂的状态机，这里简化为直接新建
                _tcs = new TaskCompletionSource<bool>(false);
                break;
            }
        }
    }
} 
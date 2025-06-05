namespace SharpBoxesCore.Helpers;

/// <summary>
/// 高级计时器，可以记录多个时间点，并计算各时间点之间的耗时
/// <example>
/// <code>
/// var stopWatch = AdvancedStopWatch.StartNew();
/// 
/// // 设置一个超时回调（5秒后触发）
/// stopWatch.SetTimeout(TimeSpan.FromSeconds(5), () =>
/// {
/// 	Console.WriteLine("[超时回调] 已经过了 5 秒！");
/// });
/// 
/// // 模拟第一个操作
/// await Task.Delay(1000); // 等待1秒
/// stopWatch.Lap("第一段操作");
/// 
/// // 模拟第二个操作
/// await Task.Delay(2000);
/// stopWatch.Lap("第二段操作");
/// 
/// // 模拟第三个操作
/// await Task.Delay(1500);
/// stopWatch.Lap("第三段操作");
/// 
/// // 停止计时器
/// stopWatch.Stop();
/// 
/// // 输出所有 lap 时间
/// Console.WriteLine("\n=== 分段时间记录 ===");
/// foreach (var lap in stopWatch.GetLaps())
/// {
///     Console.WriteLine($"{lap.Key}: {lap.Value}");
/// }
/// 
/// Console.WriteLine($"\n总耗时: {stopWatch.Elapsed}");
/// </code></example>
/// <example>
/// 以上代码的输出结果为：
/// <code>
/// === 分段时间记录 ===
/// 第一段操作: 00:00:00.9999363
/// 第二段操作: 00:00:02.0000858
/// 第三段操作: 00:00:01.5007894
/// 
/// 总耗时: 00:00:00.0001788
/// 
/// [超时回调] 已经过了 5 秒！
/// </code>
/// </example>
/// </summary>
public class AdvancedStopWatch
{
    public static AdvancedStopWatch StartNew() => new AdvancedStopWatch();

    private Dictionary<string, TimeSpan> Laps = new Dictionary<string, TimeSpan>();
    private Stopwatch _stopwatch = new Stopwatch();

    public AdvancedStopWatch()
    {
        _stopwatch.Start();
    }

    public void SetTimeout(TimeSpan timeout, Action onTimeout)
    {
        Task.Delay(timeout)
            .ContinueWith(t =>
            {
                if (t.IsCompleted)
                    onTimeout();
            });
    }

    public void Stop() => _stopwatch.Stop();

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// 记录当前耗时，并重新计时
    /// </summary>
    /// <param name="name"></param>
    public void Lap(string name)
    {
        Laps[name] = StopAndGetElapsed;
        _stopwatch.Restart();
    }

    public TimeSpan StopAndGetElapsed
    {
        get
        {
            _stopwatch.Stop();
            return _stopwatch.Elapsed;
        }
    }

    public Dictionary<string,TimeSpan> GetLaps()
    {
        return Laps;
    }
}

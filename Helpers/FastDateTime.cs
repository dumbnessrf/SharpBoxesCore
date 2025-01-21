using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxes.Helpers;

/// <summary>
/// 提供比原生DateTime.Now更快的Now属性，避免每次调用时都进行TimeZone转换。
/// </summary>
public static class FastDateTime
{
    static TimeSpan LocalUtcOffset;

    /// <summary>
    /// 获取当前时间，并根据本地时区进行时区转换。
    /// </summary>
    public static DateTime Now
    {
        get { return DateTime.UtcNow + LocalUtcOffset; }
    }

    static FastDateTime()
    {
        LocalUtcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        var a = FastDateTime.Now;
    }
}

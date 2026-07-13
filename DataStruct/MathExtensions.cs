using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using SharpBoxesCore.DataStruct.Structure;
using SharpBoxesCore.DataStruct.Structure;

namespace SharpBoxesCore.DataStruct;

public static class Extensions
{
    [DebuggerStepThrough]
    public static double Round(this double d, int digits = 5)
    {
        return Math.Round(d, digits);
    }

    [DebuggerStepThrough]
    public static decimal Round(this decimal d, int digits = 5)
    {
        return Math.Round(d, digits);
    }

    [DebuggerStepThrough]
    public static float Round(this float d, int digits = 5)
    {
        return (float)Math.Round(d, digits);
    }

   

    [DebuggerStepThrough]
    public static double RadiansToDegrees(this double radians)
    {
        return radians * 180 / Math.PI;
    }

    [DebuggerStepThrough]
    public static double DegreesToRadians(this double degrees)
    {
        return degrees * Math.PI / 180;
    }

   
    [DebuggerStepThrough]
    public static Task<List<T>> ToListAsync<T>(this IEnumerable<T> query)
    {
        return Task.Run(() => query.ToList());
    }

    /// <summary>
    /// 数值范围限制扩展方法（.NET 48 替代 Math.Clamp）
    /// </summary>
    public static T Clamp<T>(this T value, T min, T max)
        where T : IComparable<T>
    {
        if (min.CompareTo(max) > 0)
            throw new ArgumentException("min 不能大于 max", nameof(min));

        if (value.CompareTo(min) < 0)
            return min;
        if (value.CompareTo(max) > 0)
            return max;
        return value;
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using SharpBoxesCore.Helpers;

namespace SharpBoxesCore.DataStruct;

/// <summary>
/// 辅助类
/// </summary>
public static class Helpers
{
    /// <summary>
    /// 将字典转换为Json字符串
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">值的类型</typeparam>
    /// <param name="dict">字典</param>
    /// <returns>Json字符串</returns>
    [DebuggerStepThrough]
    public static string DictToJson<TKey, TValue>(this Dictionary<TKey, TValue> dict)
    {
        return JsonConvert.SerializeObject(dict);
    }
    [DebuggerStepThrough]
    public static void AddOrUpdate<TKey, TValue>(
        this Dictionary<TKey, TValue> dict,
        TKey key,
        TValue value
    )
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = value;
        }
        else
        {
            dict.Add(key, value);
        }
    }
    [DebuggerStepThrough]
    public static TValue GetOrAdd<TKey, TValue>(
        this Dictionary<TKey, TValue> dict,
        TKey key,
        Func<TValue> valueFactory
    )
    {
        if (dict.ContainsKey(key))
        {
            return dict[key];
        }
        else
        {
            var value = valueFactory();
            dict.Add(key, value);
            return value;
        }
    }
    [DebuggerStepThrough]
    public static void GetValueOrDefault<TKey, TValue>(
        this Dictionary<TKey, TValue> dict,
        TKey key,
        TValue defaultValue,
        out TValue value
    )
    {
        if (dict.ContainsKey(key))
        {
            value = dict[key];
        }
        else
        {
            value = defaultValue;
        }
    }
    [DebuggerStepThrough]
    public static void TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
    {
        if (dict.ContainsKey(key))
        {
            dict.Remove(key);
        }
    }

    /// <summary>
    /// 将Json字符串转换为字典
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">值的类型</typeparam>
    /// <param name="json">Json字符串</param>
    /// <returns>字典</returns>
    [DebuggerStepThrough]
    public static Dictionary<TKey, TValue> JsonToDict<TKey, TValue>(this string json)
    {
        return JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(json);
    }

    /// <summary>
    /// 通过Json进行深度复制
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="t">对象</param>
    /// <returns>复制的对象</returns>
    [DebuggerStepThrough]
    public static T CloneByJson<T>(this T t)
    {
        return JsonConvert.DeserializeObject<T>(
            JsonConvert.SerializeObject(
                t,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                }
            ),
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            }
        );
    }
    [DebuggerStepThrough]
    public static DataTable ListToDataTable<T>(List<T> datas, bool isUseDisplayName = false)
    {
        DataTable dt = new DataTable();
        var propertys = typeof(T).GetProperties();
        foreach (var p in propertys)
        {
            if (isUseDisplayName)
            {
                var attrs = p.GetCustomAttributes(
                    typeof(System.ComponentModel.DisplayNameAttribute),
                    false
                );
                if (attrs.Count() > 0)
                {
                    var attr = attrs[0] as System.ComponentModel.DisplayNameAttribute;
                    dt.Columns.Add(attr.DisplayName, p.PropertyType);
                }
                else
                {
                    dt.Columns.Add(p.Name, p.PropertyType);
                }
            }
            else
            {
                dt.Columns.Add(p.Name, p.PropertyType);
            }
        }
        foreach (var d in datas)
        {
            DataRow dr = dt.NewRow();
            foreach (var p in propertys)
            {
                dr[p.Name] = p.GetValue(d);
            }
            dt.Rows.Add(dr);
        }
        return dt;
    }
    [DebuggerStepThrough]
    public static List<T> DataTableToList<T>(this DataTable dt, bool isUseDisplayName = false)
    {
        List<T> list = new List<T>();
        var propertys = typeof(T).GetProperties();
        foreach (DataRow item in dt.Rows)
        {
            T t = Activator.CreateInstance<T>();
            foreach (var p in propertys)
            {
                if (isUseDisplayName)
                {
                    var attrs = p.GetCustomAttributes(
                        typeof(System.ComponentModel.DisplayNameAttribute),
                        false
                    );
                    if (attrs.Count() > 0)
                    {
                        var attr = attrs[0] as System.ComponentModel.DisplayNameAttribute;
                        p.SetValue(t, Convert.ChangeType(item[attr.DisplayName], p.PropertyType));
                    }
                    else
                    {
                        p.SetValue(t, Convert.ChangeType(item[p.Name], p.PropertyType));
                    }
                }
                else
                {
                    p.SetValue(t, Convert.ChangeType(item[p.Name], p.PropertyType));
                }
            }
            list.Add(t);
        }
        return list;
    }

    /// <summary>
    /// 查找所有符合条件的下标
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static List<int> FindAllIndex<T>(this List<T> values, Predicate<T> predicate)
    {
        var indexes = new List<int>();
        for (int i = 0; i < values.Count; i++)
        {
            if (predicate(values[i]))
                indexes.Add(i);
        }
        return indexes;
    }

    /// <summary>
    /// 查找所有符合条件的下标
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static List<int> FindAllIndex<T>(this T[] values, Predicate<T> predicate)
    {
        var indexes = new List<int>();
        for (int i = 0; i < values.Length; i++)
        {
            if (predicate(values[i]))
                indexes.Add(i);
        }
        return indexes;
    }

    /// <summary>
    /// 将字符串转换为double数组
    /// </summary>
    /// <param name="str">字符串</param>
    /// <param name="separator">分隔符，默认为逗号</param>
    /// <returns>double数组</returns>
    public static double[] ToDoubleArr(this string str, string[] separator)
    {
        return str.Split(separator, StringSplitOptions.None).Select(x => double.Parse(x)).ToArray();
    }

    /// <summary>
    /// 将字符串转换为int数组
    /// </summary>
    /// <param name="str">字符串</param>
    /// <param name="separator">分隔符，默认为逗号</param>
    /// <returns>int数组</returns>
    [DebuggerStepThrough]
    public static int[] ToIntArr(this string str, string[] separator)
    {
        return str.Split(separator, StringSplitOptions.None).Select(x => int.Parse(x)).ToArray();
    }

    /// <summary>
    /// 将字符串转换为字符串数组
    /// </summary>
    /// <param name="str">字符串</param>
    /// <param name="separator">分隔符，默认为逗号</param>
    /// <returns>字符串数组</returns>
    public static string[] ToStringArr(this string str, string[] separator)
    {
        return str.Split(separator, StringSplitOptions.None);
    }

    /// <summary>
    /// 将字符串数组转换为字符串
    /// </summary>
    /// <param name="arr">字符串数组</param>
    /// <param name="separator">分隔符，默认为逗号</param>
    /// <returns>字符串</returns>
    public static string ToStr(this IEnumerable<string> arr, string separator)
    {
        return string.Join(separator, arr);
    }

    /// <summary>
    /// 将int数组转换为字符串
    /// </summary>
    /// <param name="arr">int数组</param>
    /// <param name="separator">分隔符，默认为逗号</param>
    /// <returns>字符串</returns>
    [DebuggerStepThrough]
    public static string ToStr(this IEnumerable<int> arr, string separator)
    {
        return string.Join(separator, arr);
    }

    /// <summary>
    /// 将double数组转换为字符串
    /// </summary>
    /// <param name="arr">double数组</param>
    /// <param name="separator">分隔符，默认为逗号</param>
    /// <returns>字符串</returns>
    [DebuggerStepThrough]
    public static string ToStr(this IEnumerable<double> arr, string separator)
    {
        return string.Join(separator, arr);
    }
    [DebuggerStepThrough]
    public static float ToFloat(this string str)
    {
        return float.Parse(str);
    }
    [DebuggerStepThrough]
    public static double ToDouble(this string str)
    {
        return double.Parse(str);
    }
    
    [DebuggerStepThrough]
    public static int ToInt(this string str)
    {
        return int.Parse(str);
    }
    [DebuggerStepThrough]
    public static bool ToBool(this string str)
    {
        return bool.Parse(str);
    }
    [DebuggerStepThrough]
    public static float ToFloat(this double d)
    {
        return (float)d;
    }
    [DebuggerStepThrough]
    public static int ToInt(this double d)
    {
        return (int)d;
    }
    [DebuggerStepThrough]
    public static float ToFloat(this int i)
    {
        return (float)i;
    }
    [DebuggerStepThrough]
    public static double ToDouble(this int i)
    {
        return (double)i;
    }
    [DebuggerStepThrough]
    public static int ToInt(this float f)
    {
        return (int)f;
    }
    [DebuggerStepThrough]
    public static double ToDouble(this float f)
    {
        return (double)f;
    }
    [DebuggerStepThrough]
    public static bool InRange(this double value, double min, double max)
    {
        return value >= min && value <= max;
    }
    [DebuggerStepThrough]
    public static bool InRange(this int value, int min, int max)
    {
        return value >= min && value <= max;
    }
    [DebuggerStepThrough]
    public static bool InRange(this float value, float min, float max)
    {
        return value >= min && value <= max;
    }
    [DebuggerStepThrough]
    public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
    {
        return enumerable == null || !enumerable.Any();
    }
    [DebuggerStepThrough]
    public static bool IsEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }
    [DebuggerStepThrough]
    public static bool IsNotEmpty(this string str)
    {
        return !string.IsNullOrEmpty(str);
    }
    [DebuggerStepThrough]
    public static bool ValidIndex<T>(this List<T> values, int index)
    {
        return index >= 0 && index < values.Count;
    }

    /// <summary>
    /// 获取形状的外接矩形
    /// </summary>
    /// <param name="shape">形状</param>
    /// <returns>外接矩形</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Rectangle1D GetBoundingRectangle(
        this SharpBoxesCore.DataStruct.Structure.IShapeStructure shape
    )
    {
        if (shape == null)
        {
            throw new ArgumentNullException(nameof(shape));
        }

        if (shape is SharpBoxesCore.DataStruct.Structure.Circle circle)
        {
            // 圆的外接矩形
            double x = circle.CenterX - circle.Radius;
            double y = circle.CenterY - circle.Radius;
            double width = circle.Radius * 2;
            double height = circle.Radius * 2;
            return new SharpBoxesCore.DataStruct.Structure.Rectangle1D(width, height, x, y);
        }
        else if (shape is SharpBoxesCore.DataStruct.Structure.Rectangle1D rectangle1D)
        {
            // Rectangle1D本身就是矩形，直接返回
            return rectangle1D;
        }
        else if (shape is SharpBoxesCore.DataStruct.Structure.Rectangle2D rectangle2D)
        {
            // 带角度矩形的外接矩形
            double x = rectangle2D.CenterPoint.X - rectangle2D.HalfWidth;
            double y = rectangle2D.CenterPoint.Y - rectangle2D.HalfHeight;
            double width = rectangle2D.HalfWidth * 2;
            double height = rectangle2D.HalfHeight * 2;
            double angle = rectangle2D.AngleDegree;
            double cos = Math.Cos(angle * Math.PI / 180);
            double sin = Math.Sin(angle * Math.PI / 180);
            double x1 = x + width * cos;
            double y1 = y + height * sin;
            double x2 = x - width * cos;
            double y2 = y - height * sin;
            double minX = Math.Min(x1, x2);
            double minY = Math.Min(y1, y2);
            double maxX = Math.Max(x1, x2);
            double maxY = Math.Max(y1, y2);
            double width2 = maxX - minX;
            double height2 = maxY - minY;
            return new SharpBoxesCore.DataStruct.Structure.Rectangle1D(width2, height2, minX, minY);
        }
        else if (shape is SharpBoxesCore.DataStruct.Structure.Line line)
        {
            // 线段的外接矩形
            double minX = Math.Min(line.X1, line.X2);
            double minY = Math.Min(line.Y1, line.Y2);
            double maxX = Math.Max(line.X1, line.X2);
            double maxY = Math.Max(line.Y1, line.Y2);
            double width = maxX - minX;
            double height = maxY - minY;
            return new SharpBoxesCore.DataStruct.Structure.Rectangle1D(width, height, minX, minY);
        }
        else if (
            shape is SharpBoxesCore.DataStruct.Structure.Polygon polygon
            && polygon.Points != null
            && polygon.Points.Count > 0
        )
        {
            // 多边形的外接矩形
            double minX = polygon.Points.Min(p => p.X);
            double minY = polygon.Points.Min(p => p.Y);
            double maxX = polygon.Points.Max(p => p.X);
            double maxY = polygon.Points.Max(p => p.Y);
            double width = maxX - minX;
            double height = maxY - minY;
            return new SharpBoxesCore.DataStruct.Structure.Rectangle1D(width, height, minX, minY);
        }
        else
        {
            throw new NotSupportedException($"不支持的形状类型: {shape.GetType().Name}");
        }
    }

    /// <summary>
    /// 获取形状的外接圆
    /// </summary>
    /// <param name="shape">形状</param>
    /// <returns>外接圆</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Circle GetBoundingCircle(
        this SharpBoxesCore.DataStruct.Structure.IShapeStructure shape
    )
    {
        if (shape == null)
        {
            throw new ArgumentNullException(nameof(shape));
        }

        if (shape is SharpBoxesCore.DataStruct.Structure.Circle circle)
        {
            // 圆的外接圆就是自己
            return circle;
        }
        else if (shape is SharpBoxesCore.DataStruct.Structure.Rectangle1D rectangle1D)
        {
            // 矩形的外接圆
            double centerX = rectangle1D.CenterPoint.X;
            double centerY = rectangle1D.CenterPoint.Y;
            double radius = Math.Sqrt(
                Math.Pow(rectangle1D.Width / 2, 2) + Math.Pow(rectangle1D.Height / 2, 2)
            );
            return new SharpBoxesCore.DataStruct.Structure.Circle(radius, centerX, centerY);
        }
        else if (shape is SharpBoxesCore.DataStruct.Structure.Rectangle2D rectangle2D)
        {
            // 带角度矩形的外接圆
            double centerX = rectangle2D.CenterPoint.X;
            double centerY = rectangle2D.CenterPoint.Y;
            double radius = Math.Sqrt(
                Math.Pow(rectangle2D.HalfWidth, 2) + Math.Pow(rectangle2D.HalfHeight, 2)
            );
            return new SharpBoxesCore.DataStruct.Structure.Circle(radius, centerX, centerY);
        }
        else if (shape is SharpBoxesCore.DataStruct.Structure.Line line)
        {
            // 线段的外接圆
            double centerX = (line.X1 + line.X2) / 2;
            double centerY = (line.Y1 + line.Y2) / 2;
            double radius =
                Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2)) / 2;
            return new SharpBoxesCore.DataStruct.Structure.Circle(radius, centerX, centerY);
        }
        else if (
            shape is SharpBoxesCore.DataStruct.Structure.Polygon polygon
            && polygon.Points != null
            && polygon.Points.Count > 0
        )
        {
            // 多边形的外接圆（简化版，实际应该计算最小外接圆，这里使用中心点和最远点距离作为半径）
            SharpBoxesCore.DataStruct.Structure.Point centroid = polygon.Centroid;
            double maxDistance = 0;
            foreach (var point in polygon.Points)
            {
                double distance = point.DistanceTo(centroid);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }
            return new SharpBoxesCore.DataStruct.Structure.Circle(
                maxDistance,
                centroid.X,
                centroid.Y
            );
        }
        else
        {
            throw new NotSupportedException($"不支持的形状类型: {shape.GetType().Name}");
        }
    }
}

public static class ConcurrentExtensions
{
    public static void Clear<T>(this ConcurrentQueue<T> queue)
    {
        while (queue.TryDequeue(out var result))
        {
            if (result != null && result is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public static void Clear<T>(this ConcurrentStack<T> stack)
    {
        while (stack.TryPop(out var result))
        {
            if (result != null && result is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public static void Clear<T>(this ConcurrentBag<T> bag)
    {
        while (bag.TryTake(out var item))
        {
            if (item != null && item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public static void Clear<T>(this ConcurrentDictionary<T, T> dictionary)
    {
        foreach (var item in dictionary)
        {
            if (item.Key != null && item.Key is IDisposable disposable)
            {
                disposable.Dispose();
            }

            if (item.Value != null && item.Value is IDisposable disposable2)
            {
                disposable2.Dispose();
            }
        }
        dictionary.Clear();
    }

    public static void GetValueOrDefault<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue defaultValue,
        out TValue value
    )
    {
        if (dictionary.TryGetValue(key, out value))
        {
            return;
        }
        value = defaultValue;
    }

    public static void Clear<T>(this BlockingCollection<T> collection)
    {
        while (collection.TryTake(out var item))
        {
            if (item != null && item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

public static class StringExtensions
{
    // 匹配大写字母开头、小写字母序列、数字序列，或者常见的分隔符
    // 这个正则会忽略分隔符，只提取单词部分
    private static readonly Regex SplitRegex = new Regex(
        @"[A-Z]?[a-z]+|[A-Z]+(?=[A-Z][a-z]|\d|\W|$)|\d+",
        RegexOptions.Compiled
    );

    /// <summary>
    /// 智能转换为帕斯卡命名法 (PascalCase)
    /// 支持: "user_name", "user-name", "userName", "USER_NAME", "xml parser"
    /// 结果: "UserName", "UserName", "UserName", "UserName", "XmlParser"
    /// </summary>
    public static string ToPascalCaseSmart(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var matches = SplitRegex.Matches(input);
        if (matches.Count == 0)
            return input;

        var cultureInfo = CultureInfo.CurrentCulture;

        var parts = matches
            .Cast<Match>()
            .Select(m => m.Value.ToLower(CultureInfo.InvariantCulture)) // 先全部转小写
            .Select(s => cultureInfo.TextInfo.ToTitleCase(s)) // 再首字母大写
            .ToList();

        return string.Concat(parts);
    }

    /// <summary>
    /// 智能转换为驼峰命名法 (camelCase)
    /// </summary>
    public static string ToCamelCaseSmart(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var pascal = input.ToPascalCaseSmart();
        if (pascal.Length <= 1)
            return pascal.ToLower(CultureInfo.InvariantCulture);

        return char.ToLowerInvariant(pascal[0]) + pascal.Substring(1);
    }

    /// <summary>
    /// 将字符串转换为“标题格式” (Title Case)。
    /// 规则：
    /// 1. 识别单词（支持空格、下划线、连字符、驼峰分割）。
    /// 2. 每个单词的首字母大写。
    /// 3. 单词中剩余的所有字符（包括字母和数字混合部分）强制转为小写。
    ///
    /// 示例:
    /// "hello WORLD" -> "Hello World"
    /// "user_NAME_123" -> "User Name 123"
    /// "XMLParser2Go" -> "Xml Parser2 Go" (注意：数字后的字母也被小写了)
    /// "OCR-model" -> "Ocr Model"
    /// </summary>
    public static string ToTitleCaseWords(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // 复用之前的正则逻辑来提取单词，确保能处理各种分隔符和驼峰
        // 正则说明：匹配大写字母开头的小写序列，或连续大写字母（直到遇到小写/数字/结尾），或数字
        var matches = SplitRegex.Matches(input);
        if (matches.Count == 0)
            return input.Trim();

        var cultureInfo = CultureInfo.InvariantCulture;
        var textInfo = cultureInfo.TextInfo;

        var result = new StringBuilder();
        bool isFirstWord = true;

        foreach (Match match in matches)
        {
            string word = match.Value;
            if (string.IsNullOrEmpty(word))
                continue;

            // 如果不是第一个单词，且原字符串中单词之间主要是靠分隔符连接的（非纯驼峰紧凑连接），
            // 这里为了通用性，我们统一加一个空格分隔单词。
            // 如果你希望保留原有的分隔符逻辑会比较复杂，通常“标题格式”都意味着用空格分隔单词。
            if (!isFirstWord)
            {
                result.Append(' ');
            }
            isFirstWord = false;

            // 核心逻辑：
            // 1. 将整个单词转为小写
            // 2. 将首字母转为大写
            string lowerWord = word.ToLower(cultureInfo);

            if (lowerWord.Length > 0)
            {
                string titleWord = textInfo.ToTitleCase(lowerWord);
                result.Append(titleWord);
            }
        }

        return result.ToString();
    }
}

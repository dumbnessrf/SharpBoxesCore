using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxes.CsvServices
{
    /// <summary>
    /// CsvOprHelper静态类，提供CSV数据的操作方法。
    /// </summary>
    public static class CsvOprHelper
    {
        /// <summary>
        /// 将<see cref="List{T}"/>转换为<see cref="DataTable"/>。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="datas">数据列表。</param>
        /// <param name="isUseDisplayName">是否使用DisplayName属性作为列名，默认为false。</param>
        /// <returns>转换后的<see cref="DataTable"/>。</returns>
        public static DataTable ToDT<T>(this List<T> datas, bool isUseDisplayName = false)
        {
            DataTable dt = new DataTable();
            var type = typeof(T);
            var props = type.GetProperties();
            Dictionary<string, string> name_displayName = new Dictionary<string, string>();
            foreach (var prop in props)
            {
                if (isUseDisplayName)
                {
                    var displayName = prop.GetCustomAttributes(
                        typeof(System.ComponentModel.DisplayNameAttribute),
                        true
                    );
                    if (displayName.Length > 0)
                    {
                        var str = (
                            (System.ComponentModel.DisplayNameAttribute)displayName[0]
                        ).DisplayName;
                        dt.Columns.Add(str);
                        name_displayName.Add(prop.Name, str);
                    }
                    else
                    {
                        dt.Columns.Add(prop.Name);
                        name_displayName.Add(prop.Name, prop.Name);
                    }
                }
                else
                {
                    dt.Columns.Add(prop.Name);
                    name_displayName.Add(prop.Name, prop.Name);
                }
            }
            foreach (var item in datas)
            {
                DataRow dr = dt.NewRow();
                foreach (var prop in props)
                {
                    dr[name_displayName[prop.Name]] = prop.GetValue(item);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 将单个数据对象转换为<see cref="DataTable"/>。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="data">数据对象。</param>
        /// <param name="isUseDisplayName">是否使用DisplayName属性作为列名，默认为false。</param>
        /// <returns>转换后的<see cref="DataTable"/>。</returns>
        public static DataTable ToDT<T>(this T data, bool isUseDisplayName = false)
        {
            DataTable dt = new DataTable();
            var type = typeof(T);
            var props = type.GetProperties();
            Dictionary<string, string> name_displayName = new Dictionary<string, string>();
            foreach (var prop in props)
            {
                if (isUseDisplayName)
                {
                    var displayName = prop.GetCustomAttributes(
                        typeof(System.ComponentModel.DisplayNameAttribute),
                        true
                    );
                    if (displayName.Length > 0)
                    {
                        var str = (
                            (System.ComponentModel.DisplayNameAttribute)displayName[0]
                        ).DisplayName;
                        dt.Columns.Add(str);
                        name_displayName.Add(prop.Name, str);
                    }
                    else
                    {
                        dt.Columns.Add(prop.Name);
                        name_displayName.Add(prop.Name, prop.Name);
                    }
                }
                else
                {
                    dt.Columns.Add(prop.Name);
                    name_displayName.Add(prop.Name, prop.Name);
                }
            }

            DataRow dr = dt.NewRow();
            foreach (var prop in props)
            {
                dr[name_displayName[prop.Name]] = prop.GetValue(data);
            }
            dt.Rows.Add(dr);

            return dt;
        }

        /// <summary>
        /// 将<see cref="DataTable"/>转换为CSV格式的<see cref="StringBuilder"/>。
        /// </summary>
        /// <param name="dt"><see cref="DataTable"/>对象。</param>
        /// <returns>转换后的CSV格式的<see cref="StringBuilder"/>。</returns>
        public static StringBuilder ToCSV(this DataTable dt)
        {
            var sb = new StringBuilder();

            foreach (System.Data.DataColumn dc in dt.Columns)
            {
                sb.Append(dc.ColumnName + ",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("\r\n");
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                foreach (System.Data.DataColumn dc in dt.Columns)
                {
                    sb.Append(dr[dc.ColumnName].ToString() + ",");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("\r\n");
            }

            return sb;
        }

        /// <summary>
        /// 将<see cref="List{CsvDataBase}"/>转换为CSV格式的<see cref="StringBuilder"/>。
        /// </summary>
        /// <param name="csvDatas">CSV数据列表。</param>
        /// <returns>转换后的CSV格式的<see cref="StringBuilder"/>。</returns>
        public static StringBuilder ToCSV(List<CsvDataBase> csvDatas)
        {
            var sb = new StringBuilder();

            foreach (var item in csvDatas)
            {
                sb = item.Append(sb);
            }

            return sb;
        }

        /// <summary>
        /// 将<see cref="StringBuilder"/>保存为文件。
        /// </summary>
        /// <param name="sb"><see cref="StringBuilder"/>对象。</param>
        /// <param name="filename">文件名。</param>
        /// <exception cref="IOException">当写入文件时发生错误。</exception>
        public static void SaveToFile(this StringBuilder sb, string filename)
        {
            File.WriteAllText(filename, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 将<see cref="StringBuilder"/>追加到文件。
        /// </summary>
        /// <param name="sb"><see cref="StringBuilder"/>对象。</param>
        /// <param name="filename">文件名。</param>
        /// <exception cref="IOException">当写入文件时发生错误。</exception>
        public static void AppendDataToFile(StringBuilder sb, string filename)
        {
            if (!File.Exists(filename))
            {
                File.WriteAllText(filename, sb.ToString(), Encoding.UTF8);
                return;
            }
            File.AppendAllText(filename, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 将<see cref="CsvDataBase"/>追加到文件。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="data">数据对象。</param>
        /// <param name="filename">文件名。</param>
        /// <exception cref="IOException">当写入文件时发生错误。</exception>
        public static void AppendDataToFile<T>(T data, string filename)
            where T : CsvDataBase
        {
            var rawText = File.ReadAllText(filename);
            var sb = new StringBuilder(rawText);
            sb = data.Append(sb);
            File.WriteAllText(filename, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 将<see cref="List{CsvDataBase}"/>追加到文件。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="datas">数据列表。</param>
        /// <param name="filename">文件名。</param>
        /// <exception cref="IOException">当写入文件时发生错误。</exception>
        public static void AppendDataToFile<T>(List<T> datas, string filename)
            where T : CsvDataBase
        {
            var rawText = File.ReadAllText(filename);
            var sb = new StringBuilder(rawText);
            foreach (var data in datas)
            {
                sb = data.Append(sb);
            }
            File.WriteAllText(filename, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 从文件中移除第一个满足条件的行。
        /// </summary>
        /// <param name="match">条件。</param>
        /// <param name="filename">文件名。</param>
        /// <exception cref="IOException">当读取或写入文件时发生错误。</exception>
        public static void RemoveFirstSpecificRowInFile(Predicate<string> match, string filename)
        {
            var rawText = File.ReadAllLines(filename).ToList();
            rawText.RemoveAt(rawText.FindIndex(match));
        }

        /// <summary>
        /// 从文件中移除所有满足条件的行。
        /// </summary>
        /// <param name="match">条件。</param>
        /// <param name="filename">文件名。</param>
        /// <exception cref="IOException">当读取或写入文件时发生错误。</exception>
        public static void RemoveAllSpecificRowInFile(Predicate<string> match, string filename)
        {
            var rawText = File.ReadAllLines(filename).ToList();

            rawText.RemoveAll(match);
            File.WriteAllLines(filename, rawText, Encoding.UTF8);
        }

        /// <summary>
        /// <![CDATA[将CSV文件内容读取为嵌套列表（List<List<string>>），按行和逗号分隔]]>
        /// </summary>
        /// <param name="csvPath">CSV文件路径</param>
        /// <returns>包含所有行数据的嵌套列表</returns>
        public static List<List<string>> ReadCsvToList(string csvPath)
        {
            if (string.IsNullOrEmpty(csvPath))
                throw new ArgumentException($"CSV文件路径不能为空{csvPath}", nameof(csvPath));

            if (!File.Exists(csvPath))
                throw new FileNotFoundException($"CSV文件未找到{csvPath}", csvPath);
            var result = new List<List<string>>();
            var lines = File.ReadAllLines(csvPath, Encoding.UTF8);

            foreach (var line in lines)
            {
                // 简单分割处理（不处理带引号的复杂情况）
                var fields = line.Split(',').Select(f => f.Trim()).ToList();
                result.Add(fields);
            }
            return result;
        }

        /// <summary>
        /// 将CSV文件内容映射到指定实体类型的列表
        /// </summary>
        /// <typeparam name="T">目标实体类型</typeparam>
        /// <param name="csvPath">CSV文件路径</param>
        /// <param name="useDisplayName">是否使用DisplayName属性匹配列名</param>
        /// <param name="stringComparison">字符串比较规则（默认忽略大小写）</param>
        /// <param name="onErrorOccur">异常处理回调函数</param>
        /// <returns>包含实体对象的列表</returns>
        /// <example>
        /// 以下示例展示了如何使用DisplayName属性匹配列头并处理异常：
        /// <code>
        /// // 定义实体类
        /// public class User
        /// {
        ///     [DisplayName("用户名")]
        ///     public string Name { get; set; }
        ///
        ///     [DisplayName("年龄")]
        ///     public int Age { get; set; }
        ///
        ///     [DisplayName("邮箱")]
        ///     public string Email { get; set; }
        /// }
        ///
        /// // 自定义异常处理逻辑
        /// Action&lt;Exception&gt; onError = ex =>
        /// {
        ///     Console.WriteLine($"发生异常: {ex.Message}");
        /// };
        ///
        /// // 读取CSV到实体列表
        /// var users = CsvOprHelper.ReadCsvToListEntity&lt;User&gt;(
        ///     "data.csv",
        ///     useDisplayName: true, // 使用DisplayName属性匹配列头
        ///     onErrorOccur: onError // 自定义异常处理
        /// );
        ///
        /// // 输出结果
        /// foreach (var user in users)
        /// {
        ///     Console.WriteLine($"Name: {user.Name}, Age: {user.Age}, Email: {user.Email}");
        /// }
        /// </code>
        /// </example>
        public static List<T> ReadCsvToListEntityc<T>(
            string csvPath,
            bool useDisplayName = false,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase,
            Action<Exception> onErrorOccur = null
        )
            where T : new()
        {
            if (string.IsNullOrEmpty(csvPath))
                throw new ArgumentException($"CSV文件路径不能为空{csvPath}", nameof(csvPath));

            if (!File.Exists(csvPath))
                throw new FileNotFoundException($"CSV文件未找到{csvPath}", csvPath);

            var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
            if (lines.Length == 0)
                return [];

            // 处理列头映射
            var headers = lines[0].Split(',').Select(h => h.Trim()).ToList();
            var properties = typeof(T).GetProperties(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
            );
            var columnMap = new Dictionary<string, System.Reflection.PropertyInfo>();

            foreach (var prop in properties)
            {
                string targetHeader = useDisplayName ? GetDisplayName(prop) : prop.Name;

                // 查找匹配的列名（忽略大小写）
                var matchedHeader = headers.FirstOrDefault(h =>
                    h.Equals(targetHeader, stringComparison)
                );

                if (!string.IsNullOrEmpty(matchedHeader))
                {
                    columnMap[matchedHeader] = prop;
                }
            }

            // 处理数据行
            var result = new List<T>();
            for (int i = 1; i < lines.Length; i++)
            {
                var obj = new T();
                var fields = lines[i].Split(',').Select(f => f.Trim()).ToArray();

                for (int j = 0; j < headers.Count; j++)
                {
                    if (j >= fields.Length)
                        break;

                    var header = headers[j];
                    if (!columnMap.TryGetValue(header, out var prop))
                        continue;

                    try
                    {
                        var value = Convert.ChangeType(fields[j], prop.PropertyType);
                        prop.SetValue(obj, value);
                    }
                    catch (Exception ex)
                    {
                        // 类型转换失败时跳过（可根据需求抛出异常）
                        onErrorOccur?.Invoke(ex);
                    }
                }
                result.Add(obj);
            }

            return result;
        }

        /// <summary>
        /// 获取属性的DisplayName或默认名称
        /// </summary>
        private static string GetDisplayName(System.Reflection.PropertyInfo prop)
        {
            var displayNameAttr =
                prop.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault()
                as DisplayNameAttribute;
            return displayNameAttr?.DisplayName ?? prop.Name;
        }

        /// <summary>
        /// 将CSV文件内容映射到指定实体类型的列表，支持自定义列头匹配逻辑和异常处理
        /// </summary>
        /// <typeparam name="T">目标实体类型</typeparam>
        /// <param name="csvPath">CSV文件路径</param>
        /// <param name="headerMatchFunc">自定义列头匹配逻辑的函数</param>
        /// <param name="stringComparison">字符串比较规则（默认忽略大小写）</param>
        /// <param name="onErrorOccur">异常处理回调函数</param>
        /// <returns>包含实体对象的列表</returns>
        /// <example>
        /// 以下示例展示了如何使用自定义列头匹配逻辑和异常处理：
        /// <code>
        /// // 定义实体类
        /// public class User
        /// {
        ///     public string Name { get; set; }
        ///     public int Age { get; set; }
        ///     public string Email { get; set; }
        /// }
        ///
        /// // 自定义列头匹配逻辑
        /// Func&lt;PropertyInfo, string&gt; customHeaderMatchFunc = prop =>
        /// {
        ///     return prop.Name switch
        ///     {
        ///         "Name" => "用户名", // 将 "Name" 属性映射到 "用户名" 列
        ///         "Age" => "年龄",   // 将 "Age" 属性映射到 "年龄" 列
        ///         "Email" => "邮箱", // 将 "Email" 属性映射到 "邮箱" 列
        ///         _ => prop.Name     // 默认使用属性名
        ///     };
        /// };
        ///
        /// // 自定义异常处理逻辑
        /// Action&lt;Exception&gt; onError = ex =>
        /// {
        ///     Console.WriteLine($"发生异常: {ex.Message}");
        /// };
        ///
        /// // 读取CSV到实体列表
        /// var users = CsvOprHelper.ReadCsvToListEntity&lt;User&gt;(
        ///     "data.csv",
        ///     customHeaderMatchFunc,
        ///     onErrorOccur: onError
        /// );
        ///
        /// // 输出结果
        /// foreach (var user in users)
        /// {
        ///     Console.WriteLine($"Name: {user.Name}, Age: {user.Age}, Email: {user.Email}");
        /// }
        /// </code>
        /// </example>
        public static List<T> ReadCsvToListEntity<T>(
            string csvPath,
            Func<System.Reflection.PropertyInfo, string> headerMatchFunc,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase,
            Action<Exception> onErrorOccur = null
        ) where T : new()
        {
            if (string.IsNullOrEmpty(csvPath))
                throw new ArgumentException($"CSV文件路径不能为空{csvPath}", nameof(csvPath));

            if (!File.Exists(csvPath))
                throw new FileNotFoundException($"CSV文件未找到{csvPath}", csvPath);

            if (headerMatchFunc == null)
                throw new ArgumentNullException(nameof(headerMatchFunc), "列头匹配函数不能为空");

            var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
            if (lines.Length == 0)
                return new List<T>();

            // 处理列头映射
            var headers = lines[0].Split(',').Select(h => h.Trim()).ToList();
            var properties = typeof(T).GetProperties();
            var columnMap = new Dictionary<string, System.Reflection.PropertyInfo>();

            foreach (var prop in properties)
            {
                // 使用自定义的列头匹配函数
                string targetHeader = headerMatchFunc(prop);

                // 查找匹配的列名
                var matchedHeader = headers.FirstOrDefault(h =>
                    h.Equals(targetHeader, stringComparison)
                );

                if (!string.IsNullOrEmpty(matchedHeader))
                {
                    columnMap[matchedHeader] = prop;
                }
            }

            // 处理数据行
            var result = new List<T>();
            for (int i = 1; i < lines.Length; i++)
            {
                var obj = new T();
                var fields = lines[i].Split(',').Select(f => f.Trim()).ToArray();

                for (int j = 0; j < headers.Count; j++)
                {
                    if (j >= fields.Length) break;

                    var header = headers[j];
                    if (!columnMap.TryGetValue(header, out var prop)) continue;

                    try
                    {
                        var value = Convert.ChangeType(fields[j], prop.PropertyType);
                        prop.SetValue(obj, value);
                    }
                    catch (Exception ex)
                    {
                        // 调用异常处理回调函数
                        onErrorOccur?.Invoke(ex);
                    }
                }
                result.Add(obj);
            }

            return result;
        }



    }
}

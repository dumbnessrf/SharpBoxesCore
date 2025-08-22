// CsvLite.Net48.cs  (.NET Framework 4.8 兼容)
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpBoxesCore.Office.CSV.CsvLite;

// ---------------- 缺失值策略 ----------------
public sealed class CsvNullPolicy
{
    public string WriteNullAs { get; set; }
    public bool TreatEmptyAsNullOnWrite { get; set; }
    public HashSet<string> ReadNullTokens { get; set; }

    public CsvNullPolicy()
    {
        WriteNullAs = "";
        TreatEmptyAsNullOnWrite = false;
        ReadNullTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "",
            "NA",
            "N/A",
            "NULL",
        };
    }

    public bool IsNullToken(string s)
    {
        return s == null || ReadNullTokens.Contains(s);
    }
}

// ---------------- 配置 ----------------
public sealed class CsvOptions
{
    public char Delimiter { get; set; }
    public char Quote { get; set; }
    public string NewLine { get; set; }
    public Encoding Encoding { get; set; }
    public bool Utf8Bom { get; set; }
    public bool AutoHeaderForTypedWrite { get; set; }
    public CsvNullPolicy NullPolicy { get; set; }

    public CsvOptions()
    {
        Delimiter = ',';
        Quote = '"';
        NewLine = Environment.NewLine;
        Encoding = new UTF8Encoding(false);
        Utf8Bom = false;
        AutoHeaderForTypedWrite = true;
        NullPolicy = new CsvNullPolicy();
    }

    internal StreamWriter CreateWriter(string path, bool append)
    {
        var fs = new FileStream(
            path,
            append ? FileMode.Append : FileMode.Create,
            FileAccess.Write,
            FileShare.Read
        );
        if (Encoding is UTF8Encoding && Utf8Bom && fs.Position == 0 && !append)
        {
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            fs.Write(bom, 0, bom.Length);
        }
        var sw = new StreamWriter(fs, Encoding);
        sw.NewLine = NewLine;
        sw.AutoFlush = false;
        return sw;
    }

    internal StreamReader CreateReader(string path)
    {
        return new StreamReader(
            new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read),
            Encoding
        );
    }
}

// ---------------- 跨进程写锁 ----------------
public static class CsvConcurrency
{
    private static readonly ConcurrentDictionary<string, string> NameCache =
        new ConcurrentDictionary<string, string>();

    private static string GetMutexNameForPath(string path)
    {
        var full = Path.GetFullPath(path).ToUpperInvariant();
        int h = full.GetHashCode();
        return NameCache.GetOrAdd(full, _ => "Global/CsvLite_" + h.ToString("X8"));
    }

    public static IDisposable Acquire(string path, TimeSpan? wait = null)
    {
        bool createdNew;
        var m = new Mutex(false, GetMutexNameForPath(path), out createdNew);
        try
        {
            if (!m.WaitOne(wait ?? TimeSpan.FromMinutes(2)))
                throw new TimeoutException("获取写锁超时：" + path);
        }
        catch
        {
            m.Dispose();
            throw;
        }
        return new Releaser(m);
    }

    private sealed class Releaser : IDisposable
    {
        private Mutex _m;

        public Releaser(Mutex m)
        {
            _m = m;
        }

        public void Dispose()
        {
            try
            {
                if (_m != null)
                {
                    _m.ReleaseMutex();
                    _m.Dispose();
                    _m = null;
                }
            }
            catch { }
        }
    }
}

// ---------------- 核心解析/转义 ----------------
internal static class CsvCore
{
    public static string Escape(string field, CsvOptions o)
    {
        if (field == null || (o.NullPolicy.TreatEmptyAsNullOnWrite && field == ""))
            field = o.NullPolicy.WriteNullAs;

        bool needQuote =
            field.IndexOfAny(new[] { o.Delimiter, o.Quote, '\r', '\n' }) >= 0
            || field.StartsWith(" ")
            || field.EndsWith(" ");

        if (!needQuote)
            return field;

        var sb = new StringBuilder(field.Length + 4);
        sb.Append(o.Quote);
        for (int i = 0; i < field.Length; i++)
        {
            char c = field[i];
            if (c == o.Quote)
                sb.Append(o.Quote);
            sb.Append(c);
        }
        sb.Append(o.Quote);
        return sb.ToString();
    }

    public static string JoinRow(IEnumerable<string> row, CsvOptions o)
    {
        return string.Join(o.Delimiter.ToString(), row.Select(f => Escape(f, o)));
    }

    public static List<string> ParseLine(string line, CsvOptions o)
    {
        var res = new List<string>(16);
        var sb = new StringBuilder();
        bool inQ = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQ)
            {
                if (c == o.Quote)
                {
                    bool esc = (i + 1 < line.Length && line[i + 1] == o.Quote);
                    if (esc)
                    {
                        sb.Append(o.Quote);
                        i++;
                    }
                    else
                        inQ = false;
                }
                else
                    sb.Append(c);
            }
            else
            {
                if (c == o.Delimiter)
                {
                    res.Add(sb.ToString());
                    sb.Clear();
                }
                else if (c == o.Quote)
                {
                    inQ = true;
                }
                else
                    sb.Append(c);
            }
        }
        res.Add(sb.ToString());
        return res;
    }
}

// ---------------- 文件级 API ----------------
public static class CsvFile
{
    // 同步写
    public static void Create(string path, IEnumerable<string> headers, CsvOptions options = null)
    {
        var o = options ?? new CsvOptions();
        var dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        using (CsvConcurrency.Acquire(path))
        using (var sw = o.CreateWriter(path, false))
        {
            if (headers != null)
                sw.WriteLine(CsvCore.JoinRow(headers, o));
        }
    }

    public static void Append(
        string path,
        IEnumerable<IEnumerable<string>> rows,
        CsvOptions options = null
    )
    {
        var o = options ?? new CsvOptions();
        var dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        using (CsvConcurrency.Acquire(path))
        using (var sw = o.CreateWriter(path, File.Exists(path)))
        {
            foreach (var row in rows)
                sw.WriteLine(CsvCore.JoinRow(row, o));
        }
    }

    public static void Insert(
        string path,
        int rowIndex,
        IEnumerable<string> row,
        bool hasHeader = true,
        CsvOptions options = null
    )
    {
        if (rowIndex < 0)
            throw new ArgumentOutOfRangeException("rowIndex");
        var o = options ?? new CsvOptions();
        if (!File.Exists(path))
            throw new FileNotFoundException("CSV 文件不存在。", path);

        using (CsvConcurrency.Acquire(path))
        {
            string tmp = Path.GetTempFileName();
            using (var sr = o.CreateReader(path))
            using (var sw = o.CreateWriter(tmp, false))
            {
                int dataIdx = 0;
                string line;
                if (hasHeader && (line = sr.ReadLine()) != null)
                    sw.WriteLine(line);
                bool inserted = false;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!inserted && dataIdx == rowIndex)
                    {
                        sw.WriteLine(CsvCore.JoinRow(row, o));
                        inserted = true;
                    }
                    sw.WriteLine(line);
                    dataIdx++;
                }
                if (!inserted)
                    sw.WriteLine(CsvCore.JoinRow(row, o));
            }
            File.Copy(tmp, path, true);
            File.Delete(tmp);
        }
    }

    // 同步读
    public static List<string[]> ReadAll(
        string path,
        bool skipHeader = false,
        CsvOptions options = null
    )
    {
        var o = options ?? new CsvOptions();
        var list = new List<string[]>();
        using (var sr = o.CreateReader(path))
        {
            string line;
            bool skipped = !skipHeader;
            while ((line = sr.ReadLine()) != null)
            {
                if (!skipped)
                {
                    skipped = true;
                    continue;
                }
                var arr = CsvCore
                    .ParseLine(line, o)
                    .Select(s => o.NullPolicy.IsNullToken(s) ? null : s)
                    .Select(s => s ?? "")
                    .ToArray();
                list.Add(arr);
            }
        }
        return list;
    }

    public static IEnumerable<string[]> Enumerate(
        string path,
        bool skipHeader = false,
        CsvOptions options = null
    )
    {
        var o = options ?? new CsvOptions();
        using (var sr = o.CreateReader(path))
        {
            string line;
            bool skipped = !skipHeader;
            while ((line = sr.ReadLine()) != null)
            {
                if (!skipped)
                {
                    skipped = true;
                    continue;
                }
                var arr = CsvCore
                    .ParseLine(line, o)
                    .Select(s => o.NullPolicy.IsNullToken(s) ? null : s)
                    .Select(s => s ?? "")
                    .ToArray();
                yield return arr;
            }
        }
    }

    // 强类型写入/读取（同步）
    public static void Append<T>(string path, IEnumerable<T> items, CsvOptions options = null)
    {
        var o = options ?? new CsvOptions();
        var props = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .ToArray();
        bool exists = File.Exists(path);
        var dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using (CsvConcurrency.Acquire(path))
        using (var sw = o.CreateWriter(path, true))
        {
            if (!exists && o.AutoHeaderForTypedWrite)
                sw.WriteLine(CsvCore.JoinRow(props.Select(p => p.Name), o));

            foreach (var it in items)
            {
                var cells = props
                    .Select(p =>
                    {
                        var v = p.GetValue(it);
                        return v == null ? null : v.ToString();
                    })
                    .Select(s => s ?? "");
                sw.WriteLine(CsvCore.JoinRow(cells, o));
            }
        }
    }

    public static IEnumerable<T> ReadAs<T>(string path, CsvOptions options = null)
        where T : new()
    {
        var o = options ?? new CsvOptions();
        using (var sr = o.CreateReader(path))
        {
            string header = sr.ReadLine();
            if (header == null)
                yield break;

            var headers = CsvCore.ParseLine(header, o);
            var propMap = MapProps(typeof(T), headers);

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length == 0)
                    continue;
                var row = CsvCore.ParseLine(line, o);
                var obj = new T();
                for (int i = 0; i < headers.Count && i < row.Count; i++)
                {
                    if (propMap[i] != null)
                    {
                        var pi = propMap[i];
                        string raw = row[i];
                        string val = o.NullPolicy.IsNullToken(raw) ? null : raw;
                        object converted = ConvertTo(val, pi.PropertyType);
                        pi.SetValue(obj, converted, null);
                    }
                }
                yield return obj;
            }
        }
    }

    // ---------- 异步：Task 版 ----------
    public static async Task<List<string[]>> ReadAllAsync(
        string path,
        bool skipHeader = false,
        CsvOptions options = null,
        CancellationToken ct = default(CancellationToken)
    )
    {
        var o = options ?? new CsvOptions();
        var list = new List<string[]>();
        using (var sr = o.CreateReader(path))
        {
            string line;
            bool skipped = !skipHeader;
            while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
                if (!skipped)
                {
                    skipped = true;
                    continue;
                }
                var arr = CsvCore
                    .ParseLine(line, o)
                    .Select(s => o.NullPolicy.IsNullToken(s) ? null : s)
                    .Select(s => s ?? "")
                    .ToArray();
                list.Add(arr);
            }
        }
        return list;
    }

    // “流式异步”用回调：每读一行就调用处理器
    public static async Task EnumerateAsync(
        string path,
        Func<string[], Task> onRowAsync,
        bool skipHeader = false,
        CsvOptions options = null,
        CancellationToken ct = default(CancellationToken)
    )
    {
        if (onRowAsync == null)
            throw new ArgumentNullException("onRowAsync");
        var o = options ?? new CsvOptions();
        using (var sr = o.CreateReader(path))
        {
            string line;
            bool skipped = !skipHeader;
            while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
                if (!skipped)
                {
                    skipped = true;
                    continue;
                }
                var arr = CsvCore
                    .ParseLine(line, o)
                    .Select(s => o.NullPolicy.IsNullToken(s) ? null : s)
                    .Select(s => s ?? "")
                    .ToArray();
                await onRowAsync(arr).ConfigureAwait(false);
            }
        }
    }

    // 强类型异步（回调版）
    public static async Task ReadAsAsync<T>(
        string path,
        Func<T, Task> onItemAsync,
        CsvOptions options = null,
        CancellationToken ct = default(CancellationToken)
    )
        where T : new()
    {
        if (onItemAsync == null)
            throw new ArgumentNullException("onItemAsync");
        var o = options ?? new CsvOptions();
        using (var sr = o.CreateReader(path))
        {
            string header = await sr.ReadLineAsync().ConfigureAwait(false);
            if (header == null)
                return;

            var headers = CsvCore.ParseLine(header, o);
            var propMap = MapProps(typeof(T), headers);

            string line;
            while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
                if (line.Length == 0)
                    continue;
                var row = CsvCore.ParseLine(line, o);
                var obj = new T();
                for (int i = 0; i < headers.Count && i < row.Count; i++)
                {
                    var pi = propMap[i];
                    if (pi == null)
                        continue;

                    string raw = row[i];
                    string val = o.NullPolicy.IsNullToken(raw) ? null : raw;
                    object converted = ConvertTo(val, pi.PropertyType);
                    pi.SetValue(obj, converted, null);
                }
                await onItemAsync(obj).ConfigureAwait(false);
            }
        }
    }

    // DataTable 异步
    public static async Task<DataTable> ReadAllAsDataTableAsync(
        string path,
        bool hasHeader = true,
        bool inferTypes = false,
        CsvOptions options = null,
        CancellationToken ct = default(CancellationToken)
    )
    {
        var o = options ?? new CsvOptions();
        var dt = new DataTable(Path.GetFileName(path));
        using (var sr = o.CreateReader(path))
        {
            string header = await sr.ReadLineAsync().ConfigureAwait(false);
            if (header == null)
                return dt;

            List<string> headers;
            var buffer = new List<List<string>>();

            if (hasHeader)
            {
                headers = CsvCore.ParseLine(header, o);
            }
            else
            {
                var firstRow = CsvCore.ParseLine(header, o);
                headers = Enumerable.Range(0, firstRow.Count).Select(i => "Col" + i).ToList();
                buffer.Add(firstRow);
            }

            string line;
            while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
                buffer.Add(CsvCore.ParseLine(line, o));
            }

            if (!inferTypes)
            {
                foreach (var h in headers)
                    dt.Columns.Add(h, typeof(string));
                foreach (var row in buffer)
                    dt.Rows.Add(ToObjects(row, o, dt.Columns));
            }
            else
            {
                var types = InferTypes(headers.Count, buffer);
                for (int i = 0; i < headers.Count; i++)
                    dt.Columns.Add(headers[i], types[i]);
                foreach (var row in buffer)
                    dt.Rows.Add(ToObjects(row, o, dt.Columns));
            }
        }
        return dt;

        // local funcs
        Type[] InferTypes(int colCount, List<List<string>> buf)
        {
            var types = Enumerable.Repeat(typeof(string), colCount).ToArray();
            for (int c = 0; c < colCount; c++)
            {
                bool allInt = true,
                    allLong = true,
                    allDec = true,
                    allDouble = true,
                    allBool = true,
                    allDt = true;
                foreach (var r in buf)
                {
                    if (c >= r.Count)
                        continue;
                    var s = r[c];
                    if (string.IsNullOrWhiteSpace(s))
                        continue;
                    int _i;
                    long _l;
                    decimal _m;
                    double _d;
                    bool _b;
                    DateTime _t;
                    if (!int.TryParse(s, out _i))
                        allInt = false;
                    if (!long.TryParse(s, out _l))
                        allLong = false;
                    if (!decimal.TryParse(s, out _m))
                        allDec = false;
                    if (!double.TryParse(s, out _d))
                        allDouble = false;
                    if (!bool.TryParse(s, out _b))
                        allBool = false;
                    if (!DateTime.TryParse(s, out _t))
                        allDt = false;
                }
                types[c] =
                    allInt ? typeof(int)
                    : allLong ? typeof(long)
                    : allDec ? typeof(decimal)
                    : allDouble ? typeof(double)
                    : allBool ? typeof(bool)
                    : allDt ? typeof(DateTime)
                    : typeof(string);
            }
            return types;
        }

        object[] ToObjects(List<string> row, CsvOptions opt, DataColumnCollection cols)
        {
            int n = cols.Count;
            var vals = new object[n];
            for (int i = 0; i < n; i++)
            {
                string s = (i < row.Count) ? row[i] : null;
                string v = opt.NullPolicy.IsNullToken(s) ? null : s;
                if (v == null)
                {
                    vals[i] = DBNull.Value;
                    continue;
                }

                var t = cols[i].DataType;
                try
                {
                    vals[i] = ConvertForType(v, t);
                }
                catch
                {
                    vals[i] = DBNull.Value;
                }
            }
            return vals;
        }

        object ConvertForType(string s, Type t)
        {
            if (t == typeof(string))
                return s;
            if (t == typeof(int))
                return int.Parse(s);
            if (t == typeof(long))
                return long.Parse(s);
            if (t == typeof(decimal))
                return decimal.Parse(s);
            if (t == typeof(double))
                return double.Parse(s);
            if (t == typeof(bool))
                return bool.Parse(s);
            if (t == typeof(DateTime))
                return DateTime.Parse(s);
            return s;
        }
    }

    // ---------- ReadAsAsync<T> 的列映射/转换器（回调版） ----------
    public static async Task ReadAsAsync<T>(
        string path,
        Func<T, Task> onItemAsync,
        CsvOptions options,
        CsvReadMapping<T> mapping,
        CancellationToken ct = default(CancellationToken)
    )
        where T : new()
    {
        if (onItemAsync == null)
            throw new ArgumentNullException("onItemAsync");
        var o = options ?? new CsvOptions();
        using (var sr = o.CreateReader(path))
        {
            string header = await sr.ReadLineAsync().ConfigureAwait(false);
            if (header == null)
                return;

            var headers = CsvCore.ParseLine(header, o);
            var propsAll = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            var propMap = new PropertyInfo[headers.Count];
            for (int i = 0; i < headers.Count; i++)
            {
                string col = headers[i];
                string propName =
                    mapping != null && mapping.ColumnToProperty.ContainsKey(col)
                        ? mapping.ColumnToProperty[col]
                        : col;

                PropertyInfo pi;
                if (propsAll.TryGetValue(propName, out pi))
                    propMap[i] = pi;
                else
                {
                    if (mapping != null && mapping.OnMissingColumn == MissingColumnBehavior.Throw)
                        throw new InvalidOperationException(
                            "Column '"
                                + col
                                + "' has no matching property on "
                                + typeof(T).Name
                                + "."
                        );
                    propMap[i] = null; // 忽略
                }
            }

            string line;
            while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
                if (line.Length == 0)
                    continue;

                var row = CsvCore.ParseLine(line, o);
                var obj = new T();
                for (int i = 0; i < headers.Count && i < row.Count; i++)
                {
                    var pi = propMap[i];
                    if (pi == null)
                        continue;

                    string raw = row[i];
                    string val = o.NullPolicy.IsNullToken(raw) ? null : raw;

                    // 先用自定义转换器
                    if (mapping != null && mapping.PropertyConverters.ContainsKey(pi.Name))
                    {
                        var v = mapping.PropertyConverters[pi.Name](val);
                        pi.SetValue(obj, v, null);
                    }
                    else
                    {
                        object converted = ConvertDefault(
                            val,
                            pi.PropertyType,
                            mapping != null ? mapping.FormatProvider : null
                        );
                        pi.SetValue(obj, converted, null);
                    }
                }
                await onItemAsync(obj).ConfigureAwait(false);
            }
        }
    }

    // 反射与转换帮助
    private static PropertyInfo[] GetWritableProps(Type t)
    {
        return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .ToArray();
    }

    private static PropertyInfo[] MapProps(Type t, IList<string> headers)
    {
        var dict = GetWritableProps(t)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        var map = new PropertyInfo[headers.Count];
        for (int i = 0; i < headers.Count; i++)
        {
            PropertyInfo pi;
            map[i] = dict.TryGetValue(headers[i], out pi) ? pi : null;
        }
        return map;
    }

    private static object ConvertTo(string s, Type target)
    {
        if (target == typeof(string))
            return s;

        var underlying = Nullable.GetUnderlyingType(target);
        bool isNullable = underlying != null || !target.IsValueType;
        var t = underlying ?? target;

        if (s == null)
            return isNullable ? null : Activator.CreateInstance(t);

        if (t.IsEnum)
            return Enum.Parse(t, s, true);
        if (t == typeof(Guid))
            return Guid.Parse(s);
        if (t == typeof(DateTime))
            return DateTime.Parse(s);
        if (t == typeof(DateTimeOffset))
            return DateTimeOffset.Parse(s);
        if (t == typeof(TimeSpan))
            return TimeSpan.Parse(s);
        if (t == typeof(bool))
            return bool.Parse(s);
        if (t == typeof(byte))
            return byte.Parse(s);
        if (t == typeof(short))
            return short.Parse(s);
        if (t == typeof(int))
            return int.Parse(s);
        if (t == typeof(long))
            return long.Parse(s);
        if (t == typeof(float))
            return float.Parse(s);
        if (t == typeof(double))
            return double.Parse(s);
        if (t == typeof(decimal))
            return decimal.Parse(s);

        var converter = System.ComponentModel.TypeDescriptor.GetConverter(t);
        if (converter.CanConvertFrom(typeof(string)))
            return converter.ConvertFromInvariantString(s);

        return s;
    }

    private static object ConvertDefault(string s, Type target, IFormatProvider fmt)
    {
        if (target == typeof(string))
            return s;

        var underlying = Nullable.GetUnderlyingType(target);
        bool isNullable = underlying != null || !target.IsValueType;
        var t = underlying ?? target;

        if (s == null)
            return isNullable ? null : Activator.CreateInstance(t);

        if (t.IsEnum)
            return Enum.Parse(t, s, true);
        if (t == typeof(Guid))
            return Guid.Parse(s);
        if (t == typeof(DateTime))
            return fmt == null ? DateTime.Parse(s) : DateTime.Parse(s, fmt);
        if (t == typeof(DateTimeOffset))
            return DateTimeOffset.Parse(s, fmt);
        if (t == typeof(TimeSpan))
            return TimeSpan.Parse(s, fmt);
        if (t == typeof(bool))
            return bool.Parse(s);

        if (t == typeof(byte))
            return byte.Parse(s, fmt);
        if (t == typeof(short))
            return short.Parse(s, fmt);
        if (t == typeof(int))
            return int.Parse(s, fmt);
        if (t == typeof(long))
            return long.Parse(s, fmt);
        if (t == typeof(float))
            return float.Parse(s, fmt);
        if (t == typeof(double))
            return double.Parse(s, fmt);
        if (t == typeof(decimal))
            return decimal.Parse(s, fmt);

        var converter = System.ComponentModel.TypeDescriptor.GetConverter(t);
        if (converter.CanConvertFrom(typeof(string)))
            return converter.ConvertFromString(s);
        return s;
    }
}

// ---------------- 多文件事务：整替换 + 追加式（合并行后整体替换） ----------------
public sealed class CsvTransaction : IDisposable
{
    private readonly CsvOptions _opts;
    private readonly List<Entry> _entries = new List<Entry>();
    private bool _disposed;

    private sealed class Entry
    {
        public string TargetPath;
        public string TempPath;
        public string BackupPath;
    }

    public CsvTransaction(CsvOptions options = null)
    {
        _opts = options ?? new CsvOptions();
    }

    // 整文件替换：传入写内容逻辑
    public async Task AddReplaceAsync(
        string targetPath,
        Func<StreamWriter, CancellationToken, Task> writeContentAsync,
        CancellationToken ct = default(CancellationToken)
    )
    {
        string dir = Path.GetDirectoryName(Path.GetFullPath(targetPath));
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        string temp = Path.Combine(dir, ".~csvlite.tmp." + Guid.NewGuid().ToString("N") + ".csv");
        string bak = Path.Combine(dir, ".~csvlite.bak." + Guid.NewGuid().ToString("N") + ".csv");

        using (var sw = _opts.CreateWriter(temp, false))
        {
            await writeContentAsync(sw, ct).ConfigureAwait(false);
            await sw.FlushAsync().ConfigureAwait(false);
        }

        _entries.Add(
            new Entry
            {
                TargetPath = targetPath,
                TempPath = temp,
                BackupPath = bak,
            }
        );
    }

    // 便捷：由行构建
    public Task AddReplaceFromRowsAsync(
        string targetPath,
        IEnumerable<string> headers,
        IEnumerable<IEnumerable<string>> rows,
        CancellationToken ct = default(CancellationToken)
    )
    {
        return AddReplaceAsync(
            targetPath,
            async (sw, token) =>
            {
                if (headers != null)
                    await sw.WriteLineAsync(CsvCore.JoinRow(headers, _opts)).ConfigureAwait(false);
                foreach (var r in rows)
                {
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();
                    await sw.WriteLineAsync(CsvCore.JoinRow(r, _opts)).ConfigureAwait(false);
                }
            },
            ct
        );
    }

    // 追加式事务：把“旧内容 + 新行”写入同目录临时文件，然后统一提交
    public async Task AddAppendFromRowsAsync(
        string targetPath,
        IEnumerable<string> headersForNewFile,
        IEnumerable<IEnumerable<string>> newRows,
        CancellationToken ct = default(CancellationToken)
    )
    {
        string dir = Path.GetDirectoryName(Path.GetFullPath(targetPath));
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        string temp = Path.Combine(dir, ".~csvlite.tmp." + Guid.NewGuid().ToString("N") + ".csv");
        string bak = Path.Combine(dir, ".~csvlite.bak." + Guid.NewGuid().ToString("N") + ".csv");

        using (var fs = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.Read))
        using (var sw = new StreamWriter(fs, _opts.Encoding))
        {
            sw.NewLine = _opts.NewLine;

            bool exists = File.Exists(targetPath);
            if (exists)
            {
                using (
                    var sr = new StreamReader(
                        new FileStream(targetPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                        _opts.Encoding
                    )
                )
                {
                    string line;
                    while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        if (ct.IsCancellationRequested)
                            ct.ThrowIfCancellationRequested();
                        await sw.WriteLineAsync(line).ConfigureAwait(false);
                    }
                }
            }
            else if (headersForNewFile != null)
            {
                await sw.WriteLineAsync(CsvCore.JoinRow(headersForNewFile, _opts))
                    .ConfigureAwait(false);
            }

            foreach (var r in newRows)
            {
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
                await sw.WriteLineAsync(CsvCore.JoinRow(r, _opts)).ConfigureAwait(false);
            }
            await sw.FlushAsync().ConfigureAwait(false);
        }

        _entries.Add(
            new Entry
            {
                TargetPath = targetPath,
                TempPath = temp,
                BackupPath = bak,
            }
        );
    }

    // 提交：统一加锁 → 备份并替换 → 成功后删备份；失败回滚
    public async Task CommitAsync(CancellationToken ct = default(CancellationToken))
    {
        if (_disposed)
            throw new ObjectDisposedException("CsvTransaction");
        if (_entries.Count == 0)
            return;

        var ordered = _entries
            .OrderBy(e => e.TargetPath, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var locks = new List<IDisposable>();
        try
        {
            foreach (var e in ordered)
            {
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
                locks.Add(CsvConcurrency.Acquire(e.TargetPath));
            }

            foreach (var e in ordered)
            {
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();

                if (File.Exists(e.TargetPath))
                {
                    if (File.Exists(e.BackupPath))
                        File.Delete(e.BackupPath);
                    File.Move(e.TargetPath, e.BackupPath);
                }

                if (File.Exists(e.TargetPath))
                    File.Delete(e.TargetPath);
                File.Move(e.TempPath, e.TargetPath);
            }

            foreach (var e in ordered)
            {
                if (File.Exists(e.BackupPath))
                    File.Delete(e.BackupPath);
            }
        }
        catch
        {
            foreach (var e in _entries)
            {
                try
                {
                    if (File.Exists(e.BackupPath))
                    {
                        if (File.Exists(e.TargetPath))
                            File.Delete(e.TargetPath);
                        File.Move(e.BackupPath, e.TargetPath);
                    }
                }
                catch { }
            }
            throw;
        }
        finally
        {
            foreach (var e in _entries)
            {
                try
                {
                    if (File.Exists(e.TempPath))
                        File.Delete(e.TempPath);
                }
                catch { }
            }
            foreach (var l in locks)
            {
                try
                {
                    l.Dispose();
                }
                catch { }
            }
        }

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        foreach (var e in _entries)
        {
            try
            {
                if (File.Exists(e.TempPath))
                    File.Delete(e.TempPath);
            }
            catch { }
            try
            {
                if (File.Exists(e.BackupPath))
                    File.Delete(e.BackupPath);
            }
            catch { }
        }
        _entries.Clear();
    }
}

// ---------------- ReadAs<T> 映射/转换器 ----------------
public enum MissingColumnBehavior
{
    Ignore,
    Throw,
}

public sealed class CsvReadMapping<T>
{
    public Dictionary<string, string> ColumnToProperty { get; private set; }
    public Dictionary<string, Func<string, object>> PropertyConverters { get; private set; }
    public IFormatProvider FormatProvider { get; set; }
    public MissingColumnBehavior OnMissingColumn { get; set; }

    public CsvReadMapping()
    {
        ColumnToProperty = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        PropertyConverters = new Dictionary<string, Func<string, object>>(
            StringComparer.OrdinalIgnoreCase
        );
        OnMissingColumn = MissingColumnBehavior.Ignore;
    }

    public CsvReadMapping<T> Map(string column, string property)
    {
        ColumnToProperty[column] = property;
        return this;
    }

    public CsvReadMapping<T> Convert(string property, Func<string, object> converter)
    {
        PropertyConverters[property] = converter;
        return this;
    }
}

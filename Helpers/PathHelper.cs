using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class PathHelper : IEquatable<PathHelper>, IComparable<PathHelper>
{
    private readonly string _path;

    #region 构造函数
    /// <summary>
    /// 使用给定的路径段初始化一个新的 PathHelper 实例
    /// </summary>
    /// <param name="pathSegments">要组合的路径段</param>
    public PathHelper(params string[] pathSegments)
    {
        if (pathSegments == null || pathSegments.Length == 0)
        {
            _path = ".";
        }
        else
        {
            _path = NormalizePath(Path.Combine(pathSegments));
        }
    }

    /// <summary>
    /// 从另一个 PathHelper 实例初始化新的 PathHelper
    /// </summary>
    /// <param name="other">另一个 PathHelper 实例</param>
    public PathHelper(PathHelper other)
    {
        _path = other?._path ?? ".";
    }
    #endregion

    #region 属性
    /// <summary>
    /// 路径部分作为只读列表返回
    /// </summary>
    public IReadOnlyList<string> Parts
    {
        get
        {
            if (_path == "." || string.IsNullOrEmpty(_path))
                return new[] { "." };
            var parts = new List<string>();
            var root = Path.GetPathRoot(_path);
            if (!string.IsNullOrEmpty(root))
            {
                parts.Add(root);
                // 获取根之后的部分
                var relativePart = _path.Substring(root.Length);
                if (!string.IsNullOrEmpty(relativePart))
                {
                    parts.AddRange(
                        relativePart
                            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                            .Where(p => !string.IsNullOrEmpty(p))
                    );
                }
            }
            else
            {
                // 相对路径
                parts.AddRange(
                    _path
                        .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Where(p => !string.IsNullOrEmpty(p))
                );
            }
            return parts.AsReadOnly();
        }
    }

    /// <summary>
    /// 返回路径的最终组件
    /// </summary>
    public string Name => Path.GetFileName(_path) ?? "";

    /// <summary>
    /// 返回最终组件不带后缀的部分
    /// </summary>
    public string Stem => Path.GetFileNameWithoutExtension(_path) ?? "";

    /// <summary>
    /// 返回最终组件的文件扩展名
    /// </summary>
    public string Suffix => Path.GetExtension(_path) ?? "";

    /// <summary>
    /// 返回最终组件的所有扩展名
    /// </summary>
    public IReadOnlyList<string> Suffixes
    {
        get
        {
            var name = Name;
            var suffixes = new List<string>();
            var dotIndex = name.IndexOf('.');
            while (dotIndex >= 0 && dotIndex < name.Length - 1)
            {
                var suffix = name.Substring(dotIndex);
                var nextDot = suffix.IndexOf('.', 1);
                if (nextDot > 0)
                {
                    suffix = suffix.Substring(0, nextDot);
                }
                suffixes.Add(suffix);
                dotIndex = name.IndexOf('.', dotIndex + suffix.Length);
            }
            return suffixes.AsReadOnly();
        }
    }

    /// <summary>
    /// 返回路径的逻辑父目录
    /// </summary>
    public PathHelper Parent => new PathHelper(Path.GetDirectoryName(_path) ?? _path);

    /// <summary>
    /// 返回路径的逻辑祖先目录
    /// </summary>
    public IEnumerable<PathHelper> Parents
    {
        get
        {
            var current = Parent;
            while (current._path != _path && !string.IsNullOrEmpty(current._path))
            {
                yield return current;
                var next = current.Parent;
                if (next._path == current._path)
                    break;
                current = next;
            }
        }
    }

    /// <summary>
    /// 返回驱动器字母或名称（如果有）
    /// </summary>
    public string Drive =>
        Path.GetPathRoot(_path)
            ?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) ?? "";

    /// <summary>
    /// 返回路径的根部分（如果有）
    /// </summary>
    public string Root
    {
        get
        {
            var root = Path.GetPathRoot(_path);
            if (string.IsNullOrEmpty(root))
                return "";
            // 在 Windows 上，仅返回分隔符部分
            if (root.Length > 1)
            {
                return root.Substring(root.Length - 1);
            }
            return root;
        }
    }

    /// <summary>
    /// 返回驱动器和根的组合
    /// </summary>
    public string Anchor => Path.GetPathRoot(_path) ?? "";
    #endregion

    #region 运算符重载
    /// <summary>
    /// 使用 / 运算符连接路径
    /// </summary>
    public static PathHelper operator /(PathHelper left, string right)
    {
        return left.JoinPath(right);
    }

    /// <summary>
    /// 使用 / 运算符连接路径
    /// </summary>
    public static PathHelper operator /(PathHelper left, PathHelper right)
    {
        return left.JoinPath(right._path);
    }

    /// <summary>
    /// 从字符串隐式转换为 PathHelper
    /// </summary>
    public static implicit operator PathHelper(string path)
    {
        return new PathHelper(path);
    }

    /// <summary>
    /// 从 PathHelper 隐式转换为字符串
    /// </summary>
    public static implicit operator string(PathHelper path)
    {
        return path._path;
    }
    #endregion

    #region 路径操作
    /// <summary>
    /// 将路径段连接到当前路径上
    /// </summary>
    public PathHelper JoinPath(params string[] pathSegments)
    {
        if (pathSegments == null || pathSegments.Length == 0)
            return this;
        var segments = new string[pathSegments.Length + 1];
        segments[0] = _path;
        Array.Copy(pathSegments, 0, segments, 1, pathSegments.Length);
        return new PathHelper(Path.Combine(segments));
    }

    /// <summary>
    /// 判断该路径是否是绝对路径
    /// </summary>
    public bool IsAbsolute => Path.IsPathRooted(_path);

    /// <summary>
    /// 将路径转为绝对路径
    /// </summary>
    public PathHelper Absolute()
    {
        return new PathHelper(Path.GetFullPath(_path));
    }

    /// <summary>
    /// 解析路径（使其绝对，并解析符号链接）
    /// </summary>
    public PathHelper Resolve()
    {
        try
        {
            return new PathHelper(Path.GetFullPath(_path));
        }
        catch
        {
            return Absolute();
        }
    }

    /// <summary>
    /// 返回一个新路径，其名称已更改
    /// </summary>
    public PathHelper WithName(string name)
    {
        var currentName = Name;
        if (string.IsNullOrEmpty(currentName))
            throw new InvalidOperationException("路径没有可替换的名称");
        var dir = Path.GetDirectoryName(_path);
        return new PathHelper(dir != null ? Path.Combine(dir, name) : name);
    }

    /// <summary>
    /// 返回一个新路径，其主干名称已更改
    /// </summary>
    public PathHelper WithStem(string stem)
    {
        return WithName(stem + Suffix);
    }

    /// <summary>
    /// 返回一个新路径，其后缀已更改
    /// </summary>
    public PathHelper WithSuffix(string suffix)
    {
        return WithName(Stem + suffix);
    }
    #endregion

    #region 文件系统查询
    /// <summary>
    /// 如果路径存在则返回 true
    /// </summary>
    public bool Exists => File.Exists(_path) || Directory.Exists(_path);

    /// <summary>
    /// 如果路径指向普通文件则返回 true
    /// </summary>
    public bool IsFile => File.Exists(_path);

    /// <summary>
    /// 如果路径指向目录则返回 true
    /// </summary>
    public bool IsDirectory => Directory.Exists(_path);

    /// <summary>
    /// 如果路径指向符号链接则返回 true
    /// </summary>
    public bool IsSymlink
    {
        get
        {
            try
            {
                if (IsFile)
                {
                    var fileInfo = new FileInfo(_path);
                    return fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
                }
                if (IsDirectory)
                {
                    var dirInfo = new DirectoryInfo(_path);
                    return dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 获取文件信息
    /// </summary>
    public FileSystemInfo? Stat()
    {
        try
        {
            if (IsFile)
                return new FileInfo(_path);
            if (IsDirectory)
                return new DirectoryInfo(_path);
            return null;
        }
        catch
        {
            return null;
        }
    }
    #endregion

    #region 文件操作
    /// <summary>
    /// 打开由路径指向的文件
    /// </summary>
    public FileStream Open(
        FileMode mode = FileMode.Open,
        FileAccess access = FileAccess.Read,
        FileShare share = FileShare.Read
    )
    {
        return new FileStream(_path, mode, access, share);
    }

    /// <summary>
    /// 将整个文件读取为文本
    /// </summary>
    public string ReadText(Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return File.ReadAllText(_path, encoding);
    }

    /// <summary>
    /// 将整个文件读取为字节数组
    /// </summary>
    public byte[] ReadBytes()
    {
        return File.ReadAllBytes(_path);
    }

    /// <summary>
    /// 将文本写入文件
    /// </summary>
    public void WriteText(string contents, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        File.WriteAllText(_path, contents, encoding);
    }

    /// <summary>
    /// 将字节数组写入文件
    /// </summary>
    public void WriteBytes(byte[] bytes)
    {
        File.WriteAllBytes(_path, bytes);
    }

    /// <summary>
    /// 创建空文件或更新修改时间
    /// </summary>
    public void Touch()
    {
        if (Exists)
        {
            File.SetLastWriteTime(_path, DateTime.Now);
        }
        else
        {
            // 如果父目录不存在则创建
            var parent = Parent;
            if (!parent.Exists)
                parent.MakeDirectory(createParents: true);
            File.Create(_path).Dispose();
        }
    }
    #endregion

    #region 目录操作
    /// <summary>
    /// 遍历目录内容
    /// </summary>
    public IEnumerable<PathHelper> IterateDirectory()
    {
        if (!IsDirectory)
            throw new InvalidOperationException("路径不是目录");
        foreach (var entry in Directory.EnumerateFileSystemEntries(_path))
        {
            yield return new PathHelper(entry);
        }
    }

    /// <summary>
    /// 支持通配符模式匹配
    /// </summary>
    public IEnumerable<PathHelper> Glob(string pattern)
    {
        if (!IsDirectory)
            throw new InvalidOperationException("路径不是目录");
        var searchPattern = pattern.Replace('/', Path.DirectorySeparatorChar);
        // 处理递归模式
        if (pattern.Contains("**"))
        {
            return Directory
                .EnumerateFiles(_path, "*", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateDirectories(_path, "*", SearchOption.AllDirectories))
                .Where(p => MatchesGlobPattern(p, pattern))
                .Select(p => new PathHelper(p));
        }
        else
        {
            return Directory
                .EnumerateFiles(_path, searchPattern, SearchOption.TopDirectoryOnly)
                .Concat(
                    Directory.EnumerateDirectories(
                        _path,
                        searchPattern,
                        SearchOption.TopDirectoryOnly
                    )
                )
                .Select(p => new PathHelper(p));
        }
    }

    /// <summary>
    /// 递归通配符模式匹配
    /// </summary>
    public IEnumerable<PathHelper> RecursiveGlob(string pattern)
    {
        return Glob("**/" + pattern);
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    public void MakeDirectory(bool createParents = false, bool existOk = false)
    {
        try
        {
            if (createParents)
            {
                Directory.CreateDirectory(_path);
            }
            else
            {
                Directory.CreateDirectory(_path);
            }
        }
        catch (IOException) when (existOk && IsDirectory)
        {
            // 目录已经存在且 existOk 为 true
        }
    }

    /// <summary>
    /// 删除目录（必须为空）
    /// </summary>
    public void RemoveDirectory()
    {
        Directory.Delete(_path, false);
    }
    #endregion

    #region 文件系统操作
    /// <summary>
    /// 重命名或移动此文件/目录
    /// </summary>
    public PathHelper Rename(string newPath)
    {
        var destination = new PathHelper(newPath);
        if (IsFile)
        {
            File.Move(_path, destination._path);
        }
        else if (IsDirectory)
        {
            Directory.Move(_path, destination._path);
        }
        else
        {
            throw new FileNotFoundException($"路径未找到: {_path}");
        }
        return destination;
    }

    /// <summary>
    /// 替换此文件/目录（如果目标存在则覆盖）
    /// </summary>
    public PathHelper Replace(string newPath)
    {
        var destination = new PathHelper(newPath);
        if (IsFile)
        {
            if (File.Exists(destination._path))
            {
                File.Delete(destination._path);
            }
            File.Move(_path, destination._path);
        }
        else if (IsDirectory)
        {
            if (destination.Exists)
                Directory.Delete(destination._path, true);
            Directory.Move(_path, destination._path);
        }
        else
        {
            throw new FileNotFoundException($"路径未找到: {_path}");
        }
        return destination;
    }

    /// <summary>
    /// 删除此文件或符号链接
    /// </summary>
    public void Unlink(bool missingOk = false)
    {
        try
        {
            if (IsFile || IsSymlink)
            {
                File.Delete(_path);
            }
            else if (!missingOk)
            {
                throw new FileNotFoundException($"文件未找到: {_path}");
            }
        }
        catch (FileNotFoundException) when (missingOk)
        {
            // 如果 missingOk 为 true，则忽略异常
        }
    }
    #endregion

    #region 静态方法
    /// <summary>
    /// 返回当前工作目录
    /// </summary>
    public static PathHelper CurrentDirectory => new PathHelper(Directory.GetCurrentDirectory());

    /// <summary>
    /// 返回用户的家目录
    /// </summary>
    public static PathHelper Home =>
        new PathHelper(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    #endregion

    #region 辅助方法
    private bool MatchesGlobPattern(string path, string pattern)
    {
        // 简单的通配符模式匹配实现
        pattern = pattern.Replace("**", "*");
        var fileName = Path.GetFileName(path);
        var patternName = Path.GetFileName(pattern);
        return MatchesWildcard(fileName, patternName);
    }

    private bool MatchesWildcard(string text, string pattern)
    {
        // 简单的通配符 * 和 ? 匹配实现
        int textIndex = 0;
        int patternIndex = 0;
        int textLength = text.Length;
        int patternLength = pattern.Length;
        while (textIndex < textLength && patternIndex < patternLength)
        {
            if (pattern[patternIndex] == '*')
            {
                patternIndex++;
                if (patternIndex == patternLength)
                    return true;
                while (textIndex < textLength)
                {
                    if (MatchesWildcard(text.Substring(textIndex), pattern.Substring(patternIndex)))
                        return true;
                    textIndex++;
                }
                return false;
            }
            else if (pattern[patternIndex] == '?' || pattern[patternIndex] == text[textIndex])
            {
                textIndex++;
                patternIndex++;
            }
            else
            {
                return false;
            }
        }
        // 处理模式中剩余的 *
        while (patternIndex < patternLength && pattern[patternIndex] == '*')
            patternIndex++;
        return textIndex == textLength && patternIndex == patternLength;
    }
    #endregion

    #region 辅助方法
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return ".";
        // 处理相对路径
        if (path == "." || path == "..")
            return path;
        // 标准化路径分隔符并去除多余的分隔符
        path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return path;
    }
    #endregion

    #region 对象覆盖方法
    /// <summary>
    /// 返回路径的字符串表示形式
    /// </summary>
    public override string ToString() => _path;

    /// <summary>
    /// 判断指定对象是否与当前路径相等
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is PathHelper other && Equals(other);
    }

    /// <summary>
    /// 判断指定路径是否与当前路径相等
    /// </summary>
    public bool Equals(PathHelper? other)
    {
        if (other is null)
            return false;
        // 在 Windows 上使用不区分大小写的比较
        var comparison = StringComparison.OrdinalIgnoreCase;
        return string.Equals(_path, other._path, comparison);
    }

    /// <summary>
    /// 返回当前路径的哈希码
    /// </summary>
    public override int GetHashCode()
    {
        return _path.GetHashCode();
    }

    /// <summary>
    /// 比较当前路径与其他路径
    /// </summary>
    public int CompareTo(PathHelper? other)
    {
        if (other is null)
            return 1;
        var comparison = StringComparison.OrdinalIgnoreCase;
        return string.Compare(_path, other._path, comparison);
    }

    /// <summary>
    /// 判断两个路径是否相等
    /// </summary>
    public static bool operator ==(PathHelper? left, PathHelper? right)
    {
        return EqualityComparer<PathHelper>.Default.Equals(left, right);
    }

    /// <summary>
    /// 判断两个路径是否不相等
    /// </summary>
    public static bool operator !=(PathHelper? left, PathHelper? right)
    {
        return !(left == right);
    }
    #endregion
}
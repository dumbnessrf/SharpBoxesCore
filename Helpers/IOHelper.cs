using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxesCore.Helpers;

public static class IOHelper
{
    /// <summary>
    /// 路径后退指定级数
    /// </summary>
    /// <param name="originalPath">原始路径</param>
    /// <param name="levels">后退级数（如 2=后退2级）</param>
    /// <returns>后退后的路径（若级数超出根目录，返回根目录）</returns>
    [DebuggerStepThrough]
    public static string GetParentPath(this string originalPath, int levels)
    {
        string currentPath = originalPath;
        for (int i = 0; i < levels; i++)
        {
            string parent = Path.GetDirectoryName(currentPath);
            if (parent == null)
                break; // 已到根目录（如 C:\），停止后退
            currentPath = parent;
        }
        return currentPath;
    }
    [DebuggerStepThrough]
    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
    [DebuggerStepThrough]
    public static string SplitFilterString(bool needAllFile = false, params string[] filters)
    {
        var str = "*.";
        foreach (var filter in filters)
        {
            str += filter + ";*.";
        }
        str = str.Substring(0, str.Length - 3);
        str = "(" + str + ")";
        str += "|";
        foreach (var filter in filters)
        {
            str += "*." + filter + ";";
        }
        str = str.Substring(0, str.Length - 1);
        if (needAllFile)
        {
            str += "|All files(*.*)|*.*";
        }

        return str;
    }
    [DebuggerStepThrough]
    public static string GetFileNameWithoutExtension(this string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }
    [DebuggerStepThrough]
    public static string GetFileName(this string filePath)
    {
        return Path.GetFileName(filePath);
    }
    [DebuggerStepThrough]
    public static string GetExtension(this string filePath)
    {
        return Path.GetExtension(filePath);
    }
    [DebuggerStepThrough]
    public static string ChangeExtension(this string filePath, string newExtension)
    {
        return Path.ChangeExtension(filePath, newExtension);
    }
    [DebuggerStepThrough]
    public static string GetDirectoryName(this string filePath)
    {
        return Path.GetDirectoryName(filePath);
    }
    [DebuggerStepThrough]
    public static string PathCombine(this string path1, string path2)
    {
        return Path.Combine(path1, path2);
    }
    [DebuggerStepThrough]
    public static string PathCombine(this string path1, string path2, string path3)
    {
        return Path.Combine(path1, path2, path3);
    }
    [DebuggerStepThrough]
    public static string PathCombine(this string path1, string path2, string path3, string path4)
    {
        return Path.Combine(path1, path2, path3, path4);
    }
    [DebuggerStepThrough]
    public static string PathCombine(
        this string path1,
        string path2,
        string path3,
        string path4,
        string path5
    )
    {
        return Path.Combine(path1, path2, path3, path4, path5);
    }
    [DebuggerStepThrough]
    public static long GetFileSizeInMB(this string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File {filePath} not found", filePath);
        }
        var fileInfo = new FileInfo(filePath);
        return (int)(fileInfo.Length / 1024 / 1024);
    }
    [DebuggerStepThrough]
    public static bool IsFileLocked(this string filePath)
    {
        FileStream stream = null;
        try
        {
            stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
        }
        catch (IOException)
        {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            //or does not exist (has already been processed)
            return true;
        }
        finally
        {
            if (stream != null)
                stream.Close();
        }
        //file is not locked
        return false;
    }
    [DebuggerStepThrough]
    public static long GetFileSizeInGB(this string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File {filePath} not found", filePath);
        }
        var fileInfo = new FileInfo(filePath);
        return (int)(fileInfo.Length / 1024 / 1024 / 1024);
    }
    [DebuggerStepThrough]
    public static bool IsDirectoryEmpty(this string directoryPath)
    {
        return !Directory.EnumerateFileSystemEntries(directoryPath).Any();
    }
    [DebuggerStepThrough]
    public static long GetFileSizeInKB(this string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File {filePath} not found", filePath);
        }
        var fileInfo = new FileInfo(filePath);
        return (long)(fileInfo.Length / 1024);
    }
    [DebuggerStepThrough]
    public static bool FastCompareFiles_Net40(string filePath1, string filePath2)
    {
        // 1. 快速预检（同上面逻辑）
        if (string.Equals(filePath1, filePath2, StringComparison.OrdinalIgnoreCase))
            return true;

        var fileInfo1 = new FileInfo(filePath1);
        var fileInfo2 = new FileInfo(filePath2);
        if (fileInfo1.Length != fileInfo2.Length)
            return false;

        // 2. 最优IO配置
        const int BufferSize = 8192 * 4;
        using (
            var fs1 = new FileStream(
                filePath1,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                BufferSize,
                FileOptions.SequentialScan
            )
        )
        using (
            var fs2 = new FileStream(
                filePath2,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                BufferSize,
                FileOptions.SequentialScan
            )
        )
        {
            var buffer1 = new byte[BufferSize];
            var buffer2 = new byte[BufferSize];
            int readBytes1,
                readBytes2;

            do
            {
                readBytes1 = fs1.Read(buffer1, 0, BufferSize);
                readBytes2 = fs2.Read(buffer2, 0, BufferSize);

                // 核心：用Buffer.BlockCopy对比（底层优化，比逐字节循环快）
                if (readBytes1 != readBytes2)
                    return false;

                // 临时缓冲区用于对比（避免修改原缓冲区）
                var tempBuffer = new byte[readBytes1];
                Buffer.BlockCopy(buffer1, 0, tempBuffer, 0, readBytes1);
                if (!BufferCompare(tempBuffer, buffer2, readBytes1))
                    return false;
            } while (readBytes1 > 0 && readBytes2 > 0);

            return readBytes1 == 0 && readBytes2 == 0;
        }
    }

    /// <summary>
    /// 底层字节数组比较（.NET Framework 4.0+ 兼容）
    /// </summary>
    private static bool BufferCompare(byte[] a, byte[] b, int length)
    {
        for (int i = 0; i < length; i += 4)
        {
            // 按4字节对齐比较（CPU缓存优化，比单字节快）
            if (BitConverter.ToInt32(a, i) != BitConverter.ToInt32(b, i))
            {
                // 若4字节不一致，再逐字节校验（避免漏判）
                for (int j = 0; j < 4 && (i + j) < length; j++)
                {
                    if (a[i + j] != b[i + j])
                        return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 判断路径是否为完全限定路径（绝对路径）
    /// 兼容 .NET Framework 4.8
    /// </summary>
    [DebuggerStepThrough]
    public static bool IsPathFullyQualified(this string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            // 使用 Path.GetFullPath 会解析相对路径，若抛异常则说明路径非法
            // 但更重要的是：我们可以通过判断是否包含盘符或 UNC 前缀来判断绝对性
            // 更可靠的方式：使用 Uri 或 Path.IsPathRooted
            return System.IO.Path.IsPathRooted(path);
        }
        catch
        {
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using EverythingNet.Core;
using EverythingNet.Interfaces;
using SharpBoxesCore.Helpers;

namespace SharpBoxesCore.Everything;

public static class EveryThinging
{
    private static IEverything everything;

    static EveryThinging()
    {
        if (!EverythingState.IsStarted())
            EverythingState.StartService(true, EverythingState.StartMode.Service);
    }
    /// <summary>
    /// 根据系统硬盘性能及数量，首次初始化后，需要过段时间才能搜索到文件，单位为秒，一般为1~30秒
    /// </summary>
    public static void Init()
    {
 
    }

    public static IEnumerable<FileInfoByEverything> GetFiles(string pattern)
    {
        everything ??= new EverythingNet.Core.Everything();
        var results = everything.Search().Name.Contains(pattern);
        var fileInfos = results.Select(s => new FileInfoByEverything()
        {
            FullName = s.FullPath,
            Created = s.Created,
            Modified = s.Modified,
            Size = s.Size,
            IsFile = s.IsFile,
        });
        return fileInfos;
    }

    public static IEnumerable<FileInfoByEverything> GetFilesInFolder(string folder, string pattern)
    {
        everything ??= new EverythingNet.Core.Everything();
        //path:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework" mscorlib.xml
        //

        var results = everything.Search().Name.Contains(pattern);
        var fileInfos = results
            .Where(s => s.IsFile && IsFileUnderDirectory(s.FullPath, folder) && s.Size > 1)
            .Select(s => new FileInfoByEverything()
            {
                FullName = s.FullPath,
                Created = s.Created,
                Modified = s.Modified,
                Size = s.Size,
                IsFile = s.IsFile,
            });
        return fileInfos;
    }

    public static IEnumerable<FileInfoByEverything> GetFilesInFolderBetweenDates(
        string folder,
        string pattern,
        DateTime startDate,
        DateTime endDate
    )
    {
        everything ??= new EverythingNet.Core.Everything();
        var results = everything
            .Search()
            .ModificationDate.Between(startDate, endDate)
            .And.Name.Contains(pattern);
        var fileInfos = results
            .Where(s => s.IsFile && IsFileUnderDirectory(s.FullPath, folder) && s.Size > 1)
            .Select(s => new FileInfoByEverything()
            {
                FullName = s.FullPath,
                Created = s.Created,
                Modified = s.Modified,
                Size = s.Size,
                IsFile = s.IsFile,
            });
        return fileInfos;
    }

    public static IEnumerable<FileInfoByEverything> GetFilesInFolderBetweenDatesAndBetweenSizes(
        string folder,
        string pattern,
        DateTime startDate,
        DateTime endDate,
        int minMb,
        int maxMb
    )
    {
        everything ??= new EverythingNet.Core.Everything();
        var results = everything
            .Search()
            .ModificationDate.Between(startDate, endDate)
            .And.Name.Contains(pattern)
            .And.Size.Between(minMb, maxMb, EverythingNet.Query.SizeUnit.Mb);
        var fileInfos = results
            .Where(s => s.IsFile && IsFileUnderDirectory(s.FullPath, folder) && s.Size > 1)
            .Select(s => new FileInfoByEverything()
            {
                FullName = s.FullPath,
                Created = s.Created,
                Modified = s.Modified,
                Size = s.Size,
                IsFile = s.IsFile,
            });
        return fileInfos;
    }

    public static IEnumerable<FileInfoByEverything> GetFolderInFolderBetweenDatesAndBetweenSizes(
        string folder,
        string pattern,
        DateTime startDate,
        DateTime endDate
    )
    {
        everything ??= new EverythingNet.Core.Everything();
        string pattern1 = "";
        if (string.IsNullOrEmpty(folder))
        {
            pattern1 = "folder:" + $"\"{pattern}\"";
        }
        else
        {
            pattern1 = "folder:" + $"\"{pattern}\" path:\"{folder}\"";
        }

        var results = everything
            .Search()
            .ModificationDate.Between(startDate, endDate)
            .And.Name.Contains(pattern1);
        var fileInfos = results.Select(s => new FileInfoByEverything()
        {
            FullName = s.FullPath,
            Created = s.Created,
            Modified = s.Modified,
            Size = s.Size,
            IsFile = s.IsFile,
        });
        return fileInfos;
    }

    public static bool IsFileUnderDirectory(string filePath, string directoryPath)
    {
        everything ??= new EverythingNet.Core.Everything();
        if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(directoryPath))
            return false;

        try
        {
            // 获取绝对路径并标准化（统一大小写、分隔符、去除 .\ ..\ 等）
            string fullFilePath = Path.GetFullPath(filePath);
            string fullDirPath = Path.GetFullPath(directoryPath);

            // 确保目录路径以目录分隔符结尾（Windows: \，Unix: /）
            if (!fullDirPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                fullDirPath += Path.DirectorySeparatorChar;
            }

            // 使用当前系统大小写策略比较（Windows 不区分，Linux 区分）
            // 如果你希望在 Windows 上也严格区分大小写，改用 StringComparison.Ordinal
            return fullFilePath.StartsWith(fullDirPath, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            // 路径无效（如非法字符、不存在等）
            return false;
        }
    }
}

public class FileInfoByEverything
{
    public string FullName { get; init; }
    public string FileNameWithoutExtension => FullName.GetFileNameWithoutExtension();
    public string Extension => FullName.GetExtension();
    public string DirectoryName => FullName.GetDirectoryName();
    public string FileName => FullName.GetFileName();
    public long Size { get; init; }
    public long SizeInMB => Size / 1024 / 1024;

    public DateTime Created { get; init; }

    public DateTime Modified { get; init; }

    public bool IsFile { get; init; }

    public override string ToString()
    {
        return $"{FileName}, {SizeInMB}MB";
    }
}

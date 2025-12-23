using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxesCore.Helpers;

public static class ZipHelper
{
    /// <summary>
    /// 压缩文件夹（包含所有子文件夹和文件）
    /// </summary>
    /// <param name="outputFileName">压缩后的文件名（包含完整路径）</param>
    /// <param name="dirBePacked">待压缩的文件夹（包含完整路径）</param>
    public static void PackFiles(string outputFileName, string dirBePacked)
    {
        if (!Directory.Exists(dirBePacked))
        {
            throw new DirectoryNotFoundException($"文件夹 {dirBePacked} 不存在");
        }

        if (File.Exists(outputFileName))
        {
            File.Delete(outputFileName);
        }

        using (ZipArchive zip = ZipFile.Open(outputFileName, ZipArchiveMode.Create))
        {
            // 获取所有文件（包括子目录）
            string[] allFiles = Directory.GetFiles(dirBePacked, "*", SearchOption.AllDirectories);

            foreach (string filePath in allFiles)
            {
                // 计算相对路径（兼容 .NET Framework 4.8）
                string relativePath = GetRelativePath(dirBePacked, filePath);

                // ZIP 标准使用 '/' 作为路径分隔符
                relativePath = relativePath.Replace('\\', '/');

                // 添加到压缩包
                zip.CreateEntryFromFile(filePath, relativePath);
            }
        }
    }

    private static string GetRelativePath(string basePath, string targetPath)
    {
        if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            basePath += Path.DirectorySeparatorChar;

        Uri baseUri = new Uri(basePath);
        Uri targetUri = new Uri(targetPath);
        Uri relativeUri = baseUri.MakeRelativeUri(targetUri);

        // 转换为字符串并处理编码（如空格变 %20）
        string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        // 将 URI 的 '/' 转成系统路径分隔符（Windows 是 '\'），然后再统一转回 '/' 以符合 ZIP 标准
        relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return relativePath.Replace('\\', '/');
    }

    /// <summary>
    /// 解压缩
    /// </summary>
    /// <param name="fileBeUnpacked">待解压文件名(包含物理路径)</param>
    /// <param name="outputDir"> 解压到哪个目录中(包含物理路径)</param>
    public static void UnpackFiles(string fileBeUnpacked, string outputDir)
    {
        if (!File.Exists(fileBeUnpacked))
        {
            throw new FileNotFoundException($"文件{fileBeUnpacked}不存在");
        }
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        using (ZipArchive zip = ZipFile.OpenRead(fileBeUnpacked))
        {
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                string fileName = Path.Combine(outputDir, entry.FullName);
                if (fileName.EndsWith("/"))
                {
                    Directory.CreateDirectory(fileName);
                }
                else
                {
                    Directory.CreateDirectory(fileName.GetDirectoryName());
                    entry.ExtractToFile(fileName, true);
                }
            }
        }
    }
}

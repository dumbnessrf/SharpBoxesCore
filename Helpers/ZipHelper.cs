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
    /// 压缩
    /// </summary>
    /// <param name="outputFileName"> 压缩后的文件名(包含物理路径)</param>
    /// <param name="dirBePacked">待压缩的文件夹(包含物理路径)</param>
    public static void PackFiles(string outputFileName, string dirBePacked)
    {
        if (!Directory.Exists(dirBePacked))
        {
            throw new DirectoryNotFoundException($"文件夹{dirBePacked}不存在");
        }
        if (File.Exists(outputFileName))
        {
            File.Delete(outputFileName);
        }
        using (ZipArchive zip = ZipFile.Open(outputFileName, ZipArchiveMode.Create))
        {
            foreach (string file in Directory.GetFiles(dirBePacked))
            {
                zip.CreateEntryFromFile(file, Path.GetFileName(file));
            }
        }
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
                    entry.ExtractToFile(fileName, true);
                }
            }
        }
    }
}

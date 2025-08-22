using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxesCore.Helpers;

public static class IOHelper
{
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
            file.CopyTo(targetFilePath);
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

    public static string GetFileNameWithoutExtension(this string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }

    public static string GetFileName(this string filePath)
    {
        return Path.GetFileName(filePath);
    }

    public static string GetExtension(this string filePath)
    {
        return Path.GetExtension(filePath);
    }

    public static string GetDirectoryName(this string filePath)
    {
        return Path.GetDirectoryName(filePath);
    }

    public static string PathCombine(this string path1, string path2)
    {
        return Path.Combine(path1, path2);
    }

    public static string PathCombine(this string path1, string path2, string path3)
    {
        return Path.Combine(path1, path2, path3);
    }

    public static string PathCombine(this string path1, string path2, string path3, string path4)
    {
        return Path.Combine(path1, path2, path3, path4);
    }

    public static string PathCombine(this string path1, string path2, string path3, string path4, string path5)
    {
        return Path.Combine(path1, path2, path3, path4, path5);
    }
}

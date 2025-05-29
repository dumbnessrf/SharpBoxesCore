using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxesCore.Helpers;

public static class ConfigFileHelper
{
    /// <summary>
    /// config.json
    /// </summary>
    public static string NormalConfigFileName = "config.json";

    /// <summary>
    /// system.json
    /// </summary>
    public static string SystemConfigFileName = "system.json";

    /// <summary>
    /// setup.json
    /// </summary>
    public static string SetupConfigFileName = "setup.json";

    /// <summary>
    /// user.json
    /// </summary>
    public static string UserConfigFileName = "user.json";

    /// <summary>
    /// 获取当前exe所在目录AppDomain.CurrentDomain.BaseDirectory
    /// </summary>
    public static string ExeFolder = AppDomain.CurrentDomain.BaseDirectory;
    public static string ConfigFolder = System.IO.Path.Combine(ExeFolder, "Config");
    public static string LogFolder = System.IO.Path.Combine(ExeFolder, "Log");
    public static string LogsFolder = System.IO.Path.Combine(ExeFolder, "Logs");
    public static string DataFolder = System.IO.Path.Combine(ExeFolder, "Data");
    public static string TempFolder = System.IO.Path.Combine(ExeFolder, "Temp");
    public static string CacheFolder = System.IO.Path.Combine(ExeFolder, "Cache");
    public static string ImageFolder = System.IO.Path.Combine(ExeFolder, "Image");
    public static string ImagesFolder = System.IO.Path.Combine(ExeFolder, "Images");
    public static string ProgramFolder = System.IO.Path.Combine(ExeFolder, "Program");
    public static string SystemFolder = System.IO.Path.Combine(ExeFolder, "System");
    public static string HelpFolder = System.IO.Path.Combine(ExeFolder, "Help");
    public static string LicenseFolder = System.IO.Path.Combine(ExeFolder, "License");
    public static string LicensesFolder = System.IO.Path.Combine(ExeFolder, "Licenses");
    public static string ToolsFolder = System.IO.Path.Combine(ExeFolder, "Tools");

    public static string GetFolder(string folderName)
    {
        var folder = System.IO.Path.Combine(ExeFolder, folderName);
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string GetNormalConfigFilePathByExePath()
    {
        var exePath = AppDomain.CurrentDomain.BaseDirectory;
        return System.IO.Path.Combine(exePath, NormalConfigFileName);
    }

    public static string GetSystemConfigFilePathByExePath()
    {
        var exePath = AppDomain.CurrentDomain.BaseDirectory;
        return System.IO.Path.Combine(exePath, SystemConfigFileName);
    }

    public static string GetSetupConfigFilePathByExePath()
    {
        var exePath = AppDomain.CurrentDomain.BaseDirectory;
        return System.IO.Path.Combine(exePath, SetupConfigFileName);
    }

    public static string GetUserConfigFilePathByExePath()
    {
        var exePath = AppDomain.CurrentDomain.BaseDirectory;
        return System.IO.Path.Combine(exePath, UserConfigFileName);
    }
}

public class ConfigBase
{
    public static void Save<T>(T t, string filepath)
        where T : ConfigBase
    {
        var str = Newtonsoft.Json.JsonConvert.SerializeObject(
            t,
            Newtonsoft.Json.Formatting.Indented
        );
        File.WriteAllText(filepath, str);
    }

    public void Save(string filepath)
    {
        Save(this, filepath);
    }

    public void Load<T>(string filepath)
        where T : ConfigBase
    {
        //根据当前类型反序列化json字符串，并且通过反射设置属性值，注意，只设置public的属性和字段，且没有标记JsonIgnore
        var str = File.ReadAllText(filepath);
        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(str, this.GetType());
        var properties = typeof(T).GetProperties(
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
        );
        foreach (var property in properties)
        {
            if (
                property.CanWrite
                && property.CustomAttributes.Any(a =>
                    a.AttributeType == typeof(Newtonsoft.Json.JsonIgnoreAttribute)
                ) == false
            )
            {
                property.SetValue(this, property.GetValue(obj));
            }
        }
        var fields = typeof(T).GetFields(
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
        );
        foreach (var field in fields)
        {
            if (
                field.IsInitOnly == false
                && field.CustomAttributes.Any(a =>
                    a.AttributeType == typeof(Newtonsoft.Json.JsonIgnoreAttribute)
                ) == false
            )
            {
                field.SetValue(this, field.GetValue(obj));
            }
        }
    }

    public static void Load<T>(string filepath, out T t)
        where T : ConfigBase
    {
        var str = File.ReadAllText(filepath);
        t = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
    }
}

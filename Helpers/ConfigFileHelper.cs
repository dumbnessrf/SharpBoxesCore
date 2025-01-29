using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxes.Helpers;

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

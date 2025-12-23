using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SharpBoxesCore.DynamicLoad;

/// <summary>
/// <para>提供库加载帮助的静态类。</para>
/// <para>主要功能包括加载指定的DLL，获取指定类型的实例，从指定的文件夹中获取DLL文件，从指定的程序集中查找指定类型的子类或实现了指定接口的类等。</para>
/// </summary>
public static class DynamicLoadHelper
{
    /// <summary>
    /// <para>加载指定的DLL并获取指定类型的实例。</para>
    /// <para>首先加载指定名称的DLL，然后获取指定名称空间的指定类型，最后创建并返回该类型的实例。</para>
    /// </summary>
    /// <typeparam name="T">期望的类型</typeparam>
    /// <param name="dllName">DLL名称</param>
    /// <param name="namespaceName">命名空间名称</param>
    /// <param name="typeName">类型名称</param>
    /// <param name="args">构造函数参数</param>
    /// <param name="message">返回的消息</param>
    /// <returns>创建的实例</returns>
    public static T LoadDll<T>(
        string dllName,
        string namespaceName,
        string typeName,
        object[] args,
        out string message
    )
        where T : class
    {
        message = "加载成功";

        //加载指定名称的DLL
        var assembly = Assembly.Load(dllName);
        //获取指定名称空间的指定类型
        var type = assembly.GetType($"{namespaceName}.{typeName}");
        if (type == null)
        {
            message = $"{namespaceName}.{typeName}类型获取失败，返回null";
            return null;
        }
        //获取指定类型的实例
        try
        {
            var instance = Activator.CreateInstance(type, args) as T;
            if (instance == null)
            {
                message = $"{namespaceName}.{typeName}构造失败，返回null";
            }
            return instance;
        }
        catch (Exception ex)
        {
            message = $"{namespaceName}.{typeName}构造失败{ex}";
            return null;
        }
    }

    /// <summary>
    /// <para>加载指定的DLL并获取指定类型的实例。</para>
    /// <para>首先加载指定名称的DLL，然后获取指定名称空间的指定类型，最后创建并返回该类型的实例。</para>
    /// </summary>
    /// <typeparam name="T">期望的类型</typeparam>
    /// <param name="dllName">DLL名称</param>
    /// <param name="namespaceName">命名空间名称</param>
    /// <param name="typeName">类型名称</param>
    /// <param name="message">返回的消息</param>
    /// <returns>创建的实例</returns>
    public static T LoadDll<T>(
        string dllName,
        string namespaceName,
        string typeName,
        out string message
    )
        where T : class
    {
        message = "加载成功";
        //加载指定名称的DLL
        //var assembly = Assembly.Load(dllName);
        byte[] fileData = File.ReadAllBytes(dllName);
        var assembly = Assembly.Load(fileData);
        //获取指定名称空间的指定类型
        var type = assembly.GetType($"{namespaceName}.{typeName}");
        if (type == null)
        {
            message = $"{namespaceName}.{typeName}类型获取失败，返回null";
            return null;
        }
        //获取指定类型的实例
        try
        {
            var instance = Activator.CreateInstance(type) as T;
            if (instance == null)
            {
                message = $"{namespaceName}.{typeName}构造失败，返回null";
            }
            return instance;
        }
        catch (Exception ex)
        {
            message = $"{namespaceName}.{typeName}构造失败{ex}";
            return null;
        }
    }

    /// <summary>
    /// <para>从指定的文件夹中获取DLL文件。</para>
    /// <para>遍历文件夹中的所有文件，只保留可以成功加载的DLL文件。</para>
    /// </summary>
    /// <param name="folder">文件夹路径</param>
    /// <returns>DLL文件列表</returns>
    private static List<string> GeDllFiles(string folder, Predicate<string> predicate = null)
    {
        var files = Directory.GetFiles(folder, "*.dll").ToList();
        var rubbishes = new List<string>();
        foreach (var file in files)
        {
            if (predicate != null && !predicate(file))
            {
                continue;
            }
            try
            {
                byte[] fileData = File.ReadAllBytes(file);
                _ = Assembly.Load(fileData);
                //_ = Assembly.LoadFile(file);
            }
            catch (Exception)
            {
                rubbishes.Add(file);
            }
        }
        files.RemoveAll(s => rubbishes.Contains(s));
        return files;
    }

    /// <summary>
    /// <para>从指定的文件夹中获取DllModel列表。</para>
    /// <para>首先获取文件夹中的所有DLL文件，然后为每个文件创建一个DllModel实例。</para>
    /// </summary>
    /// <param name="folder">文件夹路径</param>
    /// <param name="baseTypeFilter">基类型过滤器</param>
    /// <returns>DllModel列表</returns>
    public static List<DllModel> GetDllModelsFromFolder(
        string folder,
        Type baseTypeFilter = null,
        Predicate<string> predicate = null
    )
    {
        var files = GeDllFiles(folder);
        var dllModels = new List<DllModel>();
        foreach (var file in files)
        {
            if (predicate != null && !predicate(file))
            {
                continue;
            }
            byte[] fileData = File.ReadAllBytes(file);
            var assembly = Assembly.Load(fileData);

            var dllModel = new DllModel(
                Path.GetFileNameWithoutExtension(file),
                file,
                assembly,
                baseTypeFilter
            );

            dllModels.Add(dllModel);
        }
        return dllModels;
    }

    public static DllModel GetDllModelFromFile(string file, Type baseTypeFilter = null)
    {
        if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
        {
            return null;
        }
        try
        {
            byte[] fileData = File.ReadAllBytes(file);
            var assembly = Assembly.Load(fileData);
            var dllModel = new DllModel(
                Path.GetFileNameWithoutExtension(file),
                file,
                assembly,
                baseTypeFilter
            );
            return dllModel;
        }
        catch
        {
            return null;
        }
    }

    public static Assembly LoadAssemblySafely(string dllFile)
    {
        byte[] fileData = File.ReadAllBytes(dllFile);
        var assembly = Assembly.Load(fileData);
        return assembly;
    }

    /// <summary>
    /// <para>从指定的程序集中查找指定类型的子类或实现了指定接口的类。</para>
    /// <para>遍历程序集中的所有类型，只保留是指定基类型的子类或实现了指定接口的类型。</para>
    /// </summary>
    /// <param name="assembly">程序集</param>
    /// <param name="baseType">基类型或接口类型</param>
    /// <returns>找到的类型列表</returns>
    public static List<Type> FindSpecifiedTypeInheritFromAssembly(
        Assembly assembly,
        Type baseType = null
    )
    {
        baseType ??= typeof(object);
        var types = assembly.GetTypes();
        return types
            .Where(type =>
                type.IsSubclassOf(baseType) || type.GetInterface(baseType.FullName) is not null
            )
            .ToList();
    }

    /// <summary>
    /// 从指定的程序集中查找指定类型的子类或实现了指定接口的类，以及元属性必须包含指定属性的类型。
    /// </summary>
    /// <param name="dllFile">DLL 文件完整路径</param>
    /// <param name="baseTypes">目标基类/接口数组</param>
    /// <param name="attributes">目标特性类型数组</param>
    /// <param name="exceptionHandler">异常处理委托</param>
    /// <returns>满足条件的所有非抽象、非泛型的可实例化类型列表</returns>
    /// <exception cref="FileNotFoundException">指定的 DLL 文件不存在</exception>
    /// <exception cref="FileLoadException">DLL 加载失败</exception>
    /// <exception cref="BadImageFormatException">DLL 不是有效的 .NET 程序集</exception>
    public static List<Type> FindSpecifiedTypeInheritFromAssembliesAndSpecifiedAttributes(
        string dllFile,
        Type[] baseTypes,
        Type[] attributes,
        Action<Exception> exceptionHandler=null)
    {
        var result = new List<Type>();

        try
        {
            // 验证文件是否存在
            if (!File.Exists(dllFile))
                throw new FileNotFoundException("指定的DLL文件不存在", dllFile);

            // 加载程序集
            Assembly assembly = Assembly.LoadFrom(dllFile);

            // 获取程序集中所有的类型
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 处理类型加载异常，仅使用成功加载的类型
                types = ex.Types.Where(t => t != null).ToArray();
                exceptionHandler?.Invoke(ex);
            }

            // 筛选符合条件的类型
            foreach (var type in types)
            {
                // 排除接口、抽象类、泛型定义
                if (type.IsInterface || type.IsAbstract || type.IsGenericTypeDefinition)
                    continue;

                // 检查是否符合基类/接口条件
                bool isBaseTypeMatch = true;
                if (baseTypes != null && baseTypes.Length > 0)
                {
                    isBaseTypeMatch = baseTypes.Any(baseType =>
                        baseType.IsAssignableFrom(type) &&
                        type != baseType // 排除类型自身
                    );
                }

                if (!isBaseTypeMatch)
                    continue;

                // 检查是否符合特性条件
                bool isAttributeMatch = true;
                if (attributes != null && attributes.Length > 0)
                {
                    isAttributeMatch = attributes.All(attrType =>
                        type.GetCustomAttributes(attrType, false).Any()
                    );
                }

                if (isAttributeMatch)
                {
                    result.Add(type);
                }
            }
        }
        catch (Exception ex) when (
            ex is FileNotFoundException ||
            ex is FileLoadException ||
            ex is BadImageFormatException
        )
        {
            // 调用异常处理委托并重新抛出指定异常
            exceptionHandler?.Invoke(ex);
            throw;
        }
        catch (Exception ex)
        {
            // 处理其他异常
            exceptionHandler?.Invoke(ex);
        }

        return result;
    }

    /// <summary>
    /// 在文件夹的第一层查找所有可加载的 DLL，从每个 DLL 中筛选符合条件的类型
    /// </summary>
    /// <param name="folder">目标文件夹路径</param>
    /// <param name="baseTypes">目标基类/接口数组</param>
    /// <param name="attributes">目标特性类型数组</param>
    /// <param name="exceptionHandler">异常处理委托</param>
    /// <returns>所有满足条件的非抽象、非泛型可实例化类型列表</returns>
    /// <exception cref="DirectoryNotFoundException">指定的文件夹不存在</exception>
    /// <exception cref="InvalidOperationException">文件夹访问权限不足或其他未知错误</exception>
    public static List<Type> FindSpecifiedTypeInheritFromFolderAndSpecifiedAttributes(
        string folder,
        Type[] baseTypes,
        Type[] attributes,
        Action<Exception> exceptionHandler=null)
    {
        var result = new List<Type>();

        try
        {
            // 验证文件夹是否存在
            if (!Directory.Exists(folder))
                throw new DirectoryNotFoundException("指定的文件夹不存在");

            // 获取文件夹中所有的DLL文件（仅第一层）
            string[] dllFiles = Directory.GetFiles(folder, "*.dll", SearchOption.TopDirectoryOnly);

            foreach (var dllFile in dllFiles)
            {
                try
                {
                    // 调用第一个方法处理每个DLL
                    var typesFromDll = FindSpecifiedTypeInheritFromAssembliesAndSpecifiedAttributes(
                        dllFile,
                        baseTypes,
                        attributes,
                        exceptionHandler
                    );
                    result.AddRange(typesFromDll);
                }
                catch (Exception ex)
                {
                    // 处理单个DLL的加载/处理异常，继续处理其他DLL
                    exceptionHandler?.Invoke(new Exception($"处理DLL文件 {dllFile} 时发生错误", ex));
                }
            }
        }
        catch (DirectoryNotFoundException ex)
        {
            // 调用异常处理委托并重新抛出指定异常
            exceptionHandler?.Invoke(ex);
            throw;
        }
        catch (Exception ex) when (
            ex is UnauthorizedAccessException ||
            ex is PathTooLongException ||
            ex is IOException
        )
        {
            // 包装为InvalidOperationException并抛出
            var invalidOpEx = new InvalidOperationException("文件夹访问权限不足或发生I/O错误", ex);
            exceptionHandler?.Invoke(invalidOpEx);
            throw invalidOpEx;
        }
        catch (Exception ex)
        {
            // 处理其他未知错误
            var invalidOpEx = new InvalidOperationException("发生未知错误", ex);
            exceptionHandler?.Invoke(invalidOpEx);
            throw invalidOpEx;
        }

        return result;
    }

    public static List<string> GetAllNamespacesFromDll(string dllFile)
    {
        byte[] fileData = File.ReadAllBytes(dllFile);
        var assembly = Assembly.Load(fileData);
        return assembly.GetTypes().Select(t => t.Namespace).Distinct().ToList();
    }

    public static List<Type> GetTypesFromDll(string dllFile)
    {
        byte[] fileData = File.ReadAllBytes(dllFile);
        var assembly = Assembly.Load(fileData);
        return assembly.GetTypes().ToList();
    }

    /// <summary>
    /// <para>创建指定类型的实例。</para>
    /// <para>使用Activator.CreateInstance方法创建指定类型的实例。</para>
    /// </summary>
    /// <typeparam name="T">期望的类型</typeparam>
    /// <param name="message">返回的消息</param>
    /// <param name="args">构造函数参数</param>
    /// <returns>创建的实例</returns>
    public static T CreateObjectFromType<T>(out string message, object[] args = null)
    {
        message = "创建实例成功";
        args ??= new object[] { };
        try
        {
            var instance = Activator.CreateInstance(typeof(T), args);
            return (T)instance;
        }
        catch (Exception ex)
        {
            message = $"创建实例{typeof(T)}失败：{ex}";
            return default(T);
        }
    }

    /// <summary>
    /// <para>从指定的程序集中查找附加了指定Attribute的类型。</para>
    /// <para>遍历程序集中的所有类型，只保留附加了指定Attribute的类型。</para>
    /// </summary>
    /// <param name="assembly">程序集</param>
    /// <param name="attr">Attribute类型</param>
    /// <returns>找到的类型列表</returns>
    public static List<Type> FindSpecifiedTypeHasAttributeFromAssembly(Assembly assembly, Type attr)
    {
        var types = assembly.GetTypes();
        return types.Where(type => type.GetCustomAttribute(attr) != null).ToList();
    }

    /// <summary>
    /// <para>从指定的类型中查找附加了指定Attribute的属性。</para>
    /// <para>遍历类型中的所有属性，只保留附加了指定Attribute的属性。</para>
    /// </summary>
    /// <param name="classType">类类型</param>
    /// <param name="attrs">Attribute类型列表</param>
    /// <returns>找到的属性列表</returns>
    public static List<PropertyInfo> FindSpecifiedPropertyHasAttributeFromType(
        Type classType,
        params Type[] attrs
    )
    {
        var properties = classType.GetProperties();
        List<PropertyInfo> propertyList = new();
        foreach (var prop in properties)
        {
            foreach (var attr in attrs)
            {
                if (prop.GetCustomAttribute(attr) != null)
                {
                    propertyList.Add(prop);
                }
            }
        }
        return propertyList;
    }

    /// <summary>
    /// <para>从指定的类型中查找没有附加指定Attribute的属性。</para>
    /// <para>遍历类型中的所有属性，只保留没有附加指定Attribute的属性。</para>
    /// </summary>
    /// <param name="classType">类类型</param>
    /// <param name="attrs">Attribute类型列表</param>
    /// <returns>找到的属性列表</returns>
    public static List<PropertyInfo> FindSpecifiedPropertyExceptAttributeFromType(
        Type classType,
        params Type[] attrs
    )
    {
        var properties = classType.GetProperties();
        List<PropertyInfo> propertyList = new();
        foreach (var prop in properties)
        {
            foreach (var attr in attrs)
            {
                if (prop.GetCustomAttribute(attr) == null)
                {
                    propertyList.Add(prop);
                }
            }
        }
        return propertyList;
    }

    /// <summary>
    /// <para>从指定的类型中查找附加了指定Attribute的方法。</para>
    /// <para>遍历类型中的所有方法，只保留附加了指定Attribute的方法。</para>
    /// </summary>
    /// <param name="classType">类类型</param>
    /// <param name="attr">Attribute类型</param>
    /// <returns>找到的方法列表</returns>
    public static List<MethodInfo> FindSpecifiedMethodHasAttributeFromType(
        Type classType,
        Type attr
    )
    {
        var methods = classType.GetMethods();
        List<MethodInfo> methodList = new();
        foreach (var method in methods)
        {
            if (method.GetCustomAttribute(attr) != null)
            {
                methodList.Add(method);
            }
        }
        return methodList;
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SharpBoxesCore.Reflection;

/// <summary>
/// 提供类的辅助方法，包括设置属性的 Display Name 和 Description...
/// </summary>
public static class ClassHelper
{
    /// <summary>
    /// 设置指定类型中指定属性的显示名称。
    /// </summary>
    /// <typeparam name="T">属性所属的类型。</typeparam>
    /// <param name="propertyName">要设置显示名称的属性的名称。</param>
    /// <param name="newDisplayName">要设置的新显示名称。</param>
    public static void SetDisplayName<T>(string propertyName, string newDisplayName)
    {
        // 获取指定类型中指定属性的 PropertyDescriptor
        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(typeof(T))[propertyName];

        // 获取属性的 DisplayNameAttribute
        DisplayNameAttribute attribute =
            descriptor.Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;

        // 获取 DisplayNameAttribute 的私有字段 "_displayName"
        FieldInfo field = attribute
            ?.GetType()
            .GetField("_displayName", BindingFlags.NonPublic | BindingFlags.Instance);

        // 将 "_displayName" 字段的值设置为新的显示名称
        field?.SetValue(attribute, newDisplayName);
    }

    /// <summary>
    /// 设置指定类型中指定属性的描述。
    /// </summary>
    /// <typeparam name="T">属性所属的类型。</typeparam>
    /// <param name="propertyName">要设置描述的属性的名称。</param>
    /// <param name="newDesc">要设置的新描述。</param>
    public static void SetDescription<T>(string propertyName, string newDesc)
    {
        // 获取指定类型中指定属性的 PropertyDescriptor
        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(typeof(T))[propertyName];

        // 获取属性的 DescriptionAttribute
        DescriptionAttribute attribute =
            descriptor.Attributes[typeof(DescriptionAttribute)] as DescriptionAttribute;

        // 获取 DescriptionAttribute 的私有字段 "description"
        FieldInfo field = attribute
            ?.GetType()
            .GetField("description", BindingFlags.NonPublic | BindingFlags.Instance);

        // 将 "description" 字段的值设置为新的描述
        field?.SetValue(attribute, newDesc);
    }

    /// <summary>
    /// 设置指定类型中指定属性的是否可见。
    /// </summary>
    /// <typeparam name="T">属性所属的类型。</typeparam>
    /// <param name="propertyName">要设置是否可见的属性的名称。</param>
    /// <param name="isBrowsable">设置是否可见。</param>
    public static void SetBrowsable<T>(string propertyName, bool isBrowsable)
    {
        // 获取指定类型中指定属性的 PropertyDescriptor
        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(typeof(T))[propertyName];
        // 获取属性的 DisplayNameAttribute
        BrowsableAttribute attribute =
            descriptor.Attributes[typeof(BrowsableAttribute)] as BrowsableAttribute;
        // 获取 DisplayNameAttribute 的私有字段 "browsable"
        FieldInfo field = attribute
            ?.GetType()
            .GetField("browsable", BindingFlags.NonPublic | BindingFlags.Instance);
        // 将 "browsable" 字段的值设置为新的显示名称
        field?.SetValue(attribute, isBrowsable);
    }

    /// <summary>
    /// 设置指定类型中指定属性的类别。
    /// </summary>
    /// <typeparam name="T">属性所属的类型。</typeparam>
    /// <param name="propertyName">要设置类别的属性的名称。</param>
    /// <param name="newCate">要设置的新类别。</param>
    public static void SetCategory<T>(string propertyName, string newCate)
    {
        // 获取指定类型中指定属性的 PropertyDescriptor
        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(typeof(T))[propertyName];

        // 获取属性的 CategoryAttribute
        CategoryAttribute attribute =
            descriptor.Attributes[typeof(CategoryAttribute)] as CategoryAttribute;

        // 获取 CategoryAttribute 的私有字段 "categoryValue"
        FieldInfo field = attribute
            ?.GetType()
            .GetField("categoryValue", BindingFlags.NonPublic | BindingFlags.Instance);

        // 将 "categoryValue" 字段的值设置为新的类别
        field?.SetValue(attribute, newCate);
    }

    /// <summary>
    /// 获取指定实例的指定字段的值。
    /// </summary>
    /// <typeparam name="TInstance">实例的类型。</typeparam>
    /// <typeparam name="TResult">字段的值的类型。</typeparam>
    /// <param name="t">要获取字段值的实例。</param>
    /// <param name="name">要获取的字段的名称。</param>
    /// <returns>字段的值，如果字段存在并且可以转换为指定类型，则返回字段的值；否则，返回默认值。</returns>
    /// <exception cref="ArgumentNullException">当<paramref name="t"/>为null时引发。</exception>
    /// <exception cref="ArgumentException">当指定的字段在类型中不存在时引发。</exception>
    public static TResult GetFieldValue<TInstance, TResult>(this TInstance t, string name)
    {
        if (t == null)
        {
            throw new ArgumentNullException(nameof(t));
        }

        var field = typeof(TInstance).GetField(
            name,
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
        if (field == null)
        {
            throw new ArgumentException(
                $"字段'{name}'在类型'{typeof(TInstance).FullName}'中找不到。",
                nameof(name)
            );
        }

        var value = field.GetValue(t);
        if (value is TResult result)
        {
            return result;
        }

        return default;
    }

    /// <summary>
    /// 获取指定实例的指定属性的值。
    /// </summary>
    /// <typeparam name="TInstance">实例的类型。</typeparam>
    /// <typeparam name="TResult">属性值的类型。</typeparam>
    /// <param name="t">要获取属性值的实例。</param>
    /// <param name="name">要获取的属性的名称。</param>
    /// <returns>指定属性的值，如果属性不存在或值无法转换为指定类型，则返回默认值。</returns>
    /// <exception cref="ArgumentNullException">当<paramref name="t"/>为null时引发。</exception>
    /// <exception cref="ArgumentException">当指定的属性在类型<typeparamref name="TInstance"/>中找不到时引发。</exception>
    public static TResult GetPropertyValue<TInstance, TResult>(this TInstance t, string name)
    {
        if (t == null)
        {
            throw new ArgumentNullException(nameof(t));
        }

        var property = typeof(TInstance).GetProperty(
            name,
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
        if (property == null)
        {
            throw new ArgumentException(
                $"属性'{name}'在类型'{typeof(TInstance).FullName}'中找不到。",
                nameof(name)
            );
        }

        var value = property.GetValue(t);
        if (value is TResult result)
        {
            return result;
        }

        return default;
    }

    public static TResult GetInstanceFieldValue<TInstance, TResult>(
        this TInstance instance,
        string name
    )
    {
        var field = typeof(TInstance).GetField(
            name,
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
        if (field == null)
        {
            throw new ArgumentException(
                $"字段'{name}'在类型'{typeof(TInstance).FullName}'中找不到。",
                nameof(name)
            );
        }

        var value = field.GetValue(instance);
        if (value is TResult result)
        {
            return result;
        }

        return default;
    }

    public static void SetInstanceFieldValue<TInstance, TValue>(
        this TInstance instance,
        string name,
        TValue value
    )
    {
        var field = typeof(TInstance).GetField(
            name,
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
        if (field == null)
        {
            throw new ArgumentException(
                $"字段'{name}'在类型'{typeof(TInstance).FullName}'中找不到。",
                nameof(name)
            );
        }

        field.SetValue(instance, value);
    }

    public static TResult GetInstancePropertyValue<TInstance, TResult>(
        this TInstance instance,
        string name
    )
    {
        var property = typeof(TInstance).GetProperty(
            name,
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
        if (property == null)
        {
            throw new ArgumentException(
                $"属性'{name}'在类型'{typeof(TInstance).FullName}'中找不到。",
                nameof(name)
            );
        }

        var value = property.GetValue(instance);
        if (value is TResult result)
        {
            return result;
        }

        return default;
    }

    public static void SetInstancePropertyValue<TInstance, TValue>(
        this TInstance instance,
        string name,
        TValue value
    )
    {
        var property = typeof(TInstance).GetProperty(
            name,
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
        if (property == null)
        {
            throw new ArgumentException(
                $"属性'{name}'在类型'{typeof(TInstance).FullName}'中找不到。",
                nameof(name)
            );
        }

        property.SetValue(instance, value);
    }

    public static TResult GetStaticFieldValue<TClass, TResult>( string name)
    {
        var field = typeof(TClass).GetField(
            name,
            BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.NonPublic
        );
        if (field == null)
        {
            throw new ArgumentException(
                $"字段'{name}'在类型'{typeof(TClass).FullName}'中找不到。",
                nameof(name)
            );
        }

        var value = field.GetValue(null);
        if (value is TResult result)
        {
            return result;
        }

        return default;
    }

    public static void SetStaticFieldValue<TClass, TValue>(
 
        string name,
        TValue value
    )
    {
        var field = typeof(TClass).GetField(
            name,
            BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.NonPublic
        );
        if (field == null)
        {
            throw new ArgumentException(
                $"字段'{name}'在类型'{typeof(TClass).FullName}'中找不到。",
                nameof(name)
            );
        }
        field.SetValue(null, value);
    }
    /// <summary>
    /// 判断一个类似是否是是指定泛型版本
    /// <example>
    /// <code>
    /// if (item.IsInstanceOfGenericType(typeof(PostUnit&lt;,&gt;)))
    /// </code>
    /// <code>
    /// if (item.IsInstanceOfGenericType(typeof(GetUnit&lt;&gt;)))
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="genericType"></param>
    /// <returns></returns>
    public static bool IsInstanceOfGenericType<T>(this T obj, Type genericType)
    {
        if (obj == null) return false;
        Type type = obj.GetType();

        while (type != null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }
            type = type.BaseType;
        }

        return false;
    }
    public static TResult GetStaticPropertyValue<TClass, TResult>( string name)
    {
        var property = typeof(TClass).GetProperty(
            name,
            BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.NonPublic
        );
        if (property == null)
        {
            throw new ArgumentException(
                $"属性'{name}'在类型'{typeof(TClass).FullName}'中找不到。",
                nameof(name)
            );
        }

        var value = property.GetValue(null);
        if (value is TResult result)
        {
            return result;
        }

        return default;
    }

    public static void SetStaticPropertyValue<TClass, TValue>(

        string name,
        TValue value
    )
    {
        var property = typeof(TClass).GetProperty(
            name,
            BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.NonPublic
        );
        if (property == null)
        {
            throw new ArgumentException(
                $"属性'{name}'在类型'{typeof(TClass).FullName}'中找不到。",
                nameof(name)
            );
        }

        property.SetValue(null, value);
    }

    public static TResult InvokeStaticMethod<TClass, TResult>(

        string name,
        params object[] args
    )
    {
        var method = typeof(TClass).GetMethod(
            name,
            BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.NonPublic
        );
        if (method == null)
        {
            throw new ArgumentException(
                $"方法'{name}'在类型'{typeof(TClass).FullName}'中找不到。",
                nameof(name)
            );
        }

        var result = method.Invoke(null, args);
        if (result is TResult resultValue)
        {
            return resultValue;
        }

        return default;
    }

    public static void InvokeStaticVoidMethod<TClass>(

        string name,
        params object[] args
    )
    {
        var method = typeof(TClass).GetMethod(
            name,
            BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.NonPublic
        );
        if (method == null)
        {
            throw new ArgumentException(
                $"方法'{name}'在类型'{typeof(TClass).FullName}'中找不到。",
                nameof(name)
            );
        }

        method.Invoke(null, args);
    }

    public static TResult InvokeInstanceMethod<TInstance, TResult>(
        this TInstance instance,
        string name,
        params object[] args
    )
    {
        var method = typeof(TInstance).GetMethod(
            name,
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
        if (method == null)
        {
            throw new ArgumentException(
                $"方法'{name}'在类型'{typeof(TInstance).FullName}'中找不到。",
                nameof(name)
            );
        }

        var result = method.Invoke(instance, args);
        if (result is TResult resultValue)
        {
            return resultValue;
        }

        return default;
    }

    public static void InvokeInstanceVoidMethod<TInstance>(
        this TInstance instance,
        string name,
        params object[] args
    )
    {
        var method = typeof(TInstance).GetMethod(
            name,
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
        if (method == null)
        {
            throw new ArgumentException(
                $"方法'{name}'在类型'{typeof(TInstance).FullName}'中找不到。",
                nameof(name)
            );
        }

        method.Invoke(instance, args);
    }

    public static MethodInfo GetStaticMethod<TClass>(  string name)
    {
        var method = typeof(TClass).GetMethod(
            name,
            BindingFlags.Public
                | BindingFlags.Static        
                | BindingFlags.NonPublic
        );
        if (method == null)
        {
            throw new ArgumentException(
                $"方法'{name}'在类型'{typeof(TClass).FullName}'中找不到。",
                nameof(name)
            );
        }

        return method;
    }

    public static MethodInfo GetInstanceMethod<TInstance>(  string name)
    {
        var method = typeof(TInstance).GetMethod(
            name,
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
        if (method == null)
        {
            throw new ArgumentException(
                $"方法'{name}'在类型'{typeof(TInstance).FullName}'中找不到。",
                nameof(name)
            );
        }

        return method;
    }

    public static FieldInfo GetStaticField<TClass>( string name)
    {
        var field = typeof(TClass).GetField(
            name,
            BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.NonPublic
        );  
        if (field == null)
        {
            throw new ArgumentException(
                $"字段'{name}'在类型'{typeof(TClass).FullName}'中找不到。",
                nameof(name)
            );
        }

        return field;
    }

    public static FieldInfo GetInstanceField<TInstance>( string name)
    {
        var field = typeof(TInstance).GetField(
            name,
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
        if (field == null)
        {
            throw new ArgumentException(
                $"字段'{name}'在类型'{typeof(TInstance).FullName}'中找不到。",
                nameof(name)
            );
        }

        return field;
    }

    public static PropertyInfo GetStaticProperty<TClass>( string name)
    {
        var property = typeof(TClass).GetProperty(
            name,
            BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.NonPublic
        );
        if (property == null)
        {
            throw new ArgumentException(
                $"属性'{name}'在类型'{typeof(TClass).FullName}'中找不到。",
                nameof(name)
            );
        }

        return property;
    }

    public static PropertyInfo GetInstanceProperty<TInstance>(  string name)
    {
        var property = typeof(TInstance).GetProperty(
            name,
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
        if (property == null)
        {
            throw new ArgumentException(
                $"属性'{name}'在类型'{typeof(TInstance).FullName}'中找不到。",
                nameof(name)
            );
        }

        return property;
    }

    public static IEnumerable<PropertyInfo> GetStaticProperties<TClass>()
    {
        return typeof(TClass).GetProperties(
            BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.NonPublic
        );
    }

    public static IEnumerable<PropertyInfo> GetInstanceProperties<TInstance>( )
    {
        return typeof(TInstance).GetProperties(
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
    }

    public static IEnumerable<FieldInfo> GetStaticFields<TClass>()
    {
        return typeof(TClass).GetFields(
            BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.NonPublic
        );
    }

    public static IEnumerable<FieldInfo> GetInstanceFields<TInstance>( )
    {
        return typeof(TInstance).GetFields(
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
    }

    public static IEnumerable<MethodInfo> GetStaticMethods<TClass>()
    {
        return typeof(TClass).GetMethods(
            BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.NonPublic
        );
    }

    public static IEnumerable<MethodInfo> GetInstanceMethods<TInstance>( )
    {
        return typeof(TInstance).GetMethods(
            BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
    }

}

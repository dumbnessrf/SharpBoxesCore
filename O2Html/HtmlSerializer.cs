using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using O2Html.Converters;
using O2Html.Dom;
using O2Html.Dom.Elements;

namespace O2Html;

/// <summary>
/// Flags describing which kinds of instance members should be serialized for single objects.
/// </summary>
[Flags]
internal enum MemberSelectionFlags
{
    None = 0,
    PublicProperties = 1 << 0,
    NonPublicProperties = 1 << 1,
    PublicFields = 1 << 2,
    NonPublicFields = 1 << 3,
}

/// <summary>
/// Provides methods for converting .NET objects or value types to HTML.
/// </summary>
public sealed class HtmlSerializer
{
    private static readonly HtmlSerializerOptions _defaultHtmlSerializerOptions = new();
    private static readonly ConcurrentDictionary<Type, HtmlConverter?> _typeConverterCache = new();
    private static readonly ConcurrentDictionary<Type, TypeCategory> _typeCategoryCache = new();
    private static readonly ConcurrentDictionary<(Type Type, MemberSelectionFlags Flags), PropertyInfo[]> _typePropertyCache = new();
    private static readonly ConcurrentDictionary<(Type Type, MemberSelectionFlags Flags), FieldInfo[]> _typeFieldCache = new();
    private static readonly ConcurrentDictionary<Type, Type?> _collectionElementTypeCache = new();

    // First converter in list that can convert type will be selected.
    private static readonly HtmlConverter[] _defaultHtmlConverters = new HtmlConverter[]
    {
        new FileSystemInfoHtmlConverter(),
        new TwoDimensionalArrayHtmlConverter(),
        new DataSetHtmlConverter(),
        new DataTableHtmlConverter(),
#if NETCOREAPP3_0_OR_GREATER
        new JsonDocumentHtmlConverter(),
#endif
        new XNodeHtmlConverter(),
        new XmlNodeHtmlConverter(),
        new DotNetTypeWithStringRepresentationHtmlConverter(),
#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER
        new TupleHtmlConverter(),
#endif
#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
        new MemoryHtmlConverter(),
#endif
        new CollectionHtmlConverter(),
        new ObjectHtmlConverter(),
    };

    public HtmlSerializer(HtmlSerializerOptions? serializerSettings = null)
    {
        SerializerOptions = serializerSettings ?? _defaultHtmlSerializerOptions;

        // First converter in list that can convert type will be selected.
        Converters = new List<HtmlConverter>();

        // Insert user-defined converters at the beginning so they take precedence.
        Converters.AddRange(SerializerOptions.Converters);

        // Add default converters after user-defined ones.
        Converters.AddRange(_defaultHtmlConverters);
    }

    public HtmlSerializerOptions SerializerOptions { get; }

    public List<HtmlConverter> Converters { get; }


    /// <summary>
    /// Serializes an object to a HTML DOM object tree.
    /// </summary>
    /// <param name="obj">The object or value type to serialize.</param>
    /// <param name="htmlSerializerSettings">Serialization settings.</param>
    /// <typeparam name="T">Type of object being serialized.</typeparam>
    /// <returns>HTML DOM object tree representing the object or value type being serialized.</returns>
    public static Node Serialize<T>(T? obj, HtmlSerializerOptions? htmlSerializerSettings = null)
    {
        var serializer = Create(htmlSerializerSettings);

        var type = typeof(T);

        if (obj != null && type == typeof(object))
        {
            type = obj.GetType();
        }

        return serializer.Serialize(obj, type);
    }

    public Node Serialize<T>(T? obj, Type type, SerializationScope? serializationScope = null)
    {
        if (obj == null)
        {
            return new Null(SerializerOptions.CssClasses.Null);
        }

        serializationScope = GetSerializationScope(serializationScope);

        if (ShouldShortCircuit(obj, type, serializationScope, SerializerOptions, out var shortCircuitValue))
        {
            return shortCircuitValue!;
        }

        var converter = GetConverter(type);
        if (converter == null)
        {
            throw new HtmlSerializationException($"Could not find a {nameof(HtmlConverter)} for type: {type}");
        }

        return converter.WriteHtml(obj, type, serializationScope, this);
    }

    public void SerializeWithinTableRow<T>(Element tr, T? obj, Type type, SerializationScope? serializationScope = null)
    {
        if (obj == null)
        {
            tr.AddAndGetElement("td")
                .AddClass(SerializerOptions.CssClasses.PropertyValue)
                .AddAndGetNull().AddClass(SerializerOptions.CssClasses.Null);

            return;
        }

        var converter = GetConverter(type);
        if (converter == null)
            throw new HtmlSerializationException($"Could not find a {nameof(HtmlConverter)} for type: {type}");

        serializationScope = GetSerializationScope(serializationScope);

        if (serializationScope.Depth > SerializerOptions.MaxDepth)
        {
            return;
        }

        converter.WriteHtmlWithinTableRow(tr, obj, type, serializationScope, this);
    }

    internal static TypeCategory GetTypeCategory(Type type)
    {
        if (_typeCategoryCache.TryGetValue(type, out var category))
            return category;

        if (IsDotNetTypeWithStringRepresentation(type))
        {
            category = TypeCategory.DotNetTypeWithStringRepresentation;
        }
        else if (IsCollectionType(type))
        {
            category = TypeCategory.Collection;
        }
        else
        {
            category = TypeCategory.SingleObject;
        }

        _typeCategoryCache.TryAdd(type, category);
        return category;
    }

    internal HtmlConverter? GetConverter(Type type)
    {
        if (_typeConverterCache.TryGetValue(type, out var match))
            return match;

        foreach (var converter in Converters)
        {
            if (!converter.CanConvert(type)) continue;
            _typeConverterCache.TryAdd(type, converter);
            return converter;
        }

        _typeConverterCache.TryAdd(type, null);
        return null;
    }

    private SerializationScope GetSerializationScope(SerializationScope? serializationScope)
    {
        return serializationScope == null
            ? new SerializationScope(0)
            : new SerializationScope(serializationScope.Depth + 1, serializationScope);
    }


    private static bool ShouldShortCircuit<T>(
        T obj,
        Type type,
        SerializationScope serializationScope,
        HtmlSerializerOptions htmlSerializerOptions,
#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
        out Node? shortCircuitValue)
    {
        shortCircuitValue = null;

        // Check #1: Check Max Depth
        var isSimpleType = GetTypeCategory(type) == TypeCategory.DotNetTypeWithStringRepresentation;

        if ((!isSimpleType && serializationScope.Depth > htmlSerializerOptions.MaxDepth) ||
            // Let's serialize values with string representations at the last depth level
            (isSimpleType && serializationScope.Depth > htmlSerializerOptions.MaxDepth + 1))
        {
            shortCircuitValue = new MaxDepthReached(htmlSerializerOptions.CssClasses.MaxDepthReached);
            return true;
        }

        // Check #2: Check if we have a reference loop
        if (!serializationScope.CheckAlreadySerializedOrAdd(obj))
        {
            return false;
        }

        var referenceLoopHandling = htmlSerializerOptions.ReferenceLoopHandling;

        if (referenceLoopHandling == ReferenceLoopHandling.IgnoreAndSerializeCyclicReference)
        {
            shortCircuitValue = new CyclicReference(type).AddClass(htmlSerializerOptions.CssClasses.CyclicReference);
        }
        else if (referenceLoopHandling == ReferenceLoopHandling.Ignore)
        {
            shortCircuitValue = new Element("div");
        }
        else if (referenceLoopHandling == ReferenceLoopHandling.Error)
        {
            throw new HtmlSerializationException($"A reference loop was detected. Object already serialized: {type.FullName}");
        }

        return shortCircuitValue != null;
    }

    /// <summary>
    /// Gets the readable properties of a type that should be serialized, according to the specified options.
    /// When <paramref name="options"/> is null, the default options (public properties only) are used.
    /// </summary>
    internal static PropertyInfo[] GetReadableProperties(Type type, HtmlSerializerOptions? options = null)
    {
        var flags = GetMemberSelectionFlags(options ?? _defaultHtmlSerializerOptions);
        var key = (type, flags);

        if (_typePropertyCache.TryGetValue(key, out var propertyInfos))
            return propertyInfos;

        var properties = new List<PropertyInfo>();

        if (flags.HasFlag(MemberSelectionFlags.PublicProperties))
        {
            properties.AddRange(
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    // Only include readable properties, and exclude indexer properties
                    .Where(p => p.CanRead && p.GetIndexParameters().Length == 0));
        }

        if (flags.HasFlag(MemberSelectionFlags.NonPublicProperties))
        {
            properties.AddRange(
                type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                    // Only include properties that have a getter which is not public,
                    // so public properties (e.g. with a private setter) are not duplicated
                    .Where(p => p.GetIndexParameters().Length == 0)
                    .Where(p => p.GetGetMethod(true) != null)
                    .Where(p => p.GetGetMethod() == null));
        }

        propertyInfos = properties
            // Exclude properties that exist in base types and are hidden by properties in derived types
            .GroupBy(p => p.Name)
            .Select(g => g.OrderBy(p => p.DeclaringType == type).First())
            .ToArray();

        _typePropertyCache.TryAdd(key, propertyInfos);
        return propertyInfos;
    }

    /// <summary>
    /// Gets the fields of a type that should be serialized, according to the specified options.
    /// When <paramref name="options"/> is null, the default options (public properties only) are used.
    /// Compiler-generated fields (e.g. auto-property backing fields) are always excluded.
    /// </summary>
    internal static FieldInfo[] GetSerializableFields(Type type, HtmlSerializerOptions? options = null)
    {
        var flags = GetMemberSelectionFlags(options ?? _defaultHtmlSerializerOptions);
        var key = (type, flags);

        if (_typeFieldCache.TryGetValue(key, out var fieldInfos))
            return fieldInfos;

        var fields = new List<FieldInfo>();

        if (flags.HasFlag(MemberSelectionFlags.PublicFields))
        {
            fields.AddRange(
                type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => !f.IsDefined(typeof(CompilerGeneratedAttribute), false)));
        }

        if (flags.HasFlag(MemberSelectionFlags.NonPublicFields))
        {
            fields.AddRange(
                type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    // Exclude compiler-generated fields: auto-property backing fields (<X>k__BackingField),
                    // closure fields, etc.
                    .Where(f => !f.IsDefined(typeof(CompilerGeneratedAttribute), false)));
        }

        fieldInfos = fields
            // Exclude fields that exist in base types and are hidden by fields in derived types
            .GroupBy(f => f.Name)
            .Select(g => g.OrderBy(f => f.DeclaringType == type).First())
            .ToArray();

        _typeFieldCache.TryAdd(key, fieldInfos);
        return fieldInfos;
    }

    private static MemberSelectionFlags GetMemberSelectionFlags(HtmlSerializerOptions options)
    {
        var flags = MemberSelectionFlags.None;

        if (options.SerializePublicProperties) flags |= MemberSelectionFlags.PublicProperties;
        if (options.SerializeNonPublicProperties) flags |= MemberSelectionFlags.NonPublicProperties;
        if (options.SerializePublicFields) flags |= MemberSelectionFlags.PublicFields;
        if (options.SerializeNonPublicFields) flags |= MemberSelectionFlags.NonPublicFields;

        return flags;
    }

    public static Type? GetCollectionElementType(Type collectionType)
    {
        if (_collectionElementTypeCache.TryGetValue(collectionType, out var elementType))
            return elementType;

        elementType = collectionType.GetCollectionElementType();

        _collectionElementTypeCache.TryAdd(collectionType, elementType);

        return elementType;
    }

    public static bool IsDotNetTypeWithStringRepresentation(Type type)
    {
        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || typeof(IFormattable).IsAssignableFrom(type)
               || typeof(Exception).IsAssignableFrom(type)
               || typeof(Type).IsAssignableFrom(type)
               || Nullable.GetUnderlyingType(type) != null
               || typeof(XNode).IsAssignableFrom(type)
               || typeof(XmlNode).IsAssignableFrom(type)
            ;
    }

    public static bool IsObjectType(Type type)
    {
        return !IsDotNetTypeWithStringRepresentation(type) && !IsCollectionType(type);
    }

    public static bool IsCollectionType(Type type)
    {
        return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }

    public static HtmlSerializer Create(HtmlSerializerOptions? serializerSettings = null)
    {
        return new HtmlSerializer(serializerSettings);
    }
}

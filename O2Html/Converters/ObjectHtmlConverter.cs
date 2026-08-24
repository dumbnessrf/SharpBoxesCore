using System;
using System.Reflection;
using O2Html.Dom;
using O2Html.Dom.Elements;

namespace O2Html.Converters;

public class ObjectHtmlConverter : HtmlConverter
{
    public override bool CanConvert(Type type)
    {
        return HtmlSerializer.GetTypeCategory(type) == TypeCategory.SingleObject;
    }

    public override Node WriteHtml<T>(T obj, Type type, SerializationScope serializationScope, HtmlSerializer htmlSerializer)
    {
        var table = new Table();

        table.Head
            .AddAndGetRow()
            .AddClass(htmlSerializer.SerializerOptions.CssClasses.TableInfoHeader)
            .AddAndGetElement("th").SetAttribute("colspan", "2")
            .AddEscapedText(type.GetReadableName())
            .SetTitle(type.GetReadableName(true));

        var properties = GetReadableProperties(htmlSerializer, type);
        var fields = GetSerializableFields(htmlSerializer, type);

        foreach (var property in properties)
        {
            AddMemberRow(table, property.Name, GetPropertyValue(property, ref obj!), property.PropertyType, serializationScope, htmlSerializer);
        }

        foreach (var field in fields)
        {
            AddMemberRow(table, field.Name, GetFieldValue(field, obj!), field.FieldType, serializationScope, htmlSerializer);
        }

        return table;
    }

    public override void WriteHtmlWithinTableRow<T>(Element tr, T obj, Type type, SerializationScope serializationScope, HtmlSerializer htmlSerializer)
    {
        var properties = GetReadableProperties(htmlSerializer, type);

        foreach (var property in properties)
        {
            AddMemberValueCell(tr, GetPropertyValue(property, ref obj!), property.PropertyType, serializationScope, htmlSerializer);
        }

        var fields = GetSerializableFields(htmlSerializer, type);

        foreach (var field in fields)
        {
            AddMemberValueCell(tr, GetFieldValue(field, obj!), field.FieldType, serializationScope, htmlSerializer);
        }
    }

    protected virtual PropertyInfo[] GetReadableProperties(HtmlSerializer htmlSerializer, Type type)
    {
        return HtmlSerializer.GetReadableProperties(type, htmlSerializer.SerializerOptions);
    }

    protected virtual FieldInfo[] GetSerializableFields(HtmlSerializer htmlSerializer, Type type)
    {
        return HtmlSerializer.GetSerializableFields(type, htmlSerializer.SerializerOptions);
    }

    private void AddMemberRow(Table table, string name, object? value, Type memberType, SerializationScope serializationScope, HtmlSerializer htmlSerializer)
    {
        var memberTypeOrValueType = value?.GetType() ?? memberType;

        var tr = table.Body.AddAndGetRow();

        // Add member name
        tr.AddAndGetElement("th")
            .AddClass(htmlSerializer.SerializerOptions.CssClasses.PropertyName)
            .SetTitle($"[{memberTypeOrValueType.GetReadableName(true)}] {name}")
            .AddText(name);

        // Add member value
        tr.AddAndGetElement("td")
            .AddClass(htmlSerializer.SerializerOptions.CssClasses.PropertyValue)
            .AddChild(htmlSerializer.Serialize(value, memberTypeOrValueType, serializationScope));
    }

    private void AddMemberValueCell(Element tr, object? value, Type memberType, SerializationScope serializationScope, HtmlSerializer htmlSerializer)
    {
        var memberTypeOrValueType = value?.GetType() ?? memberType;

        tr.AddAndGetElement("td")
            .AddClass(htmlSerializer.SerializerOptions.CssClasses.PropertyValue)
            .AddChild(htmlSerializer.Serialize(value, memberTypeOrValueType, serializationScope));
    }

    private object? GetPropertyValue<T>(PropertyInfo property, ref T? obj)
    {
        try
        {
            return property.GetValue(obj);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private object? GetFieldValue(FieldInfo field, object obj)
    {
        try
        {
            return field.GetValue(obj);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}

using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bartz24.RandoWPF;

public class RowIndexAttribute : Attribute
{
    public int Index { get; set; }

    public RowIndexAttribute(int index)
    {
        Index = index;
    }
}

public enum FieldType
{
    Undefined,
    Int,
    Float,
    FloatFromInt100,
    String,
    ListString,
    ListInt,
    HexInt,
    ItemReq
}

public class FieldTypeOverrideAttribute : Attribute
{
    public FieldType Value { get; set; }

    public FieldTypeOverrideAttribute(FieldType val)
    {
        Value = val;
    }
}

public class CSVDataRow
{
    private static readonly Dictionary<Type, PropertyInfo[]> propertyCache = new();
    public CSVDataRow(string[] row)
    {
        // Use reflection to initialize the properties using the values
        // in the row and the attributes defining the row index for each property
        PropertyInfo[] properties = propertyCache.ContainsKey(GetType()) ? propertyCache[GetType()] : GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            RowIndexAttribute attribute = property.GetCustomAttribute<RowIndexAttribute>();

            if (attribute != null)
            {
                string value = row[attribute.Index];

                FieldType? type = GetFieldType(property);

                ConvertToField(property, value, type);
            }
        }
    }

    private static FieldType? GetFieldType(PropertyInfo property)
    {
        FieldType? type = property.GetCustomAttribute<FieldTypeOverrideAttribute>()?.Value;
        if (type is null or FieldType.Undefined)
        {
            if (property.PropertyType == typeof(int))
            {
                type = (FieldType?)FieldType.Int;
            }
            else if (property.PropertyType == typeof(float))
            {
                type = (FieldType?)FieldType.Float;
            }
            else if (property.PropertyType == typeof(string))
            {
                type = (FieldType?)FieldType.String;
            }
            else if (property.PropertyType == typeof(List<string>))
            {
                type = (FieldType?)FieldType.ListString;
            }
            else if (property.PropertyType == typeof(List<int>))
            {
                type = (FieldType?)FieldType.ListInt;
            }
            else if (property.PropertyType == typeof(ItemReq))
            {
                type = (FieldType?)FieldType.ItemReq;
            }
            else
            {
                throw new Exception("Missing map for property type: " + property.PropertyType);
            }
        }

        return type;
    }

    private void ConvertToField(PropertyInfo property, string value, FieldType? type)
    {
        switch (type)
        {
            case FieldType.Int:
                property.SetValue(this, string.IsNullOrEmpty(value) ? -1 : int.Parse(value));
                break;
            case FieldType.HexInt:
                property.SetValue(this, string.IsNullOrEmpty(value) ? -1 : Convert.ToInt32(value, 16));
                break;
            case FieldType.Float:
                property.SetValue(this, string.IsNullOrEmpty(value) ? -1 : float.Parse(value));
                break;
            case FieldType.FloatFromInt100:
                property.SetValue(this, string.IsNullOrEmpty(value) ? -1 : int.Parse(value) / 100f);
                break;
            case FieldType.String:
                property.SetValue(this, value);
                break;
            case FieldType.ListString:
                List<string> strings = value.Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                property.SetValue(this, strings);
                break;
            case FieldType.ListInt:
                List<int> ints = value.Split("|").Where(s => !string.IsNullOrEmpty(s)).Select(s => int.Parse(s)).ToList();
                property.SetValue(this, ints);
                break;
            case FieldType.ItemReq:
                property.SetValue(this, ItemReq.Parse(value));
                break;
            default:
                throw new Exception("Missing conversion for field type: " + type);
        }
    }
}

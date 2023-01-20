using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF
{
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
        private static Dictionary<Type, PropertyInfo[]> propertyCache = new Dictionary<Type, PropertyInfo[]>();
        public CSVDataRow(string[] row)
        {
            // Use reflection to initialize the properties using the values
            // in the row and the attributes defining the row index for each property
            PropertyInfo[] properties;
            if (propertyCache.ContainsKey(this.GetType()))
            {
                properties = propertyCache[this.GetType()];
            }
            else
            {
                properties = this.GetType().GetProperties();
            }
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<RowIndexAttribute>();

                if (attribute != null)
                {
                    var value = row[attribute.Index];

                    FieldType? type = GetFieldType(property);

                    ConvertToField(property, value, type);
                }
            }
        }

        private static FieldType? GetFieldType(PropertyInfo property)
        {
            FieldType? type = property.GetCustomAttribute<FieldTypeOverrideAttribute>()?.Value;
            if (type == null || type == FieldType.Undefined)
            {
                if (property.PropertyType == typeof(int))
                {
                    type = FieldType.Int;
                }
                else if (property.PropertyType == typeof(float))
                {
                    type = FieldType.Float;
                }
                else if (property.PropertyType == typeof(string))
                {
                    type = FieldType.String;
                }
                else if (property.PropertyType == typeof(List<string>))
                {
                    type = FieldType.ListString;
                }
                else if (property.PropertyType == typeof(ItemReq))
                {
                    type = FieldType.ItemReq;
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
                    var values = value.Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                    property.SetValue(this, values);
                    break;
                case FieldType.ItemReq:
                    property.SetValue(this, ItemReq.Parse(value));
                    break;
                default:
                    throw new Exception("Missing conversion for field type: " + type);
            }
        }
    }
}

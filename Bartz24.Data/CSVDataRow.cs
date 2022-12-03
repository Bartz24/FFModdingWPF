using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.Data
{
    // Define the RowIndexAttribute class
    public class RowIndexAttribute : Attribute
    {
        public int Index { get; set; }

        public RowIndexAttribute(int index)
        {
            Index = index;
        }
    }
    public class CSVDataRow
    {
        public CSVDataRow(string[] row)
        {
            // Use reflection to initialize the properties using the values
            // in the row and the attributes defining the row index for each property
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<RowIndexAttribute>();
                if (attribute != null)
                {
                    var value = row[attribute.Index];
                    if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(this, int.Parse(value));
                    }
                    else if (property.PropertyType == typeof(float))
                    {
                        property.SetValue(this, int.Parse(value) == -1 ? -1 : int.Parse(value) / 100f);
                    }
                    else if (property.PropertyType == typeof(List<string>))
                    {
                        var values = value.Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                        property.SetValue(this, values);
                    }
                    else
                    {
                        property.SetValue(this, value);
                    }
                }
            }
        }
    }
}

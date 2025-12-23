using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOPRando;
public class AreaData : CSVDataRow
{
    [RowIndex(0), FieldTypeOverride(FieldType.HexInt)]
    public int ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }
    public AreaData(string[] row) : base(row)
    {
    }
}

using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF12Rando;

public class PriceData : CSVDataRow
{
    [RowIndex(0)]
    public int ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }

    public PriceData(string[] row) : base(row)
    {
    }
}

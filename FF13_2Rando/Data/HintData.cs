using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;

public class HintData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public List<string> Areas { get; set; }
    public HintData(string[] row) : base(row)
    {
    }
}

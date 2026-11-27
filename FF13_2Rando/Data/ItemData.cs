using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;

public class ItemData : CSVDataRow, IItem
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public ItemClassification Classification { get; set; }
    [RowIndex(2)]
    public string Name { get; set; }
    [RowIndex(3)]
    public string Category { get; set; }
    [RowIndex(4)]
    public int Rank { get; set; }
    [RowIndex(5)]
    public List<string> Traits { get; set; }
    [RowIndex(6)]
    public int OverrideBuyGil { get; set; }
    [RowIndex(7)]
    public int OverrideSellGil { get; set; }
    [RowIndex(8)]
    public int OverrideCount { get; set; }
    public ItemData(string[] row) : base(row)
    {
    }
}

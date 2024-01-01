using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13Rando;

public class ItemData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }
    [RowIndex(2)]
    public string Category { get; set; }
    [RowIndex(3)]
    public int Rank { get; set; }
    [RowIndex(4)]
    public string DefaultShop { get; set; }
    [RowIndex(5)]
    public List<string> Traits { get; set; }
    public int SortIndex { get; set; }
    [RowIndex(6)]
    public int OverrideBuy { get; set; }
    public ItemData(string[] row) : base(row)
    {
    }
}

using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace LRRando;

public class ItemData : CSVDataRow, IItem
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
    public List<string> Traits { get; set; }
    [RowIndex(5)]
    public int OverrideBuyGil { get; set; }
    [RowIndex(6)]
    public int OverrideBuyEP { get; set; }
    public ItemData(string[] row) : base(row)
    {
    }
}
using Bartz24.RandoWPF;

namespace LRRando;

public class AbilityData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public int BasePower { get; set; }
    [RowIndex(2)]
    public int HitMultiplier { get; set; }
    [RowIndex(3)]
    public int ATBCost { get; set; }
    [RowIndex(4)]
    public int MenuIcon { get; set; }
    public AbilityData(string[] row) : base(row)
    {
    }
}

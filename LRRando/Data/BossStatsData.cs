using Bartz24.RandoWPF;

namespace LRRando;

public class BossStatsData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public int Tier { get; set; }
    [RowIndex(2)]
    public int HP { get; set; }
    [RowIndex(3)]
    public int Strength { get; set; }
    [RowIndex(4)]
    public int Magic { get; set; }
    [RowIndex(5)]
    public int Keep { get; set; }
    [RowIndex(6)]
    public int PhysicalRes { get; set; }
    [RowIndex(7)]
    public int MagicRes { get; set; }
    [RowIndex(8)]
    public int BreakPoint { get; set; }
    public BossStatsData(string[] row) : base(row)
    {
    }
}

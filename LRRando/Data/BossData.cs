using Bartz24.RandoWPF;

namespace LRRando;

public class BossData : CSVDataRow
{
    [RowIndex(0)]
    public string Group { get; set; }
    [RowIndex(1)]
    public int Tier { get; set; }
    [RowIndex(2)]
    public string ID { get; set; }
    [RowIndex(3)]
    public string NameID { get; set; }
    [RowIndex(4)]
    public string ScoreID { get; set; }
    [RowIndex(5)]
    public string Name { get; set; }
    public BossData(string[] row) : base(row)
    {
    }
}

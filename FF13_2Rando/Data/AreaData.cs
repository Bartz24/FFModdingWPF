using Bartz24.RandoWPF;

namespace FF13_2Rando;

public class AreaData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }
    [RowIndex(2)]
    public string BattleTableID { get; set; }
    public AreaData(string[] row) : base(row)
    {
    }
}

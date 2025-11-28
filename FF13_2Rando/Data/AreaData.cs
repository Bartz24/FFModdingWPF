using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;

public class AreaData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }
    [RowIndex(2)]
    public string BattleTableID { get; set; }
    [RowIndex(3)]
    public List<string> Traits { get; set; }

    [RowIndex(4)]
    public int FixedBattleRank { get; set; }
    [RowIndex(5)]
    public int OutgoingLinkCount { get; set; }
    public AreaData(string[] row) : base(row)
    {
    }
}

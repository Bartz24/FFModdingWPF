using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;

public class BattleData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }
    [RowIndex(2)]
    public string Location { get; set; }
    [RowIndex(3)]
    public List<string> LocationIDs { get; set; }
    [RowIndex(4)]
    public List<string> Charasets { get; set; }
    [RowIndex(5)]
    public List<string> Traits { get; set; }
    [RowIndex(6)]
    public int CharasetLimit { get; set; }
    public BattleData(string[] row) : base(row)
    {
    }
}

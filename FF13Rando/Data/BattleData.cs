using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13Rando;
public class BattleData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }
    [RowIndex(2)]
    public string Location { get; set; }
    [RowIndex(3)]
    public List<string> Charasets { get; set; }
    [RowIndex(4)]
    public string MissionID { get; set; }
    [RowIndex(5)]
    public List<string> Traits { get; set; }
    public BattleData(string[] row) : base(row)
    {
    }
}

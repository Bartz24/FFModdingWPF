using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13Rando;
public class EnemyData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }
    [RowIndex(2)]
    public List<string> Traits { get; set; }
    [RowIndex(3)]
    public int Rank { get; set; }
    [RowIndex(4)]
    public List<int> LYBForced { get; set; }
    public EnemyData(string[] row) : base(row)
    {
    }
}

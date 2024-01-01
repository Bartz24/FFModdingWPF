using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;

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
    public int Size { get; set; }
    [RowIndex(5)]
    public List<string> Parts { get; set; }

    public EnemyData(string[] row) : base(row)
    {
    }
}

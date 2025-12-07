using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;

public class BossScalingData : CSVDataRow
{
    [RowIndex(0)]
    public int Rank { get; set; }
    [RowIndex(1)]
    public float HP { get; set; }
    [RowIndex(2)]
    public float STRMAG { get; set; }

    public BossScalingData(string[] row) : base(row)
    {
    }
}

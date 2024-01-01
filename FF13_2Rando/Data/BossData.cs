using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;

public class BossData : CSVDataRow
{
    [RowIndex(0)]
    public string Group { get; set; }
    [RowIndex(1)]
    public int Rank { get; set; }
    [RowIndex(2)]
    public string ID { get; set; }
    [RowIndex(3), FieldTypeOverride(FieldType.FloatFromInt100)]
    public float HPMult { get; set; }
    [RowIndex(4), FieldTypeOverride(FieldType.FloatFromInt100)]
    public float STRMult { get; set; }
    [RowIndex(5), FieldTypeOverride(FieldType.FloatFromInt100)]
    public float MAGMult { get; set; }
    [RowIndex(6), FieldTypeOverride(FieldType.FloatFromInt100)]
    public float StaggerPointMult { get; set; }
    [RowIndex(7), FieldTypeOverride(FieldType.FloatFromInt100)]
    public float ChainResMult { get; set; }
    [RowIndex(8), FieldTypeOverride(FieldType.FloatFromInt100)]
    public float CPGilMult { get; set; }
    [RowIndex(9)]
    public List<string> Traits { get; set; }

    public BossData(string[] row) : base(row)
    {
    }
}

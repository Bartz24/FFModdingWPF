using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF12Rando;

public class EnemyData : CSVDataRow
{
    [RowIndex(0)]
    public string Name { get; set; }
    [RowIndex(1), FieldTypeOverride(FieldType.HexInt)]
    public int IntID { get; set; }
    public string ID { get; set; }
    [RowIndex(2)]
    public int Rank { get; set; }
    [RowIndex(3)]
    public string Area { get; set; }
    [RowIndex(4)]
    public int Index { get; set; }
    [RowIndex(5)]
    public int EXPLPScale { get; set; }
    [RowIndex(6)]
    public List<string> Traits { get; set; }
    public EnemyData(string[] row) : base(row)
    {
        ID = row[3] + ":" + row[1] + ":" + row[4];
    }
}

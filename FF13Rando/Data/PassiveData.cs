using Bartz24.RandoWPF;

namespace FF13Rando;

public class PassiveData : CSVDataRow
{
    [RowIndex(0)]
    public string Name { get; set; }
    [RowIndex(1)]
    public string ID { get; set; }
    [RowIndex(2)]
    public float StrengthMult { get; set; }
    [RowIndex(3)]
    public float MagicMult { get; set; }
    [RowIndex(4)]
    public string DisplayNameID { get; set; }
    [RowIndex(5)]
    public string HelpID { get; set; }
    [RowIndex(6)]
    public int StatInitial { get; set; }
    [RowIndex(7)]
    public int StatType1 { get; set; }
    [RowIndex(8)]
    public int StatType2 { get; set; }
    [RowIndex(9)]
    public int MaxValue { get; set; }

    // b and p are used as follows:
    // stat = initial * b ^ (rank - 1) + p * (rank - 1)
    [RowIndex(10)]
    public float RankConstB { get; set; }
    [RowIndex(11)]
    public float RankConstP { get; set; }
    [RowIndex(12)]
    public string Upgrade { get; set; }
    public PassiveData(string[] row) : base(row)
    {
    }
}

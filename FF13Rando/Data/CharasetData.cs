using Bartz24.RandoWPF;

namespace FF13Rando;

public class CharasetData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public int Limit { get; set; }
    public CharasetData(string[] row) : base(row)
    {
    }
}

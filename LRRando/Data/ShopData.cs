using Bartz24.RandoWPF;

namespace LRRando;

public class ShopData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public string Area { get; set; }
    [RowIndex(2)]
    public string SubArea { get; set; }
    [RowIndex(3)]
    public string AdditionalInfo { get; set; }
    [RowIndex(4)]
    public int DayStart { get; set; }
    [RowIndex(5)]
    public int DayEnd { get; set; }
    public ShopData(string[] row) : base(row)
    {
    }
}

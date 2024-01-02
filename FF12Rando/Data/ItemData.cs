using Bartz24.RandoWPF;

namespace FF12Rando;

public class ItemData : CSVDataRow, IItem
{
    [RowIndex(0)]
    public string Name { get; set; }
    [RowIndex(1), FieldTypeOverride(FieldType.HexInt)]
    public int IntID { get; set; }
    [RowIndex(1)]
    public string ID { get; set; }
    [RowIndex(2)]
    public int Rank { get; set; }
    [RowIndex(3), FieldTypeOverride(FieldType.HexInt)]
    public int IntUpgrade { get; set; }
    [RowIndex(3)]
    public string Upgrade { get; set; }
    [RowIndex(4)]
    public string Category { get; set; }
    public ItemData(string[] row) : base(row)
    {
    }
}

using Bartz24.RandoWPF;

namespace LRRando;

public class HintData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }
    [RowIndex(2)]
    public ItemReq Requirements { get; set; }
    public HintData(string[] row) : base(row)
    {
    }
}

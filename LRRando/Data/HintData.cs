using Bartz24.RandoWPF;

namespace LRRando;

public class HintData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }
    [RowIndex(2)]
    public string Area { get; set; }
    [RowIndex(3)]
    public ItemReq Requirements { get; set; }

    // Used to link the fake hint rewards in the treasure rando
    public string FakeLocationLink { get; set; }
    public HintData(string[] row) : base(row)
    {
    }
}

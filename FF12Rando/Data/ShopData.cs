using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF12Rando;

public class ShopData : CSVDataRow
{
    [RowIndex(0)]
    public string Name { get; set; }
    [RowIndex(1)]
    public int ID { get; set; }
    [RowIndex(2)]
    public string Area { get; set; }
    [RowIndex(3)]
    public List<string> Traits { get; set; }
    [RowIndex(4)]
    public ItemReq Requirements { get; set; }

    // Used to link the fake shop rewards in the treasure rando
    public string ShopFakeLocationLink { get; set; }
    public ShopData(string[] row) : base(row)
    {
    }
}

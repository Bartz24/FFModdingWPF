using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;

public class AbilityData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }
    [RowIndex(2)]
    public string Role { get; set; }
    [RowIndex(3)]
    public List<string> Traits { get; set; }
    [RowIndex(4)]
    public ItemReq Requirements { get; set; }
    public AbilityData(string[] row) : base(row)
    {
    }
}

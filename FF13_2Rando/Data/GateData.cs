using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;

public class GateData : CSVDataRow
{
    [RowIndex(0)]
    public string Location { get; set; }
    [RowIndex(1)]
    public string ID { get; set; }
    [RowIndex(2)]
    public List<string> Traits { get; set; }
    [RowIndex(3)]
    public List<string> Requirements { get; set; }
    [RowIndex(4)]
    public int MinMogLevel { get; set; }
    [RowIndex(5)]
    public string GateOriginal { get; set; }
    [RowIndex(6)]
    public ItemReq ItemRequirements { get; set; }
    public GateData(string[] row) : base(row)
    {
    }
}

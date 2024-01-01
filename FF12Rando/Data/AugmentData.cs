using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF12Rando;

public class AugmentData : CSVDataRow
{
    [RowIndex(0)]
    public string Name { get; set; }
    [RowIndex(1)]
    public int IntID { get; set; }
    [RowIndex(1)]
    public string ID { get; set; }
    [RowIndex(2)]
    public string Description { get; set; }
    [RowIndex(3)]
    public List<string> Traits { get; set; }
    public AugmentData(string[] row) : base(row)
    {
    }
}

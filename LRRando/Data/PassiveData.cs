using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace LRRando;

public class PassiveData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }

    [RowIndex(1)]
    public string Name { get; set; }

    [RowIndex(2)]
    public List<string> UpgradeInto { get; set; }
    public PassiveData(string[] row) : base(row)
    {
    }
}
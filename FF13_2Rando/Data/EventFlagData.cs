using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;


public class EventFlagData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    public EventFlagData(string[] row) : base(row)
    {

    }
}


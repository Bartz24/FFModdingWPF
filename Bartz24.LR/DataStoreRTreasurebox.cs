using Bartz24.FF13_2_LR;
using System.Collections.Generic;

namespace Bartz24.LR;

public class DataStoreRTreasurebox : DataStoreDB3SubEntry
{
    public int iItemCount { get; set; }
    public int s11ItemResourceId_pointer { get; set; }
    public string s11ItemResourceId_string { get; set; }
    public int s10NextTreasureBoxResourceId_pointer { get; set; }
    public string s10NextTreasureBoxResourceId_string { get; set; }

    public override Dictionary<string, int> GetStringArrayMapping()
    {
        Dictionary<string, int> mapping = new()
        {
            { nameof(s11ItemResourceId_pointer), 0 },
            { nameof(s10NextTreasureBoxResourceId_pointer), 1 }
        };
        return mapping;
    }
}

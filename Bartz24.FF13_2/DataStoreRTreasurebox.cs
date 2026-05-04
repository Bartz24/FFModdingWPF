using Bartz24.FF13_2_LR;

namespace Bartz24.FF13_2;

public class DataStoreRTreasurebox : DataStoreWDBEntry
{
    public int iItemCount { get; set; }
    public string s11ItemResourceId { get; set; }
    public string s8NextTreasureBoxResourceId { get; set; }
    public int i2Live { get; set; }
}

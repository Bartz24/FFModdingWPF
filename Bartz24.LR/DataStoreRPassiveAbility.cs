using Bartz24.FF13_2_LR;

namespace Bartz24.LR;

public class DataStoreRPassiveAbility : DataStoreWDBEntry
{
    public string sStringResId { get; set; }
    public string sInfoStResId { get; set; }
        public int u8StatusModKind0 { get; set; }
    public int u8StatusModKind1 { get; set; }
    public int u4StatusModType { get; set; }
}

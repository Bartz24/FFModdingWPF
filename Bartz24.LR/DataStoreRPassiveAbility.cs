using Bartz24.FF13_2_LR;

namespace Bartz24.LR
{
    public class DataStoreRPassiveAbility : DataStoreDB3SubEntry
    {
        public int sStringResId_pointer { get; set; }
        public string sStringResId_string { get; set; }
        public int sInfoStResId_pointer { get; set; }
        public string sInfoStResId_string { get; set; }
        public int u8StatusModKind0 { get; set; }
        public int u8StatusModKind1 { get; set; }
        public int u4StatusModType { get; set; }
    }
}

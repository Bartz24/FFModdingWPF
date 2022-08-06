using Bartz24.FF13_2_LR;

namespace Bartz24.LR
{
    public class DataStoreBtAutoAbility : DataStoreDB3SubEntry
    {
        public int sStringResId_pointer { get; set; }
        public string sStringResId_string { get; set; }
        public int sInfoStResId_pointer { get; set; }
        public string sInfoStResId_string { get; set; }
        public int sScriptId_pointer { get; set; }
        public string sScriptId_string { get; set; }
        public int sAutoAblArgStr0_pointer { get; set; }
        public string sAutoAblArgStr0_string { get; set; }
        public int sAutoAblArgStr1_pointer { get; set; }
        public string sAutoAblArgStr1_string { get; set; }
        public int u1RsvFlag0 { get; set; }
        public int u1RsvFlag1 { get; set; }
        public int u1RsvFlag2 { get; set; }
        public int u1RsvFlag3 { get; set; }
        public int i16MenuSortNo { get; set; }
        public int u9AutoAblKind { get; set; }
        public int i16ScriptArg0 { get; set; }
        public int i16ScriptArg1 { get; set; }
        public int i16AutoAblArgInt0 { get; set; }
        public int i16AutoAblArgInt1 { get; set; }
    }
}

using Bartz24.FF13_2_LR;

namespace Bartz24.LR;

public class DataStoreBtAutoAbility : DataStoreWDBEntry
{
    public string sStringResId { get; set; }
    public string sInfoStResId { get; set; }
    public string sScriptId { get; set; }
    public string sAutoAblArgStr0 { get; set; }
    public string sAutoAblArgStr1 { get; set; }
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

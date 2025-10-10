using Bartz24.FF13_2_LR;

namespace Bartz24.FF13_2;

public class DataStoreItem : DataStoreWDBEntry
{
    public string sItemNameStringId { get; set; }
    public string sHelpStringId { get; set; }
    public string sScriptId { get; set; }
    public int uPurchasePrice { get; set; }
    public int uSellPrice { get; set; }
    public string sRequiredItem { get; set; }
    public int u8MenuIcon { get; set; }
    public int u8ItemCategory { get; set; }
    public int i16ScriptArg0 { get; set; }
    public int i16ScriptArg1 { get; set; }
    public int u1IsUseBattleMenu { get; set; }
    public int u1IsUseMenu { get; set; }
    public int u1IsDisposable { get; set; }
    public int u1IsSellable { get; set; }
    public int u1OnlyOne { get; set; }
    public int u5Rank { get; set; }
    public int u6Genre { get; set; }
    public int u16SortAllByKCategory { get; set; }
    public int u16SortCategoryByCategory { get; set; }
    public int u16Experience { get; set; }
    public int u1IsIgnoreGenre { get; set; }
    public int i8Noise { get; set; }
    public int u1IsUseItemChange { get; set; }
    public int u2BonusParam { get; set; }
}

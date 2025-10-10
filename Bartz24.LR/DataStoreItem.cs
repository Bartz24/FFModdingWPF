using Bartz24.FF13_2_LR;

namespace Bartz24.LR;

public class DataStoreItem : DataStoreWDBEntry
{
    public string sItemNameStringId { get; set; }
    public string sHelpStringId { get; set; }
    public string sScriptId { get; set; }
    public int uGpCost { get; set; }
    public int uPurchasePrice { get; set; }
    public int uSellPrice { get; set; }
    public int uItemNum { get; set; }
    public string sRequiredItem { get; set; }
    public string sNextItem { get; set; }
        public int iNextItemCount { get; set; }
    public int u8MenuIcon { get; set; }
    public int u8ItemCategory { get; set; }
    public int u1IsUseBattleMenu { get; set; }
    public int u1IsSellable { get; set; }
    public int u1OnlyOne { get; set; }
    public int u1IsTargetFill { get; set; }
    public int u1IsPresentable { get; set; }
    public int u1IsPermanent { get; set; }
    public int u16SortAllByKCategory { get; set; }
    public int u16SortCategoryByCategory { get; set; }
    public int u16SortCategoryByGraphics { get; set; }
    public int u16Padding { get; set; }
}

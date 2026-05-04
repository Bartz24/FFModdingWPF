using Bartz24.FF13_2_LR;

namespace Bartz24.LR;

public class DataStoreRBtUpgrade : DataStoreWDBEntry
{
    public string sNextId { get; set; }
    public string sPhyAtkItemId { get; set; }
    public int uPhyAtkGil { get; set; }
    public string sMagAtkItemId { get; set; }
    public int uMagAtkGil { get; set; }
    public string sBrkBonusItemId { get; set; }
    public int uBrkBonusGil { get; set; }
    public string sMaxHpItemId { get; set; }
    public int uMaxHpGil { get; set; }
    public string sAtbSpdItemId { get; set; }
    public int uAtbSpdGil { get; set; }
    public string sGuardItemId { get; set; }
    public int uGuardGil { get; set; }
    public string sAbi1Id { get; set; }
    public string sAbi1ItemId { get; set; }
    public int uAbi1Gil { get; set; }
    public string sAbi2Id { get; set; }
    public string sAbi2ItemId { get; set; }
    public int uAbi2Gil { get; set; }
    public int u2Rank { get; set; }
    public int i16PhyAtkLimit { get; set; }
    public int u8PhyAtkItemCount { get; set; }
    public int i16MagAtkLimit { get; set; }
    public int u8MagAtkItemCount { get; set; }
    public int u8BrkBonusItemCount { get; set; }
    public int i16BrkBonusLimit { get; set; }
    public int i16MaxHpLimit { get; set; }
    public int u8MaxHpItemCount { get; set; }
    public int i16AtbSpdLimit { get; set; }
    public int u8AtbSpdItemCount { get; set; }
    public int i16GuardLimit { get; set; }
    public int u8GuardItemCount { get; set; }
    public int u8Abi1ItemCount { get; set; }
    public int i16Abi1Limit { get; set; }
    public int i16Abi2Limit { get; set; }
    public int u8Abi2ItemCount { get; set; }
}

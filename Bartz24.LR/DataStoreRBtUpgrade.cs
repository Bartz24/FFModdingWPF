using Bartz24.FF13_2_LR;

namespace Bartz24.LR;

public class DataStoreRBtUpgrade : DataStoreDB3SubEntry
{
    public int sNextId_pointer { get; set; }
    public string sNextId_string { get; set; }
    public int sPhyAtkItemId_pointer { get; set; }
    public string sPhyAtkItemId_string { get; set; }
    public int uPhyAtkGil { get; set; }
    public int sMagAtkItemId_pointer { get; set; }
    public string sMagAtkItemId_string { get; set; }
    public int uMagAtkGil { get; set; }
    public int sBrkBonusItemId_pointer { get; set; }
    public string sBrkBonusItemId_string { get; set; }
    public int uBrkBonusGil { get; set; }
    public int sMaxHpItemId_pointer { get; set; }
    public string sMaxHpItemId_string { get; set; }
    public int uMaxHpGil { get; set; }
    public int sAtbSpdItemId_pointer { get; set; }
    public string sAtbSpdItemId_string { get; set; }
    public int uAtbSpdGil { get; set; }
    public int sGuardItemId_pointer { get; set; }
    public string sGuardItemId_string { get; set; }
    public int uGuardGil { get; set; }
    public int sAbi1Id_pointer { get; set; }
    public string sAbi1Id_string { get; set; }
    public int sAbi1ItemId_pointer { get; set; }
    public string sAbi1ItemId_string { get; set; }
    public int uAbi1Gil { get; set; }
    public int sAbi2Id_pointer { get; set; }
    public string sAbi2Id_string { get; set; }
    public int sAbi2ItemId_pointer { get; set; }
    public string sAbi2ItemId_string { get; set; }
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

using Bartz24.FF13_2_LR;

namespace Bartz24.LR;

public class DataStoreItemWeapon : DataStoreWDBEntry
{
    public string sWeaponCharaSpecId { get; set; }
    public string sWeaponCharaSpecId2 { get; set; }
    public string sAbility { get; set; }
    public string sAbility2 { get; set; }
    public string sAbility3 { get; set; }
    public string sAbilityName { get; set; }
    public string sOtherItemId { get; set; }
    public string sDefStyleName { get; set; }
    public string sCosAbilityCir { get; set; }
    public string sCosAbilityCro { get; set; }
    public string sCosAbilityTri { get; set; }
    public string sCosAbilitySqu { get; set; }
        public int iBreakBonus { get; set; }
    public int iGuardModVal { get; set; }
    public string sNextItemId { get; set; }
    public string sUpgradeId { get; set; }
        public int iRankupGil { get; set; }
    public string sRankupItem1 { get; set; }
    public string sRankupItem2 { get; set; }
    public string sRankupItem3 { get; set; }
    public float fBreakRate0 { get; set; }
    public float fBreakRate1 { get; set; }
    public int u4WeaponKind { get; set; }
    public int u4AccessoryPos { get; set; }
    public int u8StatusModKind0 { get; set; }
    public int u8StatusModKind1 { get; set; }
    public int u4StatusModType { get; set; }
    public int u2Rank { get; set; }
    public int u1Ability1Open { get; set; }
    public int u1Ability2Open { get; set; }
    public int i16StatusModVal { get; set; }
    public int i16AtbModVal { get; set; }
    public int i16AtbStartModVal { get; set; }
    public int i16AttackModVal { get; set; }
    public int i16MagicModVal { get; set; }
    public int i16HpModVal { get; set; }
    public int i16AtbSpeedModVal { get; set; }
    public int u16UpgradeLimit { get; set; }
    public int u8RankupItem1Count { get; set; }
    public int u8RankupItem2Count { get; set; }
    public int u8RankupItem3Count { get; set; }
    public int u6BreakAttr0 { get; set; }
    public int u6BreakAttr1 { get; set; }
    public int u8Weight { get; set; }
}

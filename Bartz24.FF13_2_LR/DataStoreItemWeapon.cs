using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF13_2_LR
{
	public class DataStoreItemWeapon : DataStoreDB3SubEntry
	{
		public int sWeaponCharaSpecId_pointer { get; set; }
		public string sWeaponCharaSpecId_string { get; set; }
		public int sWeaponCharaSpecId2_pointer { get; set; }
		public string sWeaponCharaSpecId2_string { get; set; }
		public int sAbility_pointer { get; set; }
		public string sAbility_string { get; set; }
		public int sAbility2_pointer { get; set; }
		public string sAbility2_string { get; set; }
		public int sAbility3_pointer { get; set; }
		public string sAbility3_string { get; set; }
		public int sAbilityName_pointer { get; set; }
		public string sAbilityName_string { get; set; }
		public int sOtherItemId_pointer { get; set; }
		public string sOtherItemId_string { get; set; }
		public int sDefStyleName_pointer { get; set; }
		public string sDefStyleName_string { get; set; }
		public int sCosAbilityCir_pointer { get; set; }
		public string sCosAbilityCir_string { get; set; }
		public int sCosAbilityCro_pointer { get; set; }
		public string sCosAbilityCro_string { get; set; }
		public int sCosAbilityTri_pointer { get; set; }
		public string sCosAbilityTri_string { get; set; }
		public int sCosAbilitySqu_pointer { get; set; }
		public string sCosAbilitySqu_string { get; set; }
		public int iBreakBonus { get; set; }
		public int iGuardModVal { get; set; }
		public int sNextItemId_pointer { get; set; }
		public string sNextItemId_string { get; set; }
		public int sUpgradeId_pointer { get; set; }
		public string sUpgradeId_string { get; set; }
		public int iRankupGil { get; set; }
		public int sRankupItem1_pointer { get; set; }
		public string sRankupItem1_string { get; set; }
		public int sRankupItem2_pointer { get; set; }
		public string sRankupItem2_string { get; set; }
		public int sRankupItem3_pointer { get; set; }
		public string sRankupItem3_string { get; set; }
		public int fBreakRate0 { get; set; }
		public int fBreakRate1 { get; set; }
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
}

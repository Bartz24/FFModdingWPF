﻿using Bartz24.FF13_2_LR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF13_2
{
	public class DataStoreBtCharaSpec : DataStoreDB3SubEntry
	{
		public int sCharaSpec_pointer { get; set; }
		public string sCharaSpec_string { get; set; }
		public int sBaseBtSpec_pointer { get; set; }
		public string sBaseBtSpec_string { get; set; }
		public int sNameStrResID_pointer { get; set; }
		public string sNameStrResID_string { get; set; }
		public int sNameStrRsv0_pointer { get; set; }
		public string sNameStrRsv0_string { get; set; }
		public int sWandIdNag_pointer { get; set; }
		public string sWandIdNag_string { get; set; }
		public int sWandIdAg_pointer { get; set; }
		public string sWandIdAg_string { get; set; }
		public int fAtkLen { get; set; }
		public int sScriptId_pointer { get; set; }
		public string sScriptId_string { get; set; }
		public int sAiSheetName_pointer { get; set; }
		public string sAiSheetName_string { get; set; }
		public int sAbility0_pointer { get; set; }
		public string sAbility0_string { get; set; }
		public int sAbility1_pointer { get; set; }
		public string sAbility1_string { get; set; }
		public int sAbility2_pointer { get; set; }
		public string sAbility2_string { get; set; }
		public int sAbility3_pointer { get; set; }
		public string sAbility3_string { get; set; }
		public int sAbility4_pointer { get; set; }
		public string sAbility4_string { get; set; }
		public int sAbility5_pointer { get; set; }
		public string sAbility5_string { get; set; }
		public int sAbility6_pointer { get; set; }
		public string sAbility6_string { get; set; }
		public int sAbility7_pointer { get; set; }
		public string sAbility7_string { get; set; }
		public int sAbility8_pointer { get; set; }
		public string sAbility8_string { get; set; }
		public int sAbility9_pointer { get; set; }
		public string sAbility9_string { get; set; }
		public int sAbility10_pointer { get; set; }
		public string sAbility10_string { get; set; }
		public int sAbility11_pointer { get; set; }
		public string sAbility11_string { get; set; }
		public int sAbility12_pointer { get; set; }
		public string sAbility12_string { get; set; }
		public int sAbility13_pointer { get; set; }
		public string sAbility13_string { get; set; }
		public int sAbility14_pointer { get; set; }
		public string sAbility14_string { get; set; }
		public int sAbility15_pointer { get; set; }
		public string sAbility15_string { get; set; }
		public int fFReserve0 { get; set; }
		public int fFReserve1 { get; set; }
		public int fFReserve2 { get; set; }
		public int fFReserve3 { get; set; }
		public int sEffId0_pointer { get; set; }
		public string sEffId0_string { get; set; }
		public int iEffArg0 { get; set; }
		public int sSndId0_pointer { get; set; }
		public string sSndId0_string { get; set; }
		public int sEffId1_pointer { get; set; }
		public string sEffId1_string { get; set; }
		public int iEffArg1 { get; set; }
		public int sSndId1_pointer { get; set; }
		public string sSndId1_string { get; set; }
		public int sPartCharSpec0_pointer { get; set; }
		public string sPartCharSpec0_string { get; set; }
		public int sPartCharSpec1_pointer { get; set; }
		public string sPartCharSpec1_string { get; set; }
		public int sPartCharSpec2_pointer { get; set; }
		public string sPartCharSpec2_string { get; set; }
		public int sPartCharSpec3_pointer { get; set; }
		public string sPartCharSpec3_string { get; set; }
		public int sPartCharSpec4_pointer { get; set; }
		public string sPartCharSpec4_string { get; set; }
		public int sPartCharSpec5_pointer { get; set; }
		public string sPartCharSpec5_string { get; set; }
		public int sPartCharSpec6_pointer { get; set; }
		public string sPartCharSpec6_string { get; set; }
		public int sPartCharSpec7_pointer { get; set; }
		public string sPartCharSpec7_string { get; set; }
		public int sMogClockId_pointer { get; set; }
		public string sMogClockId_string { get; set; }
		public int u6PcKind { get; set; }
		public int u6RoleStyle { get; set; }
		public int u8Rank { get; set; }
		public int u1NoHide { get; set; }
		public int u1FixPos { get; set; }
		public int u1FixFinArts { get; set; }
		public int u1NoRdrToPa { get; set; }
		public int u1NoHitBack { get; set; }
		public int u1FlagRsv0 { get; set; }
		public int u1FlagRsv1 { get; set; }
		public int u1FlagRsv2 { get; set; }
		public int u1FlagRsv3 { get; set; }
		public int u1FlagRsv4 { get; set; }
		public int u1FlagRsv5 { get; set; }
		public int u1FlagRsv6 { get; set; }
		public int u1FlagRsv7 { get; set; }
		public int u1FlagRsv8 { get; set; }
		public int u1FlagRsv9 { get; set; }
		public int u1FlagRsv10 { get; set; }
		public int u24MaxHp { get; set; }
		public int u1NoBlast { get; set; }
		public int u1NoSlam { get; set; }
		public int u1CamBigEnemy { get; set; }
		public int i1Reserve5 { get; set; }
		public int u12MaxAtb { get; set; }
		public int u16AgCount { get; set; }
		public int u4AiArg { get; set; }
		public int u16AgCountRnd { get; set; }
		public int u16AtbInit { get; set; }
		public int u16AtbInitRnd { get; set; }
		public int u16StatusStr { get; set; }
		public int u16StatusMgk { get; set; }
		public int u16StatusDef { get; set; }
		public int u16StatusMgDef { get; set; }
		public int i16StEfEndRecSpd { get; set; }
		public int i8StEfEndCoef { get; set; }
		public int u12KeepVal { get; set; }
		public int i8RegBlast { get; set; }
		public int u4Race { get; set; }
		public int i8RegBlastBr { get; set; }
		public int i8RegSlam { get; set; }
		public int i8RegSlamBr { get; set; }
		public int i8AiParam0 { get; set; }
		public int u12BrChainBonus { get; set; }
		public int u12MaxBp { get; set; }
		public int i8AiParam1 { get; set; }
		public int i16SynchroUpProp { get; set; }
		public int u8IdleRndCoef { get; set; }
		public int u4ElemDefEx0 { get; set; }
		public int u4ElemDefEx1 { get; set; }
		public int i16CamLimRotDeg { get; set; }
		public int u4ElemDefEx2 { get; set; }
		public int u4ElemDefEx3 { get; set; }
		public int u4ElemDefEx4 { get; set; }
		public int u4ElemDefEx5 { get; set; }
		public int u4ElemDefEx6 { get; set; }
		public int u4ElemDefEx7 { get; set; }
		public int u8StatusDef0 { get; set; }
		public int u8StatusDef1 { get; set; }
		public int u8StatusDef2 { get; set; }
		public int u8StatusDef3 { get; set; }
		public int u8StatusDef4 { get; set; }
		public int u8StatusDef5 { get; set; }
		public int u8StatusDef6 { get; set; }
		public int u8StatusDef7 { get; set; }
		public int u8StatusDef8 { get; set; }
		public int u8StatusDef9 { get; set; }
		public int u8StatusDef10 { get; set; }
		public int u8StatusDef11 { get; set; }
		public int u8StatusDef12 { get; set; }
		public int u8StatusDef13 { get; set; }
		public int u8StatusDef14 { get; set; }
		public int u8StatusDef15 { get; set; }
		public int u8StatusDef16 { get; set; }
		public int u8StatusDef17 { get; set; }
		public int u8StatusDef18 { get; set; }
		public int u8StatusDef19 { get; set; }
		public int u8StatusDef20 { get; set; }
		public int u8StatusDef21 { get; set; }
		public int u8StatusDef22 { get; set; }
		public int u8StatusDef23 { get; set; }
		public int u8StatusDef24 { get; set; }
		public int u8StatusDef25 { get; set; }
		public int u8StatusDef26 { get; set; }
		public int u8StatusDef27 { get; set; }
		public int u8StatusDef28 { get; set; }
		public int u8StatusDef29 { get; set; }
		public int u8StatusDef30 { get; set; }
		public int u8StatusDef31 { get; set; }
		public int u10Prop0 { get; set; }
		public int u10Prop1 { get; set; }
		public int u4PartCharKind { get; set; }
		public int u10Prop2 { get; set; }
		public int u10Prop3 { get; set; }
		public int u10Prop4 { get; set; }
		public int u1NoInitPart { get; set; }
		public int i16Reserve0 { get; set; }
		public int i16Reserve1 { get; set; }
		public int i16Reserve2 { get; set; }
		public int u12Reserve3 { get; set; }
		public int u6Reserve4 { get; set; }
		public int u14NameElemId { get; set; }
		public int s10DropItem0_pointer { get; set; }
		public string s10DropItem0_string { get; set; }
		public int u14ElemIdRsv0 { get; set; }
		public int u14ElemIdRsv1 { get; set; }
		public int u14EffPos0 { get; set; }
		public int u14EffPos1 { get; set; }
		public int u14TgElemId0 { get; set; }
		public int u8NumDrop0 { get; set; }
		public int s10DropItem1_pointer { get; set; }
		public string s10DropItem1_string { get; set; }
		public int u24AbilityPoint { get; set; }
		public int u8NumDrop1 { get; set; }
		public int u14DropProb0 { get; set; }
		public int u14DropProb1 { get; set; }
		public int s10DropItem2_pointer { get; set; }
		public string s10DropItem2_string { get; set; }
		public int u8NumDrop2 { get; set; }
		public int u14DropProb2 { get; set; }
		public int u16DropGil { get; set; }
		public int u16ResultTime { get; set; }
		public override Dictionary<string, int> GetStringArrayMapping()
		{
			Dictionary<string, int> mapping = new Dictionary<string, int>();
			mapping.Add(nameof(s10DropItem0_pointer), 0);
			mapping.Add(nameof(s10DropItem1_pointer), 1);
			mapping.Add(nameof(s10DropItem2_pointer), 2);
			return mapping;
		}
	}
}
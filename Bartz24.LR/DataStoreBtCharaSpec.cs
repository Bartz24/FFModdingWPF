using Bartz24.FF13_2_LR;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.LR
{
    public class DataStoreBtCharaSpec : DataStoreDB3SubEntry
    {
        public int sCharaSpec_pointer { get; set; }
        public string sCharaSpec_string { get; set; }
        public int sBaseBtSpec_pointer { get; set; }
        public string sBaseBtSpec_string { get; set; }
        public int sNameStrResID_pointer { get; set; }
        public string sNameStrResID_string { get; set; }
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
        public int fFReserve0 { get; set; }
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
        public int sDropCndItem0_pointer { get; set; }
        public string sDropCndItem0_string { get; set; }
        public int sDropCndItem1_pointer { get; set; }
        public string sDropCndItem1_string { get; set; }
        public int sDropCndItem2_pointer { get; set; }
        public string sDropCndItem2_string { get; set; }
        public int sBrkDefState_pointer { get; set; }
        public string sBrkDefState_string { get; set; }
        public int fBrkDefDecVal { get; set; }
        public int sBrkState0_pointer { get; set; }
        public string sBrkState0_string { get; set; }
        public int fBrkLoopTime0 { get; set; }
        public int fBrkDecVal0 { get; set; }
        public int fBrkCoolTime0 { get; set; }
        public int fBrkGrgTime0 { get; set; }
        public int sBrkState1_pointer { get; set; }
        public string sBrkState1_string { get; set; }
        public int fBrkLoopTime1 { get; set; }
        public int fBrkDecVal1 { get; set; }
        public int fBrkCoolTime1 { get; set; }
        public int fBrkGrgTime1 { get; set; }
        public int sBrkState2_pointer { get; set; }
        public string sBrkState2_string { get; set; }
        public int fBrkLoopTime2 { get; set; }
        public int fBrkDecVal2 { get; set; }
        public int fBrkCoolTime2 { get; set; }
        public int fBrkGrgTime2 { get; set; }
        public int sBrkState3_pointer { get; set; }
        public string sBrkState3_string { get; set; }
        public int fBrkLoopTime3 { get; set; }
        public int fBrkDecVal3 { get; set; }
        public int fBrkCoolTime3 { get; set; }
        public int fBrkGrgTime3 { get; set; }
        public int sBrkState4_pointer { get; set; }
        public string sBrkState4_string { get; set; }
        public int fBrkLoopTime4 { get; set; }
        public int fBrkDecVal4 { get; set; }
        public int fBrkCoolTime4 { get; set; }
        public int fBrkGrgTime4 { get; set; }
        public int u6PcKind { get; set; }
        public int u8Rank { get; set; }
        public int u1NoHide { get; set; }
        public int u1FixPos { get; set; }
        public int u1NoRdrToPa { get; set; }
        public int u1NoHitBack { get; set; }
        public int u1FlagRsv0 { get; set; }
        public int u1FlagRsv1 { get; set; }
        public int u1FlagRsv2 { get; set; }
        public int u1FlagRsv3 { get; set; }
        public int u1FlagRsv4 { get; set; }
        public int u1FlagRsv5 { get; set; }
        public int u1FlagRsv6 { get; set; }
        public int u1FlagRsv8 { get; set; }
        public int u1FlagRsv10 { get; set; }
        public int u1NoBlast { get; set; }
        public int u1NoSlam { get; set; }
        public int u1CamBigEnemy { get; set; }
        public int u1NoInitPart { get; set; }
        public int u1DropField { get; set; }
        public int u24MaxHp { get; set; }
        public int i8StEfEndCoef { get; set; }
        public int u12MaxAtb { get; set; }
        public int u16AtbInit { get; set; }
        public int u4AiArg { get; set; }
        public int u16AtbInitRnd { get; set; }
        public int u16StatusStr { get; set; }
        public int u16StatusMgk { get; set; }
        public int i16StEfEndRecSpd { get; set; }
        public int u12KeepVal { get; set; }
        public int i8AiParam0 { get; set; }
        public int i8AiParam1 { get; set; }
        public int u4Race { get; set; }
        public int u8IdleRndCoef { get; set; }
        public int i16CamLimRotDeg { get; set; }
        public int u8StatusDef0 { get; set; }
        public int i10ElemDefExVal0 { get; set; }
        public int i10ElemDefExVal1 { get; set; }
        public int i10ElemDefExVal2 { get; set; }
        public int u1GainGpNoDir { get; set; }
        public int u1DailyAdjust { get; set; }
        public int i10ElemDefExVal3 { get; set; }
        public int i10ElemDefExVal4 { get; set; }
        public int i10ElemDefExVal6 { get; set; }
        public int u1BrkActTiming0 { get; set; }
        public int u1BrkEnbFly0 { get; set; }
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
        public int s8Ability16_pointer { get; set; }
        public string s8Ability16_string { get; set; }
        public int u10Prop0 { get; set; }
        public int u10Prop1 { get; set; }
        public int u10Prop2 { get; set; }
        public int u1BrkValReset0 { get; set; }
        public int u1BrkActTiming1 { get; set; }
        public int u10Prop3 { get; set; }
        public int u10Prop4 { get; set; }
        public int s12Ability8_pointer { get; set; }
        public string s12Ability8_string { get; set; }
        public int s12Ability9_pointer { get; set; }
        public string s12Ability9_string { get; set; }
        public int s12Ability10_pointer { get; set; }
        public string s12Ability10_string { get; set; }
        public int s8Ability17_pointer { get; set; }
        public string s8Ability17_string { get; set; }
        public int s12Ability11_pointer { get; set; }
        public string s12Ability11_string { get; set; }
        public int s12Ability12_pointer { get; set; }
        public string s12Ability12_string { get; set; }
        public int s8Ability18_pointer { get; set; }
        public string s8Ability18_string { get; set; }
        public int s12Ability13_pointer { get; set; }
        public string s12Ability13_string { get; set; }
        public int s12Ability14_pointer { get; set; }
        public string s12Ability14_string { get; set; }
        public int s8Ability19_pointer { get; set; }
        public string s8Ability19_string { get; set; }
        public int s12Ability15_pointer { get; set; }
        public string s12Ability15_string { get; set; }
        public int s8Ability20_pointer { get; set; }
        public string s8Ability20_string { get; set; }
        public int s8Ability21_pointer { get; set; }
        public string s8Ability21_string { get; set; }
        public int u4PartCharKind { get; set; }
        public int s8Ability22_pointer { get; set; }
        public string s8Ability22_string { get; set; }
        public int s8Ability23_pointer { get; set; }
        public string s8Ability23_string { get; set; }
        public int s8Ability24_pointer { get; set; }
        public string s8Ability24_string { get; set; }
        public int s8Ability25_pointer { get; set; }
        public string s8Ability25_string { get; set; }
        public int s8Ability26_pointer { get; set; }
        public string s8Ability26_string { get; set; }
        public int s8Ability27_pointer { get; set; }
        public string s8Ability27_string { get; set; }
        public int s8Ability28_pointer { get; set; }
        public string s8Ability28_string { get; set; }
        public int s8Ability29_pointer { get; set; }
        public string s8Ability29_string { get; set; }
        public int s8Ability30_pointer { get; set; }
        public string s8Ability30_string { get; set; }
        public int s8Ability31_pointer { get; set; }
        public string s8Ability31_string { get; set; }
        public int u6Reserve4 { get; set; }
        public int s8PartCharSpec2_pointer { get; set; }
        public string s8PartCharSpec2_string { get; set; }
        public int u1BrkEnbFly1 { get; set; }
        public int u1BrkValReset1 { get; set; }
        public int u14NameElemId { get; set; }
        public int u14ElemIdRsv0 { get; set; }
        public int u3DropUnlockTmg0 { get; set; }
        public int u1BrkActTiming2 { get; set; }
        public int u14ElemIdRsv1 { get; set; }
        public int u14EffPos0 { get; set; }
        public int u3DropUnlockTmg1 { get; set; }
        public int u1BrkEnbFly2 { get; set; }
        public int u14EffPos1 { get; set; }
        public int s16PartCharSpec0_pointer { get; set; }
        public string s16PartCharSpec0_string { get; set; }
        public int u1BrkValReset2 { get; set; }
        public int u1BrkActTiming3 { get; set; }
        public int s16PartCharSpec1_pointer { get; set; }
        public string s16PartCharSpec1_string { get; set; }
        public int s8PartCharSpec3_pointer { get; set; }
        public string s8PartCharSpec3_string { get; set; }
        public int s8PartCharSpec4_pointer { get; set; }
        public string s8PartCharSpec4_string { get; set; }
        public int s8PartCharSpec5_pointer { get; set; }
        public string s8PartCharSpec5_string { get; set; }
        public int s8PartCharSpec6_pointer { get; set; }
        public string s8PartCharSpec6_string { get; set; }
        public int s8PartCharSpec7_pointer { get; set; }
        public string s8PartCharSpec7_string { get; set; }
        public int u7NumDrop0 { get; set; }
        public int u1BrkEnbFly3 { get; set; }
        public int u14TgElemId0 { get; set; }
        public int s10DropItem0_pointer { get; set; }
        public string s10DropItem0_string { get; set; }
        public int u7NumMaxDrop0 { get; set; }
        public int u1BrkValReset3 { get; set; }
        public int u14DropProbEnd0 { get; set; }
        public int u14DropProb0 { get; set; }
        public int u3DropUnlockTmg2 { get; set; }
        public int u1BrkActTiming4 { get; set; }
        public int u12DropProbStartTime0 { get; set; }
        public int u12DropProbEndTime0 { get; set; }
        public int u6DropConditionA { get; set; }
        public int u1BrkEnbFly4 { get; set; }
        public int u1BrkValReset4 { get; set; }
        public int u8DropCndArg0A { get; set; }
        public int u14DropCndAddProbA { get; set; }
        public int s10DropItem1_pointer { get; set; }
        public string s10DropItem1_string { get; set; }
        public int u7NumDrop1 { get; set; }
        public int u7NumMaxDrop1 { get; set; }
        public int u14DropProbEnd1 { get; set; }
        public int u3DropUnlockTmg3 { get; set; }
        public int u14DropProb1 { get; set; }
        public int u12DropProbStartTime1 { get; set; }
        public int u6DropConditionB { get; set; }
        public int u12DropProbEndTime1 { get; set; }
        public int u8DropCndArg0B { get; set; }
        public int u7DropCndNum0 { get; set; }
        public int u3DropUnlockTmg4 { get; set; }
        public int u14DropCndAddProbB { get; set; }
        public int u7DropCndNumMax0 { get; set; }
        public int u6DropCondition0 { get; set; }
        public int u4BrkActionKind0 { get; set; }
        public int u14DropProbEnd2 { get; set; }
        public int u14DropProb2 { get; set; }
        public int u4BrkPriority0 { get; set; }
        public int u12DropProbStartTime2 { get; set; }
        public int u12DropProbEndTime2 { get; set; }
        public int u8DropCndArg00 { get; set; }
        public int u14DropCndAddProb0 { get; set; }
        public int u7DropCndNum1 { get; set; }
        public int u7DropCndNumMax1 { get; set; }
        public int u4BrkExecLimitCnt0 { get; set; }
        public int u14DropProbEnd3 { get; set; }
        public int u14DropProb3 { get; set; }
        public int u4BrkEnableKind0 { get; set; }
        public int u12DropProbStartTime3 { get; set; }
        public int u12DropProbEndTime3 { get; set; }
        public int u6DropCondition1 { get; set; }
        public int u8DropCndArg01 { get; set; }
        public int u14DropCndAddProb1 { get; set; }
        public int u7DropCndNum2 { get; set; }
        public int u3BrkTxtIdx0 { get; set; }
        public int u7DropCndNumMax2 { get; set; }
        public int u14DropProbEnd4 { get; set; }
        public int u6DropCondition2 { get; set; }
        public int u4BrkReplaceAct0 { get; set; }
        public int u14DropProb4 { get; set; }
        public int u12DropProbStartTime4 { get; set; }
        public int u4BrkReplaceFinish0 { get; set; }
        public int u12DropProbEndTime4 { get; set; }
        public int u8DropCndArg02 { get; set; }
        public int u12Reserve3 { get; set; }
        public int u14DropCndAddProb2 { get; set; }
        public int u16DropGil { get; set; }
        public int u16GainLp { get; set; }
        public int u16DmgDecVal { get; set; }
        public int u12BrkLimitVal { get; set; }
        public int u10BrkDecMax { get; set; }
        public int u8BekDecSpd { get; set; }
        public int u16BrkLimitVal0 { get; set; }
        public int i16BrkAddVal0 { get; set; }
        public int u8BrkLimitCnt0 { get; set; }
        public int u4BrkGrgType0 { get; set; }
        public int u4BrkExecAttr0 { get; set; }
        public int u10BrkChrProp0 { get; set; }
        public int u4BrkAccIdx0 { get; set; }
        public int u4BrkActionKind1 { get; set; }
        public int u4BrkPriority1 { get; set; }
        public int u16BrkLimitVal1 { get; set; }
        public int u8BrkLimitCnt1 { get; set; }
        public int i16BrkAddVal1 { get; set; }
        public int u4BrkExecLimitCnt1 { get; set; }
        public int u4BrkEnableKind1 { get; set; }
        public int u4BrkReplaceAct1 { get; set; }
        public int u4BrkReplaceFinish1 { get; set; }
        public int u4BrkGrgType1 { get; set; }
        public int u4BrkExecAttr1 { get; set; }
        public int u10BrkChrProp1 { get; set; }
        public int u4BrkAccIdx1 { get; set; }
        public int u3BrkTexIdx1 { get; set; }
        public int u4BrkActionKind2 { get; set; }
        public int u3BrkTexIdx2 { get; set; }
        public int u4BrkPriority2 { get; set; }
        public int u16BrkLimitVal2 { get; set; }
        public int u8BrkLimitCnt2 { get; set; }
        public int u4BrkExecLimitCnt2 { get; set; }
        public int i16BrkAddVal2 { get; set; }
        public int u4BrkEnableKind2 { get; set; }
        public int u4BrkReplaceAct2 { get; set; }
        public int u4BrkReplaceFinish2 { get; set; }
        public int u4BrkGrgType2 { get; set; }
        public int u4BrkExecAttr2 { get; set; }
        public int u10BrkChrProp2 { get; set; }
        public int u4BrkAccIdx2 { get; set; }
        public int u4BrkActionKind3 { get; set; }
        public int u4BrkPriority3 { get; set; }
        public int u4BrkExecLimitCnt3 { get; set; }
        public int u16BrkLimitVal3 { get; set; }
        public int i16BrkAddVal3 { get; set; }
        public int u8BrkLimitCnt3 { get; set; }
        public int u4BrkEnableKind3 { get; set; }
        public int u4BrkReplaceAct3 { get; set; }
        public int u4BrkReplaceFinish3 { get; set; }
        public int u4BrkGrgType3 { get; set; }
        public int u4BrkExecAttr3 { get; set; }
        public int u4BrkAccIdx3 { get; set; }
        public int u10BrkChrProp3 { get; set; }
        public int u3BrkTexIdx3 { get; set; }
        public int u4BrkActionKind4 { get; set; }
        public int u4BrkPriority4 { get; set; }
        public int u8BrkLimitCnt4 { get; set; }
        public int u3BrkTexIdx4 { get; set; }
        public int u16BrkLimitVal4 { get; set; }
        public int i16BrkAddVal4 { get; set; }
        public int u4BrkExecLimitCnt4 { get; set; }
        public int u4BrkEnableKind4 { get; set; }
        public int u4BrkReplaceAct4 { get; set; }
        public int u4BrkReplaceFinish4 { get; set; }
        public int u4BrkGrgType4 { get; set; }
        public int u4BrkExecAttr4 { get; set; }
        public int u4BrkAccIdx4 { get; set; }
        public int u10BrkChrProp4 { get; set; }
        public override Dictionary<string, int> GetStringArrayMapping()
        {
            Dictionary<string, int> mapping = new Dictionary<string, int>();
            mapping.Add(nameof(s8Ability16_pointer), 0);
            mapping.Add(nameof(s12Ability8_pointer), 1);
            mapping.Add(nameof(s12Ability9_pointer), 2);
            mapping.Add(nameof(s12Ability10_pointer), 3);
            mapping.Add(nameof(s8Ability17_pointer), 4);
            mapping.Add(nameof(s12Ability11_pointer), 5);
            mapping.Add(nameof(s12Ability12_pointer), 6);
            mapping.Add(nameof(s8Ability18_pointer), 7);
            mapping.Add(nameof(s12Ability13_pointer), 8);
            mapping.Add(nameof(s12Ability14_pointer), 9);
            mapping.Add(nameof(s8Ability19_pointer), 10);
            mapping.Add(nameof(s12Ability15_pointer), 11);
            mapping.Add(nameof(s8Ability20_pointer), 12);
            mapping.Add(nameof(s8Ability21_pointer), 13);
            mapping.Add(nameof(s8Ability22_pointer), 14);
            mapping.Add(nameof(s8Ability23_pointer), 15);
            mapping.Add(nameof(s8Ability24_pointer), 16);
            mapping.Add(nameof(s8Ability25_pointer), 17);
            mapping.Add(nameof(s8Ability26_pointer), 18);
            mapping.Add(nameof(s8Ability27_pointer), 19);
            mapping.Add(nameof(s8Ability28_pointer), 20);
            mapping.Add(nameof(s8Ability29_pointer), 21);
            mapping.Add(nameof(s8Ability30_pointer), 22);
            mapping.Add(nameof(s8Ability31_pointer), 23);
            mapping.Add(nameof(s8PartCharSpec2_pointer), 24);
            mapping.Add(nameof(s16PartCharSpec0_pointer), 25);
            mapping.Add(nameof(s16PartCharSpec1_pointer), 26);
            mapping.Add(nameof(s8PartCharSpec3_pointer), 27);
            mapping.Add(nameof(s8PartCharSpec4_pointer), 28);
            mapping.Add(nameof(s8PartCharSpec5_pointer), 29);
            mapping.Add(nameof(s8PartCharSpec6_pointer), 30);
            mapping.Add(nameof(s8PartCharSpec7_pointer), 31);
            mapping.Add(nameof(s10DropItem0_pointer), 32);
            mapping.Add(nameof(s10DropItem1_pointer), 33);
            return mapping;
        }
        public void SetAbilities(List<string> list)
        {
            sAbility0_string = "";
            sAbility1_string = "";
            sAbility2_string = "";
            sAbility3_string = "";
            sAbility4_string = "";
            sAbility5_string = "";
            sAbility6_string = "";
            sAbility7_string = "";
            s12Ability8_string = "";
            s12Ability9_string = "";
            s12Ability10_string = "";
            s12Ability11_string = "";
            s12Ability12_string = "";
            s12Ability13_string = "";
            s12Ability14_string = "";
            s12Ability15_string = "";
            s8Ability16_string = "";
            s8Ability17_string = "";
            s8Ability18_string = "";
            s8Ability19_string = "";
            s8Ability20_string = "";
            s8Ability21_string = "";
            s8Ability22_string = "";
            s8Ability23_string = "";
            s8Ability24_string = "";
            s8Ability25_string = "";
            s8Ability26_string = "";
            s8Ability27_string = "";
            s8Ability28_string = "";
            s8Ability29_string = "";
            s8Ability30_string = "";
            s8Ability31_string = "";
            if (list.Count > 0)
                sAbility0_string = list[0];
            if (list.Count > 1)
                sAbility1_string = list[1];
            if (list.Count > 2)
                sAbility2_string = list[2];
            if (list.Count > 3)
                sAbility3_string = list[3];
            if (list.Count > 4)
                sAbility4_string = list[4];
            if (list.Count > 5)
                sAbility5_string = list[5];
            if (list.Count > 6)
                sAbility6_string = list[6];
            if (list.Count > 7)
                sAbility7_string = list[7];
            if (list.Count > 8)
                s12Ability8_string = list[8];
            if (list.Count > 9)
                s12Ability9_string = list[9];
            if (list.Count > 10)
                s12Ability10_string = list[10];
            if (list.Count > 11)
                s12Ability11_string = list[11];
            if (list.Count > 12)
                s12Ability12_string = list[12];
            if (list.Count > 13)
                s12Ability13_string = list[13];
            if (list.Count > 14)
                s12Ability14_string = list[14];
            if (list.Count > 15)
                s12Ability15_string = list[15];
            if (list.Count > 16)
                s8Ability16_string = list[16];
            if (list.Count > 17)
                s8Ability17_string = list[17];
            if (list.Count > 18)
                s8Ability18_string = list[18];
            if (list.Count > 19)
                s8Ability19_string = list[19];
            if (list.Count > 20)
                s8Ability20_string = list[20];
            if (list.Count > 21)
                s8Ability21_string = list[21];
            if (list.Count > 22)
                s8Ability22_string = list[22];
            if (list.Count > 23)
                s8Ability23_string = list[23];
            if (list.Count > 24)
                s8Ability24_string = list[24];
            if (list.Count > 25)
                s8Ability25_string = list[25];
            if (list.Count > 26)
                s8Ability26_string = list[26];
            if (list.Count > 27)
                s8Ability27_string = list[27];
            if (list.Count > 28)
                s8Ability28_string = list[28];
            if (list.Count > 29)
                s8Ability29_string = list[29];
            if (list.Count > 30)
                s8Ability30_string = list[30];
            if (list.Count > 31)
                s8Ability31_string = list[31];
        }

        public List<string> GetAbilities()
        {
            List<string> list = new List<string>();
            list.Add(sAbility0_string);
            list.Add(sAbility1_string);
            list.Add(sAbility2_string);
            list.Add(sAbility3_string);
            list.Add(sAbility4_string);
            list.Add(sAbility5_string);
            list.Add(sAbility6_string);
            list.Add(sAbility7_string);
            list.Add(s12Ability8_string);
            list.Add(s12Ability9_string);
            list.Add(s12Ability10_string);
            list.Add(s12Ability11_string);
            list.Add(s12Ability12_string);
            list.Add(s12Ability13_string);
            list.Add(s12Ability14_string);
            list.Add(s12Ability15_string);
            list.Add(s8Ability16_string);
            list.Add(s8Ability17_string);
            list.Add(s8Ability18_string);
            list.Add(s8Ability19_string);
            list.Add(s8Ability20_string);
            list.Add(s8Ability21_string);
            list.Add(s8Ability22_string);
            list.Add(s8Ability23_string);
            list.Add(s8Ability24_string);
            list.Add(s8Ability25_string);
            list.Add(s8Ability26_string);
            list.Add(s8Ability27_string);
            list.Add(s8Ability28_string);
            list.Add(s8Ability29_string);
            list.Add(s8Ability30_string);
            list.Add(s8Ability31_string);
            return list.Where(s => s != "").ToList();
        }
    }
}

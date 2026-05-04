using Bartz24.FF13_2_LR;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.LR;

public class DataStoreBtCharaSpec : DataStoreWDBEntry
{
    public string sCharaSpec { get; set; }
    public string sBaseBtSpec { get; set; }
    public string sNameStrResID { get; set; }
    public string sWandIdAg { get; set; }
    public float fAtkLen { get; set; }
    public string sScriptId { get; set; }
    public string sAiSheetName { get; set; }
    public string sAbility0 { get; set; }
    public string sAbility1 { get; set; }
    public string sAbility2 { get; set; }
    public string sAbility3 { get; set; }
    public string sAbility4 { get; set; }
    public string sAbility5 { get; set; }
    public string sAbility6 { get; set; }
    public string sAbility7 { get; set; }
    public float fFReserve0 { get; set; }
    public string sEffId0 { get; set; }
    public int iEffArg0 { get; set; }
    public string sSndId0 { get; set; }
    public string sEffId1 { get; set; }
    public int iEffArg1 { get; set; }
    public string sSndId1 { get; set; }
    public string sDropCndItem0 { get; set; }
    public string sDropCndItem1 { get; set; }
    public string sDropCndItem2 { get; set; }
    public string sBrkDefState { get; set; }
    public float fBrkDefDecVal { get; set; }
    public string sBrkState0 { get; set; }
    public float fBrkLoopTime0 { get; set; }
    public float fBrkDecVal0 { get; set; }
    public float fBrkCoolTime0 { get; set; }
    public float fBrkGrgTime0 { get; set; }
    public string sBrkState1 { get; set; }
    public float fBrkLoopTime1 { get; set; }
    public float fBrkDecVal1 { get; set; }
    public float fBrkCoolTime1 { get; set; }
    public float fBrkGrgTime1 { get; set; }
    public string sBrkState2 { get; set; }
    public float fBrkLoopTime2 { get; set; }
    public float fBrkDecVal2 { get; set; }
    public float fBrkCoolTime2 { get; set; }
    public float fBrkGrgTime2 { get; set; }
    public string sBrkState3 { get; set; }
    public float fBrkLoopTime3 { get; set; }
    public float fBrkDecVal3 { get; set; }
    public float fBrkCoolTime3 { get; set; }
    public float fBrkGrgTime3 { get; set; }
    public string sBrkState4 { get; set; }
    public float fBrkLoopTime4 { get; set; }
    public float fBrkDecVal4 { get; set; }
    public float fBrkCoolTime4 { get; set; }
    public float fBrkGrgTime4 { get; set; }
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
    public string s8Ability16 { get; set; }
    public int u10Prop0 { get; set; }
    public int u10Prop1 { get; set; }
    public int u10Prop2 { get; set; }
    public int u1BrkValReset0 { get; set; }
    public int u1BrkActTiming1 { get; set; }
    public int u10Prop3 { get; set; }
    public int u10Prop4 { get; set; }
    public string s12Ability8 { get; set; }
    public string s12Ability9 { get; set; }
    public string s12Ability10 { get; set; }
    public string s8Ability17 { get; set; }
    public string s12Ability11 { get; set; }
    public string s12Ability12 { get; set; }
    public string s8Ability18 { get; set; }
    public string s12Ability13 { get; set; }
    public string s12Ability14 { get; set; }
    public string s8Ability19 { get; set; }
    public string s12Ability15 { get; set; }
    public string s8Ability20 { get; set; }
    public string s8Ability21 { get; set; }
    public int u4PartCharKind { get; set; }
    public string s8Ability22 { get; set; }
    public string s8Ability23 { get; set; }
    public string s8Ability24 { get; set; }
    public string s8Ability25 { get; set; }
    public string s8Ability26 { get; set; }
    public string s8Ability27 { get; set; }
    public string s8Ability28 { get; set; }
    public string s8Ability29 { get; set; }
    public string s8Ability30 { get; set; }
    public string s8Ability31 { get; set; }
    public int u6Reserve4 { get; set; }
    public string s8PartCharSpec2 { get; set; }
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
    public string s16PartCharSpec0 { get; set; }
    public int u1BrkValReset2 { get; set; }
    public int u1BrkActTiming3 { get; set; }
    public string s16PartCharSpec1 { get; set; }
    public string s8PartCharSpec3 { get; set; }
    public string s8PartCharSpec4 { get; set; }
    public string s8PartCharSpec5 { get; set; }
    public string s8PartCharSpec6 { get; set; }
    public string s8PartCharSpec7 { get; set; }
    public int u7NumDrop0 { get; set; }
    public int u1BrkEnbFly3 { get; set; }
    public int u14TgElemId0 { get; set; }
    public string s10DropItem0 { get; set; }
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
    public string s10DropItem1 { get; set; }
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
    public void SetAbilities(List<string> list)
    {
        sAbility0 = "";
        sAbility1 = "";
        sAbility2 = "";
        sAbility3 = "";
        sAbility4 = "";
        sAbility5 = "";
        sAbility6 = "";
        sAbility7 = "";
        s12Ability8 = "";
        s12Ability9 = "";
        s12Ability10 = "";
        s12Ability11 = "";
        s12Ability12 = "";
        s12Ability13 = "";
        s12Ability14 = "";
        s12Ability15 = "";
        s8Ability16 = "";
        s8Ability17 = "";
        s8Ability18 = "";
        s8Ability19 = "";
        s8Ability20 = "";
        s8Ability21 = "";
        s8Ability22 = "";
        s8Ability23 = "";
        s8Ability24 = "";
        s8Ability25 = "";
        s8Ability26 = "";
        s8Ability27 = "";
        s8Ability28 = "";
        s8Ability29 = "";
        s8Ability30 = "";
        s8Ability31 = "";
        if (list.Count > 0)
        {
            sAbility0 = list[0];
        }

        if (list.Count > 1)
        {
            sAbility1 = list[1];
        }

        if (list.Count > 2)
        {
            sAbility2 = list[2];
        }

        if (list.Count > 3)
        {
            sAbility3 = list[3];
        }

        if (list.Count > 4)
        {
            sAbility4 = list[4];
        }

        if (list.Count > 5)
        {
            sAbility5 = list[5];
        }

        if (list.Count > 6)
        {
            sAbility6 = list[6];
        }

        if (list.Count > 7)
        {
            sAbility7 = list[7];
        }

        if (list.Count > 8)
        {
            s12Ability8 = list[8];
        }

        if (list.Count > 9)
        {
            s12Ability9 = list[9];
        }

        if (list.Count > 10)
        {
            s12Ability10 = list[10];
        }

        if (list.Count > 11)
        {
            s12Ability11 = list[11];
        }

        if (list.Count > 12)
        {
            s12Ability12 = list[12];
        }

        if (list.Count > 13)
        {
            s12Ability13 = list[13];
        }

        if (list.Count > 14)
        {
            s12Ability14 = list[14];
        }

        if (list.Count > 15)
        {
            s12Ability15 = list[15];
        }

        if (list.Count > 16)
        {
            s8Ability16 = list[16];
        }

        if (list.Count > 17)
        {
            s8Ability17 = list[17];
        }

        if (list.Count > 18)
        {
            s8Ability18 = list[18];
        }

        if (list.Count > 19)
        {
            s8Ability19 = list[19];
        }

        if (list.Count > 20)
        {
            s8Ability20 = list[20];
        }

        if (list.Count > 21)
        {
            s8Ability21 = list[21];
        }

        if (list.Count > 22)
        {
            s8Ability22 = list[22];
        }

        if (list.Count > 23)
        {
            s8Ability23 = list[23];
        }

        if (list.Count > 24)
        {
            s8Ability24 = list[24];
        }

        if (list.Count > 25)
        {
            s8Ability25 = list[25];
        }

        if (list.Count > 26)
        {
            s8Ability26 = list[26];
        }

        if (list.Count > 27)
        {
            s8Ability27 = list[27];
        }

        if (list.Count > 28)
        {
            s8Ability28 = list[28];
        }

        if (list.Count > 29)
        {
            s8Ability29 = list[29];
        }

        if (list.Count > 30)
        {
            s8Ability30 = list[30];
        }

        if (list.Count > 31)
        {
            s8Ability31 = list[31];
        }
    }

    public List<string> GetAbilities()
    {
        List<string> list = new()
        {
            sAbility0,
            sAbility1,
            sAbility2,
            sAbility3,
            sAbility4,
            sAbility5,
            sAbility6,
            sAbility7,
            s12Ability8,
            s12Ability9,
            s12Ability10,
            s12Ability11,
            s12Ability12,
            s12Ability13,
            s12Ability14,
            s12Ability15,
            s8Ability16,
            s8Ability17,
            s8Ability18,
            s8Ability19,
            s8Ability20,
            s8Ability21,
            s8Ability22,
            s8Ability23,
            s8Ability24,
            s8Ability25,
            s8Ability26,
            s8Ability27,
            s8Ability28,
            s8Ability29,
            s8Ability30,
            s8Ability31
        };
        return list.Where(s => s != "").ToList();
    }
}

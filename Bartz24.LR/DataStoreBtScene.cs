using Bartz24.FF13_2_LR;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.LR;

public class DataStoreBtScene : DataStoreWDBEntry
{
    public string sDebugStr { get; set; }
    public string sMapSetId0 { get; set; }
    public string sMapSetId1 { get; set; }
    public string sBtSpaceId { get; set; }
    public string sFinCondEntry0 { get; set; }
    public string sDropItem0 { get; set; }
    public string sEntryBtChSpec1 { get; set; }
    public float fForcePopPositionX { get; set; }
    public float fForcePopPositionY { get; set; }
    public float fForcePopPositionZ { get; set; }
    public string s10BtSceStrResId { get; set; }
    public string s10ExEntrySheet0 { get; set; }
        public int u1NoOpening { get; set; }
    public int u1NoEnding { get; set; }
    public int u1Seamless { get; set; }
    public int u1NoDispResult { get; set; }
    public int u1ContFromPrev { get; set; }
    public int u1ContToNext { get; set; }
    public int u6TutNo { get; set; }
    public int u1Flag0 { get; set; }
    public int u1Flag2 { get; set; }
    public int u1Flag3 { get; set; }
    public int u16ResltTimeArg { get; set; }
    public int u4FinCond0 { get; set; }
    public int u4FinType { get; set; }
    public int u1RareDrop0 { get; set; }
    public int u4BtChInitSetNum { get; set; }
    public int i16FinCondArg0 { get; set; }
    public int u8NumDrop0 { get; set; }
    public string s8PartyEntryId { get; set; }
    public int u16DropProb0 { get; set; }
    public string s8BtChEntryId { get; set; }
    public int u1EvenNoShift { get; set; }
    public int u1ForceFindPlayer { get; set; }
    public int u1NoMove { get; set; }
    public int u1NoEnhanceDeathZone { get; set; }
    public string s10EntryBtChSpec2 { get; set; }
    public string s10EntryBtChSpec3 { get; set; }
    public string s10EntryBtChSpec4 { get; set; }
    public string s8EntryBtChSpec5 { get; set; }
    public string s8EntryBtChSpec6 { get; set; }
    public string s8EntryBtChSpec7 { get; set; }
    public string s8EntryBtChSpec8 { get; set; }
    public string s8EntryBtChSpec9 { get; set; }
    public string s8EntryBtChSpec10 { get; set; }
    public string s8BgmResourceId { get; set; }
    public string s8DominantBgmResourceId { get; set; }
    public void SetCharSpecs(List<string> list)
    {
        sEntryBtChSpec1 = "";
        s10EntryBtChSpec2 = "";
        s10EntryBtChSpec3 = "";
        s10EntryBtChSpec4 = "";
        s8EntryBtChSpec5 = "";
        s8EntryBtChSpec6 = "";
        s8EntryBtChSpec7 = "";
        s8EntryBtChSpec8 = "";
        s8EntryBtChSpec9 = "";
        s8EntryBtChSpec10 = "";
        if (list.Count > 0)
        {
            sEntryBtChSpec1 = list[0];
        }

        if (list.Count > 1)
        {
            s10EntryBtChSpec2 = list[1];
        }

        if (list.Count > 2)
        {
            s10EntryBtChSpec3 = list[2];
        }

        if (list.Count > 3)
        {
            s10EntryBtChSpec4 = list[3];
        }

        if (list.Count > 4)
        {
            s8EntryBtChSpec5 = list[4];
        }

        if (list.Count > 5)
        {
            s8EntryBtChSpec6 = list[5];
        }

        if (list.Count > 6)
        {
            s8EntryBtChSpec7 = list[6];
        }

        if (list.Count > 7)
        {
            s8EntryBtChSpec8 = list[7];
        }

        if (list.Count > 8)
        {
            s8EntryBtChSpec9 = list[8];
        }

        if (list.Count > 9)
        {
            s8EntryBtChSpec10 = list[9];
        }
    }

    public List<string> GetCharSpecs()
    {
        List<string> list = new()
        {
            sEntryBtChSpec1,
            s10EntryBtChSpec2,
            s10EntryBtChSpec3,
            s10EntryBtChSpec4,
            s8EntryBtChSpec5,
            s8EntryBtChSpec6,
            s8EntryBtChSpec7,
            s8EntryBtChSpec8,
            s8EntryBtChSpec9,
            s8EntryBtChSpec10
        };
        return list.Where(s => s != "").ToList();
    }
}

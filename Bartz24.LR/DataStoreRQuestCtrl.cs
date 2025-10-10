using Bartz24.FF13_2_LR;
using System.Collections.Generic;

namespace Bartz24.LR;

public class DataStoreRQuestCtrl : DataStoreWDBEntry
{
    public int iQuestIndex { get; set; }
    public string sRewardId { get; set; }
    public string sIsActiveScript { get; set; }
    public string sIsClearScript { get; set; }
    public string sStartScript { get; set; }
    public string sEndScript { get; set; }
    public string sAcceptScript { get; set; }
    public string sQuestNameLabel { get; set; }
    public string sQuestTextLabel { get; set; }
    public string sClientLabel { get; set; }
    public string sRewardTextLabel { get; set; }
    public string sMissionClientName { get; set; }
    public string sPicture { get; set; }
    public string sClearTextLabel { get; set; }
    public string sFailureText { get; set; }
    public string sStepText1 { get; set; }
    public string sStepText2 { get; set; }
    public string sStepText3 { get; set; }
    public string sStepText4 { get; set; }
    public string sStepText5 { get; set; }
    public int uSortIndex { get; set; }
    public int u4ActivePeriod { get; set; }
    public string s9ClearItem { get; set; }
    public int u7ClearItemNum { get; set; }
    public string s9ClearItem2 { get; set; }
    public int u3Rank { get; set; }
    public int u7ClearItemNum2 { get; set; }
    public string s9ClearItem3 { get; set; }
    public int u7ClearItemNum3 { get; set; }
    public int u4BulletinIndex { get; set; }
}

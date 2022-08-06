using Bartz24.FF13_2_LR;
using System.Collections.Generic;

namespace Bartz24.LR
{
    public class DataStoreRQuestCtrl : DataStoreDB3SubEntry
    {
        public int iQuestIndex { get; set; }
        public int sRewardId_pointer { get; set; }
        public string sRewardId_string { get; set; }
        public int sIsActiveScript_pointer { get; set; }
        public string sIsActiveScript_string { get; set; }
        public int sIsClearScript_pointer { get; set; }
        public string sIsClearScript_string { get; set; }
        public int sStartScript_pointer { get; set; }
        public string sStartScript_string { get; set; }
        public int sEndScript_pointer { get; set; }
        public string sEndScript_string { get; set; }
        public int sAcceptScript_pointer { get; set; }
        public string sAcceptScript_string { get; set; }
        public int sQuestNameLabel_pointer { get; set; }
        public string sQuestNameLabel_string { get; set; }
        public int sQuestTextLabel_pointer { get; set; }
        public string sQuestTextLabel_string { get; set; }
        public int sClientLabel_pointer { get; set; }
        public string sClientLabel_string { get; set; }
        public int sRewardTextLabel_pointer { get; set; }
        public string sRewardTextLabel_string { get; set; }
        public int sMissionClientName_pointer { get; set; }
        public string sMissionClientName_string { get; set; }
        public int sPicture_pointer { get; set; }
        public string sPicture_string { get; set; }
        public int sClearTextLabel_pointer { get; set; }
        public string sClearTextLabel_string { get; set; }
        public int sFailureText_pointer { get; set; }
        public string sFailureText_string { get; set; }
        public int sStepText1_pointer { get; set; }
        public string sStepText1_string { get; set; }
        public int sStepText2_pointer { get; set; }
        public string sStepText2_string { get; set; }
        public int sStepText3_pointer { get; set; }
        public string sStepText3_string { get; set; }
        public int sStepText4_pointer { get; set; }
        public string sStepText4_string { get; set; }
        public int sStepText5_pointer { get; set; }
        public string sStepText5_string { get; set; }
        public int uSortIndex { get; set; }
        public int u4ActivePeriod { get; set; }
        public int s9ClearItem_pointer { get; set; }
        public string s9ClearItem_string { get; set; }
        public int u7ClearItemNum { get; set; }
        public int s9ClearItem2_pointer { get; set; }
        public string s9ClearItem2_string { get; set; }
        public int u3Rank { get; set; }
        public int u7ClearItemNum2 { get; set; }
        public int s9ClearItem3_pointer { get; set; }
        public string s9ClearItem3_string { get; set; }
        public int u7ClearItemNum3 { get; set; }
        public int u4BulletinIndex { get; set; }

        public override Dictionary<string, int> GetStringArrayMapping()
        {
            Dictionary<string, int> mapping = new Dictionary<string, int>();
            mapping.Add(nameof(s9ClearItem_pointer), 0);
            mapping.Add(nameof(s9ClearItem2_pointer), 1);
            mapping.Add(nameof(s9ClearItem3_pointer), 2);
            return mapping;
        }
    }
}

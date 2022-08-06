using Bartz24.FF13_2_LR;

namespace Bartz24.LR
{
    public class DataStoreRQuest : DataStoreDB3SubEntry
    {
        public int iQuestIndex { get; set; }
        public int iGp { get; set; }
        public int sTreasureBoxId_pointer { get; set; }
        public string sTreasureBoxId_string { get; set; }
        public int iMaxGp { get; set; }
        public int iMaxHp { get; set; }
        public int iAtkPhy { get; set; }
        public int iAtkMag { get; set; }
        public int iMaxAtb { get; set; }
        public int iAtbSpeed { get; set; }
        public int iItemBagSize { get; set; }
        public int sTreasureBoxId2_pointer { get; set; }
        public string sTreasureBoxId2_string { get; set; }
        public int iMaxGp2 { get; set; }
        public int iMaxHp2 { get; set; }
        public int iAtkPhy2 { get; set; }
        public int iAtkMag2 { get; set; }
        public int iMaxAtb2 { get; set; }
        public int iAtbSpeed2 { get; set; }
        public int iItemBagSize2 { get; set; }
    }
}

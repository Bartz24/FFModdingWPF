using Bartz24.FF13_2_LR;

namespace Bartz24.FF13_2;

public class DataStoreRFragment : DataStoreWDBEntry
{
    public string sHistoryId { get; set; }
    public string sNameStringId { get; set; }
    public string sDetailStringId { get; set; }
    public string sKeyString { get; set; }
    public string sActionCinemaBoxId { get; set; }
    public string sTalkId { get; set; }
    public string sTalkIdResponceYes { get; set; }
    public string sTalkIdResponceNo { get; set; }
    public string sTalkIdAcceptedConditionNG { get; set; }
    public string sTalkIdAcceptedConditionOK { get; set; }
    public string sTalkIdAcceptedConditionTimeLag { get; set; }
    public string sTalkIdCleared { get; set; }
    public string sEndScriptId { get; set; }
    public string sAcceptedScriptId { get; set; }
    public string sItemMissionClearedScriptId { get; set; }
    public string sAutoClipId0 { get; set; }
    public string sAutoClipId1 { get; set; }
    public string sNoticePointId { get; set; }
    public string sTargetName { get; set; }
    public string sBattleSceneTableName { get; set; }
        public int iBattleSituationNum { get; set; }
    public string sPicture { get; set; }
    public int u4Kind { get; set; }
    public int u5MenuCategory { get; set; }
    public int u3Difficulty { get; set; }
    public int u4MissionIndex { get; set; }
    public int u3KeyType { get; set; }
    public int u1ItemLost { get; set; }
    public int u5CateSortId { get; set; }
    public int u4AreaSortId { get; set; }
    public int u20CrystalPoint { get; set; }
}

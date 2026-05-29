using System.Collections.Generic;

namespace Bartz24.FF13_2;

public class HistoriaCruxConstants
{
    public const string VALHALLA_FINAL = "h_va_NA0000";
    public const string VALHALLA_DLC = "h_va_NA0001";
    public const string NEW_BODHUM_3 = "h_hm_AD0003";
    public const string NEW_BODHUM_700 = "h_hm_AD0700";
    public const string NEW_BODHUM_900 = "h_hm_AD0900";
    public const string NEW_BODHUM_3X = "h_hp_AD0003";
    public const string BRESHA_RUINS_5 = "h_bj_AD0005";
    public const string BRESHA_RUINS_100 = "h_bj_AD0100";
    public const string BRESHA_RUINS_300 = "h_bj_AD0300";
    public const string YASCHAS_10 = "h_gy_AD0010";
    public const string YASCHAS_100 = "h_gy_AD0100";
    public const string YASCHAS_110 = "h_gy_AD0200";
    public const string YASCHAS_1X = "h_gh_AD0010";
    public const string OERBA_200 = "h_gw_AD0200";
    public const string OERBA_300 = "h_gw_AD0300";
    public const string OERBA_400 = "h_gw_AD0400";
    public const string OERBA_900 = "h_gw_AD0900";
    public const string SUNLETH_300 = "h_sn_AD0300";
    public const string SUNLETH_400 = "h_sn_AD0400";
    public const string SUNLETH_900 = "h_sn_AD0900";
    public const string COLISEUM = "h_cl_NA0000";
    public const string COLISEUM_DLC = "h_cl_NA0001";
    public const string ARCHYLTE = "h_gd_NA0000";
    public const string ARCHYLTE_900 = "h_gd_NA0900";
    public const string SERENDIPITY = "h_cs_NA0000";
    public const string SERENDIPITY_DLC = "h_cs_NA0001";
    public const string ACADEMIA_400 = "h_ac_AD0400";
    public const string ACADEMIA_500 = "h_ac_AD0500";
    public const string ACADEMIA_4XX = "h_aa_AD0400";
    public const string AUGUSTA_200 = "h_gt_AD0200";
    public const string AUGUSTA_300 = "h_gt_AD0300";
    public const string AUGUSTA_900 = "h_gt_AD0900";
    public const string VILE_PEAKS_10 = "h_vp_AD0010";
    public const string VILE_PEAKS_200 = "h_vp_AD0200";
    public const string DYING_WORLD_700 = "h_dd_AD0700";
    public const string DYING_WORLD_900 = "h_dd_NA0900";
    public const string VOID_BEYOND_A = "h_sp_NA0001";
    public const string VOID_BEYOND_B = "h_sp_NA0100";
    public const string BLANK_1 = "h_zz_NA0910";
    public const string BLANK_2 = "h_zz_NA0920";
    public const string BLANK_3 = "h_zz_NA0930";
    public const string BLANK_4 = "h_zz_NA0940";
    public const string BLANK_5 = "h_zz_NA0950";
    public const string BLANK_6 = "h_zz_NA0960";
    public const string BLANK_7 = "h_zz_NA0970";
    public const string BLANK_8 = "h_zz_NA0980";

    public const string AREA_SUFFIX = "_a";

    public static readonly Dictionary<string, string> AREA_PREFIX_LOOKUP = new Dictionary<string, string>()
    {
        {"hm","New Bodhum"},
        {"hp","New Bodhum"},
        {"bj","Bresha Ruins"},
        {"va","Valhalla"},
        {"ac","Academia"},
        {"aa","Academia"},
        {"gy","Yaschas Massif"},
        {"gh","Yaschas Massif"},
        {"gd","Archylte Steppe"},
        {"sn","Sunleth Waterscape"},
        {"gw","Oerba"},
        {"gt","Augusta Tower"},
        {"vp","Vile Peaks"},
        {"cl","Coliseum"},
        {"cs","Serendipity"},
        {"dd","A Dying World"},
        {"sp","The Void Beyond"},
        {"zz","Please alert a rando dev if you see this!" }
    };

    public static readonly Dictionary<string, string> DATE_SPECIAL_CASES = new()
    {
        {YASCHAS_1X, "1X" },
        {ACADEMIA_4XX, "4XX" },
        {YASCHAS_110, "110" },
    };
}

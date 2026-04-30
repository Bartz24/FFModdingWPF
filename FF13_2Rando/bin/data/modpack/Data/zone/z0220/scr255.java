import cmn.common;
import fake.Resource;
import fake.White;
import fld.com;
import fld.mon;

public class scr255 {
    public static String evname = "ev_aaex_110";

    public static boolean efIsSecretEventEnable() {
        return common.sfHasFragment((String)"frg_cmn_pdxe001") && common.sfHasFragment((String)"frg_cmn_pdxe002") && common.sfHasFragment((String)"frg_cmn_pdxe003") && common.sfHasFragment((String)"frg_cmn_pdxe004") && common.sfHasFragment((String)"frg_cmn_pdxe005") && common.sfHasFragment((String)"frg_cmn_pdxe006") && common.sfHasFragment((String)"frg_cmn_pdxe007") && common.sfHasFragment((String)"frg_cmn_pdxe008") && common.sfIsFragmentCompleted() && common.sfIsGameCleared();
    }

    public static int efEndRollCallBack(int n) {
        common.sfCutSceneCallBackBasic((String)"ev_comn_204");
        common.sfClearBgmVolContinue();
        common.sfStartScriptEventMode((String)common.sfMakeScript((String)"scr255", (String)"efOpenVaza", (int)0), (String)common.sfMakeScript((String)"scr255", (String)"efOpenVaza_cb", (int)0));
        return 1;
    }

    public static int onFieldPrepare(int n) {
        mon.sfHideChara((String)"mog");
        mon.sfHideChara((String)"sera");
        mon.sfHideChara((String)"noel");
        mon.sfSetPos2((String)"mog", (float)10000.0f, (float)0.0f, (float)0.0f);
        mon.sfSetPos2((String)"sera", (float)10000.0f, (float)0.0f, (float)0.0f);
        mon.sfSetPos2((String)"noel", (float)10000.0f, (float)0.0f, (float)0.0f);
        evname = White.getPlatformCode() == 2 ? "ev_aaex_111" : "ev_aaex_110";
        White.wait((int)Resource.loadCutSceneScheduleData((String)evname));
        White.startCinema((int)1, (String)evname, (String)"suc_ev_on_fldd", (String)White.registScriptDatabase((String)"scr255", (String)"onCinemaEnd", (int)0));
        return 0;
    }

    public static int efOpenVaza(int n) {
        White.sleep((int)3000);
        common.randoAddItem("frg_cmn_acfa002");
        com.sfAddPartyKeyItemWithCheck((String)"opt_vaza_re");
        common.sfSetEvFlag((String)"tm_abst_080");
        common.sfSetHistoriaCross((String)"", (String)"hs_acfa01_zz");
        common.sfSetHistoriaCross((String)"", (String)"hs_acfa01_va");
        common.sfShowFieldTutorialOneTime((String)"", (String)"aatc_0096");
        common.sfGetTroEnd();
        common.sfAutoSaveOnlyGlobal((String)"");
        common.sfSetCurrentHistoryId((String)"h_va_NA0000_a");
        return 1;
    }

    public static int ef212EndCallBack(int n) {
        common.sfReadAndStartCutEvent((String)"", (String)"ev_comn_204", (String)common.sfMakeScript((String)"scr255", (String)"efEndRollCallBack", (int)0));
        return 1;
    }

    public static int efOpenVaza_cb(int n) {
        common.sfStartScriptZoneJumpMode((String)common.sfMakeScript((String)"scr255", (String)"efAfter_Evcomn204_cb", (int)0));
        return 1;
    }

    public static int onCinemaEnd(int n) {
        common.sfCutSceneCallBackBasic((String)evname);
        common.sfSetBgmVolContinue();
        common.sfMusicFadeInSysSoon((String)"");
        common.sfReadAndStartCutEvent((String)"", (String)"ev_comn_212", (String)common.sfMakeScript((String)"scr255", (String)"ef212EndCallBack", (int)0));
        return 1;
    }

    public static int efAfter_Evcomn204_cb(int n) {
        if (scr255.efIsSecretEventEnable()) {
            common.sfShowNowLoading();
            common.sfDoZoneJump((int)62);
        } else {
            common.sfDoZoneJump((int)0);
        }
        return 1;
    }

    public static void doMain(boolean bl) {
        if (!bl) {
            common.sfDefaultFadeOutNow();
            White.startField((String)"suc_field_ucoff", null, (int)-1, (String)White.registScriptDatabase((String)"scr255", (String)"onFieldPrepare", (int)0));
        } else {
            White.halt((String)"hoge");
        }
    }
}

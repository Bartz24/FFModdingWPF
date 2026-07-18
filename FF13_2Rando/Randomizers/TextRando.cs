using Bartz24.Data;
using Bartz24.FF13Series;
using Bartz24.RandoWPF;

namespace FF13_2Rando;

public class TextRando : Randomizer
{
    public DataStoreZTRText mainSysUS = new();
    public DataStoreZTRText quizUS = new();

    public TextRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Text Data...");
        {
            string path = Nova.GetNovaFile("13-2", @"txtres\resident\system\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["13-2"]);
            string outPath = Generator.DataOutFolder + @"\txtres\resident\system\txtres_us.ztr";
            FileHelpers.CopyFile(path, outPath);

            mainSysUS.Load("13-2", outPath, SetupData.Paths["Nova"]);
        }

        {
            string path = Nova.GetNovaFile("13-2", @"txtres\resident\game\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["13-2"]);
            string outPath = Generator.DataOutFolder + @"\txtres\resident\game\txtres_us.ztr";
            FileHelpers.CopyFile(path, outPath);

            quizUS.Load("13-2", outPath, SetupData.Paths["Nova"]);
        }

        EquipRando equipRando = Generator.Get<EquipRando>();
        mainSysUS[equipRando.items["key_mog_level"].sItemNameStringId] = "Progressive Mog Level{End}{Many}Progressive Mog Levels{End}{Article}a{End}";
        mainSysUS[equipRando.items["key_mog_level"].sHelpStringId] = "Each obtained unlocks the following in order: Moogle Hunt, Moogle Throw, and Improved Moogle Hunt.";

        mainSysUS[equipRando.items["key_r_victory"].sItemNameStringId] = "Victory!";

        mainSysUS[equipRando.items["key_shop_level"].sItemNameStringId] = "Progressive Shop Level{End}{Many}Progressive Shop Levels{End}{Article}a{End}";
        mainSysUS[equipRando.items["key_shop_level"].sHelpStringId] = "Each obtained unlocks more items in Chocolina's shop.";

        mainSysUS[equipRando.items["key_boss_token"].sItemNameStringId] = "Boss Token";
        mainSysUS[equipRando.items["key_boss_token"].sHelpStringId] = "Granted for defeating a powerful foe";


        // Custom artefact names
        mainSysUS[equipRando.items["opt_bjaa03_bj"].sItemNameStringId] = "Frozen Artefact";
        mainSysUS[equipRando.items["opt_ddha01_bj"].sItemNameStringId] = "Overgrown Artefact";
        mainSysUS[equipRando.items["opt_bjba01_gy"].sItemNameStringId] = "Artefact of Penumbra";
        mainSysUS[equipRando.items["opt_ghaa01_gt"].sItemNameStringId] = "Spire Artefact";
        mainSysUS[equipRando.items["opt_gdaa01_vp"].sItemNameStringId] = "Blitz Artefact";
        mainSysUS[equipRando.items["opt_acea02_gy"].sItemNameStringId] = "Artefact of Umbra";
        mainSysUS[equipRando.items["opt_gyba01_sn"].sItemNameStringId] = "Rainbow Artefact";
        mainSysUS[equipRando.items["opt_gtca02_gw"].sItemNameStringId] = "Sandstorm Artefact";
        mainSysUS[equipRando.items["opt_gwda01_gw"].sItemNameStringId] = "Arenaceous Artefact";
        mainSysUS[equipRando.items["opt_aaea03_vp"].sItemNameStringId] = "Artefact of Wrack and Ruin";

        // Artefact help strings updated with gate details
        mainSysUS[equipRando.items["opt_aaea02_sp"].sHelpStringId] = "Opens the Story Gate in Academia 4XX AF";
        mainSysUS[equipRando.items["opt_acea01_gt"].sHelpStringId] = "Opens the Story Gate in Academia 400 AF";
        mainSysUS[equipRando.items["opt_aaea03_vp"].sHelpStringId] = "Opens the Wild Gate in Academia 4XX AF";
        mainSysUS[equipRando.items["opt_bjaa01_gy"].sHelpStringId] = "Opens the Outside Gate in Bresha Ruins 005 AF";
        mainSysUS[equipRando.items["opt_bjaa02_sn"].sHelpStringId] = "Opens the Inside Gate in Bresha Ruins 005 AF";
        mainSysUS[equipRando.items["opt_bjaa03_bj"].sHelpStringId] = "Opens the South West Gate in Bresha Ruins 005 AF";
        mainSysUS[equipRando.items["opt_ghsn01_cs"].sHelpStringId] = "Opens the Story Gate in Yaschas Massif 01X AF";
        mainSysUS[equipRando.items["opt_ghsn02_ac"].sHelpStringId] = "Opens the Central Gate in Sunleth Waterscape 300 AF";
        mainSysUS[equipRando.items["opt_gtca01_aa"].sHelpStringId] = "Opens the Gate on the Top Floor of Augusta Tower 200 AF";
        mainSysUS[equipRando.items["opt_gtca02_gw"].sHelpStringId] = "Opens the Wild Gate on Floor 49 of Augusta Tower 200 AF";
        mainSysUS[equipRando.items["opt_gwca01_gh"].sHelpStringId] = "Opens the Gate in Oerba 200 AF";
        mainSysUS[equipRando.items["opt_gyaa01_gw"].sHelpStringId] = "Opens the Gate in Yaschas Massif 010 AF";
        mainSysUS[equipRando.items["opt_hmaa01_bj"].sHelpStringId] = "Opens the Gate in New Bodhum 003 AF";
        mainSysUS[equipRando.items["opt_hmha01_ac"].sHelpStringId] = "Opens the Gate in New Bodhum 700 AF";
        mainSysUS[equipRando.items["opt_snda01_cl"].sHelpStringId] = "Opens the South Gate in Sunleth Waterscape 300 AF";
        mainSysUS[equipRando.items["opt_snda02_gd"].sHelpStringId] = "Opens the North Gate in Sunleth Waterscape 300 AF";
        mainSysUS[equipRando.items["opt_ghaa01_gt"].sHelpStringId] = "Opens the Wild Gate in Yaschas Massif 01X AF";
        mainSysUS[equipRando.items["opt_gdaa01_vp"].sHelpStringId] = "Opens the Gate in Archylte Steppe ??? AF";
        mainSysUS[equipRando.items["opt_acea02_gy"].sHelpStringId] = "Opens the Wild Gate in Academia 400 AF";
        mainSysUS[equipRando.items["opt_gyba01_sn"].sHelpStringId] = "Opens the Gate in Yaschas Massif 100 AF";
        mainSysUS[equipRando.items["opt_gwda01_gw"].sHelpStringId] = "Opens the Gate in Oerba 300 AF";
        mainSysUS[equipRando.items["opt_ddha01_bj"].sHelpStringId] = "Opens the Gate in A Dying World 700 AF";
        mainSysUS[equipRando.items["opt_bjba01_gy"].sHelpStringId] = "Opens the Gate in Bresha Ruins 300 AF";

        // Copy across fragment skills
        mainSysUS["$amca_frg_sk01"] = "The fragment skill Mog's Manifestation has been unlocked!";
        mainSysUS["$amca_frg_sk02"] = "The fragment skill Bargain Hunter has been unlocked!";
        mainSysUS["$amca_frg_sk03"] = "The fragment skill Haggler has been unlocked!";
        mainSysUS["$amca_frg_sk04"] = "The fragment skill Chocobo Music has been unlocked!";
        mainSysUS["$amca_frg_sk05"] = "The fragment skill Anti-grav Jump has been unlocked!";
        mainSysUS["$amca_frg_sk06"] = "The fragment skill Paradox Scope has been unlocked!";
        mainSysUS["$amca_frg_sk07"] = "The fragment skill Limit Breaker has been unlocked!";
        mainSysUS["$amca_frg_sk08"] = "The fragment skill Rolling in CP has been unlocked!";
        mainSysUS["$amca_frg_sk09"] = "The fragment skill Good Drops has been unlocked!";
        mainSysUS["$amca_frg_sk10"] = "The fragment skill Mobile Mog has been unlocked!";
        mainSysUS["$amca_frg_sk11"] = "The fragment skill Monster Collector has been unlocked!";
        mainSysUS["$amca_frg_sk12"] = "The fragment skill Encounter Master has been unlocked!";
        mainSysUS["$amca_frg_sk13"] = "The fragment skill Less Enemies has been unlocked!";
        mainSysUS["$amca_frg_sk14"] = "The fragment skill Battlemania has been unlocked!";
        mainSysUS["$amca_frg_sk15"] = "The fragment skill Field Killer has been unlocked!";
        mainSysUS["$amca_frg_sk16"] = "The fragment skill Clock Master has been unlocked!";
        mainSysUS["$amca_frg_sk17"] = "The fragment skill Clock Master has been unlocked!";
        mainSysUS["$amca_frg_sk18"] = "The fragment skill Eyes of the Goddess has been unlocked!";
        mainSysUS["$amca_frg_sk19"] = "The fragment skill New Game, Same Stats has been unlocked!";
        // To make it clear its not the fragment in your inventory
        mainSysUS["$privilege06"] = "Paradox Scope (Skill)";
        mainSysUS["$fl_rando_opt"] = "{Color Yellow}{Entity 1}{Color White} is required for this gate.";
        mainSysUS["$rando_win_cond_frag"] = "You have {Color Yellow}{Counter Type 1}{Color White} / {Counter Type 2} Fragments required to progress";
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Text Data...");
    }

    private string GetHash()
    {
        string numberForm = RandomNum.GetHash(6, 9);
        string iconForm = "";

        foreach (char c in numberForm)
        {
            switch (c)
            {
                case '0':
                    iconForm += "{Icon Clock}";
                    break;
                case '1':
                    iconForm += "{Icon Warning}";
                    break;
                case '2':
                    iconForm += "{Icon Tamed_Crystal}";
                    break;
                case '3':
                    iconForm += "{Icon Gil}";
                    break;
                case '4':
                    iconForm += "{Icon Arrow_Right}";
                    break;
                case '5':
                    iconForm += "{Icon Arrow_Left}";
                    break;
                case '6':
                    iconForm += "{Icon Lock_Type2}";
                    break;
                case '7':
                    iconForm += "{Icon Check_Mark}";
                    break;
                case '8':
                    iconForm += "{Icon Monster}";
                    break;
            }
        }

        return iconForm;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Text Data...");
        string hash = GetHash();

        mainSysUS["$dif_conf_e"] = "{Icon Warning} Begin game in {Color LightRed}EASY MODE{Color White}?{Text NewLine}" +
            "Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
            "Seed Hash (for validation): " + hash + "{Text NewLine}|Yes|No";
        mainSysUS["$dif_conf_n"] = "{Icon Warning} Begin game in {Color LightRed}NORMAL MODE{Color White}?{Text NewLine}" +
            "Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
            "Seed Hash (for validation): " + hash + "{Text NewLine}|Yes|No";

        TempTextCleanup(mainSysUS);
        TempTextCleanup(quizUS);

        {
            string outPath = Generator.DataOutFolder + @"\txtres\resident\system\txtres_us.ztr";
            mainSysUS.Save("13-2", outPath, SetupData.Paths["Nova"]);
        }

        {
            string outPath = Generator.DataOutFolder + @"\txtres\resident\game\txtres_us.ztr";
            quizUS.Save("13-2", outPath, SetupData.Paths["Nova"]);
        }
    }

    private void TempTextCleanup(DataStoreZTRText text)
    {
        text.Keys.ForEach(k =>
        {
            text[k] = text[k].Replace("Ⅷ", "");
            text[k] = text[k].Replace("×", "x");
        });
    }
}

using Bartz24.Data;
using Bartz24.FF13Series;
using Bartz24.RandoWPF;
using System;
using System.Linq;

namespace FF13Rando;

public class TextRando : Randomizer
{
    public DataStoreZTRText mainSysUS = new();

    public TextRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Text Data...");
        {
            string path = Nova.GetNovaFile("13", @"txtres\resident\system\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["13"]);
            string outPath = Generator.DataOutFolder + @"\txtres\resident\system\txtres_us.ztr";
            FileHelpers.CopyFile(path, outPath);

            mainSysUS.Load("13", outPath, SetupData.Paths["Nova"]);

            mainSysUS["$acc_039_000"] = "Collector Catalog{End}{StraightLine}Collector Catalogs{End}{Article}a";
            mainSysUS["$acc_039_001"] = "Connoisseur Catalog{End}{StraightLine}Connoisseur Catalogs{End}{Article}a";

            mainSysUS["$am_000_00h"] = "Chapter 1";
            mainSysUS["$am_010_00"] = "Chapter 2";
            mainSysUS["$am_010_00h"] = "Chapter 3";
            mainSysUS["$am_020_00"] = "Chapter 4";
            mainSysUS["$am_020_00h"] = "Chapter 5";
            mainSysUS["$am_030_00"] = "Chapter 6";
            mainSysUS["$am_030_00h"] = "Chapter 7";
            mainSysUS["$am_040_00"] = "Chapter 8";
            mainSysUS["$am_040_00h"] = "Chapter 9";
            mainSysUS["$am_050_00"] = "Chapter 10";
            mainSysUS["$am_050_00h"] = "Chapter 11";
            mainSysUS["$am_100_00"] = "Chapter 12";
            mainSysUS["$am_100_00h"] = "Chapter 13";
            mainSysUS["$am_110_00"] = "Cancel";
            mainSysUS["$am_110_00h"] = "Return to the {Italic}Lindblum{Italic}";
            mainSysUS["$chpt_save_ttl"] = "Return to the {Italic}Lindblum{Italic}?";
            mainSysUS["$flar_ttl_000"] = "FF13 Randomizer";

            mainSysUS["$pause_03"] = "{Btn LSPress}+{Btn RSPress}+{Btn RB} {Italic}on the field{Italic} Return to Lindblum     {Btn Back}Quit     {Btn Start}Resume";

            string[] randomLocs = { "Somewhere", "Unknown", "Nautilus Park?", "The {Italic}Lindblum{Italic}?", "The {Italic}Palamecia{Italic}?", "", "FINAL FANTASY XIII", "Totally a Hallway", "Before 000 AF", "Hi :)", "Why are you looking here?", "DELETED TEXT" };
            RandomNum.SetRand(new Random(RandomNum.GetIntSeed(SetupData.Seed) + randomLocs.Length));
            string mainLoc = randomLocs.Take(6).Shuffle().First();
            mainSysUS.Keys.Where(s => s.StartsWith("$m_res_mn_m")).ForEach(s =>
            {
                mainSysUS[s] = RandomNum.RandInt(0, 999) < 995 ? mainLoc : randomLocs.Shuffle().First() + "{End}{Escape}";
            });
            RandomNum.ClearRand();

        }
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
                    iconForm += "{Icon Notification}";
                    break;
                case '3':
                    iconForm += "{Icon Gil}";
                    break;
                case '4':
                    iconForm += "{Icon Mission_Note}";
                    break;
                case '5':
                    iconForm += "{Icon Check_Mark}";
                    break;
                case '6':
                    iconForm += "{Icon Ability_Synthesized}";
                    break;
                case '7':
                    iconForm += "{Icon Gunblade}";
                    break;
                case '8':
                    iconForm += "{Icon Pistol}";
                    break;
            }
        }

        return iconForm;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Text Data...");
        string hash = GetHash();

        SetDiffText(hash);

        {
            string outPath = Generator.DataOutFolder + @"\txtres\resident\system\txtres_us.ztr";
            mainSysUS.Save("13", outPath, SetupData.Paths["Nova"]);
        }
    }

    protected virtual void SetDiffText(string hash)
    {
        mainSysUS["$dif_conf_e"] = "{Icon Warning} You have selected {Color LightRed}EASY MODE{Color White} for battles.{Text NewLine}" +
            "Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
            "Seed Hash (for validation): " + hash + "{Text NewLine}" +
            "Begin playing in {Color LightRed}EASY MODE{Color White}?|Yes|No";
        mainSysUS["$dif_conf_n"] = "{Icon Warning} You have selected {Color LightRed}NORMAL MODE{Color White} for battles.{Text NewLine}" +
            "Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
            "Seed Hash (for validation): " + hash + "{Text NewLine}" +
            "Begin playing in {Color LightRed}NORMAL MODE{Color White}?|Yes|No";
    }
}

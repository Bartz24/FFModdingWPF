using Bartz24.Data;
using Bartz24.FF13Series;
using Bartz24.RandoWPF;
using System;
using System.Linq;

namespace FF13Rando
{
    public class TextRando : Randomizer
    {
        public DataStoreZTRText mainSysUS = new DataStoreZTRText();

        public TextRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetUIProgress("Loading Text Data...", -1, 100);
            {
                string path = Nova.GetNovaFile("13", @"txtres\resident\system\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["13"]);
                string outPath = SetupData.OutputFolder + @"\txtres\resident\system\txtres_us.ztr";
                FileHelpers.CopyFile(path, outPath);

                mainSysUS.Load(outPath, SetupData.Paths["Nova"]);

                mainSysUS["$acc_039_000"] = "Collector Catalog{End}{Many}Collector Catalogs{End}{Article}a";
                mainSysUS["$acc_039_001"] = "Connoisseur Catalog{End}{Many}Connoisseur Catalogs{End}{Article}a";

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

                mainSysUS["$restart_00"] = "Retry|Softlock|Cancel";
                mainSysUS["$restart_01"] = "Retry|Softlock";
                mainSysUS["$pause_03"] = "{Key Select}Softlock   {Key Start}Resume";
                mainSysUS["$restart_03"] = "{Icon Attention} The Quit function does not work yet in the randomizer.{Text NewLine}{Text NewLine}This will softlock your game for reasons that make the randomizer work.{Text NewLine}To exit the game, press the 'Esc' key.";
                mainSysUS["$ask_end_title"] = "The Quit function does not work yet in the randomizer.{Text NewLine}{Text NewLine}This will softlock your game for reasons that make the randomizer work.{Text NewLine}To exit the game, press the 'Esc' key.{End}{Escape}";
                
                string[] randomLocs = { "Somewhere", "Unknown", "Nautilus Park?", "The {Italic}Lindblum{Italic}?", "The {Italic}Palamecia{Italic}?", "", "FINAL FANTASY XIII", "Totally a Hallway", "Before 000 AF", "Hi :)", "Why are you looking here?", "DELETED TEXT" };
                RandomNum.SetRand(new Random(RandomNum.GetIntSeed(SetupData.Seed) + randomLocs.Length));
                string mainLoc = randomLocs.Take(6).Shuffle().First();
                mainSysUS.Keys.Where(s => s.StartsWith("$m_res_mn_m")).ForEach(s => {
                    mainSysUS[s] = RandomNum.RandInt(0, 999) < 995 ? mainLoc : (randomLocs.Shuffle().First()) + "{End}{Escape}";
                });
                RandomNum.ClearRand();

            }
        }
        public override void Randomize()
        {
            Randomizers.SetUIProgress("Randomizing Text Data...", -1, 100);
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
                        iconForm += "{Icon Attention}";
                        break;
                    case '2':
                        iconForm += "{Icon Exclamation}";
                        break;
                    case '3':
                        iconForm += "{Icon EmptryCirlces}";
                        break;
                    case '4':
                        iconForm += "{Icon Greather}";
                        break;
                    case '5':
                        iconForm += "{Icon Less}";
                        break;
                    case '6':
                        iconForm += "{Icon Doc}";
                        break;
                    case '7':
                        iconForm += "{Icon Ok}";
                        break;
                    case '8':
                        iconForm += "{Icon FilledCirlces}";
                        break;
                }
            }

            return iconForm;
        }

        public override void Save()
        {
            Randomizers.SetUIProgress("Saving Text Data...", -1, 100);
            string hash = GetHash();

            mainSysUS["$dif_conf_e"] = "{Icon Attention} You have selected {Color Red}EASY MODE{Color SkyBlue} for battles.{Text NewLine}" +
                "Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
                "Seed Hash (for validation): " + hash + "{Text NewLine}" +
                "Begin playing in {Color Red}EASY MODE{Color SkyBlue}?|Yes|No";
            mainSysUS["$dif_conf_n"] = "{Icon Attention} You have selected {Color Red}NORMAL MODE{Color SkyBlue} for battles.{Text NewLine}" +
                "Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
                "Seed Hash (for validation): " + hash + "{Text NewLine}" +
                "Begin playing in {Color Red}NORMAL MODE{Color SkyBlue}?|Yes|No";

            TempTextCleanup(mainSysUS);

            {
                string outPath = SetupData.OutputFolder + @"\txtres\resident\system\txtres_us.ztr";
                mainSysUS.Save("13", outPath, SetupData.Paths["Nova"]);
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
}

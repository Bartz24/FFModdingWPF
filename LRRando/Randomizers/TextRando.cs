using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bartz24.FF13_2_LR.Enums;

namespace LRRando
{
    public class TextRando : Randomizer
    {
        public DataStoreZTRText zone100SysUS = new DataStoreZTRText();
        public DataStoreZTRText mainSysUS = new DataStoreZTRText();

        public TextRando(RandomizerManager randomizers) : base(randomizers) {  }

        public override string GetProgressMessage()
        {
            return "Randomizing Text...";
        }
        public override string GetID()
        {
            return "Text";
        }

        public override void Load()
        {
            {
                string path = Nova.GetNovaFile("LR", @"txtres\zone\z0100\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["LR"]);
                string outPath = SetupData.OutputFolder + @"\txtres\zone\z0100\txtres_us.ztr";
                FileExtensions.CopyFile(path, outPath);

                zone100SysUS.Load(outPath, SetupData.Paths["Nova"]);
            }

            zone100SysUS["$inn_no_time"] = "You usually need an {Color Gold}ID card{Color SkyBlue}. Open at the cost of all EP?";
            zone100SysUS["$sys_yu_noopn"] = "You need an {Color Gold}ID card{Color SkyBlue} and have to complete the Warehouse to open the gate.";

            {
                string path = Nova.GetNovaFile("LR", @"txtres\resident\system\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["LR"]);
                string outPath = SetupData.OutputFolder + @"\txtres\resident\system\txtres_us.ztr";
                FileExtensions.CopyFile(path, outPath);

                mainSysUS.Load(outPath, SetupData.Paths["Nova"]);
            }
        }
        public override void Randomize(Action<int> progressSetter)
        {
            if (LRFlags.Other.Enemies.FlagEnabled)
            {
                mainSysUS["$m_355"] += " (I may crash with {Key R2})";
                mainSysUS["$m_455"] += " (I may crash with {Key R2})";
                mainSysUS["$m_805"] += " (I may crash with {Key R2})";
                mainSysUS["$m_806w"] += " (I may crash with {Key R2})";
                mainSysUS["$m_821w"] += " (I may crash with {Key R2})";
                mainSysUS["$m_896w"] += " (I may crash with {Key R2})";

                if (LRFlags.Other.Bosses.FlagEnabled)
                {
                }
            }
        }

        private string GetHash()
        {
            string numberForm = RandomNum.GetHash(6);
            string iconForm = "";

            foreach (char c in numberForm)
            {
                switch(c)
                {
                    case '0':
                        iconForm += "{Icon Shotgun}";
                        break;
                    case '1':
                        iconForm += "{Icon Wrench}";
                        break;
                    case '2':
                        iconForm += "{Icon Eye01}";
                        break;
                    case '3':
                        iconForm += "{Icon Doc}";
                        break;
                    case '4':
                        iconForm += "{Icon Potion03}";
                        break;
                    case '5':
                        iconForm += "{Icon Feather}";
                        break;
                    case '6':
                        iconForm += "{Icon Boomerang}";
                        break;
                    case '7':
                        iconForm += "{Icon Gunblade}";
                        break;
                    case '8':
                        iconForm += "{Icon Weapon03}";
                        break;
                    case '9':
                        iconForm += "{Icon Clock}";
                        break;
                }
            }

            return iconForm;
        }

        public override void Save()
        {
            string hash = GetHash();

            mainSysUS["$dif_conf_e"] = "{Icon Attention} You have selected {Color Red}EASY MODE{Color SkyBlue}.{Text NewLine}" +
                "{Text NewLine}" +
                "{VarFF 208}Battle Difficulty: Easy{Text NewLine}" +
                "{VarFF 208}Fleeing battle: No penalty{Text NewLine}" +
                "{VarFF 208}HP (health): Auto-recovery in field{Text NewLine}" +
                "{VarFF 208}Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
                "{VarFF 208}Seed Hash (for validation): " + hash + "{Text NewLine}" +
                "{Text NewLine}" +
                "Game difficulty cannot be changed once the game has started.{Text NewLine}" +
                "{Many}Do you want to continue?|Yes|No";
            mainSysUS["$dif_conf_n"] = "{Icon Attention} You have selected {Color Red}NORMAL MODE{Color SkyBlue}.{Text NewLine}" +
                "{Text NewLine}" +
                "{VarFF 208}Battle Difficulty: Normal{Text NewLine}" +
                "{VarFF 208}Fleeing battle: Penalty imposed{Text NewLine}" +
                "{VarFF 208}HP (health): No auto-recovery in field{Text NewLine}" +
                "{VarFF 208}Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
                "{VarFF 208}Seed Hash (for validation): " + hash + "{Text NewLine}" +
                "{Text NewLine}" +
                "Game difficulty cannot be changed once the game has started.{Text NewLine}" +
                "{Many}Do you want to continue?|Yes|No";
            mainSysUS["$dif_conf_h"] = "{Icon Attention} You have selected {Color Red}HARD MODE{Color SkyBlue}.{Text NewLine}" +
                "{Text NewLine}" +
                "{VarFF 208}Battle Difficulty: Hard{Text NewLine}" +
                "{VarFF 208}Fleeing battle: Penalty imposed{Text NewLine}" +
                "{VarFF 208}HP (health): No auto-recovery in field{Text NewLine}" +
                "{VarFF 208}Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
                "{VarFF 208}Seed Hash (for validation): " + hash + "{Text NewLine}" +
                "{Text NewLine}" +
                "Game difficulty cannot be changed once the game has started.{Text NewLine}" +
                "{Many}Do you want to continue?|Yes|No";

            TempTextCleanup(zone100SysUS);
            TempTextCleanup(mainSysUS);

            {
                string outPath = SetupData.OutputFolder + @"\txtres\zone\z0100\txtres_us.ztr";
                zone100SysUS.Save("LR", outPath, SetupData.Paths["Nova"]);
            }
            {
                string outPath = SetupData.OutputFolder + @"\txtres\resident\system\txtres_us.ztr";
                mainSysUS.Save("LR", outPath, SetupData.Paths["Nova"]);
            }
        }

        private void TempTextCleanup(DataStoreZTRText text)
        {
            text.Keys.ForEach(k =>
            {
                text[k] = text[k].Replace("Ⅷ", "");
                text[k] = text[k].Replace("×", "x");
                text[k] = text[k].Replace("{VarF5 SkyBlue}", "Soul Seeds");
                text[k] = text[k].Replace("{VarF2 PurpleDark}", "  ");
            });
        }
    }
}

using Bartz24.Data;
using Bartz24.FF13Series;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13Rando
{
    public class TextRando : Randomizer
    {
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
                string path = Nova.GetNovaFile("13", @"txtres\resident\system\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["13"]);
                string outPath = SetupData.OutputFolder + @"\txtres\resident\system\txtres_us.ztr";
                FileHelpers.CopyFile(path, outPath);

                mainSysUS.Load(outPath, SetupData.Paths["Nova"]);

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

            }
        }
        public override void Randomize(Action<int> progressSetter)
        {
        }

        private string GetHash()
        {
            string numberForm = RandomNum.GetHash(6);
            string iconForm = "";

            return numberForm;
        }

        public override void Save()
        {

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

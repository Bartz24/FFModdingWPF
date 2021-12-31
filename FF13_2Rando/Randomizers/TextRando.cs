using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.FF13_2LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bartz24.FF13_2_LR.Enums;

namespace FF13_2Rando
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
                string path = Nova.GetNovaFile("13-2", @"txtres\resident\system\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["13-2"]);
                string outPath = SetupData.OutputFolder + @"\txtres\resident\system\txtres_us.ztr";
                FileExtensions.CopyFile(path, outPath);

                mainSysUS.Load(outPath, SetupData.Paths["Nova"]);
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
                mainSysUS.Save("13-2", outPath, SetupData.Paths["Nova"]);
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

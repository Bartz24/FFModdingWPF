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
        Randomizers.SetUIProgress("Loading Text Data...", 0, -1);
        {
            string path = Nova.GetNovaFile("13-2", @"txtres\resident\system\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["13-2"]);
            string outPath = SetupData.OutputFolder + @"\txtres\resident\system\txtres_us.ztr";
            FileHelpers.CopyFile(path, outPath);

            mainSysUS.Load(outPath, SetupData.Paths["Nova"]);
        }

        {
            string path = Nova.GetNovaFile("13-2", @"txtres\resident\game\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["13-2"]);
            string outPath = SetupData.OutputFolder + @"\txtres\resident\game\txtres_us.ztr";
            FileHelpers.CopyFile(path, outPath);

            quizUS.Load(outPath, SetupData.Paths["Nova"]);
        }
    }
    public override void Randomize()
    {
        Randomizers.SetUIProgress("Randomizing Text Data...", 0, -1);
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

        mainSysUS["$dif_conf_e"] = "{Icon Attention} Begin game in {Color Red}EASY MODE{Color SkyBlue}?{Text NewLine}" +
            "Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
            "Seed Hash (for validation): " + hash + "{Text NewLine}|Yes|No";
        mainSysUS["$dif_conf_n"] = "{Icon Attention} Begin game in {Color Red}NORMAL MODE{Color SkyBlue}?{Text NewLine}" +
            "Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
            "Seed Hash (for validation): " + hash + "{Text NewLine}|Yes|No";

        TempTextCleanup(mainSysUS);
        TempTextCleanup(quizUS);

        {
            string outPath = SetupData.OutputFolder + @"\txtres\resident\system\txtres_us.ztr";
            mainSysUS.Save("13-2", outPath, SetupData.Paths["Nova"]);
        }

        {
            string outPath = SetupData.OutputFolder + @"\txtres\resident\game\txtres_us.ztr";
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

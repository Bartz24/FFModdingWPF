using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13Rando;
public class FF13SeedGenerator : SeedGenerator
{
    public FF13SeedGenerator()
    {
        Randomizers = new()
        {
            new EquipRando(this),
            new TreasureRando(this),
            new CrystariumRando(this),
            new ShopRando(this),
            new BattleRando(this),
            new EnemyRando(this),
            new MusicRando(this),
            new TextRando(this)
        };

        OutFolder = Path.GetTempPath() + @"ff13_rando_temp";
        DataOutFolder = OutFolder + @"\Data";
        SetupData.OutputFolder = DataOutFolder;

        PackPrefixName = "FF13Rando";
        DocsDisplayName = "FF13 Randomizer";
    }

    protected override void PrepareData()
    {

        if (string.IsNullOrEmpty(SetupData.Paths["13"]) || !Directory.Exists(SetupData.Paths["13"]))
        {
            throw new RandoException("The path for FF13 is not valid. Setup the path in the '1. Setup' step.", "FF13 not found.");
        }

        if (string.IsNullOrEmpty(SetupData.Paths["Nova"]) || !File.Exists(SetupData.Paths["Nova"]))
        {
            throw new RandoException("NovaChrysalia.exe needs to be selected. Download Nova Chrysalia and setup the path in the '1. Setup' step.", "Nova Chrysalia not found.");
        }

        if (!Nova.IsUnpacked("13", @"db\resident\treasurebox.wdb", SetupData.GetSteamPath("13")))
        {
            throw new RandoException("FF13 needs to be unpacked.\nOpen NovaChrysalia and 'Unpack Game Data' for FF13.", "FF13 is not unpacked");
        }

        if (Directory.Exists(OutFolder))
        {
            Directory.Delete(OutFolder, true);
        }

        Directory.CreateDirectory(OutFolder);
        FileHelpers.CopyFromFolder(OutFolder, "data\\modpack");
        RandoHelpers.UpdateSeedInFile(OutFolder + "\\modconfig.ini", GetIntSeed().ToString());
        File.Move(OutFolder + "\\Code\\patch.nccp", OutFolder + $"\\Code\\FF13 Randomizer {GetIntSeed()}.nccp");

        SetupData.WPDTracking.Clear();

        base.PrepareData();
    }

    public override string GetPackPath()
    {
        return $"{PackPrefixName}_{GetIntSeed()}.ncmp";
    }

    protected override void GeneratePackAndDocs()
    {
        base.GeneratePackAndDocs();

        SetUIProgress($"Complete! Ready to install in Nova Chrysalia! The modpack '{GetPackPath()}' and documentation have been generated in the packs folder of this application.", 100, 100);
    }
}

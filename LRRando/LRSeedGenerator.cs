
using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;

namespace LRRando;
public class LRSeedGenerator : SeedGenerator
{
    public LRSeedGenerator(Action<string, int, int> setUIProgress) : base(setUIProgress)
    {
        Randomizers = new()
        {
            new QuestRando(this),
            new TreasureRando(this),
            new EquipRando(this),
            new ShopRando(this),
            new AbilityRando(this),
            new EnemyRando(this),
            new BattleRando(this),
            new MusicRando(this),
            new TextRando(this)
        };

        OutFolder = Path.GetTempPath() + @"lr_rando_temp";
        DataOutFolder = OutFolder + @"\Data";

        PackPrefixName = "LRRando";
        DocsDisplayName = "LR Randomizer";
    }

    protected override void PrepareData()
    {
        if (string.IsNullOrEmpty(SetupData.Paths["LR"]) || !Directory.Exists(SetupData.Paths["LR"]))
        {
           throw new RandoException("The path for LR is not valid. Setup the path in the '1. Setup' step.", "LR not found.");
        }

        if (string.IsNullOrEmpty(SetupData.Paths["Nova"]) || !File.Exists(SetupData.Paths["Nova"]))
        {
            throw new RandoException("NovaChrysalia.exe needs to be selected. Download Nova Chrysalia and setup the path in the '1. Setup' step.", "Nova Chrysalia not found.");
        }

        if (!Nova.IsUnpacked("LR", @"db\resident\wdbpack.bin", SetupData.GetSteamPath("LR")))
        {
            throw new RandoException("LR needs to be unpacked.\nOpen NovaChrysalia and 'Unpack Game Data' for LR.", "LR is not unpacked");
        }

        if (Directory.Exists(OutFolder))
        {
            Directory.Delete(OutFolder, true);
        }

        Directory.CreateDirectory(OutFolder);
        FileHelpers.CopyFromFolder(OutFolder, "data\\modpack");
        RandoHelpers.UpdateSeedInFile(OutFolder + "\\modconfig.ini", GetIntSeed().ToString());

        string wdbpackPath = Nova.GetNovaFile("LR", @"db\resident\wdbpack.bin", SetupData.Paths["Nova"], SetupData.Paths["LR"]);
        string wdbpackOutPath = DataOutFolder + @"\db\resident\wdbpack.bin";
        FileHelpers.CopyFile(wdbpackPath, wdbpackOutPath);
        SetupData.WPDTracking.Clear();
        SetupData.WPDTracking.Add(wdbpackOutPath, new List<string>());
        Nova.UnpackWPD(wdbpackOutPath, SetupData.Paths["Nova"]);

        FileHelpers.CopyFromFolder(DataOutFolder + "\\db\\resident\\_wdbpack.bin", DataOutFolder + "\\db\\resident\\_wdbpack.bin.rando");
        foreach (string file in Directory.GetFiles(DataOutFolder + "\\db\\resident\\_wdbpack.bin.rando"))
        {
            SetupData.WPDTracking[wdbpackOutPath].Add(System.IO.Path.GetFileName(file));
        }

        Directory.Delete(DataOutFolder + "\\db\\resident\\_wdbpack.bin.rando", true);

        base.PrepareData();
    }

    public override string GetPackPath()
    {
        return $"{PackPrefixName}_{GetIntSeed()}.ncmp";
    }

    protected override void Save()
    {
        base.Save();

        string wdbpackOutPath = DataOutFolder + @"\db\resident\wdbpack.bin";
        Nova.CleanWPD(wdbpackOutPath, SetupData.WPDTracking[wdbpackOutPath]);
    }

    protected override void GeneratePackAndDocs()
    {
        base.GeneratePackAndDocs();

        SetUIProgress($"Complete! Ready to install in Nova Chrysalia! The modpack '{GetPackPath()}' and documentation have been generated in the packs folder of this application.", 100, 100);
    }
}

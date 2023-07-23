using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF12Rando;
public class FF12SeedGenerator : SeedGenerator
{
    public FF12SeedGenerator()
    {
        Randomizers = new()
        {
            new PartyRando(this),
            new TreasureRando(this),
            new EquipRando(this),
            new LicenseBoardRando(this),
            new EnemyRando(this),
            new ShopRando(this),
            new TextRando(this),
            new MusicRando(this)
        };

        OutFolder = Path.GetTempPath() + @"ff12_rando_temp";
        DataOutFolder = OutFolder + @"\Data";
        SetupData.OutputFolder = DataOutFolder;

        PackPrefixName = "FF12Rando";
        DocsDisplayName = "FF12 Randomizer";
    }

    protected override void PrepareData()
    {
        if (!ToolsInstalled())
        {
            throw new RandoException("Text and script tools are not installed. Download and install them on 1. Setup.", "Tools missing.");
        }

        if (Directory.Exists(OutFolder))
        {
            Directory.Delete(OutFolder, true);
        }

        Directory.CreateDirectory(OutFolder);
        FileHelpers.CopyFromFolder(OutFolder, "data\\modpack");

        SetupData.WPDTracking.Clear();

        base.PrepareData();
    }

    protected override void GeneratePackAndDocs()
    {
        base.GeneratePackAndDocs();

        SetUIProgress($"Complete! Ready to install in Nova Chrysalia! The modpack '{GetPackPath()}' and documentation have been generated in the packs folder of this application.", 100, 100);
    }
    public static bool ToolsInstalled()
    {
        return File.Exists("data\\tools\\ff12-text.exe") && File.Exists("data\\tools\\ff12-ebppack.exe") && File.Exists("data\\tools\\ff12-ebpunpack.exe");
    }
}

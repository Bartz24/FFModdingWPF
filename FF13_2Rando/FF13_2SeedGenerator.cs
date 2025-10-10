using Bartz24.Data;
using Bartz24.FF13Series;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando;
public class FF13_2SeedGenerator : SeedGenerator
{  
    public FF13_2SeedGenerator() : base()
    {
        OutFolder = Path.GetTempPath() + @"ff13_2_rando_temp";
        DataOutFolder = OutFolder + @"\Data";

        PackPrefixName = "FF13_2Rando";
        DocsDisplayName = "FF13-2 Randomizer";
    }

    public override void PrepareData()
    {
        if (string.IsNullOrEmpty(SetupData.Paths["13-2"]) || !Directory.Exists(SetupData.Paths["13-2"]))
        {
            throw new RandoException("The path for FF13-2 is not valid. Setup the path in the '1. Setup' step.", "FF13-2 not found.");
        }

        if (string.IsNullOrEmpty(SetupData.Paths["Nova"]) || !File.Exists(SetupData.Paths["Nova"]))
        {
            throw new RandoException("NovaChrysalia.exe needs to be selected. Download Nova Chrysalia and setup the path in the '1. Setup' step.", "Nova Chrysalia not found.");
        }

        if (!Nova.IsNovaVersion2(SetupData.Paths["Nova"]))
        {
            throw new RandoException("Nova Chrysalia needs to be version 2.X+. Nova Chrysalia version 2.X+ is currently available as a beta. Click the download button in the '1. Setup' step to get a supported version.", "Nova Chrysalia is not version 2.X+");
        }

        if (!Nova.IsUnpacked("13-2", @"db\resident\wdbpack.bin", SetupData.Paths["13-2"]))
        {
            throw new RandoException("FF13-2 needs to be unpacked.\nOpen NovaChrysalia and 'Unpack Game Data' for FF13-2.", "FF13-2 is not unpacked");
        }

        if (FF13_2Flags.Other.RandoDLC.Enabled && !Nova.IsModInstalled(SetupData.Paths["Nova"], "DLC Restoration - Console Content", "13-2"))
        {
            throw new RandoException("The 'Include DLC Areas' flag was turned on and requires the following mod that is detected to be missing:\n" +
                "'DLC Restoration - Console Content'\n\n" +
                "Download and install the mod from the Core Mods download in the Nova discord server.\n" +
                "Once this mod is installed, you will be able to generate the rando modpack.", "Additional mods required");
        }

        if (Directory.Exists(OutFolder))
        {
            Directory.Delete(OutFolder, true);
        }

        Directory.CreateDirectory(OutFolder);
        FileHelpers.CopyFromFolder(OutFolder, "data\\modpack");
        RandoHelpers.UpdateSeedInFile(OutFolder + "\\modconfig.ini", SetupData.Seed.Clean());

        string wdbpackPath = Nova.GetNovaFile("13-2", @"db\resident\wdbpack.bin", SetupData.Paths["Nova"], SetupData.Paths["13-2"]);
        string wdbpackOutPath = DataOutFolder + @"\db\resident\wdbpack.bin";
        FileHelpers.CopyFile(wdbpackPath, wdbpackOutPath);

        string x000Path = Nova.GetNovaFile("13-2", @"btscene\pack\wdb\x000.bin", SetupData.Paths["Nova"], SetupData.Paths["13-2"]);
        string x000OutPath = DataOutFolder + @"\btscene\pack\wdb\x000.bin";
        FileHelpers.CopyFile(x000Path, x000OutPath);

        SetupData.WPDTracking.Clear();
        SetupData.WPDTracking.Add(wdbpackOutPath, new List<string>());
        SetupData.WPDTracking.Add(x000OutPath, new List<string>());
        Nova.UnpackWPD(wdbpackOutPath, SetupData.Paths["Nova"]);
        Nova.UnpackWPD(x000OutPath, SetupData.Paths["Nova"]);

        base.PrepareData();
    }

    public override string GetPackPath()
    {
        return $"{PackPrefixName}_{SetupData.Seed.Clean()}.ncmp";
    }

    public override void Save()
    {
        base.Save();

        string wdbpackOutPath = DataOutFolder + @"\db\resident\wdbpack.bin";
        string x000OutPath = DataOutFolder + @"\btscene\pack\wdb\x000.bin";
        Nova.CleanWPD(wdbpackOutPath, SetupData.WPDTracking[wdbpackOutPath]);
        Nova.CleanWPD(x000OutPath, SetupData.WPDTracking[x000OutPath]);
    }

    public override void GeneratePackAndDocs()
    {
        base.GeneratePackAndDocs();

        RandoUI.SetUIProgressDeterminate($"Complete! Ready to install in Nova Chrysalia! The modpack '{GetPackPath()}' and documentation have been generated in the packs folder of this application.", 100, 100);
    }

    protected override void SetRandomizers()
    {
        Randomizers = new()
        {
            new CrystariumRando(this),
            new EquipRando(this),
            new HistoriaCruxRando(this),
            new TreasureRando(this),
            new BattleRando(this),
            new EnemyRando(this),
            new MusicRando(this),
            new TextRando(this)
        };
    }
}

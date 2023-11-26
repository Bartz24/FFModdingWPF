using Bartz24.Data;
using Bartz24.RandoWPF;
using FF12Rando.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF12Rando;
public class FF12SeedGenerator : SeedGenerator
{
    private static List<string> ToolPaths
    {
        get => new()
        {
            "data\\tools\\ff12-text.exe",
            "data\\tools\\ff12-ebppack.exe",
            "data\\tools\\ff12-ebpunpack.exe"
        };
    }
    private static List<string> FileLoaderPaths
    {
        get => new()
        {
            Path.Combine(SetupData.Paths["12"], "x64\\ff12-trampoline.dll"),
            Path.Combine(SetupData.Paths["12"], "x64\\vcruntime140_1.dll"),
            Path.Combine(SetupData.Paths["12"], "x64\\modules\\ff12-file-loader.dll"),
            Path.Combine(SetupData.Paths["12"], "x64\\modules\\config\\ff12-file-loader.ini")
        };
    }
    private static List<string> LuaLoaderPaths
    {
        get => new()
        {
            Path.Combine(SetupData.Paths["12"], "x64\\modules\\ff12-lua-loader.dll")
        };
    }
    private static List<string> DescriptivePaths
    {
        get => new()
        {
            Path.Combine(SetupData.Paths["12"], "x64\\scripts\\TheInsurgentsDescriptiveInventory.lua"),
            Path.Combine(SetupData.Paths["12"], "x64\\scripts\\TheInsurgentsDescriptiveInventory\\helpers.lua"),
            Path.Combine(SetupData.Paths["12"], "x64\\scripts\\config\\TheInsurgentsDescriptiveInventoryConfig\\us.lua")
        };
    }

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
            new MusicRando(this),
            new TextRando(this)
        };

        OutFolder = Path.Combine(SetupData.Paths["12"], "rando");
        DataOutFolder = Path.Combine(OutFolder, "ps2data");

        PackPrefixName = "FF12Rando";
        DocsDisplayName = "FF12 Randomizer";
        DocsOutFolder = "docs";

        AeropassItemReq.Init();
    }

    protected override void PrepareData()
    {
        if (!ToolsInstalled())
        {
            throw new RandoException("Text and script tools are not properly installed. Download and install them on 1. Setup.", "Tools missing.");
        }

        if (!FileLoaderInstalled())
        {
            throw new RandoException("External File Loader is not properly installed. Download and install them on 1. Setup.", "External File Loader missing.");
        }

        if (!LuaLoaderInstalled())
        {
            throw new RandoException("Lua Loader is not properly installed. Download and install them on 1. Setup.", "Lua missing.");
        }

        if (Directory.Exists(OutFolder))
        {
            Directory.Delete(OutFolder, true);
        }

        Directory.CreateDirectory(OutFolder);
        FileHelpers.CopyFromFolder(Path.Combine(OutFolder, "ps2data"), "data\\ps2data");

        SetupData.WPDTracking.Clear();

        UpdateLoaderConfig();
        RemoveLuaScripts();
        CopyLuaScripts();

        base.PrepareData();
    }

    private void UpdateLoaderConfig()
    {
        string filePath = Path.Combine(SetupData.Paths["12"], "x64\\modules\\config\\ff12-file-loader.ini");
        List<string> lines = File.ReadAllLines(filePath).ToList();

        int pathsStart = lines.FindIndex(s => s.Trim() == "[Paths]");

        lines = lines.Where(s => !s.Trim().StartsWith("rando=")).ToList();
        lines.Insert(pathsStart + 1, "rando=rando");

        File.WriteAllLines(filePath, lines);
    }

    private void CopyLuaScripts()
    {
        string scriptsFolder = Path.Combine(SetupData.Paths["12"], "x64\\scripts");

        FileHelpers.CopyFromFolder(scriptsFolder, "data\\scripts");
    }

    public void RemoveLuaScripts()
    {
        string scriptsFolder = Path.Combine(SetupData.Paths["12"], "x64\\scripts");

        Directory.GetFiles(scriptsFolder).Where(s => Path.GetFileName(s).StartsWith("Rando")).ForEach(s => File.Delete(s));

        string descriptiveFolder = $"{SetupData.Paths["12"]}\\x64\\scripts\\config\\TheInsurgentsDescriptiveInventoryConfig";
        if (File.Exists(Path.Combine(descriptiveFolder, "us.lua.before_rando")))
        {
            File.Move(Path.Combine(descriptiveFolder, "us.lua.before_rando"), Path.Combine(descriptiveFolder, "us.lua"), true);
        }
    }

    protected override void GeneratePack()
    {
        // No pack for FF12
    }

    protected override void GeneratePackAndDocs()
    {
        base.GeneratePackAndDocs();

        SetUIProgress($"Complete! Ready to play! The documentation have been generated in the docs folder of this application.", 100, 100);
    }
    public static bool ToolsInstalled()
    {
        return ToolPaths.All(s => File.Exists(s));
    }

    public static bool FileLoaderInstalled()
    {
        return FileLoaderPaths.All(s => File.Exists(s));
    }

    public static void UninstallFileLoader()
    {
        FileLoaderPaths.Where(s => File.Exists(s)).ForEach(s => File.Delete(s));
    }

    public static bool LuaLoaderInstalled()
    {
        return LuaLoaderPaths.All(s => File.Exists(s));
    }

    public static void UninstallLuaLoader()
    {
        LuaLoaderPaths.Where(s => File.Exists(s)).ForEach(s => File.Delete(s));
    }

    public static bool DescriptiveInstalled()
    {
        return DescriptivePaths.All(s => File.Exists(s));
    }

    public static void UninstallDescriptive()
    {
        DescriptivePaths.Where(s => File.Exists(s)).ForEach(s => File.Delete(s));
    }
}

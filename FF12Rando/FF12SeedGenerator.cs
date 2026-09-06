using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FF12Rando;
public class FF12SeedGenerator : SeedGenerator
{
    /// <summary>
    /// The rando and a locally installed Manifesto share the seed folder, but they write to disjoint
    /// paths inside it. Uninstalling either one removes only the paths it owns and leaves the rest of
    /// the folder alone.
    /// </summary>
    private static readonly List<string> RandoDataSubPaths = new()
    {
        "image\\ff12\\myoshiok",
        "image\\ff12\\test_battle",
        "plan_master",
        "sound"
    };

    public static string SeedFolder => Path.Combine(SetupData.Paths["12"], "rando");

    public static string SeedDataFolder => Path.Combine(SeedFolder, "ps2data");

    public FF12SeedGenerator() : base()
    {
        OutFolder = SeedFolder;
        DataOutFolder = SeedDataFolder;

        PackPrefixName = "FF12Rando";
        DocsDisplayName = "FF12 Randomizer";
        DocsOutFolder = "docs";

        ItemReq.ItemProvider = () => Get<EquipRando>().itemData.ToDictionary(kv => kv.Key, i => (IItem)i.Value);
        ItemReq.ItemLocationProvider = () => Get<TreasureRando>().ItemLocations;
    }

    protected override void SetRandomizers()
    {
        Randomizers = new()
        {
            new PartyRando(this),
            new TreasureRando(this),
            new EquipRando(this),
            new LicenseRando(this),
            new LicenseBoardRando(this),
            new EnemyRando(this),
            new ShopRando(this),
            new MusicRando(this),
            new TextRando(this)
        };
    }

    public override void PrepareData()
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            throw new RandoException("Missing steam path", "Invalid path");
        }

        foreach (FF12Mod mod in FF12Mods.Required)
        {
            if (!mod.IsInstalled())
            {
                throw new RandoException($"{mod.Name} is not properly installed. Download and install it on 1. Setup.", $"{mod.Name} missing.");
            }

            if (!mod.IsUpToDate())
            {
                throw new RandoException($"{mod.Name} is {FF12Mod.DescribeVersion(mod.GetVersion())}, but version {mod.MinimumVersion} or newer is required. Download and install a newer version on 1. Setup.", $"{mod.Name} outdated.");
            }
        }

        if (RandoFlags.Mode != RandoFlags.SeedMode.Archipelago)
        {
            if (FF12Flags.Items.Treasures.FlagEnabled && FF12Flags.Items.WritGoals.SelectedIndices.Count == 0)
            {
                throw new RandoException("Item location randomization is turned on but there is no goal selected. Select at least 1 Bahamut unlock condition.", "No goal selected.");
            }
        }

        // Clear out the last seed without touching a Manifesto sharing the folder.
        RemoveFromSeedFolder(RandoDataSubPaths);

        Directory.CreateDirectory(OutFolder);
        FileHelpers.CopyFromFolder(Path.Combine(OutFolder, "ps2data"), "data\\ps2data");

        SetupData.WPDTracking.Clear();

        UpdateLoaderConfig();
        RemoveAndMoveLuaScripts();
        CopyLuaScripts();

        base.PrepareData();
    }

    private static string LoaderConfigPath => Path.Combine(SetupData.Paths["12"], "x64\\modules\\config\\ff12-file-loader.ini");

    private static string DescriptiveConfigFolder => Path.Combine(SetupData.Paths["12"], "x64\\scripts\\config\\TheInsurgentsDescriptiveInventoryConfig");

    public static void UpdateLoaderConfig()
    {
        string filePath = LoaderConfigPath;
        if (!File.Exists(filePath))
        {
            return;
        }

        List<string> lines = File.ReadAllLines(filePath).ToList();
        lines.RemoveAll(s => s.Trim().StartsWith("rando="));

        // Find the header after the removal so the insert index cannot be shifted by it.
        int pathsStart = lines.FindIndex(s => s.Trim() == "[Paths]");
        if (pathsStart < 0)
        {
            return;
        }

        if (Directory.Exists(SeedFolder))
        {
            lines.Insert(pathsStart + 1, "rando=rando");
        }

        File.WriteAllLines(filePath, lines);
    }

    public static void RemoveFromSeedFolder(List<string> subPaths)
    {
        foreach (string subPath in subPaths)
        {
            string path = Path.Combine(SeedDataFolder, subPath);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        if (Directory.Exists(SeedFolder))
        {
            DeleteEmptyFolders(SeedFolder);
        }

        UpdateLoaderConfig();
    }

    private static void DeleteEmptyFolders(string folder)
    {
        foreach (string subFolder in Directory.GetDirectories(folder))
        {
            DeleteEmptyFolders(subFolder);
        }

        if (!Directory.EnumerateFileSystemEntries(folder).Any())
        {
            Directory.Delete(folder);
        }
    }

    protected virtual void CopyLuaScripts()
    {
        string scriptsFolder = Path.Combine(SetupData.Paths["12"], "x64\\scripts");

        FileHelpers.CopyFromFolder(scriptsFolder, "data\\scripts");
    }

    public void RemoveAndMoveLuaScripts()
    {
        RemoveRandoLuaScripts();

        // Only back up the descriptive inventory config the first time, so a rando generated config
        // never gets mistaken for the player's own.
        if (!File.Exists(Path.Combine(DescriptiveConfigFolder, "us.lua.before_rando")))
        {
            MoveToBackup(Path.Combine(DescriptiveConfigFolder, "us.lua"), false);
        }

        RemoveGeneratedDescriptiveConfig();
    }

    public void UninstallSeed()
    {
        RemoveRandoLuaScripts();
        RemoveGeneratedDescriptiveConfig();
        RestoreFromBackup(Path.Combine(DescriptiveConfigFolder, "us.lua"));
        RemoveFromSeedFolder(RandoDataSubPaths);
    }

    private static void RemoveRandoLuaScripts()
    {
        string scriptsFolder = Path.Combine(SetupData.Paths["12"], "x64\\scripts");
        if (!Directory.Exists(scriptsFolder))
        {
            return;
        }

        Directory.GetFiles(scriptsFolder).Where(s => Path.GetFileName(s).StartsWith("Rando")).ForEach(s => File.Delete(s));
    }

    private static void RemoveGeneratedDescriptiveConfig()
    {
        foreach (string name in new List<string>() { "us.lua", "us.lua.page1", "us.lua.page2" })
        {
            string path = Path.Combine(DescriptiveConfigFolder, name);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    public override void GeneratePack()
    {
        // No pack for FF12
    }

    public override void GeneratePackAndDocs()
    {
        RandoUI.SetUIProgressIndeterminate("Generating documentation...");

        base.GeneratePackAndDocs();

        RandoUI.SetUIProgressDeterminate($"Complete! Ready to play! The documentation has been generated in the docs folder of this application.", 100, 100);
    }
    public static void MoveToBackup(string path, bool makeCopy = false)
    {
        if (File.Exists(path) && !File.Exists(path + ".before_rando"))
        {
            if (makeCopy)
            {
                File.Copy(path, path + ".before_rando");
            }
            else
            {
                File.Move(path, path + ".before_rando");
            }
        }
    }

    public static void RestoreFromBackup(string path)
    {
        if (!File.Exists(path + ".before_rando"))
        {
            return;
        }

        File.Move(path + ".before_rando", path, true);
    }
}

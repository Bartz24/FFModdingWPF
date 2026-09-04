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
        get
        {
            if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
            {
                throw new RandoException("Missing steam path", "Invalid path");
            }

            return new()
            {
                Path.Combine(SetupData.Paths["12"], "x64\\ff12-trampoline.dll"),
                Path.Combine(SetupData.Paths["12"], "x64\\simpleLog.dll"),
                Path.Combine(SetupData.Paths["12"], "x64\\modules\\ff12-file-loader.dll"),
                Path.Combine(SetupData.Paths["12"], "x64\\modules\\config\\ff12-file-loader.ini")
            };
        }
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
        get
        {
            if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
            {
                throw new RandoException("Missing steam path", "Invalid path");
            }

            return new()
            {
                Path.Combine(SetupData.Paths["12"], "x64\\scripts\\TheInsurgentsDescriptiveInventory.lua"),
                Path.Combine(SetupData.Paths["12"], "x64\\scripts\\TheInsurgentsDescriptiveInventory\\helpers.lua")
            };
        }
    }

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

    private static readonly List<string> ManifestoDataSubPaths = new()
    {
        "image\\ff12\\in",
        "obj_finish"
    };

    private const string ManifestoSkillMotionSubPath = "image\\ff12\\in\\common\\pc_skillmotion.bin";
    private const string ManifestoCharaSubPath = "obj_finish\\in\\chara";

    public static string SeedFolder => Path.Combine(SetupData.Paths["12"], "rando");

    public static string SeedDataFolder => Path.Combine(SeedFolder, "ps2data");

    private static string ManifestoScriptPath => Path.Combine(SetupData.Paths["12"], "x64\\scripts\\TheInsurgentsManifesto.lua");

    private static List<string> ManifestoRequiredPathsVortexInstall
    {
        get
        {
            if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
            {
                throw new RandoException("Missing steam path", "Invalid path");
            }

            return new()
            {
                Path.Combine(SetupData.Paths["12"], "mods\\deploy\\ps2data\\image\\ff12\\in\\common\\pc_skillmotion.bin"),
                ManifestoScriptPath
            };
        }
    }

    public static readonly Version MinFileLoaderVersion = new(1, 5, 2);
    public static readonly Version MinLuaLoaderVersion = new(1, 10, 2);

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
        if (!ToolsInstalled())
        {
            throw new RandoException("Text and script tools are not properly installed. Download and install them on 1. Setup.", "Tools missing.");
        }

        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            throw new RandoException("Missing steam path", "Invalid path");
        }

        if (!FileLoaderInstalled())
        {
            throw new RandoException("External File Loader is not properly installed. Download and install them on 1. Setup.", "External File Loader missing.");
        }

        if (!FileLoaderUpToDate())
        {
            throw new RandoException($"The External File Loader is {DescribeVersion(GetFileLoaderVersion())}, but version {MinFileLoaderVersion} or newer is required. Download and install a newer version on 1. Setup.", "External File Loader outdated.");
        }

        if (!LuaLoaderInstalled())
        {
            throw new RandoException("Lua Loader is not properly installed. Download and install them on 1. Setup.", "Lua missing.");
        }

        if (!LuaLoaderUpToDate())
        {
            throw new RandoException($"The Lua Loader is {DescribeVersion(GetLuaLoaderVersion())}, but version {MinLuaLoaderVersion} or newer is required. Download and install a newer version on 1. Setup.", "Lua Loader outdated.");
        }

        if (ManifestoInstalled() == ManifestoInstallType.Missing)
        {
            throw new RandoException("The Insurgent's Manifesto is not properly installed. Download and install them on 1. Setup.", "Insurgent's Manifesto missing.");
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

    private static void RemoveFromSeedFolder(List<string> subPaths)
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
    public static bool ToolsInstalled()
    {
        return ToolPaths.All(s => File.Exists(s));
    }

    public static bool FileLoaderInstalled()
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            return false;
        }

        return FileLoaderPaths.All(s => File.Exists(s));
    }

    public static void UninstallFileLoader()
    {
        FileLoaderPaths.Where(s => File.Exists(s)).ForEach(s => File.Delete(s));

        // Remove the old vcruntime140_1.dll
        if (File.Exists(Path.Combine(SetupData.Paths["12"], "x64\\vcruntime140_1.dll")))
        {
            File.Delete(Path.Combine(SetupData.Paths["12"], "x64\\vcruntime140_1.dll"));
        }

        // Remove dinput8.dll
        if (File.Exists(Path.Combine(SetupData.Paths["12"], "x64\\dinput8.dll")))
        {
            File.Delete(Path.Combine(SetupData.Paths["12"], "x64\\dinput8.dll"));
        }

        // Remove dxgi.dll
        if (File.Exists(Path.Combine(SetupData.Paths["12"], "x64\\dxgi.dll")))
        {
            File.Delete(Path.Combine(SetupData.Paths["12"], "x64\\dxgi.dll"));
        }
    }

    public static bool LuaLoaderInstalled()
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            return false;
        }

        return LuaLoaderPaths.All(s => File.Exists(s));
    }

    public static void UninstallLuaLoader()
    {
        LuaLoaderPaths.Where(s => File.Exists(s)).ForEach(s => File.Delete(s));
    }

    public static bool FileLoaderUpToDate()
    {
        return IsAtLeast(GetFileLoaderVersion(), MinFileLoaderVersion);
    }

    public static bool LuaLoaderUpToDate()
    {
        return IsAtLeast(GetLuaLoaderVersion(), MinLuaLoaderVersion);
    }

    /// <summary>
    /// A dll with no readable version is treated as too old: every loader new enough to be supported
    /// stamps its version, so an unreadable one is never a build the rando can rely on.
    /// </summary>
    private static bool IsAtLeast(Version actual, Version minimum)
    {
        return actual != null && actual >= minimum;
    }

    public static string DescribeVersion(Version version)
    {
        return version == null ? "an unknown version" : $"version {version}";
    }

    public static Version GetFileLoaderVersion()
    {
        return GetDllVersion("x64\\modules\\ff12-file-loader.dll");
    }

    public static Version GetLuaLoaderVersion()
    {
        return GetDllVersion("x64\\modules\\ff12-lua-loader.dll");
    }

    /// <summary>
    /// Reads a loader module's file version, or null when the game path is unset, the dll is not
    /// there, or it carries no version resource.
    /// </summary>
    private static Version GetDllVersion(string relativePath)
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            return null;
        }

        string path = Path.Combine(SetupData.Paths["12"], relativePath);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
            if (info.FileMajorPart == 0 && info.FileMinorPart == 0 && info.FileBuildPart == 0)
            {
                return null;
            }

            // The loaders version themselves as major.minor.patch; the private part is always 0.
            return new Version(info.FileMajorPart, info.FileMinorPart, info.FileBuildPart);
        }
        catch
        {
            return null;
        }
    }

    public static bool DescriptiveInstalled()
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            return false;
        }

        return DescriptivePaths.All(s => File.Exists(s));
    }

    public static void UninstallDescriptive()
    {
        DescriptivePaths.Where(s => File.Exists(s)).ForEach(s => File.Delete(s));
    }

    public enum ManifestoInstallType
    {
        Missing,
        Vortex,
        Rando
    }

    public static ManifestoInstallType ManifestoInstalled()
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            return ManifestoInstallType.Missing;
        }

        if (File.Exists(Path.Combine(SeedDataFolder, ManifestoSkillMotionSubPath))
            && File.Exists(ManifestoScriptPath)
            && Directory.Exists(Path.Combine(SeedDataFolder, ManifestoCharaSubPath)))
        {
            return ManifestoInstallType.Rando;
        }

        if (ManifestoRequiredPathsVortexInstall.All(s => File.Exists(s)) && Directory.Exists(Path.Combine(SetupData.Paths["12"], "mods\\deploy\\ps2data\\obj_finish\\in\\chara")))
        {
            return ManifestoInstallType.Vortex;
        }

        return ManifestoInstallType.Missing;
    }

    public static void UninstallManifesto()
    {
        if (File.Exists(ManifestoScriptPath))
        {
            File.Delete(ManifestoScriptPath);
        }

        RemoveFromSeedFolder(ManifestoDataSubPaths);
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

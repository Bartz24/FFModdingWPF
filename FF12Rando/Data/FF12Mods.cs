using Bartz24.Data;
using SharpCompress.Archives.SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando;

/// <summary>
/// The tools ship as loose exes next to the rando rather than in the game folder, and the archive
/// has no folder worth mirroring, so everything is flattened into one directory.
/// </summary>
public class FF12ToolsMod : FF12Mod
{
    public const string ToolsFolder = "data\\tools";

    protected override bool NeedsGamePath => false;

    /// <summary>
    /// These sit in the rando's own folder rather than the game's, so there is nothing to hand back
    /// to the player by removing them: it would only stop the rando working until reinstalled.
    /// </summary>
    public override bool CanUninstall => false;

    protected override string ResolvePath(string path)
    {
        return path;
    }

    public override void Install(string archivePath)
    {
        if (Directory.Exists(ToolsFolder))
        {
            Directory.Delete(ToolsFolder, true);
        }

        Directory.CreateDirectory(ToolsFolder);

        using SevenZipArchive archive = SevenZipArchive.Open(archivePath);
        using SharpCompress.Readers.IReader reader = archive.ExtractAllEntries();
        while (reader.MoveToNextEntry())
        {
            if (reader.Entry.IsDirectory || !reader.Entry.Key.EndsWith(".exe"))
            {
                continue;
            }

            string extractedPath = Path.Combine(ToolsFolder, Path.GetFileName(reader.Entry.Key));
            using SharpCompress.Common.EntryStream entryStream = reader.OpenEntryStream();
            using FileStream writeStream = File.OpenWrite(extractedPath);
            entryStream.CopyTo(writeStream);
        }
    }
}

/// <summary>
/// The Manifesto can come from Vortex instead of the rando, which is a working install the rando
/// must not tread on. Its data also shares the seed folder, so removing it goes through the seed
/// folder logic rather than deleting paths directly.
/// </summary>
public class FF12ManifestoMod : FF12Mod
{
    public const string SkillMotionSubPath = "image\\ff12\\in\\common\\pc_skillmotion.bin";
    public const string CharaSubPath = "obj_finish\\in\\chara";

    public static string ScriptPath => GameFile("x64\\scripts\\TheInsurgentsManifesto.lua");

    /// <summary>The parts of the shared seed folder that belong to the Manifesto, not the rando.</summary>
    public static readonly List<string> DataSubPaths = new()
    {
        "image\\ff12\\in",
        "obj_finish"
    };

    private static bool InstalledByRando()
    {
        return File.Exists(Path.Combine(FF12SeedGenerator.SeedDataFolder, SkillMotionSubPath))
            && File.Exists(ScriptPath)
            && Directory.Exists(Path.Combine(FF12SeedGenerator.SeedDataFolder, CharaSubPath));
    }

    private static bool InstalledByVortex()
    {
        return File.Exists(GameFile("mods\\deploy\\ps2data\\" + SkillMotionSubPath))
            && File.Exists(ScriptPath)
            && Directory.Exists(GameFile("mods\\deploy\\ps2data\\" + CharaSubPath));
    }

    public override bool IsInstalled()
    {
        return GamePathValid && (InstalledByRando() || InstalledByVortex());
    }

    public override ModStatus GetStatus()
    {
        if (!GamePathValid)
        {
            return ModStatus.Missing;
        }

        if (InstalledByRando())
        {
            return ModStatus.Installed;
        }

        return InstalledByVortex() ? ModStatus.InstalledExternally : ModStatus.Missing;
    }

    public override void Install(string archivePath)
    {
        base.Install(archivePath);

        // The loader has to know about the shared folder even when no seed has been generated yet.
        FF12SeedGenerator.UpdateLoaderConfig();
    }

    public override void Uninstall()
    {
        if (File.Exists(ScriptPath))
        {
            File.Delete(ScriptPath);
        }

        // Shared with the rando's own output, so only the Manifesto's paths come out and the folder
        // and its loader entry only go once nothing is left in it.
        FF12SeedGenerator.RemoveFromSeedFolder(DataSubPaths);
    }
}

/// <summary>
/// Every third party mod the rando knows about. Adding one here is enough for the setup screen to
/// list it and for generation to require it.
/// </summary>
public static class FF12Mods
{
    public static FF12Mod Tools { get; } = new FF12ToolsMod
    {
        Name = "VM script tools",
        DownloadUrl = "https://www.nexusmods.com/finalfantasy12/mods/124",
        RequiredFiles =
        {
            "data\\tools\\ff12-text.exe",
            "data\\tools\\ff12-ebppack.exe",
            "data\\tools\\ff12-ebpunpack.exe"
        }
    };

    public static FF12Mod FileLoader { get; } = new()
    {
        Name = "External File Loader",
        DownloadUrl = "https://www.nexusmods.com/finalfantasy12/mods/170",
        MinimumVersion = new Version(1, 5, 2),
        VersionFile = "x64\\modules\\ff12-file-loader.dll",
        RequiredFiles =
        {
            "x64\\ff12-trampoline.dll",
            "x64\\simpleLog.dll",
            "x64\\modules\\ff12-file-loader.dll",
            "x64\\modules\\config\\ff12-file-loader.ini"
        },
        Extractions =
        {
            new ModExtraction("x64", () => GameFile("x64")),
            new ModExtraction("dinput", () => GameFile("x64"))
        },
        // Left behind by older versions of the loader.
        ExtraUninstallFiles =
        {
            "x64\\vcruntime140_1.dll",
            "x64\\dinput8.dll",
            "x64\\dxgi.dll"
        }
    };

    public static FF12Mod LuaLoader { get; } = new()
    {
        Name = "Lua Loader",
        DownloadUrl = "https://www.nexusmods.com/finalfantasy12/mods/171",
        MinimumVersion = new Version(1, 10, 2),
        VersionFile = "x64\\modules\\ff12-lua-loader.dll",
        RequiredFiles = { "x64\\modules\\ff12-lua-loader.dll" },
        Extractions = { new ModExtraction("modules", () => GameFile("x64\\modules")) }
    };

    public static FF12Mod Manifesto { get; } = new FF12ManifestoMod
    {
        Name = "Insurgent's Manifesto",
        DownloadUrl = "https://www.nexusmods.com/finalfantasy12/mods/218",
        Extractions =
        {
            new ModExtraction("data\\x64\\scripts", () => GameFile("x64\\scripts")),
            new ModExtraction("data\\mods\\deploy\\ff12data\\ps2data\\image", () => Path.Combine(FF12SeedGenerator.SeedDataFolder, "image")),
            new ModExtraction("data\\mods\\deploy\\ff12data\\ps2data\\obj_finish", () => Path.Combine(FF12SeedGenerator.SeedDataFolder, "obj_finish"))
        }
    };

    public static FF12Mod TKMalloc { get; } = new()
    {
        Name = "tkMalloc Crash Fix",
        Optional = true,
        DownloadUrl = "https://www.nexusmods.com/finalfantasy12/mods/475",
        RequiredFiles =
        {
            "x64\\modules\\00-ff12-tkmalloc.dll",
            "x64\\modules\\config\\ff12-tkmalloc.ini",
        },
        Extractions = { new ModExtraction("modules", () => GameFile("x64\\modules")) }
    };

    public static FF12Mod Deadlands { get; } = new()
    {
        Name = "Deadlands Crash Fix",
        Optional = true,
        DownloadUrl = "https://www.nexusmods.com/finalfantasy12/mods/506",
        RequiredFiles =
        {
            "x64\\scripts\\modules\\simplePatcher.lua",
            "x64\\scripts\\nabreusCrashFix.lua",
        },
        Extractions = { new ModExtraction("scripts", () => GameFile("x64\\scripts")) }
    };

    public static FF12Mod Descriptive { get; } = new()
    {
        Name = "Insurgent's Descriptive Inventory",
        Optional = true,
        DownloadUrl = "https://www.nexusmods.com/finalfantasy12/mods/319",
        RequiredFiles =
        {
            "x64\\scripts\\TheInsurgentsDescriptiveInventory.lua",
            "x64\\scripts\\TheInsurgentsDescriptiveInventory\\helpers.lua"
        },
        Extractions = { new ModExtraction("data\\x64\\scripts", () => GameFile("x64\\scripts")) },
        UninstallFolders = { "x64\\scripts\\config\\TheInsurgentsDescriptiveInventoryConfig" }
    };

    public static IReadOnlyList<FF12Mod> All { get; } = new List<FF12Mod>
    {
        Tools, FileLoader, LuaLoader, Manifesto, TKMalloc, Deadlands, Descriptive
    };

    public static IEnumerable<FF12Mod> Required => All.Where(m => !m.Optional);

    public static IEnumerable<FF12Mod> OptionalMods => All.Where(m => m.Optional);

    private static string GameFile(string relativePath)
    {
        return FF12Mod.GameFile(relativePath);
    }
}

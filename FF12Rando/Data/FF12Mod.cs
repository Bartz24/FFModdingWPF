using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FF12Rando;

/// <summary>
/// What a mod's files on disk currently amount to.
/// </summary>
public enum ModStatus
{
    Missing,
    /// <summary>Present, but older than the rando needs.</summary>
    Outdated,
    Installed,
    /// <summary>Present, but put there by something other than the rando.</summary>
    InstalledExternally
}

/// <summary>
/// One folder to pull out of a mod's archive, and where it lands. The destination is resolved late
/// because it depends on the configured game path.
/// </summary>
public record ModExtraction(string ArchiveSubfolder, Func<string> TargetFolder);

/// <summary>
/// A third party mod the rando needs or can make use of: where its files belong, where to get it,
/// how to install, verify, version check and remove it.
///
/// One instance per mod lives in <see cref="FF12Mods"/>. The setup screen and seed generation both
/// ask those instances rather than each carrying their own copy of the paths, which is what used to
/// let the two disagree about whether a mod was really installed.
///
/// This class deliberately holds no display strings and no WPF types. It answers what is on disk;
/// how that reads to the player belongs to the setup screen.
/// </summary>
public class FF12Mod
{
    /// <summary>Identifies the mod, and reads naturally mid sentence: "The External File Loader".</summary>
    public string Name { get; init; }

    /// <summary>Optional mods are counted separately on the setup screen and never block generation.</summary>
    public bool Optional { get; init; }

    public string DownloadUrl { get; init; }

    /// <summary>Null when the mod ships nothing the rando can read a version out of.</summary>
    public Version MinimumVersion { get; init; }

    /// <summary>Path of the file carrying the version resource, resolved by <see cref="ResolvePath"/>.</summary>
    public string VersionFile { get; init; }

    /// <summary>Everything that has to be present for the mod to count as installed.</summary>
    public List<string> RequiredFiles { get; init; } = new();

    public List<string> RequiredFolders { get; init; } = new();

    public List<ModExtraction> Extractions { get; init; } = new();

    /// <summary>Removed on uninstall on top of <see cref="RequiredFiles"/>.</summary>
    public List<string> ExtraUninstallFiles { get; init; } = new();

    public List<string> UninstallFolders { get; init; } = new();

    /// <summary>False for a mod that lives next to the rando rather than in the game folder.</summary>
    protected virtual bool NeedsGamePath => true;

    /// <summary>Whether installing this mod requires the game path to be configured first.</summary>
    public bool NeedsGamePathForInstall => NeedsGamePath;

    /// <summary>
    /// False for a mod there is no reason to remove, which keeps it off the uninstall screen.
    /// </summary>
    public virtual bool CanUninstall => true;

    public static bool GamePathValid => SetupData.Paths.ContainsKey("12") && Directory.Exists(SetupData.Paths["12"]);

    public static string GameFile(string relativePath)
    {
        return Path.Combine(SetupData.Paths["12"], relativePath);
    }

    /// <summary>Turns a declared path into a real one. Declared paths are relative to the game folder.</summary>
    protected virtual string ResolvePath(string path)
    {
        return GameFile(path);
    }

    public virtual bool IsInstalled()
    {
        if (NeedsGamePath && !GamePathValid)
        {
            return false;
        }

        return RequiredFiles.All(p => File.Exists(ResolvePath(p)))
            && RequiredFolders.All(p => Directory.Exists(ResolvePath(p)));
    }

    /// <summary>
    /// The mod's own version, or null when the game path is unset, the file is not there, or it
    /// carries no version resource.
    /// </summary>
    public Version GetVersion()
    {
        if (string.IsNullOrEmpty(VersionFile) || (NeedsGamePath && !GamePathValid))
        {
            return null;
        }

        string path = ResolvePath(VersionFile);
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

            // These mods version themselves as major.minor.patch; the private part is always 0.
            return new Version(info.FileMajorPart, info.FileMinorPart, info.FileBuildPart);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// A file with no readable version counts as too old: anything new enough to be supported stamps
    /// its version, so an unreadable one is never a build the rando can rely on.
    /// </summary>
    public bool IsUpToDate()
    {
        if (MinimumVersion == null)
        {
            return true;
        }

        Version version = GetVersion();
        return version != null && version >= MinimumVersion;
    }

    /// <summary>True when generation can proceed: installed and current, or not needed at all.</summary>
    public bool IsUsable()
    {
        return Optional || (IsInstalled() && IsUpToDate());
    }

    /// <summary>Formats a version for a message. Not view specific: generation reports it too.</summary>
    public static string DescribeVersion(Version version)
    {
        return version == null ? "an unknown version" : $"version {version}";
    }

    public virtual ModStatus GetStatus()
    {
        if (!IsInstalled())
        {
            return ModStatus.Missing;
        }

        return IsUpToDate() ? ModStatus.Installed : ModStatus.Outdated;
    }

    public virtual void Install(string archivePath)
    {
        foreach (ModExtraction extraction in Extractions)
        {
            FileHelpers.ExtractSubfolderFromArchive(archivePath, extraction.TargetFolder(), extraction.ArchiveSubfolder);
        }
    }

    public virtual void Uninstall()
    {
        foreach (string path in RequiredFiles.Concat(ExtraUninstallFiles).Select(ResolvePath).Where(File.Exists))
        {
            File.Delete(path);
        }

        foreach (string path in UninstallFolders.Select(ResolvePath).Where(Directory.Exists))
        {
            Directory.Delete(path, true);
        }
    }
}

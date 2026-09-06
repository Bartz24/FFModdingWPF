using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace FF12Rando;

/// <summary>
/// The screens' view of one <see cref="FF12Mod"/>: the wording and colour for its status row, and
/// the dialogs behind its download, install and uninstall buttons.
///
/// Everything view specific lives here so <see cref="FF12Mod"/> stays a description of what is on
/// disk. The mod answers whether it is installed; this decides how that reads.
///
/// Status is recomputed from disk on every read, so a refresh is just a change notification rather
/// than anything this has to cache.
/// </summary>
public class ModStatusView : INotifyPropertyChanged
{
    public FF12Mod Mod { get; }

    /// <summary>Short name for the left hand column of the setup screen.</summary>
    public string RowLabel { get; init; }

    /// <summary>Shown once the mod is present and current.</summary>
    public string InstalledText { get; init; }

    /// <summary>Shown when the mod is absent.</summary>
    public string MissingText { get; init; }

    /// <summary>Shown when something other than the rando installed it. Only used by the Manifesto.</summary>
    public string ExternalText { get; init; }

    /// <summary>Asked before installing over an install the rando did not make. Null to just install.</summary>
    public string ExternalInstallWarning { get; init; }

    /// <summary>Title of the file picker used to install from an archive.</summary>
    public string ArchiveDialogTitle { get; init; }

    public ModStatusView(FF12Mod mod)
    {
        Mod = mod;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string Text
    {
        get
        {
            switch (Mod.GetStatus())
            {
                case ModStatus.Missing:
                    return MissingText;
                case ModStatus.Outdated:
                    return $"{Mod.Name} is {FF12Mod.DescribeVersion(Mod.GetVersion())}, but version {Mod.MinimumVersion} or newer is required.\nDownload and install a newer version with the buttons to the right.";
                case ModStatus.InstalledExternally:
                    return ExternalText ?? InstalledText;
                default:
                    System.Version version = Mod.GetVersion();
                    return version == null ? InstalledText : $"{InstalledText}\nVersion {version}";
            }
        }
    }

    public SolidColorBrush Color
    {
        get
        {
            if (Mod.GetStatus() is ModStatus.Installed or ModStatus.InstalledExternally)
            {
                return Brushes.LightGreen;
            }

            // A missing optional mod is a suggestion rather than a problem, so it reads softer.
            return Mod.Optional ? Brushes.Yellow : Brushes.Orange;
        }
    }

    public string UninstallLabel => $"Uninstall {Mod.Name}";

    public string UninstallTooltip => $"Removes {Mod.Name} if the rando installed it. Do NOT uninstall here if it was installed through Vortex.";

    /// <summary>Re-reads the status from disk. Both derived values are recomputed on access.</summary>
    public void Refresh()
    {
        OnPropertyChanged(nameof(Text));
        OnPropertyChanged(nameof(Color));
    }

    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public void Download()
    {
        string url = Mod.DownloadUrl;
        if (MessageBox.Show($"This will open your default browser at the below link to download {Mod.Name} from NexusMods. Continue?\n{url}", $"Download {Mod.Name}", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            Process.Start(url);
        }
        catch
        {
            Process.Start(new ProcessStartInfo(url.Replace("&", "^&")) { UseShellExecute = true });
        }
    }

    /// <summary>
    /// Prompts for the mod's archive and installs it, reporting the result. Returns true when the
    /// mod verified as installed afterwards.
    /// </summary>
    public bool Install()
    {
        if (Mod.NeedsGamePathForInstall && !FF12Mod.GamePathValid)
        {
            MessageBox.Show("The path for FF12 is not valid. Setup the Steam path in the '1. Setup' step. first", "FF12 not found.");
            return false;
        }

        // An install the rando did not make is already working, so check before writing over it.
        if (ExternalInstallWarning != null && Mod.GetStatus() == ModStatus.InstalledExternally &&
            MessageBox.Show(ExternalInstallWarning, $"{Mod.Name} already installed") == MessageBoxResult.No)
        {
            return false;
        }

        VistaOpenFileDialog dialog = new()
        {
            Title = ArchiveDialogTitle,
            Multiselect = false,
            Filter = "7zip|*.7z"
        };

        if (dialog.ShowDialog() != true)
        {
            return false;
        }

        string path = dialog.FileName.Replace("/", "\\");
        if (!File.Exists(path))
        {
            MessageBox.Show("Make sure the selected file is a 7z file.", "The selected file is not valid");
            return false;
        }

        try
        {
            Mod.Install(path);
        }
        catch
        {
            MessageBox.Show($"Failed to install {Mod.Name} when extracting the files.");
            return false;
        }

        // Verified from the files on disk rather than assumed from the extraction succeeding.
        if (Mod.IsInstalled())
        {
            MessageBox.Show($"{Mod.Name} has been successfully installed.");
            return true;
        }

        MessageBox.Show($"Failed to install {Mod.Name}. Expected files are missing.");
        return false;
    }

    /// <summary>Confirms, then removes the mod. Returns true when it actually ran.</summary>
    public bool Uninstall()
    {
        string confirm = $"Remove {Mod.Name} files?\nIf you installed it through Vortex, click 'Cancel' and uninstall it through Vortex instead.";
        if (MessageBox.Show(confirm, $"Remove {Mod.Name}?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
        {
            return false;
        }

        return UninstallWithoutConfirming();
    }

    /// <summary>Removes the mod with no prompt of its own, for when the caller already confirmed.</summary>
    public bool UninstallWithoutConfirming()
    {
        try
        {
            Mod.Uninstall();
        }
        catch
        {
            MessageBox.Show($"Encountered an error while removing {Mod.Name} files.");
            return false;
        }

        return true;
    }
}

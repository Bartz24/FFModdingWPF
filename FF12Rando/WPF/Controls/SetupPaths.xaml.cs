using Bartz24.Data;
using Bartz24.RandoWPF;
using Ookii.Dialogs.Wpf;
using SharpCompress.Archives.SevenZip;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FF12Rando;

/// <summary>
/// Interaction logic for SetupPaths.xaml
/// </summary>
public partial class SetupPaths : UserControl
{
    private const string ToolsInstalledText = "The tools for editing scripts and text are correctly installed.";
    private const string ToolsNotInstalledText = "The required tools for editing scripts and text are not detected.\nDownload and then install the tools.";

    private const string FileLoaderInstalledText = "The External File Loader is correctly installed.";
    private const string FileLoaderNotInstalledText = "The required External File Loader files are not detected.\nEither download through the Vortex mod manager,\nor download and then install the loader directly with the buttons to the right.";

    private const string LuaLoaderInstalledText = "The Lua Loader is correctly installed.";
    private const string LuaLoaderNotInstalledText = "The required Lua Loader files are not detected.\nEither download through the Vortex mod manager,\nor download and then install the loader directly with the buttons to the right.";

    private const string DescriptiveInstalledText = "The Insurgent's Descriptive Inventory is correctly installed.";
    private const string DescriptiveNotInstalledText = "The OPTIONAL mod for improved equipment descriptions is not installed.\nEither download through the Vortex mod manager,\nor download and then install the loader directly with the buttons to the right.";

    public string FF12Path => SetupData.GetSteamPath("12");
    public string ToolsText { get; set; }
    public SolidColorBrush ToolsTextColor { get; set; }
    public string LoaderText { get; set; }
    public SolidColorBrush LoaderTextColor { get; set; }
    public string LuaLoaderText { get; set; }
    public SolidColorBrush LuaLoaderTextColor { get; set; }
    public string DescriptiveText { get; set; }
    public SolidColorBrush DescriptiveTextColor { get; set; }

    public SetupPaths()
    {
        InitializeComponent();
        DataContext = this;

        SetupData.PathFileName = @"data\RandoPaths.csv";
        SetupData.PathRegistrySearch.Add("12", @"\x64\FFXII_TZA.exe");

        SetupData.PathRegistrySearch.Keys.ToList().ForEach(s => SetupData.Paths.Add(s, SetupData.GetSteamPath(s)));

        UpdateText();
    }

    public void UpdateText()
    {
        ToolsText = FF12SeedGenerator.ToolsInstalled() ? ToolsInstalledText : ToolsNotInstalledText;
        ToolsTextColor = FF12SeedGenerator.ToolsInstalled() ? Brushes.LightGreen : Brushes.Orange;
        ToolsTextLabel.GetBindingExpression(ContentProperty).UpdateTarget();
        ToolsTextLabel.GetBindingExpression(ForegroundProperty).UpdateTarget();

        LoaderText = FF12SeedGenerator.FileLoaderInstalled() ? FileLoaderInstalledText : FileLoaderNotInstalledText;
        LoaderTextColor = FF12SeedGenerator.FileLoaderInstalled() ? Brushes.LightGreen : Brushes.Orange;
        LoaderTextLabel.GetBindingExpression(ContentProperty).UpdateTarget();
        LoaderTextLabel.GetBindingExpression(ForegroundProperty).UpdateTarget();

        LuaLoaderText = FF12SeedGenerator.LuaLoaderInstalled() ? LuaLoaderInstalledText : LuaLoaderNotInstalledText;
        LuaLoaderTextColor = FF12SeedGenerator.LuaLoaderInstalled() ? Brushes.LightGreen : Brushes.Orange;
        LuaLoaderTextLabel.GetBindingExpression(ContentProperty).UpdateTarget();
        LuaLoaderTextLabel.GetBindingExpression(ForegroundProperty).UpdateTarget();

        DescriptiveText = FF12SeedGenerator.DescriptiveInstalled() ? DescriptiveInstalledText : DescriptiveNotInstalledText;
        DescriptiveTextColor = FF12SeedGenerator.DescriptiveInstalled() ? Brushes.LightGreen : Brushes.Yellow;
        DescriptiveTextLabel.GetBindingExpression(ContentProperty).UpdateTarget();
        DescriptiveTextLabel.GetBindingExpression(ForegroundProperty).UpdateTarget();
    }

    private void steamPath12Button_Click(object sender, RoutedEventArgs e)
    {
        VistaFolderBrowserDialog dialog = new()
        {
            Description = "Please select the folder for FF12 Steam.",
            UseDescriptionForTitle = true
        };
        if ((bool)dialog.ShowDialog())
        {
            string path = dialog.SelectedPath.Replace("/", "\\") + SetupData.PathRegistrySearch["12"];
            if (File.Exists(path))
            {
                SetupData.Paths["12"] = dialog.SelectedPath.Replace("/", "\\");
                SaveRandoPaths();
                steamPath12Text.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
            else
            {
                MessageBox.Show("Make sure the folder is something like 'FINAL FANTASY XII THE ZODIAC AGE'.", "The selected folder is not valid");
            }
        }
    }

    private void SaveRandoPaths()
    {
        File.WriteAllLines(SetupData.PathFileName, SetupData.Paths.Select(p => $"{p.Key};{p.Value + (SetupData.PathRegistrySearch.ContainsKey(p.Key) ? SetupData.PathRegistrySearch[p.Key] : "")}"));
    }

    private void toolsInstallButton_Click(object sender, RoutedEventArgs e)
    {

        VistaOpenFileDialog dialog = new()
        {
            Title = "Please select a compressed file of the tools.",
            Multiselect = false,
            Filter = "7zip|*.7z"
        };
        if ((bool)dialog.ShowDialog())
        {
            string path = dialog.FileName.Replace("/", "\\");
            if (File.Exists(path))
            {
                try
                {
                    if (Directory.Exists("data\\tools"))
                    {
                        Directory.Delete("data\\tools", true);
                    }

                    Directory.CreateDirectory("data\\tools");

                    using (SevenZipArchive archive = SevenZipArchive.Open(path))
                    using (SharpCompress.Readers.IReader reader = archive.ExtractAllEntries())
                    {
                        while (reader.MoveToNextEntry())
                        {
                            if (!reader.Entry.IsDirectory && reader.Entry.Key.EndsWith(".exe"))
                            {
                                using (SharpCompress.Common.EntryStream entryStream = reader.OpenEntryStream())
                                {
                                    string extractedPath = "data\\tools\\" + System.IO.Path.GetFileName(reader.Entry.Key);
                                    if (!Directory.Exists(System.IO.Path.GetDirectoryName(extractedPath)))
                                    {
                                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(extractedPath));
                                    }

                                    using (FileStream writeStream = File.OpenWrite(extractedPath))
                                    {
                                        entryStream.CopyTo(writeStream);
                                    }
                                }
                            }
                        }
                    }

                    if (FF12SeedGenerator.ToolsInstalled())
                    {
                        MessageBox.Show("Tools have been successfully installed.");
                    }
                    else
                    {
                        MessageBox.Show("Failed to install the tools. Expected tools are missing.");
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to install the tools when extracting the files.");
                }

                UpdateText();
            }
            else
            {
                MessageBox.Show("Make sure the selected file is a 7z file.", "The selected file is not valid");
            }
        }
    }

    private void toolsDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        string url = "https://www.nexusmods.com/finalfantasy12/mods/124";
        if (MessageBox.Show("This will open your default browser at the below link to download the VM scripts from NexusMods. Continue?\n" + url, "Download tools", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
    }

    private void loaderDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        string url = "https://www.nexusmods.com/finalfantasy12/mods/170";
        if (MessageBox.Show("This will open your default browser at the below link to download the External File Loader from NexusMods. Continue?\n" + url, "Download file loader", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
    }

    private void loaderInstallButton_Click(object sender, RoutedEventArgs e)
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            MessageBox.Show("The path for FF12 is not valid. Setup the Steam path in the '1. Setup' step. first", "FF12 not found.");
            return;        
        }

        VistaOpenFileDialog dialog = new()
        {
            Title = "Please select a compressed file of the file loader.",
            Multiselect = false,
            Filter = "7zip|*.7z"
        };
        if ((bool)dialog.ShowDialog())
        {
            string path = dialog.FileName.Replace("/", "\\");
            if (File.Exists(path))
            {
                try
                {
                    FileHelpers.ExtractSubfolderFromArchive(path, System.IO.Path.Combine(SetupData.Paths["12"], "x64"), "loader-multi\\x64");

                    FileHelpers.ExtractSubfolderFromArchive(path, System.IO.Path.Combine(SetupData.Paths["12"], "x64"), "loader-multi\\dinput");

                    if (FF12SeedGenerator.FileLoaderInstalled())
                    {
                        MessageBox.Show("External File Loader has been successfully installed.");
                    }
                    else
                    {
                        MessageBox.Show("Failed to install the External File Loader. Expected files are missing.");
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to install the External File Loader when extracting the files.");
                }

                UpdateText();
            }
            else
            {
                MessageBox.Show("Make sure the selected file is a 7z file.", "The selected file is not valid");
            }
        }
    }

    private void luaLoaderDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        string url = "https://www.nexusmods.com/finalfantasy12/mods/171";
        if (MessageBox.Show("This will open your default browser at the below link to download the Lua Loader from NexusMods. Continue?\n" + url, "Download file loader", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
    }

    private void luaLoaderInstallButton_Click(object sender, RoutedEventArgs e)
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            MessageBox.Show("The path for FF12 is not valid. Setup the Steam path in the '1. Setup' step. first", "FF12 not found.");
            return;
        }

        VistaOpenFileDialog dialog = new()
        {
            Title = "Please select a compressed file of the lua loader.",
            Multiselect = false,
            Filter = "7zip|*.7z"
        };
        if ((bool)dialog.ShowDialog())
        {
            string path = dialog.FileName.Replace("/", "\\");
            if (File.Exists(path))
            {
                try
                {
                    FileHelpers.ExtractSubfolderFromArchive(path, System.IO.Path.Combine(SetupData.Paths["12"], "x64\\modules"), "modules");

                    if (FF12SeedGenerator.LuaLoaderInstalled())
                    {
                        MessageBox.Show("Lua Loader has been successfully installed.");
                    }
                    else
                    {
                        MessageBox.Show("Failed to install the Lua Loader. Expected files are missing.");
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to install the Lua Loader when extracting the files.");
                }

                UpdateText();
            }
            else
            {
                MessageBox.Show("Make sure the selected file is a 7z file.", "The selected file is not valid");
            }
        }
    }

    private void descriptiveDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        string url = "https://www.nexusmods.com/finalfantasy12/mods/319";
        if (MessageBox.Show("This will open your default browser at the below link to download The Insurgent's Descriptive Inventory from NexusMods. Continue?\n" + url, "Download file loader", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
    }

    private void descriptiveInstallButton_Click(object sender, RoutedEventArgs e)
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            MessageBox.Show("The path for FF12 is not valid. Setup the Steam path in the '1. Setup' step. first", "FF12 not found.");
            return;
        }

        VistaOpenFileDialog dialog = new()
        {
            Title = "Please select a compressed file of The Insurgent's Descriptive Inventory.",
            Multiselect = false,
            Filter = "7zip|*.7z"
        };
        if ((bool)dialog.ShowDialog())
        {
            string path = dialog.FileName.Replace("/", "\\");
            if (File.Exists(path))
            {
                try
                {
                    FileHelpers.ExtractSubfolderFromArchive(path, System.IO.Path.Combine(SetupData.Paths["12"], "x64\\scripts"), "data\\x64\\scripts");

                    if (FF12SeedGenerator.LuaLoaderInstalled())
                    {
                        MessageBox.Show("The Insurgent's Descriptive Inventory has been successfully installed.");
                    }
                    else
                    {
                        MessageBox.Show("Failed to install The Insurgent's Descriptive Inventory. Expected files are missing.");
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to install The Insurgent's Descriptive Inventory when extracting the files.");
                }

                UpdateText();
            }
            else
            {
                MessageBox.Show("Make sure the selected file is a 7z file.", "The selected file is not valid");
            }
        }
    }
}
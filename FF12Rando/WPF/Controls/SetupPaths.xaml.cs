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
    public string FF12Path => SetupData.GetSteamPath("12");
    public string ToolsText { get; set; }
    public SolidColorBrush ToolsTextColor { get; set; }

    public SetupPaths()
    {
        InitializeComponent();
        DataContext = this;

        if (FF12SeedGenerator.ToolsInstalled())
        {
            ToolsText = "The required tools for editing scripts and text are installed.";
            ToolsTextColor = Brushes.White;
        }
        else
        {
            ToolsText = "The required tools for editing scripts and text are not detected.\nDownload and then install the tools.";
            ToolsTextColor = Brushes.Red;
        }

        SetupData.OutputFolder = @"outdata\ps2data";

        SetupData.PathFileName = @"data\RandoPaths.csv";
        SetupData.PathRegistrySearch.Add("12", @"\x64\FFXII_TZA.exe");

        SetupData.PathRegistrySearch.Keys.ToList().ForEach(s => SetupData.Paths.Add(s, SetupData.GetSteamPath(s)));
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

                if (FF12SeedGenerator.ToolsInstalled())
                {
                    ToolsText = "The required tools for editing scripts and text are installed.";
                    ToolsTextColor = Brushes.White;
                }
                else
                {
                    ToolsText = "The required tools for editing scripts and text are not detected.\nDownload and then install the tools.";
                    ToolsTextColor = Brushes.Red;
                }

                ToolsTextLabel.GetBindingExpression(Label.ContentProperty).UpdateTarget();
                ToolsTextLabel.GetBindingExpression(Label.ForegroundProperty).UpdateTarget();
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
}
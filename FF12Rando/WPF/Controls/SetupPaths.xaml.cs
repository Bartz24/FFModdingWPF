using Ookii.Dialogs.Wpf;
using SharpCompress.Archives.SevenZip;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FF12Rando
{
    /// <summary>
    /// Interaction logic for SetupPaths.xaml
    /// </summary>
    public partial class SetupPaths : UserControl
    {
        public string ToolsText { get; set; }
        public SolidColorBrush ToolsTextColor { get; set; }

        public SetupPaths()
        {
            InitializeComponent();
            this.DataContext = this;

            if (ToolsInstalled())
            {
                ToolsText = "The required tools for editing scripts and text are installed.";
                ToolsTextColor = Brushes.White;
            }
            else
            {
                ToolsText = "The required tools for editing scripts and text are not detected.\nDownload and then install the tools.";
                ToolsTextColor = Brushes.Red;
            }
        }

        private void toolsInstallButton_Click(object sender, RoutedEventArgs e)
        {

            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Title = "Please select a compressed file of the tools.";
            dialog.Multiselect = false;
            dialog.Filter = "7zip|*.7z";
            if ((bool)dialog.ShowDialog())
            {
                string path = dialog.FileName.Replace("/", "\\");
                if (File.Exists(path))
                {
                    try
                    {
                        if (Directory.Exists("data\\tools"))
                            Directory.Delete("data\\tools", true);
                        Directory.CreateDirectory("data\\tools");

                        using (var archive = SevenZipArchive.Open(path))
                        using (var reader = archive.ExtractAllEntries())
                        {
                            while (reader.MoveToNextEntry())
                            {
                                if (!reader.Entry.IsDirectory && reader.Entry.Key.EndsWith(".exe"))
                                {
                                    using (var entryStream = reader.OpenEntryStream())
                                    {
                                        string extractedPath = "data\\tools\\" + System.IO.Path.GetFileName(reader.Entry.Key);
                                        if (!Directory.Exists(System.IO.Path.GetDirectoryName(extractedPath)))
                                            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(extractedPath));
                                        using (var writeStream = File.OpenWrite(extractedPath))
                                        {
                                            entryStream.CopyTo(writeStream);
                                        }
                                    }
                                }
                            }
                        }

                        if (ToolsInstalled())
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

                    if (ToolsInstalled())
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
                    MessageBox.Show("Make sure the selected file is a 7z file.", "The selected file is not valid");
            }
        }

        private static bool ToolsInstalled()
        {
            return File.Exists("data\\tools\\ff12-text.exe") && File.Exists("data\\tools\\ff12-ebppack.exe") && File.Exists("data\\tools\\ff12-ebpunpack.exe");
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
}
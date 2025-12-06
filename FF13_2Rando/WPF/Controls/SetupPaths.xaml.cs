using Bartz24.FF13Series;
using Bartz24.RandoWPF;
using Ookii.Dialogs.Wpf;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FF13_2Rando;

/// <summary>
/// Interaction logic for SetupPaths.xaml
/// </summary>
public partial class SetupPaths : UserControl
{
    public string FF13_2Path => SetupData.GetSteamPath("13-2");
    public string NovaPath => SetupData.GetSteamPath("Nova", false);
    public string NovaVersionText { get; set; }
    public SolidColorBrush NovaVersionColor { get; set; }

    public SetupPaths()
    {
        InitializeComponent();
        DataContext = this;

        SetupData.PathFileName = @"data\RandoPaths.csv";
        SetupData.PathRegistrySearch.Add("13-2", @"\alba_data\prog\win\bin\ffxiii2img.exe");

        SetupData.PathRegistrySearch.Keys.ToList().ForEach(s => SetupData.Paths.Add(s, SetupData.GetSteamPath(s)));
        SetupData.Paths.Add("Nova", SetupData.GetSteamPath("Nova", false));

        UpdateText();
    }
    private void novaPathButton_Click(object sender, RoutedEventArgs e)
    {
        VistaOpenFileDialog dialog = new()
        {
            Title = "Please select the exe for the Nova Chrysalia mod manager.",
            Filter = "Executable|*.exe",
            Multiselect = false
        };
        if ((bool)dialog.ShowDialog())
        {
            string path = dialog.FileName.Replace("/", "\\");
            if (File.Exists(path))
            {
                SetupData.Paths["Nova"] = path;
                SaveRandoPaths();
                novaPathText.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                UpdateText();
            }
            else
            {
                MessageBox.Show("Make sure the executable is something like 'NovaChrysalia.exe'.", "The selected executable is not valid");
            }
        }
    }
    private void UpdateText()
    {
        string version = Nova.GetVersion(NovaPath);
        if (version.StartsWith("Please close"))
        {
            NovaVersionText = version;
        }
        else
        {
            NovaVersionText = Nova.IsNovaVersion2(NovaPath) ? $"Version {version}" : $"Version {version} (Unsupported)";
        }

        NovaVersionColor = Nova.IsNovaVersion2(NovaPath) ? Brushes.LightGreen : Brushes.Orange;
        NovaVersionLabel.GetBindingExpression(ContentProperty).UpdateTarget();
        NovaVersionLabel.GetBindingExpression(ForegroundProperty).UpdateTarget();
    }

    private void steamPath13_2Button_Click(object sender, RoutedEventArgs e)
    {
        VistaFolderBrowserDialog dialog = new()
        {
            Description = "Please select the folder for FF13-2 Steam.",
            UseDescriptionForTitle = true
        };
        if ((bool)dialog.ShowDialog())
        {
            string path = dialog.SelectedPath.Replace("/", "\\") + SetupData.PathRegistrySearch["13-2"];
            if (File.Exists(path))
            {
                SetupData.Paths["13-2"] = dialog.SelectedPath.Replace("/", "\\");
                SaveRandoPaths();
                steamPath13_2Text.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
            else
            {
                MessageBox.Show("Make sure the folder is something like 'FINAL FANTASY XIII-2'.", "The selected folder is not valid");
            }
        }
    }

    private void SaveRandoPaths()
    {
        File.WriteAllLines(SetupData.PathFileName, SetupData.Paths.Select(p => $"{p.Key};{p.Value + (SetupData.PathRegistrySearch.ContainsKey(p.Key) ? SetupData.PathRegistrySearch[p.Key] : "")}"));
    }

    private void novaDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        string url = "https://mega.nz/file/24gyiB7b#nTIlktJb8ZCo8dXZDxCgwsdHAUDPURQhixSfucypIVg";
        if (MessageBox.Show("This will open your default browser at the below link to download Nova Chrysalia v2.0.3. Continue?\n" + url, "Download Nova Chrysalia", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
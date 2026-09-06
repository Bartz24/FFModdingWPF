using Bartz24.Data;
using Bartz24.RandoWPF;
using Ookii.Dialogs.Wpf;
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
    public string PathsCountText { get; set; }
    public string ToolsText { get; set; }
    public SolidColorBrush ToolsTextColor { get; set; }
    public string LoaderText { get; set; }
    public SolidColorBrush LoaderTextColor { get; set; }
    public string LuaLoaderText { get; set; }
    public SolidColorBrush LuaLoaderTextColor { get; set; }
    public string DescriptiveText { get; set; }
    public SolidColorBrush DescriptiveTextColor { get; set; }
    public string ManifestoText { get; set; }
    public SolidColorBrush ManifestoTextColor { get; set; }

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
        int numReqInstalled = FF12Mods.Required.Count(m => m.IsInstalled() && m.IsUpToDate());
        int numReq = FF12Mods.Required.Count();
        int numOptInstalled = FF12Mods.OptionalMods.Count(m => m.IsInstalled() && m.IsUpToDate());
        int numOpt = FF12Mods.OptionalMods.Count();
        PathsCountText = $"Required: {numReqInstalled}/{numReq}    Optional: {numOptInstalled}/{numOpt}";
        PathsCountLabel.GetBindingExpression(ContentProperty).UpdateTarget();

        (ToolsText, ToolsTextColor) = (FF12ModViews.Tools.Text, FF12ModViews.Tools.Color);
        (LoaderText, LoaderTextColor) = (FF12ModViews.FileLoader.Text, FF12ModViews.FileLoader.Color);
        (LuaLoaderText, LuaLoaderTextColor) = (FF12ModViews.LuaLoader.Text, FF12ModViews.LuaLoader.Color);
        (ManifestoText, ManifestoTextColor) = (FF12ModViews.Manifesto.Text, FF12ModViews.Manifesto.Color);
        (DescriptiveText, DescriptiveTextColor) = (FF12ModViews.Descriptive.Text, FF12ModViews.Descriptive.Color);

        RefreshLabel(ToolsTextLabel);
        RefreshLabel(LoaderTextLabel);
        RefreshLabel(LuaLoaderTextLabel);
        RefreshLabel(ManifestoTextLabel);
        RefreshLabel(DescriptiveTextLabel);
    }

    private static void RefreshLabel(Label label)
    {
        label.GetBindingExpression(ContentProperty).UpdateTarget();
        label.GetBindingExpression(ForegroundProperty).UpdateTarget();
    }

    /// <summary>Runs a mod's install flow and refreshes the screen with whatever it left on disk.</summary>
    private void InstallMod(ModStatusView view)
    {
        view.Install();
        UpdateText();
    }

    private void toolsInstallButton_Click(object sender, RoutedEventArgs e)
    {
        InstallMod(FF12ModViews.Tools);
    }

    private void toolsDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        FF12ModViews.Tools.Download();
    }

    private void loaderInstallButton_Click(object sender, RoutedEventArgs e)
    {
        InstallMod(FF12ModViews.FileLoader);
    }

    private void loaderDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        FF12ModViews.FileLoader.Download();
    }

    private void luaLoaderInstallButton_Click(object sender, RoutedEventArgs e)
    {
        InstallMod(FF12ModViews.LuaLoader);
    }

    private void luaLoaderDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        FF12ModViews.LuaLoader.Download();
    }

    private void descriptiveInstallButton_Click(object sender, RoutedEventArgs e)
    {
        InstallMod(FF12ModViews.Descriptive);
    }

    private void descriptiveDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        FF12ModViews.Descriptive.Download();
    }

    private void manifestoInstallButton_Click(object sender, RoutedEventArgs e)
    {
        // Vortex's copy is a working install, so warn before the rando's files land on top of it.
        if (FF12Mods.Manifesto.GetStatus() == ModStatus.InstalledExternally &&
            MessageBox.Show("The Insurgent's Manifesto looks to already be installed through Vortex. No need to install it again. Continue? This will overwrite your Manifesto config with the default.", "Manifesto already installed") == MessageBoxResult.No)
        {
            return;
        }

        InstallMod(FF12ModViews.Manifesto);
    }

    private void manifestoDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        FF12ModViews.Manifesto.Download();
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
                UpdateText();
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

    private void refreshButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateText();
        RandoUI.ShowTempUIMessage("Refreshed the paths and tools status!");
    }
}

using Bartz24.Data;
using Bartz24.RandoWPF;
using Ookii.Dialogs.Wpf;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FF12Rando;

/// <summary>
/// Interaction logic for SetupPaths.xaml
/// </summary>
public partial class SetupPaths : UserControl
{
    public string FF12Path => SetupData.GetSteamPath("12");
    public string PathsCountText { get; set; }

    /// <summary>
    /// Drives the mod rows. Reading straight off the mod list means a new mod gets a row, a status
    /// line and its two buttons without any markup change.
    /// </summary>
    public IReadOnlyList<ModStatusView> ModViews => FF12ModViews.All;

    public SetupPaths()
    {
        InitializeComponent();
        DataContext = this;

        SetupData.PathFileName = @"data\RandoPaths.csv";
        SetupData.PathRegistrySearch.Add("12", @"\x64\FFXII_TZA.exe");

        SetupData.PathRegistrySearch.Keys.ToList().ForEach(s => SetupData.Paths.Add(s, SetupData.GetSteamPath(s)));

        UpdateText();

        // Only decided once, at startup. Reacting to installs afterwards would collapse the section
        // out from under someone who is still working in it.
        ModsExpander.IsExpanded = !AllModsReady();
    }

    /// <summary>
    /// Whether nothing about the mods needs the player's attention. An optional mod they never
    /// installed is a choice rather than a problem, so it does not hold the section open; one that
    /// is installed but out of date does.
    /// </summary>
    private static bool AllModsReady()
    {
        return FF12Mods.All.All(m => (m.Optional && !m.IsInstalled()) || (m.IsInstalled() && m.IsUpToDate()));
    }

    public void UpdateText()
    {
        int numReqInstalled = FF12Mods.Required.Count(m => m.IsInstalled() && m.IsUpToDate());
        int numReq = FF12Mods.Required.Count();
        int numOptInstalled = FF12Mods.OptionalMods.Count(m => m.IsInstalled() && m.IsUpToDate());
        int numOpt = FF12Mods.OptionalMods.Count();
        PathsCountText = $"Required: {numReqInstalled}/{numReq}    Optional: {numOptInstalled}/{numOpt}";
        PathsCountLabel.GetBindingExpression(ContentProperty).UpdateTarget();

        // Each row rereads its own status; this just tells the bindings to ask again.
        foreach (ModStatusView view in ModViews)
        {
            view.Refresh();
        }
    }

    private static ModStatusView ViewFor(object sender)
    {
        return (ModStatusView)((FrameworkElement)sender).DataContext;
    }

    private void modInstallButton_Click(object sender, RoutedEventArgs e)
    {
        ViewFor(sender).Install();
        UpdateText();
    }

    private void modDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        ViewFor(sender).Download();
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

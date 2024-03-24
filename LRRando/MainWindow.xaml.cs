using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.RandoWPF;
using CsvHelper.Configuration;
using LRRando;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LRRando;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : RandoMainWindow
{
    protected override SeedGenerator Generator => new LRSeedGenerator();

    protected override SegmentedProgressBar TotalProgressBar => totalProgressBar;

    protected override TabControl MainWindowTabs => WindowTabs;

    public MainWindow() : base()
    {
        RandoSeeds.DocsFolder = "packs";
        RandoSeeds.DeleteFilter = "LRRando_${SEED}_Docs.zip";
        LRFlags.Init();
        RandoPresets.Init();
        InitializeComponent();
        DataContext = this;
        DataExtensions.Mode = ByteMode.BigEndian;

        if (string.IsNullOrEmpty(SetupData.Paths["Nova"]))
        {
            RootDialog.ShowDialog(RootDialog.DialogContent);
        }
    }

    private void openNovaButton_Click(object sender, RoutedEventArgs e)
    {
        if (File.Exists(SetupData.GetSteamPath("Nova", false)))
        {
            Process.Start(SetupData.GetSteamPath("Nova", false));
        }
        else
        {
            MessageBox.Show("Cannot open Nova. Select the correct executable first.", "Nova Chrysalia does not exist.");
        }
    }

    private void openModpackFolder_Click(object sender, RoutedEventArgs e)
    {
        string dir = Directory.GetCurrentDirectory() + "\\packs";
        if (!Directory.Exists(dir) || Directory.GetFiles(dir).Length == 0)
        {
            MessageBox.Show("No packs seem to be generated. Generate a seed first first.", "No packs generated.");
        }
        else
        {
            Process.Start("explorer.exe", dir);
        }
    }
}

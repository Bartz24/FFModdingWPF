using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.RandoWPF;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;

namespace FF12Rando;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    public static readonly DependencyProperty ProgressBarValueProperty =
    DependencyProperty.Register(nameof(ProgressBarValue), typeof(int), typeof(MainWindow));
    public int ProgressBarValue
    {
        get => (int)GetValue(ProgressBarValueProperty);
        set => SetValue(ProgressBarValueProperty, value);
    }
    public static readonly DependencyProperty ProgressBarMaximumProperty =
    DependencyProperty.Register(nameof(ProgressBarMaximum), typeof(int), typeof(MainWindow));
    public int ProgressBarMaximum
    {
        get => (int)GetValue(ProgressBarMaximumProperty);
        set => SetValue(ProgressBarMaximumProperty, value);
    }
    public static readonly DependencyProperty ProgressBarVisibleProperty =
    DependencyProperty.Register(nameof(ProgressBarVisible), typeof(Visibility), typeof(MainWindow));
    public Visibility ProgressBarVisible
    {
        get => (Visibility)GetValue(ProgressBarVisibleProperty);
        set => SetValue(ProgressBarVisibleProperty, value);
    }
    public static readonly DependencyProperty ProgressBarIndeterminateProperty =
    DependencyProperty.Register(nameof(ProgressBarIndeterminate), typeof(bool), typeof(MainWindow));
    public bool ProgressBarIndeterminate
    {
        get => (bool)GetValue(ProgressBarIndeterminateProperty);
        set => SetValue(ProgressBarIndeterminateProperty, value);
    }

    public static readonly DependencyProperty ProgressBarTextProperty =
    DependencyProperty.Register(nameof(ProgressBarText), typeof(string), typeof(MainWindow));
    public string ProgressBarText
    {
        get => (string)GetValue(ProgressBarTextProperty);
        set => SetValue(ProgressBarTextProperty, value);
    }

    public static readonly DependencyProperty ChangelogTextProperty =
    DependencyProperty.Register(nameof(ChangelogText), typeof(string), typeof(MainWindow));
    public string ChangelogText
    {
        get => (string)GetValue(ChangelogTextProperty);
        set => SetValue(ChangelogTextProperty, value);
    }

    public MainWindow()
    {
        FF12Flags.Init();
        Presets.Init();
        InitializeComponent();
        DataContext = this;
        HideProgressBar();
        DataExtensions.Mode = ByteMode.LittleEndian;

        ChangelogText = File.ReadAllText(@"data\changelog.txt");

        if (!Directory.Exists("data\\musicPacks"))
        {
            Directory.CreateDirectory("data\\musicPacks");
        }
    }
    private static bool ToolsInstalled()
    {
        return File.Exists("data\\tools\\ff12-text.exe") && File.Exists("data\\tools\\ff12-ebppack.exe") && File.Exists("data\\tools\\ff12-ebpunpack.exe");
    }

    private async void generateButton_Click(object sender, RoutedEventArgs e)
    {
        RandomizerManager randomizers = new()
        {
            SetUIProgress = SetProgressBar
        };
        randomizers.Add(new PartyRando(randomizers));
        randomizers.Add(new TreasureRando(randomizers));
        randomizers.Add(new EquipRando(randomizers));
        randomizers.Add(new LicenseBoardRando(randomizers));
        randomizers.Add(new EnemyRando(randomizers));
        randomizers.Add(new ShopRando(randomizers));
        randomizers.Add(new TextRando(randomizers));
        randomizers.Add(new MusicRando(randomizers));

        totalProgressBar.TotalSegments = (randomizers.Count * 3) + 2;
        totalProgressBar.SetProgress(0, 0);

#if !DEBUG
        if (!File.Exists("..\\FFXII_TZA.exe"))
        {
            MessageBox.Show("Can't detect FFXII_TZA.exe. Make sure to run the randomizer from the steam folder of FF12.", "Incorrect location.");
            return;
        }
#endif

        if (!ToolsInstalled())
        {
            MessageBox.Show("Text and script tools are not installed. Download and install them on 1. Setup.", "Tools missing.");
            return;
        }

#if DEBUG
        bool tests = false;
        if (MessageBox.Show("Run tests?", "Tests", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            tests = true;
        }

        for (int i = 0; i < (tests ? 10 : 1); i++)
        {
#endif
            try
            {

                IsEnabled = false;
                await Task.Run(() =>
                {
                    string outFolder = "outdata/ps2data";
                    SetupData.OutputFolder = "outdata/ps2data";

                    int seed = RandomNum.GetIntSeed(SetupData.Seed);
#if DEBUG
                    if (tests)
                    {
                        seed = RandomNum.RandSeed();
                    }
#endif

                    foreach (Flag flag in RandoFlags.FlagsList)
                    {
                        flag.ResetRandom(seed);
                    }
#if DEBUG
                    if (tests)
                    {
                        RandomNum.SetRand(new Random());
                        foreach (Flag flag in RandoFlags.FlagsList)
                        {
                            flag.FlagEnabled = RandomNum.RandInt(0, 99) < 50;
                        }
                    }
#endif

                    RandomNum.ClearRand();

                    SetProgressBar("Preparing data folder...", -1);
                    if (Directory.Exists(outFolder))
                    {
                        Directory.Delete(outFolder, true);
                    }

                    Directory.CreateDirectory(outFolder);
                    CopyFromTemplate(outFolder, "data\\ps2data");

                    totalProgressBar.IncrementProgress();

                    randomizers.ForEach(r =>
                    {
                        r.Load();
                        totalProgressBar.IncrementProgress();
                    });

                    randomizers.ForEach(r =>
                    {
                        r.Randomize();
                        totalProgressBar.IncrementProgress();
                    });

                    randomizers.ForEach(r =>
                    {
                        r.Save();
                        totalProgressBar.IncrementProgress();
                    });

                    SetProgressBar("Generating documentation...", -1);
                    Docs docs = new();
                    docs.Settings.Name = "FF12 Randomizer";
                    for (int i = 0; i < randomizers.Count; i++)
                    {
                        Dictionary<string, HTMLPage> pages = randomizers[i].GetDocumentation();
                        pages.ForEach(p => docs.AddPage(p.Key, p.Value));
                    }

                    docs.Generate(@"docs\docs_latest", @"data\docs\template");
                    SaveSeedJSON(@"docs\docs_latest\FF12Rando_" + seed + "_Seed.json");
                    string zipDocsName = $"docs\\FF12Rando_{seed}_Docs.zip";
                    if (File.Exists(zipDocsName))
                    {
                        File.Delete(zipDocsName);
                    }

                    ZipFile.CreateFromDirectory(@"docs\docs_latest", zipDocsName);

                    File.WriteAllText("outdata\\rando.seed", "");

                    totalProgressBar.IncrementProgress();
                    SetProgressBar($"Complete! Ready to play! The documentation has been generated in the docs folder of this application.", 100);
                });
                IsEnabled = true;
            }
            catch (Exception ex)
            {
                Exception innerMost = ex;
                while (innerMost.InnerException != null)
                {
                    innerMost = innerMost.InnerException;
                }

                MessageBox.Show("Randomizer encountered an error:\n" + innerMost.Message, "Rando failed");
            }
#if DEBUG
        }
#endif
    }

    private void CopyFromTemplate(string mainFolder, string templateFolder)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(templateFolder, "*",
            SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(templateFolder, mainFolder));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(templateFolder, "*.*",
            SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(templateFolder, mainFolder), true);
        }
    }

    private void SetProgressBar(string text, int value, int maxValue = 100)
    {
        Dispatcher.Invoke(() =>
        {
            ProgressBarVisible = Visibility.Visible;
            ProgressBarText = text;
            ProgressBarIndeterminate = value < 0;
            ProgressBarValue = value;
            ProgressBarMaximum = maxValue;
            totalProgressBar.SetProgress(totalProgressBar.GetProgress(), ProgressBarIndeterminate ? -1 : (float)ProgressBarValue / ProgressBarMaximum);
        });
    }

    private void HideProgressBar()
    {
        Dispatcher.Invoke(() =>
        {
            ProgressBarVisible = Visibility.Hidden;
        });
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
        string dir = Directory.GetCurrentDirectory() + "\\docs";
        if (!Directory.Exists(dir) || Directory.GetFiles(dir).Length == 0)
        {
            MessageBox.Show("No docs seem to be generated. Generate a seed first first.", "No docs generated.");
        }
        else
        {
            Process.Start("explorer.exe", dir);
        }
    }

    private void NextStepButton_Click(object sender, RoutedEventArgs e)
    {
        WindowTabs.SelectedIndex++;
    }

    private void PrevStepButton_Click(object sender, RoutedEventArgs e)
    {
        WindowTabs.SelectedIndex--;
    }

    private void shareSeedFolder_Click(object sender, RoutedEventArgs e)
    {
        int seed = RandomNum.GetIntSeed(SetupData.Seed);

        VistaSaveFileDialog dialog = new()
        {
            Filter = "JSON|*.json",
            DefaultExt = ".json",
            AddExtension = true,
            FileName = "FF12Rando_" + seed + "_Seed"
        };
        if ((bool)dialog.ShowDialog())
        {
            string path = dialog.FileName.Replace("/", "\\");
            SaveSeedJSON(path);
        }
    }

    private void shareSeedModpackFolder_Click(object sender, RoutedEventArgs e)
    {

    }

    private void SaveSeedJSON(string file)
    {
        int seed = RandomNum.GetIntSeed(SetupData.Seed);
        string output = RandoFlags.Serialize(seed.ToString(), SetupData.Version);
        File.WriteAllText(file, output);
    }
}

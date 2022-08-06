using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.RandoWPF;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;

namespace FF13Rando
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static readonly DependencyProperty ProgressBarValueProperty =
        DependencyProperty.Register(nameof(ProgressBarValue), typeof(int), typeof(MainWindow));
        public int ProgressBarValue
        {
            get { return (int)GetValue(ProgressBarValueProperty); }
            set { SetValue(ProgressBarValueProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarMaximumProperty =
        DependencyProperty.Register(nameof(ProgressBarMaximum), typeof(int), typeof(MainWindow));
        public int ProgressBarMaximum
        {
            get { return (int)GetValue(ProgressBarMaximumProperty); }
            set { SetValue(ProgressBarMaximumProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarVisibleProperty =
        DependencyProperty.Register(nameof(ProgressBarVisible), typeof(Visibility), typeof(MainWindow));
        public Visibility ProgressBarVisible
        {
            get { return (Visibility)GetValue(ProgressBarVisibleProperty); }
            set { SetValue(ProgressBarVisibleProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarIndeterminateProperty =
        DependencyProperty.Register(nameof(ProgressBarIndeterminate), typeof(bool), typeof(MainWindow));
        public bool ProgressBarIndeterminate
        {
            get { return (bool)GetValue(ProgressBarIndeterminateProperty); }
            set { SetValue(ProgressBarIndeterminateProperty, value); }
        }

        public static readonly DependencyProperty ProgressBarTextProperty =
        DependencyProperty.Register(nameof(ProgressBarText), typeof(string), typeof(MainWindow));
        public string ProgressBarText
        {
            get { return (string)GetValue(ProgressBarTextProperty); }
            set { SetValue(ProgressBarTextProperty, value); }
        }

        public static readonly DependencyProperty ChangelogTextProperty =
        DependencyProperty.Register(nameof(ChangelogText), typeof(string), typeof(MainWindow));
        public string ChangelogText
        {
            get { return (string)GetValue(ChangelogTextProperty); }
            set { SetValue(ChangelogTextProperty, value); }
        }

        public MainWindow()
        {
            FF13Flags.Init();
            FF13Presets.Init();
            InitializeComponent();
            this.DataContext = this;
            HideProgressBar();
            DataExtensions.Mode = ByteMode.BigEndian;

            if (string.IsNullOrEmpty(SetupData.Paths["Nova"]))
            {
                RootDialog.ShowDialog(RootDialog.DialogContent);
            }
            ChangelogText = File.ReadAllText(@"data\changelog.txt");
        }

        private async void generateButton_Click(object sender, RoutedEventArgs e)
        {
            RandomizerManager randomizers = new RandomizerManager();
            randomizers.SetProgressFunc = SetProgressBar;
            randomizers.Add(new EquipRando(randomizers));
            randomizers.Add(new TreasureRando(randomizers));
            randomizers.Add(new CrystariumRando(randomizers));
            randomizers.Add(new BattleRando(randomizers));
            randomizers.Add(new EnemyRando(randomizers));
            randomizers.Add(new MusicRando(randomizers));
            /*
            randomizers.Add(new HistoriaCruxRando(randomizers));
            */
            randomizers.Add(new TextRando(randomizers));

            if (String.IsNullOrEmpty(SetupData.Paths["13"]) || !Directory.Exists(SetupData.Paths["13"]))
            {
                MessageBox.Show("The path for FF13 is not valid. Setup the path in the '1. Setup' step.", "FF13 not found.");
                return;
            }

            if (String.IsNullOrEmpty(SetupData.Paths["Nova"]) || !File.Exists(SetupData.Paths["Nova"]))
            {
                MessageBox.Show("NovaChrysalia.exe needs to be selected. Download Nova Chrysalia and setup the path in the '1. Setup' step.", "Nova Chrysalia not found.");
                return;
            }

            if (!Nova.IsUnpacked("13", @"db\resident\treasurebox.wdb", SetupData.GetSteamPath("13")))
            {
                MessageBox.Show("FF13 needs to be unpacked.\nOpen NovaChrysalia and 'Unpack Game Data' for FF13.", "FF13 is not unpacked");
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

                    this.IsEnabled = false;
                    await Task.Run(() =>
                    {
                        string outFolder = System.IO.Path.GetTempPath() + @"ff13_rando_temp";
                        SetupData.OutputFolder = outFolder + @"\Data";

                        int seed = RandomNum.GetIntSeed(SetupData.Seed);
#if DEBUG
                        if (tests)
                            seed = RandomNum.RandSeed();
#endif

                        foreach (Flag flag in Flags.FlagsList)
                        {
                            flag.ResetRandom(seed);
                        }
#if DEBUG
                        if (tests)
                        {
                            RandomNum.SetRand(new Random());
                            foreach (Flag flag in Flags.FlagsList)
                            {
                                if (RandomNum.RandInt(0, 99) < 50)
                                    flag.FlagEnabled = true;
                                else
                                    flag.FlagEnabled = false;
                            }
                        }
#endif

                        RandomNum.ClearRand();

                        SetProgressBar("Preparing data folder...", -1);
                        if (Directory.Exists(outFolder))
                            Directory.Delete(outFolder, true);
                        Directory.CreateDirectory(outFolder);
                        CopyFromTemplate(outFolder, "data\\modpack");
                        RandoHelpers.UpdateSeedInFile(outFolder + "\\modconfig.ini", seed.ToString());

                        SetProgressBar("Loading Data...", -1);

                        SetupData.WPDTracking.Clear();

                        randomizers.ForEach(r => r.Load());
                        randomizers.ForEach(r =>
                        {
                            SetProgressBar(r.GetProgressMessage(), 0);
                            r.Randomize(v => ProgressBarValue = v);
                        });
                        SetProgressBar("Saving Data...", -1);
                        randomizers.ForEach(r => r.Save());

                        SetProgressBar("Generating ModPack and documentation...", -1);
                        Task taskModpack = new Task(() =>
                        {
#if DEBUG
                            if (!tests)
#endif
                            {
                                string zipName = $"packs\\FF13Rando_{seed}.ncmp";
                                if (File.Exists(zipName))
                                    File.Delete(zipName);
                                if (!Directory.Exists("packs"))
                                    Directory.CreateDirectory("packs");
                                ZipFile.CreateFromDirectory(outFolder, zipName);

                                Directory.Delete(outFolder, true);
                            }
                        });
                        Task taskDocs = new Task(() =>
                        {
                            Docs docs = new Docs();
                            docs.Settings.Name = "FF13 Randomizer";
                            for (int i = 0; i < randomizers.Count; i++)
                            {
                                HTMLPage page = randomizers[i].GetDocumentation();
                                if (page != null)
                                {
                                    docs.AddPage(randomizers[i].GetID().ToLower(), page);
                                }
                            }

                            docs.Generate(@"packs\docs_latest", @"data\docs\template");
                            SaveSeedJSON(@"packs\docs_latest\FF13Rando_" + seed + "_Seed.json");
                            string zipDocsName = $"packs\\FF13Rando_{seed}_Docs.zip";
                            if (File.Exists(zipDocsName))
                                File.Delete(zipDocsName);
                            ZipFile.CreateFromDirectory(@"packs\docs_latest", zipDocsName);
                        });
                        taskModpack.Start();
                        taskDocs.Start();
                        Task.WaitAll(taskModpack, taskDocs);

                        SetProgressBar("Generating Documentation...", -1);

                        SetProgressBar($"Complete! Ready to install in Nova Chrysalia! The modpack 'FF13Rando_{seed}.ncmp' and documentation have been generated in the packs folder of this application.", 100);
                    });
                    this.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    Exception innerMost = ex;
                    while (innerMost.InnerException != null)
                    {
                        innerMost = innerMost.InnerException;
                    }
                    MessageBox.Show("Randomizer encountered an error:\n" + innerMost.Message + "\n\n" + innerMost.StackTrace, "Rando failed");
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
                Directory.CreateDirectory(dirPath.Replace(templateFolder, mainFolder));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(templateFolder, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(templateFolder, mainFolder), true);
        }

        private void SetProgressBar(string text, int value, int maxValue = 100)
        {
            this.Dispatcher.Invoke(() =>
            {
                ProgressBarVisible = Visibility.Visible;
                ProgressBarText = text;
                ProgressBarIndeterminate = value < 0;
                ProgressBarValue = value;
                ProgressBarMaximum = maxValue;
            });
        }

        private void HideProgressBar()
        {
            this.Dispatcher.Invoke(() =>
            {
                ProgressBarVisible = Visibility.Hidden;
            });
        }

        private void openNovaButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(SetupData.GetSteamPath("Nova", false)))
                Process.Start(SetupData.GetSteamPath("Nova", false));
            else
                MessageBox.Show("Cannot open Nova. Select the correct executable first.", "Nova Chrysalia does not exist.");

        }

        private void openModpackFolder_Click(object sender, RoutedEventArgs e)
        {
            string dir = Directory.GetCurrentDirectory() + "\\packs";
            if (!Directory.Exists(dir) || Directory.GetFiles(dir).Length == 0)
                MessageBox.Show("No modpacks seem to be generated. Generate one first.", "No modpacks generated.");
            else
                Process.Start("explorer.exe", dir);
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

            VistaSaveFileDialog dialog = new VistaSaveFileDialog();
            dialog.Filter = "JSON|*.json";
            dialog.DefaultExt = ".json";
            dialog.AddExtension = true;
            dialog.FileName = "FF13Rando_" + seed + "_Seed";
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
            string output = Flags.Serialize(seed.ToString(), SetupData.Version);
            File.WriteAllText(file, output);
        }
    }
}

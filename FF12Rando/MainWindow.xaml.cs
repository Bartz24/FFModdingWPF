using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.RandoWPF;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FF12Rando
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
            FF12Flags.Init();
            FF12Presets.Init();
            InitializeComponent();
            this.DataContext = this;
            HideProgressBar();
            DataExtensions.Mode = ByteMode.LittleEndian;

            ChangelogText = File.ReadAllText(@"data\changelog.txt");
        }

        private async void generateButton_Click(object sender, RoutedEventArgs e)
        {
            RandomizerManager randomizers = new RandomizerManager();
            randomizers.Add(new TreasureRando(randomizers));
            randomizers.Add(new LicenseBoardRando(randomizers));
            randomizers.Add(new EnemyRando(randomizers));
            randomizers.Add(new ShopRando(randomizers));
            randomizers.Add(new TextRando(randomizers));

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
                        string outFolder = "outdata/ps2data";
                        SetupData.OutputFolder = "outdata/ps2data";

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
                        CopyFromTemplate(outFolder, "data\\ps2data");

                        SetProgressBar("Loading Data...", -1);

                        randomizers.ForEach(r => r.Load());
                        randomizers.ForEach(r =>
                        {
                            SetProgressBar(r.GetProgressMessage(), 0);
                            r.Randomize(v => ProgressBarValue = v);
                        });
                        SetProgressBar("Saving Data...", -1);
                        randomizers.ForEach(r => r.Save());

                        SetProgressBar("Generating documentation...", -1);
                        Docs docs = new Docs();
                        docs.Settings.Name = "FF12 Randomizer";
                        for (int i = 0; i < randomizers.Count; i++)
                        {
                            HTMLPage page = randomizers[i].GetDocumentation();
                            if (page != null)
                            {
                                docs.AddPage(randomizers[i].GetID().ToLower(), page);
                            }
                        }

                        docs.Generate(@"packs\docs_latest", @"data\docs\template");
                        SaveSeedJSON(@"packs\docs_latest\FF12Rando_" + seed + "_Seed.json");
                        string zipDocsName = $"packs\\FF12Rando_{seed}_Docs.zip";
                        if (File.Exists(zipDocsName))
                            File.Delete(zipDocsName);
                        ZipFile.CreateFromDirectory(@"packs\docs_latest", zipDocsName);

                        SetProgressBar($"Complete! Ready to play! The documentation has been generated in the packs folder of this application.", 100);
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
                Directory.CreateDirectory(dirPath.Replace(templateFolder, mainFolder));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(templateFolder, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(templateFolder, mainFolder), true);
        }

        private void SetProgressBar(string text, int value)
        {
            this.Dispatcher.Invoke(() =>
            {
                ProgressBarVisible = Visibility.Visible;
                ProgressBarText = text;
                ProgressBarIndeterminate = value < 0;
                ProgressBarValue = value;
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
            dialog.FileName = "FF12Rando_" + seed + "_Seed";
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

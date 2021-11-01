using Bartz24.Data;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data;
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

namespace LRRando
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

        public MainWindow()
        {
            LRFlags.Init();
            Flags.FlagsList.Where(f => !f.Experimental).ForEach(f => f.FlagEnabled = true);
            InitializeComponent();
            this.DataContext = this;
            HideProgressBar();
            DataExtensions.Mode = ByteMode.BigEndian;
        }

        private async void generateButton_Click(object sender, RoutedEventArgs e)
        {
            RandomizerManager randomizers = new RandomizerManager();
            randomizers.Add(new TreasureRando(randomizers));
            randomizers.Add(new EquipRando(randomizers));
            randomizers.Add(new ShopRando(randomizers));
            randomizers.Add(new AbilityRando(randomizers));
            randomizers.Add(new EnemyRando(randomizers));
            randomizers.Add(new BattleRando(randomizers));
            randomizers.Add(new QuestRando(randomizers));
            randomizers.Add(new MusicRando(randomizers));

            if (String.IsNullOrEmpty(SetupData.Paths["Nova"]) || !File.Exists(SetupData.Paths["Nova"]))
            {
                MessageBox.Show("NovaChrysalia.exe needs to be selected. Download Nova Chrysalia and setup the path in the '1. Setup' step.", "Nova Chrysalia not found.");
                return;
            }

            if (!Nova.IsUnpacked("LR", @"db\resident\wdbpack.bin", SetupData.GetSteamPath("LR")))
            {
                MessageBox.Show("LR needs to be unpacked.\nOpen NovaChrysalia and 'Unpack Game Data' for LR.", "LR is not unpacked");
                return;
            }

            try
            {

                this.IsEnabled = false;
                await Task.Run(() =>
                {
                    string outFolder = System.IO.Path.GetTempPath() + @"lr_rando_temp";
                    SetupData.OutputFolder = outFolder + @"\Data";

                    int seed = RandomNum.GetIntSeed(SetupData.Seed);
                    foreach (Flag flag in Flags.FlagsList)
                    {
                        flag.ResetRandom(seed);
                    }
                    RandomNum.ClearRand();

                    SetProgressBar("Preparing data folder...", -1);
                    if (Directory.Exists(outFolder))
                        Directory.Delete(outFolder, true);
                    Directory.CreateDirectory(outFolder);
                    CopyFromTemplate(outFolder, "data\\modpack");

                    SetProgressBar("Loading Data...", -1);

                    string wdbpackPath = Nova.GetNovaFile("LR", @"db\resident\wdbpack.bin", SetupData.Paths["Nova"], SetupData.Paths["LR"]);
                    string wdbpackOutPath = SetupData.OutputFolder + @"\db\resident\wdbpack.bin";
                    FileExtensions.CopyFile(wdbpackPath, wdbpackOutPath);
                    SetupData.WPDTracking.Clear();
                    SetupData.WPDTracking.Add(wdbpackOutPath, new List<string>());
                    Nova.UnpackWPD(wdbpackOutPath, SetupData.Paths["Nova"]);

                    randomizers.ForEach(r => r.Load());
                    randomizers.ForEach(r =>
                    {
                        SetProgressBar(r.GetProgressMessage(), 0);
                        r.Randomize(v => ProgressBarValue = v);
                    });
                    SetProgressBar("Saving Data...", -1);
                    randomizers.ForEach(r => r.Save());

                    Nova.CleanWPD(wdbpackOutPath, SetupData.WPDTracking[wdbpackOutPath]);

                    SetProgressBar("Generating ModPack...", -1);
                    string zipName = $"packs\\LRRando_{seed}.ncmp";
                    if (File.Exists(zipName))
                        File.Delete(zipName);
                    if (!Directory.Exists("packs"))
                        Directory.CreateDirectory("packs");
                    ZipFile.CreateFromDirectory(outFolder, zipName);

                    Directory.Delete(outFolder, true);

                    SetProgressBar($"Complete! Ready to install in Nova Chrysalia! The modpack 'LRRando_{seed}.ncmp' has been generated in the packs folder of this application.", 100);
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
    }
}

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
        RandoUI.Init(SetProgressBar, SwitchTab);
        RandoSeeds.DocsFolder = "docs";
        RandoSeeds.DeleteFilter = "FF12Rando_${SEED}_Docs.zip";
        FF12Flags.Init();
        RandoPresets.Init();
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

    private async void generateButton_Click(object sender, RoutedEventArgs e)
    {
        using (FF12SeedGenerator generator = new(SetProgressBar)
        {
            IncrementTotalProgress = totalProgressBar.IncrementProgress
        })
        {
            totalProgressBar.TotalSegments = (generator.Randomizers.Count * 3) + 2;
            totalProgressBar.SetProgress(0, 0);

            try
            {
                IsEnabled = false;
                await Task.Run(() =>
                {
                    generator.GenerateSeed();
                });
                IsEnabled = true;
            }
            catch (RandoException ex)
            {
                if (ex.Title == SeedGenerator.UNEXPECTED_ERROR)
                {
                    Exception innerMost = ex;
                    while (innerMost.InnerException != null)
                    {
                        innerMost = innerMost.InnerException;
                    }

                    MessageBox.Show("Seed generation failed with an unexpected error.\n\n" + ex.Message + "\n\nStack trace:\n" + innerMost.StackTrace, ex.Title);
                }
                else
                {
                    MessageBox.Show("Seed generation failed.\n\n" + ex.Message, ex.Title);
                }

                SetProgressBar("Seed generation failed.", 0);

                IsEnabled = true;
            }
        }
    }
    private void SetProgressBar(string text, int value, int maxValue = 100)
    {
        Dispatcher.Invoke(() =>
        {
            ProgressBarIndeterminate = value < 0;
            ProgressBarVisible = ProgressBarIndeterminate || maxValue > 0 ? Visibility.Visible : Visibility.Hidden;
            ProgressBarText = text;
            ProgressBarValue = value;
            ProgressBarMaximum = maxValue;
            totalProgressBar.SetProgress(totalProgressBar.GetProgress(), ProgressBarIndeterminate ? -1 : (float)ProgressBarValue / ProgressBarMaximum);

            if (ProgressBarVisible == Visibility.Hidden)
            {
                // Start a timer to hide the progress bar after a short delay
                Task.Delay(3000).ContinueWith(t =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (ProgressBarVisible == Visibility.Hidden)
                        {
                            ProgressBarText = "";
                        }
                    });
                });
            }
        });
    }

    private void SwitchTab(int tabIndex)
    {
        Dispatcher.Invoke(() =>
        {
            WindowTabs.SelectedIndex = tabIndex;
        });
    }

    private void HideProgressBar()
    {
        Dispatcher.Invoke(() =>
        {
            ProgressBarVisible = Visibility.Hidden;
        });
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

    private void SaveSeedJSON(string file)
    {
        int seed = RandomNum.GetIntSeed(SetupData.Seed);
        string output = RandoFlags.Serialize(seed.ToString(), SetupData.Version);
        File.WriteAllText(file, output);
    }

    private void uninstallButton_Click(object sender, RoutedEventArgs e)
    {
        FF12SeedGenerator gen = new (null);
        if (Directory.Exists(gen.OutFolder))
        {
            try
            {
                gen.RemoveLuaScripts();
                Directory.Delete(gen.OutFolder, true);
            }
            catch
            {
                MessageBox.Show("Encountered an error while removing the current seed files.");
                return;
            }

            MessageBox.Show("Seed uninstall complete.");
        }
        else
        {
            MessageBox.Show("No seed was installed to delete.");
        }
    }

    private void uninstallLoadersButton_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Remove mod loader files?\nIf you installed the loaders through Vortex, click 'Cancel' and then uninstall them through Vortex.", "Remove mod loader files?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
        {
            try
            {
                FF12SeedGenerator.UninstallFileLoader();
                FF12SeedGenerator.UninstallLuaLoader();
            }
            catch
            {
                MessageBox.Show("Encountered an error while removing mod loader files.");
                return;
            }

            // Workaround since we can't set the name on this for some reason
            SetupPaths setupPaths = (SetupPaths)this.GetByUid("setupPaths");
            setupPaths.UpdateText();

            MessageBox.Show("Removed any remaining mod loader files.");
        }
    }

    private void uninstallDescriptiveButton_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Remove The Insurgent's Descriptive Inventory files?\nIf you installed the mod through Vortex, click 'Cancel' and then uninstall them through Vortex.", "Remove mod?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
        {
            try
            {
                FF12SeedGenerator.UninstallDescriptive();
                string configFolder = Path.Combine(SetupData.Paths["12"], "x64\\scripts\\config\\TheInsurgentsDescriptiveInventoryConfig");
                if (Directory.Exists(configFolder))
                {
                    Directory.Delete(configFolder, true);
                }
            }
            catch
            {
                MessageBox.Show("Encountered an error while removing mod loader files.");
                return;
            }

            // Workaround since we can't set the name on this for some reason
            SetupPaths setupPaths = (SetupPaths)this.GetByUid("setupPaths");
            setupPaths.UpdateText();

            MessageBox.Show("Removed The Insurgent's Descriptive Inventory files.");
        }
    }
}

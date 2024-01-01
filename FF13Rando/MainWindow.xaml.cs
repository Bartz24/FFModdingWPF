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
using System.Threading.Tasks;
using System.Windows;

namespace FF13Rando;

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
        FF13Flags.Init();
        RandoPresets.Init();
        InitializeComponent();
        DataContext = this;
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
        using (FF13SeedGenerator generator = new())
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
                Exception innerMost = ex;
                while (innerMost.InnerException != null)
                {
                    innerMost = innerMost.InnerException;
                }

                MessageBox.Show("Randomizer encountered an error.\n\n" + ex.Message + "\n\nStack trace:\n" + innerMost.StackTrace, ex.Title);
                IsEnabled = true;
            }
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
            FileName = "FF13Rando_" + seed + "_Seed"
        };
        if ((bool)dialog.ShowDialog())
        {
            string path = dialog.FileName.Replace("/", "\\");
            RandoHelpers.SaveSeedJSON(path);
        }
    }

    private void shareSeedModpackFolder_Click(object sender, RoutedEventArgs e)
    {

    }
}

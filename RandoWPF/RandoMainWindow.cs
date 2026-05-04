using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Bartz24.RandoWPF;
public abstract class RandoMainWindow : Window
{

    public static readonly DependencyProperty ProgressBarValueProperty =
    DependencyProperty.Register(nameof(ProgressBarValue), typeof(int), typeof(RandoMainWindow));
    public int ProgressBarValue
    {
        get => (int)GetValue(ProgressBarValueProperty);
        set => SetValue(ProgressBarValueProperty, value);
    }
    public static readonly DependencyProperty ProgressBarMaximumProperty =
    DependencyProperty.Register(nameof(ProgressBarMaximum), typeof(int), typeof(RandoMainWindow));
    public int ProgressBarMaximum
    {
        get => (int)GetValue(ProgressBarMaximumProperty);
        set => SetValue(ProgressBarMaximumProperty, value);
    }
    public static readonly DependencyProperty ProgressBarVisibleProperty =
    DependencyProperty.Register(nameof(ProgressBarVisible), typeof(Visibility), typeof(RandoMainWindow));
    public Visibility ProgressBarVisible
    {
        get => (Visibility)GetValue(ProgressBarVisibleProperty);
        set => SetValue(ProgressBarVisibleProperty, value);
    }
    public static readonly DependencyProperty ProgressBarIndeterminateProperty =
    DependencyProperty.Register(nameof(ProgressBarIndeterminate), typeof(bool), typeof(RandoMainWindow));
    public bool ProgressBarIndeterminate
    {
        get => (bool)GetValue(ProgressBarIndeterminateProperty);
        set => SetValue(ProgressBarIndeterminateProperty, value);
    }

    public static readonly DependencyProperty ProgressBarTextProperty =
    DependencyProperty.Register(nameof(ProgressBarText), typeof(string), typeof(RandoMainWindow));
    public string ProgressBarText
    {
        get => (string)GetValue(ProgressBarTextProperty);
        set => SetValue(ProgressBarTextProperty, value);
    }

    public static readonly DependencyProperty ChangelogTextProperty =
    DependencyProperty.Register(nameof(ChangelogText), typeof(string), typeof(RandoMainWindow));
    public string ChangelogText
    {
        get => (string)GetValue(ChangelogTextProperty);
        set => SetValue(ChangelogTextProperty, value);
    }
    public static readonly DependencyProperty APVisibleProperty =
    DependencyProperty.Register(nameof(APVisible), typeof(Visibility), typeof(RandoMainWindow));
    public Visibility APVisible
    {
        get => (Visibility)GetValue(APVisibleProperty);
        set => SetValue(APVisibleProperty, value);
    }
    public RandoMainWindow()
    {
        RandoUI.Init(SetProgressBar, () => TotalProgressBar.IncrementProgress(), SwitchTab);
        HideProgressBar();

        APVisible = Visibility.Hidden;
        RandoFlags.SelectedChanged += (s, e) =>
        {
            APVisible = RandoFlags.Mode == RandoFlags.SeedMode.Archipelago ? Visibility.Visible : Visibility.Hidden;
        };

        ChangelogText = File.ReadAllText(@"data\changelog.txt");
    }

    protected async void generateButton_Click(object sender, RoutedEventArgs e)
    {
        using (var generator = Generator)
        {
            TotalProgressBar.TotalSegments = (generator.Randomizers.Count * 3) + 2;
            TotalProgressBar.SetProgress(0, 0);

#if !DEBUG
            try
            {
#endif
            IsEnabled = false;
            await Task.Run(() =>
            {
                generator.GenerateSeed();
            });
            IsEnabled = true;
#if !DEBUG
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
#endif
        }
    }

    protected async void generateDocsButton_Click(object sender, RoutedEventArgs e)
    {
        using (var generator = Generator)
        {
            TotalProgressBar.TotalSegments = (generator.Randomizers.Count * 2) + 1;
            TotalProgressBar.SetProgress(0, 0);

#if !DEBUG
            try
            {
#endif
            IsEnabled = false;
            await Task.Run(() =>
            {
                generator.GenerateSeedDryRun();
            });
            IsEnabled = true;
#if !DEBUG
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
#endif
        }
    }

    protected abstract SeedGenerator Generator { get; }

    protected abstract SegmentedProgressBar TotalProgressBar { get; }

    protected void SetProgressBar(string text, int value, int maxValue = 100)
    {
        Dispatcher.Invoke(() =>
        {
            ProgressBarIndeterminate = value < 0;
            ProgressBarVisible = ProgressBarIndeterminate || maxValue > 0 ? Visibility.Visible : Visibility.Hidden;
            ProgressBarText = text;
            ProgressBarValue = value;
            ProgressBarMaximum = maxValue;
            TotalProgressBar.SetProgress(TotalProgressBar.GetProgress(), ProgressBarIndeterminate ? -1 : (float)ProgressBarValue / ProgressBarMaximum);

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

    protected void SwitchTab(int tabIndex)
    {
        Dispatcher.Invoke(() =>
        {
            MainWindowTabs.SelectedIndex = tabIndex;
        });
    }

    protected abstract System.Windows.Controls.TabControl MainWindowTabs { get; }

    protected void HideProgressBar()
    {
        Dispatcher.Invoke(() =>
        {
            ProgressBarVisible = Visibility.Hidden;
        });
    }

    protected void NextStepButton_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            MainWindowTabs.SelectedIndex++;
        });
    }

    protected void PrevStepButton_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            MainWindowTabs.SelectedIndex--;
        });
    }
}

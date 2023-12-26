using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Bartz24.RandoWPF;

/// <summary>
/// Interaction logic for SeedInfoPage.xaml
/// </summary>
public partial class SeedInfoPage : UserControl
{
    public ObservableCollection<SeedInformation> SeedInformationList { get; set; }
    public ObservableCollection<string> CategoryList { get; set; } = new ObservableCollection<string>();

    public Visibility SaveVisible => RandoPresets.Selected.CustomModified ? Visibility.Visible : Visibility.Collapsed;
    public Visibility DeleteVisible => RandoPresets.Selected.CustomLoaded ? Visibility.Visible : Visibility.Collapsed;

    public SeedInfoPage()
    {
        InitializeComponent();

        SeedInformationList = new ();
        RandoSeeds.SeedsLoaded += RandoSeeds_SeedsLoaded;
        RandoSeeds.SeedDeleted += RandoSeeds_SeedDeleted;
        RandoSeeds.LoadSeeds();

        DataContext = this;
    }

    private void RandoSeeds_SeedDeleted(object sender, RandoSeeds.DeleteEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            SeedInformationList.Remove(e.Information);

            noSeedsText.Visibility = SeedInformationList.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        });
    }

    private void RandoSeeds_SeedsLoaded(object sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            SeedInformationList.Clear();

            foreach (var seed in RandoSeeds.Seeds)
            {
                SeedInformationList.Add(seed);
            }

            noSeedsText.Visibility = SeedInformationList.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        });
    }

    private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!e.Handled)
        {
            e.Handled = true;
            MouseWheelEventArgs eventArg = new(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            UIElement parent = ((Control)sender).Parent as UIElement;
            parent.RaiseEvent(eventArg);
        }
    }
}
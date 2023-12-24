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
/// Interaction logic for FlagsPage.xaml
/// </summary>
public partial class FlagsPage : UserControl
{

    public ObservableCollection<Preset> PresetsList { get; set; } = new ObservableCollection<Preset>();
    public ObservableCollection<string> CategoryList { get; set; } = new ObservableCollection<string>();

    public Visibility SaveVisible => RandoPresets.Selected.CustomModified ? Visibility.Visible : Visibility.Hidden;
    public Visibility DeleteVisible => RandoPresets.Selected.CustomLoaded ? Visibility.Visible : Visibility.Hidden;

    public FlagsPage()
    {
        InitializeComponent();
        DataContext = this;
        PresetsList = new ObservableCollection<Preset>(RandoPresets.PresetsList);
        RandoPresets.SelectedChanged += Presets_SelectedChanged;
        CategoryList = new ObservableCollection<string>(RandoFlags.CategoryList);
        RandoFlags.SelectedChanged += Flags_SelectedChanged;

        flagsListBox.ItemsSource = new ObservableCollection<Flag>(RandoFlags.FlagsList);
        CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(flagsListBox.ItemsSource);
        view.Filter = FlagFilter;
    }

    private void Presets_SelectedChanged(object sender, EventArgs e)
    {
        SaveButton.GetBindingExpression(VisibilityProperty).UpdateTarget();
        DeleteButton.GetBindingExpression(VisibilityProperty).UpdateTarget();
    }

    private bool FlagFilter(object item)
    {
        Flag flag = (Flag)item;
        if (RandoFlags.SelectedCategory == RandoFlags.CategoryMap[RandoFlags.FlagTypeDebug])
        {
            return flag.Debug || flag.FlagPropertiesDebugIncluded.Where(p => p.Debug).Count() > 0;
        }
        // Never show debug flags otherwise
        return !flag.Debug
&& (RandoFlags.SelectedCategory == RandoFlags.CategoryMap[RandoFlags.FlagTypeAll] || RandoFlags.SelectedCategory == RandoFlags.CategoryMap[flag.FlagType]);
    }

    private void Flags_SelectedChanged(object sender, EventArgs e)
    {
        CollectionViewSource.GetDefaultView(flagsListBox.ItemsSource).Refresh();
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

    private void LoadPreset_Click(object sender, RoutedEventArgs e)
    {
        VistaOpenFileDialog dialog = new()
        {
            Title = "Please select a JSON preset.",
            Multiselect = false,
            Filter = "JSON|*.json"
        };
        if ((bool)dialog.ShowDialog())
        {
            string path = dialog.FileName.Replace("/", "\\");
            if (File.Exists(path))
            {
                try
                {
                    RandoPresets.LoadPreset(path, true);
                    RandoPresets.Selected = RandoPresets.PresetsList.Last(p => !p.CustomModified);
                    PresetsList = new ObservableCollection<Preset>(RandoPresets.PresetsList);
                    PresetComboBox.GetBindingExpression(ComboBox.ItemsSourceProperty).UpdateTarget();

                    if (!Directory.Exists("presets"))
                    {
                        Directory.CreateDirectory("presets");
                    }

                    File.Copy(path, @"presets\" + System.IO.Path.GetFileName(path));
                    RandoUI.SetUIMessage($"Loaded preset {RandoPresets.Selected.Name}.");
                }
                catch
                {
                    MessageBox.Show("Failed to load the preset file.");
                }
            }
            else
            {
                MessageBox.Show("Make sure the JSON file is a preset for rando.", "The selected file is not valid");
            }
        }
    }

    private async void SavePreset_Click(object sender, RoutedEventArgs e)
    {
        NewPresetName.Text = "New Preset";
        bool result = (bool)await DialogHost.Show(PresetNameDialog.DialogContent, "Main");
        if (result)
        {
            string name = NewPresetName.Text;
            string output = RandoPresets.Serialize(name, SetupData.Version);
            if (File.Exists(@"presets\" + name + "_Preset.json"))
            {
                // Ask for confirmation to override. Append a number if they say no.
                if (MessageBox.Show("A preset with this name already exists. Do you want to override it?", "Override preset?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    int i = 1;
                    while (File.Exists(@"presets\" + name + "_" + i + "_Preset.json"))
                    {
                        i++;
                    }

                    name += "_" + i;
                }
            }

            if (!Directory.Exists("presets"))
            {
                Directory.CreateDirectory("presets");
            }

            File.WriteAllText(@"presets\" + name + "_Preset.json", output);

            RandoPresets.LoadPreset(@"presets\" + name + "_Preset.json", true);
            RandoPresets.Selected = RandoPresets.PresetsList.Last(p => !p.CustomModified);
            PresetsList = new ObservableCollection<Preset>(RandoPresets.PresetsList);
            PresetComboBox.GetBindingExpression(ComboBox.ItemsSourceProperty).UpdateTarget();
            RandoUI.SetUIMessage($"Saved preset to the presets folder as {name + "_Preset.json"}.");
        }
    }

    private void DeletePreset_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Delete the selected preset?", "Delete preset?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            string presetName = RandoPresets.Selected.Name;
            File.Delete(RandoPresets.Selected.PresetPath);
            RandoPresets.PresetsList.Remove(RandoPresets.Selected);
            RandoPresets.Selected = RandoPresets.PresetsList[0];
            PresetsList = new ObservableCollection<Preset>(RandoPresets.PresetsList);
            PresetComboBox.GetBindingExpression(ComboBox.ItemsSourceProperty).UpdateTarget();
            RandoUI.SetUIMessage($"Deleted preset {presetName}.");
        }
    }
}
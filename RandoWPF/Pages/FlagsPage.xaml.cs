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

    public bool SaveEnabled => RandoPresets.Selected.CustomModified;
    public bool DeleteEnabled => RandoPresets.Selected.CustomLoaded;

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
        SaveButton.GetBindingExpression(IsEnabledProperty).UpdateTarget();
        DeleteButton.GetBindingExpression(IsEnabledProperty).UpdateTarget();
    }

    private bool FlagFilter(object item)
    {
        Flag flag = (Flag)item;
        if (RandoFlags.SelectedCategory == RandoFlags.CategoryMap[RandoFlags.FlagTypeDebug])
        {
            return flag.Debug || flag.FlagPropertiesDebugIncluded.Where(p => p.Debug).Count() > 0;
        }
        if (RandoFlags.SelectedCategory == RandoFlags.CategoryMap[RandoFlags.FlagTypeArchipelago])
        {
            if (flag.HasArchipelagoOverride)
            {
                return false;
            }

            if (flag.FlagPropertiesDebugIncluded.Count == 0)
            {
                return true;
            }

            return flag.FlagPropertiesDebugIncluded.Where(p => p.DisabledByArchipelago).Count() == 0;
        }
        // Never show debug flags otherwise
        return !flag.Debug
&& (RandoFlags.SelectedCategory == RandoFlags.CategoryMap[RandoFlags.FlagTypeAll] || RandoFlags.SelectedCategory == RandoFlags.CategoryMap[flag.FlagType]);
    }

    private void Flags_SelectedChanged(object sender, EventArgs e)
    {
        CollectionViewSource.GetDefaultView(flagsListBox.ItemsSource).Refresh();
        if ((string)categoryCombo.SelectedValue != RandoFlags.SelectedCategory)
        {
            categoryCombo.SelectedValue = RandoFlags.SelectedCategory;
        }
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
                    RandoUI.ShowTempUIMessage($"Loaded preset {RandoPresets.Selected.Name}.");
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

    private void SavePreset_Click(object sender, RoutedEventArgs e)
    {
        string presetsFolder = Path.GetFullPath("presets");
        if (!Directory.Exists(presetsFolder))
        {
            Directory.CreateDirectory(presetsFolder);
        }

        VistaSaveFileDialog dialog = new()
        {
            Title = "Save the preset as a JSON file.",
            Filter = "JSON|*.json",
            DefaultExt = "json",
            AddExtension = true,
            OverwritePrompt = true,
            InitialDirectory = presetsFolder,
            FileName = Path.Combine(presetsFolder, "New Preset_Preset.json")
        };
        if ((bool)dialog.ShowDialog())
        {
            string path = dialog.FileName.Replace("/", "\\");
            string name = Path.GetFileNameWithoutExtension(path);
            if (name.EndsWith("_Preset"))
            {
                name = name[..^"_Preset".Length];
            }

            File.WriteAllText(path, RandoPresets.Serialize(name, SetupData.Version));

            RandoPresets.LoadPreset(path, true);
            RandoPresets.Selected = RandoPresets.PresetsList.Last(p => !p.CustomModified);
            PresetsList = new ObservableCollection<Preset>(RandoPresets.PresetsList);
            PresetComboBox.GetBindingExpression(ComboBox.ItemsSourceProperty).UpdateTarget();
            RandoUI.ShowTempUIMessage($"Saved preset to {path}.");
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
            RandoUI.ShowTempUIMessage($"Deleted preset {presetName}.");
        }
    }
}
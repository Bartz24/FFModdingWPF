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

namespace Bartz24.RandoWPF
{
    /// <summary>
    /// Interaction logic for FlagsPage.xaml
    /// </summary>
    public partial class FlagsPage : UserControl
    {

        public ObservableCollection<Preset> PresetsList { get; set; } = new ObservableCollection<Preset>();
        public ObservableCollection<string> CategoryList { get; set; } = new ObservableCollection<string>();

        public Visibility SaveVisible
        {
            get => Presets.Selected.CustomModified ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility DeleteVisible
        {
            get => Presets.Selected.CustomLoaded ? Visibility.Visible : Visibility.Collapsed;
        }

        public FlagsPage()
        {
            InitializeComponent();
            this.DataContext = this;
            PresetsList = new ObservableCollection<Preset>(Presets.PresetsList);
            Presets.SelectedChanged += Presets_SelectedChanged;
            CategoryList = new ObservableCollection<string>(Flags.CategoryList);
            Flags.SelectedChanged += Flags_SelectedChanged;

            flagsListBox.ItemsSource = new ObservableCollection<Flag>(Flags.FlagsList);
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
            return Flags.SelectedCategory == Flags.CategoryMap[-1] || Flags.SelectedCategory == Flags.CategoryMap[((Flag)item).FlagType];
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
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void LoadPreset_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Title = "Please select a JSON preset.";
            dialog.Multiselect = false;
            dialog.Filter = "JSON|*.json";
            if ((bool)dialog.ShowDialog())
            {
                string path = dialog.FileName.Replace("/", "\\");
                if (File.Exists(path))
                {
                    try
                    {
                        Presets.LoadPreset(path, true);
                        Presets.Selected = Presets.PresetsList.Last(p => !p.CustomModified);
                        PresetsList = new ObservableCollection<Preset>(Presets.PresetsList);
                        PresetComboBox.GetBindingExpression(ComboBox.ItemsSourceProperty).UpdateTarget();

                        if (!Directory.Exists("presets"))
                            Directory.CreateDirectory("presets");
                        File.Copy(path, @"presets\" + System.IO.Path.GetFileName(path));
                    }
                    catch
                    {
                        MessageBox.Show("Failed to load the preset file.");
                    }
                }
                else
                    MessageBox.Show("Make sure the JSON file is a preset for rando.", "The selected file is not valid");
            }
        }

        private async void SavePreset_Click(object sender, RoutedEventArgs e)
        {
            NewPresetName.Text = "New Preset";
            bool result = (bool)await DialogHost.Show(PresetNameDialog.DialogContent, "Main");
            if (result)
            {
                string name = NewPresetName.Text;
                int index = 0;
                string output = Presets.Serialize(name, SetupData.Version);
                while (File.Exists(@"presets\" + name + (index > 0 ? $" ({index})" : "") + "_Preset.json"))
                {
                    index++;
                }
                if (!Directory.Exists("presets"))
                    Directory.CreateDirectory("presets");
                File.WriteAllText(@"presets\" + name + (index > 0 ? $" ({index})" : "") + "_Preset.json", output);

                Presets.LoadPreset(@"presets\" + name + (index > 0 ? $" ({index})" : "") + "_Preset.json", true);
                Presets.Selected = Presets.PresetsList.Last(p => !p.CustomModified);
                PresetsList = new ObservableCollection<Preset>(Presets.PresetsList);
                PresetComboBox.GetBindingExpression(ComboBox.ItemsSourceProperty).UpdateTarget();
            }
        }

        private void DeletePreset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Delete the selected preset?", "Delete preset?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                File.Delete(Presets.Selected.PresetPath);
                Presets.PresetsList.Remove(Presets.Selected);
                Presets.Selected = Presets.PresetsList[0];
                PresetsList = new ObservableCollection<Preset>(Presets.PresetsList);
                PresetComboBox.GetBindingExpression(ComboBox.ItemsSourceProperty).UpdateTarget();
            }
        }
    }
}
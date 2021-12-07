using Bartz24.RandoWPF.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bartz24.RandoWPF
{
    /// <summary>
    /// Interaction logic for FlagsPage.xaml
    /// </summary>
    public partial class FlagsPage : UserControl
    {

        public ObservableCollection<Preset> PresetsList { get; set; } = new ObservableCollection<Preset>();
        public ObservableCollection<string> CategoryList { get; set; } = new ObservableCollection<string>();
        public FlagsPage()
        {
            InitializeComponent();
            this.DataContext = this;
            PresetsList = new ObservableCollection<Preset>(Presets.PresetsList);
            CategoryList = new ObservableCollection<string>(Flags.CategoryList);
            Flags.SelectedChanged += Flags_SelectedChanged;

            flagsListBox.ItemsSource = new ObservableCollection<Flag>(Flags.FlagsList);
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(flagsListBox.ItemsSource);
            view.Filter = FlagFilter;
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
    }
}
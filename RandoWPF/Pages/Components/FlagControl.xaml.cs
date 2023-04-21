using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Bartz24.RandoWPF
{
    /// <summary>
    /// Interaction logic for FlagControl.xaml
    /// </summary>
    public partial class FlagControl : UserControl
    {
        public ObservableCollection<FlagProperty> PropertyList { get; set; } = new ObservableCollection<FlagProperty>();

        public static readonly DependencyProperty FlagProperty =
        DependencyProperty.Register(nameof(Flag), typeof(Flag), typeof(FlagControl), new PropertyMetadata(PropChanged));

        public Flag Flag
        {
            get => (Flag)GetValue(FlagProperty);
            set => SetValue(FlagProperty, value);
        }
        public string Description { get => Flag.Description; }

        public FlagControl()
        {
            InitializeComponent();

            if (PropertyList != null)
            {
                var view = CollectionViewSource.GetDefaultView(PropertyList);
                view.Filter = FlagFilter;
            }
        }
        private bool FlagFilter(object item)
        {
            FlagProperty prop = (FlagProperty)item;
            if (Flags.SelectedCategory == Flags.CategoryMap[Flags.FlagTypeDebug])
            {
                return prop.Debug;
            }
            return !prop.Debug;
        }

        public static void PropChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FlagControl control = (FlagControl)sender;
            control.PropertyList = new ObservableCollection<FlagProperty>(control.Flag.FlagPropertiesDebugIncluded);
            if (control.propertiesListBox.ItemsSource == null)
            {
                control.propertiesListBox.ItemsSource = control.PropertyList;
            }
            var view = CollectionViewSource.GetDefaultView(control.propertiesListBox.ItemsSource);
            view.Filter = control.FlagFilter;
        }
    }
}

using Bartz24.RandoWPF;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Bartz24.RandoWPF;

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
    public string Description => Flag.Description;

    public FlagControl()
    {
        InitializeComponent();

        if (PropertyList != null)
        {
            System.ComponentModel.ICollectionView view = CollectionViewSource.GetDefaultView(PropertyList);
            view.Filter = FlagFilter;
        }
    }
    private bool FlagFilter(object item)
    {
        FlagProperty prop = (FlagProperty)item;
        return RandoFlags.SelectedCategory == RandoFlags.CategoryMap[RandoFlags.FlagTypeDebug] ? prop.Debug : !prop.Debug;
    }

    public static void PropChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        FlagControl control = (FlagControl)sender;
        control.PropertyList = new ObservableCollection<FlagProperty>(control.Flag.FlagPropertiesDebugIncluded);
        control.propertiesListBox.ItemsSource ??= control.PropertyList;
        System.ComponentModel.ICollectionView view = CollectionViewSource.GetDefaultView(control.propertiesListBox.ItemsSource);
        view.Filter = control.FlagFilter;
    }
}

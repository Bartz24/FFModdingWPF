using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

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
        }

        public static void PropChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FlagControl control = (FlagControl)sender;
            control.PropertyList = new ObservableCollection<FlagProperty>(control.Flag.FlagProperties);
        }
    }
}

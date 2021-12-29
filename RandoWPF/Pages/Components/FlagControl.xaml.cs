using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            FlagControl control = (FlagControl) sender;
            control.PropertyList = new ObservableCollection<FlagProperty>(control.Flag.FlagProperties);
        }
    }
}

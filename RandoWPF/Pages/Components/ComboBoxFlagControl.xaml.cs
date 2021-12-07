using System;
using System.Collections.Generic;
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
    /// Interaction logic for ComboBoxFlagControl.xaml
    /// </summary>
    public partial class ComboBoxFlagControl : UserControl
    {
        public static readonly DependencyProperty FlagPropertyProperty =
        DependencyProperty.Register(nameof(FlagProperty), typeof(ComboBoxFlagProperty), typeof(ComboBoxFlagControl));

        public ComboBoxFlagProperty FlagProperty
        {
            get => (ComboBoxFlagProperty)GetValue(FlagPropertyProperty);
            set => SetValue(FlagPropertyProperty, value);
        }

        public ComboBoxFlagControl()
        {
            InitializeComponent();
        }
    }
}

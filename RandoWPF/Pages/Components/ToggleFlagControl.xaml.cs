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
    /// Interaction logic for ToggleFlagControl.xaml
    /// </summary>
    public partial class ToggleFlagControl : UserControl
    {
        public static readonly DependencyProperty FlagPropertyProperty =
        DependencyProperty.Register(nameof(FlagProperty), typeof(ToggleFlagProperty), typeof(ToggleFlagControl));

        public ToggleFlagProperty FlagProperty
        {
            get => (ToggleFlagProperty)GetValue(FlagPropertyProperty);
            set => SetValue(FlagPropertyProperty, value);
        }

        public ToggleFlagControl()
        {
            InitializeComponent();
        }
    }
}

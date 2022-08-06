using System.Windows;
using System.Windows.Controls;

namespace Bartz24.RandoWPF
{
    /// <summary>
    /// Interaction logic for NumberFlagControl.xaml
    /// </summary>
    public partial class NumberFlagControl : UserControl
    {
        public static readonly DependencyProperty FlagPropertyProperty =
        DependencyProperty.Register(nameof(FlagProperty), typeof(NumberFlagProperty), typeof(NumberFlagControl));

        public NumberFlagProperty FlagProperty
        {
            get => (NumberFlagProperty)GetValue(FlagPropertyProperty);
            set => SetValue(FlagPropertyProperty, value);
        }

        public NumberFlagControl()
        {
            InitializeComponent();
        }
    }
}

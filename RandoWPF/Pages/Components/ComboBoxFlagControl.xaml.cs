using System.Windows;
using System.Windows.Controls;

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

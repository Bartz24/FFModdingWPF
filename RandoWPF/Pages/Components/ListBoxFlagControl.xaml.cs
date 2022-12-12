using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Bartz24.RandoWPF
{
    /// <summary>
    /// Interaction logic for ListBoxFlagControl.xaml
    /// </summary>
    public partial class ListBoxFlagControl : UserControl
    {
        public static readonly DependencyProperty FlagPropertyProperty =
        DependencyProperty.Register(nameof(FlagProperty), typeof(ListBoxFlagProperty), typeof(ListBoxFlagControl));

        public ListBoxFlagProperty FlagProperty
        {
            get => (ListBoxFlagProperty)GetValue(FlagPropertyProperty);
            set => SetValue(FlagPropertyProperty, value);
        }

        public ListBoxFlagControl()
        {
            InitializeComponent();            
        }

        private void selectAllButton_Click(object sender, RoutedEventArgs e)
        {
            FlagProperty.SelectedValues = new List<string>(FlagProperty.Values);
        }

        private void deselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            FlagProperty.SelectedValues = new List<string>();
        }
    }
}

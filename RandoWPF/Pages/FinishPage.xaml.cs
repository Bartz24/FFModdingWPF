using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Bartz24.RandoWPF
{
    /// <summary>
    /// Interaction logic for FinishPage.xaml
    /// </summary>
    [ContentProperty(nameof(Children))]
    public partial class FinishPage : UserControl
    {
        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
            nameof(Children),
            typeof(UIElementCollection),
            typeof(FinishPage),
            new PropertyMetadata());

        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }
        public FinishPage()
        {
            InitializeComponent();
            Children = PART_Host.Children;
        }
    }
}
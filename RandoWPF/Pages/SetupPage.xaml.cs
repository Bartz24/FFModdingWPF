using Bartz24.RandoWPF.Data;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bartz24.RandoWPF
{
    /// <summary>
    /// Interaction logic for SetupPage.xaml
    /// </summary>
    [ContentProperty(nameof(Children))]
    public partial class SetupPage : UserControl
    {
        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
            nameof(Children),
            typeof(UIElementCollection),
            typeof(SetupPage),
            new PropertyMetadata());
        public string Seed
        {
            get => SetupData.Seed; 
            set
            {
                SetupData.Seed = value;
                seedText.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }

        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }
        public SetupPage()
        {
            InitializeComponent();
            this.DataContext = this;
            Children = PART_Host.Children;
            Seed = RandomNum.RandSeed().ToString();
        }

        private void importJSONButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void importHistoryButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void seedButton_Click(object sender, RoutedEventArgs e)
        {
            Seed = RandomNum.RandSeed().ToString();
        }
    }
}
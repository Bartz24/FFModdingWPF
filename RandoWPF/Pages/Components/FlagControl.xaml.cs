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
        public static readonly DependencyProperty PresetProperty =
        DependencyProperty.Register(nameof(Flag), typeof(Flag), typeof(FlagControl));

        public Flag Flag
        {
            get => (Flag)GetValue(PresetProperty);
            set => SetValue(PresetProperty, value);
        }
        public string Description { get => Flag.Description; }

        public FlagControl()
        {
            InitializeComponent();
        }

        private void Flag_OnFlagUpdated(object sender, EventArgs e)
        {
            SetValue(PresetProperty, Flag);
        }
    }
}

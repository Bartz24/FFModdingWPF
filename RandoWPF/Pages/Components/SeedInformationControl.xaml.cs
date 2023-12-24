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

namespace Bartz24.RandoWPF;
/// <summary>
/// Interaction logic for SeedInformationControl.xaml
/// </summary>
public partial class SeedInformationControl : UserControl
{
    private SeedInformation Info { get => (SeedInformation)DataContext; }
    public SeedInformationControl()
    {
        InitializeComponent();
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        RandoSeeds.LoadSeed(Info);
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) || MessageBox.Show("Are you sure you want to delete this seed?", "Delete Seed", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            RandoSeeds.DeleteSeed(Info);
        }
    }

    private void ShareButton_Click(object sender, RoutedEventArgs e)
    {
        RandoSeeds.ShareStringSeed(Info);
    }

    private void ShareJSONButton_Click(object sender, RoutedEventArgs e)
    {
        RandoSeeds.ShareFileSeed(Info);
    }
}

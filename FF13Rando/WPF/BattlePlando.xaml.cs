using Bartz24.RandoWPF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace FF13Rando;

/// <summary>
/// Interaction logic for BattlePlando.xaml
/// </summary>
public partial class BattlePlando : UserControl, PlandoPage
{
    BattleRandoState state;
    BattlePlandomizer plandomizer;

    public List<string> battleNames { get => state.btscs == null ? new() : state.btscs.Keys.ToList(); }
    public List<string> sceneNames { get => state.btscs == null ? new() : state.charasets.Keys.ToList(); }

    public event Action<object> OnComplete;

    public BattlePlando()
    {
        InitializeComponent();
        DataContext = this;
    }

    public void Setup(BattleRandoState state, BattlePlandomizer plandomizer)
    {
        this.state = state;
        this.plandomizer = plandomizer;
        Battles.ItemsSource = state.btscs.Keys;
        Scenes.ItemsSource = state.charasets.Keys;
        RegionSelector.ItemsSource = plandomizer.battleData.Values.Select(b => b.Location).Distinct().ToList();
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        var stringState = JsonConvert.SerializeObject(state);
        if (File.Exists("packs\\battle-plando.json"))
        {
            File.Delete("packs\\battle-plando.json");
        }
        File.WriteAllText("packs\\battle-plando.json", stringState);
    }

    private void Import_Click(object sender, RoutedEventArgs e)
    {
        this.state = JsonConvert.DeserializeObject<BattleRandoState>(File.ReadAllText("packs\\battle-plando.json"));
    }

    private void Complete_Click(object sender, RoutedEventArgs e)
    {
        OnComplete.Invoke(state);
    }

    private void Scenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SceneContents.ItemsSource = state.charasets[((sender as ListBox).SelectedItem as string)];
    }

    private void Battles_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        BattleContents.ItemsSource = state.btscs[((sender as ListBox).SelectedItem as string)];
    }
}

using Bartz24.Data;
using Bartz24.RandoWPF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
    BattleRandoState state_orig;
    BattlePlandomizer plandomizer;

    public string activeRegion;

    public List<DisplayableLabel> battleNames
    {
        get
        {
            return state.btscs == null ? new() : state.btscs.Keys.Where(k => activeRegion == null || activeRegion == "None" || plandomizer?.battleData[k].Location == activeRegion).Select(b => {
                var name = plandomizer?.battleData[b].Name;
                var displayName = b;
                if(name!=null && name.Length > 0)
                {
                    displayName = b + ": " + name;
                }
                return new DisplayableLabel() { Value = b, DisplayName = displayName };
                }).ToList();
        }
    }
    public List<DisplayableLabel> sceneNames
    {
        get
        {
            return state.btscs == null ? new() : state.charasets.Keys.Select(k => new DisplayableLabel() { DisplayName=k, Value=k }).ToList();
        }
    }

    public event Action<object> OnComplete;

    public List<BattleContentRow> battleContents;
    public List<BattleContentRow> newBattleContents;

    public BattlePlando()
    {
        InitializeComponent();
        DataContext = this;
    }

    public void Setup(BattleRandoState state, BattlePlandomizer plandomizer)
    {
        this.state = state;
        this.state_orig = state;
        this.plandomizer = plandomizer;
        Battles.ItemsSource = battleNames;
        Battles.DisplayMemberPath = "DisplayName";
        Scenes.ItemsSource = sceneNames;
        Scenes.DisplayMemberPath = "DisplayName";
        RegionSelector.ItemsSource = plandomizer.battleData.Values.Select(b => b.Location).Distinct().Append("None").ToList();
        battleContents = new List<BattleContentRow>(); // new ObservableCollection<BattleContentRow>();
        newBattleContents = new List<BattleContentRow>(); // new ObservableCollection<BattleContentRow>();
        BattleContents.ItemsSource = battleContents;
        NewBattleContents.ItemsSource = newBattleContents;
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
        //TODO: import breaks things? might be because of old json data though.
        this.state = JsonConvert.DeserializeObject<BattleRandoState>(File.ReadAllText("packs\\battle-plando.json"));
        UpdateData();
    }

    private void Complete_Click(object sender, RoutedEventArgs e)
    {
        OnComplete.Invoke(state);
    }

    private void Scenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selection = ((sender as ListBox).SelectedItem as DisplayableLabel?)?.Value;
        if (selection == null)
        {
            return;
        }
        try
        {
            var baseSceneContents = state.charasets[selection];
            SceneContents.ItemsSource = baseSceneContents.SelectMany(scene => state.btToCharaSpec.Where(s => s.Value == scene).Select(kvp => kvp.Key)).Select(e => plandomizer.enemyData[e].Name);
        } catch
        {
            SceneContents.ItemsSource = new List<DisplayableLabel>();
        }
    }

    private void Battles_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //TODO: battle contents binding still isn't working? Need to play around with column definitions probably. Also check the structs have correct data.
        var selection = ((sender as ListBox).SelectedItem as DisplayableLabel?)?.Value;
        if (selection == null)
        {
            return;
        }
        try
        {
            var baseData = state.btscs[selection];
            var baseData_orig = state_orig.btscs[selection];
            // battleContents.Clear();
            BattleContents.ItemsSource = baseData_orig.GroupBy(s => s).Select(g => new BattleContentRow() { Enemy = g.Key, Count = g.Count() }); //.ForEach(battleContents.Add);
            // newBattleContents.Clear();
            NewBattleContents.ItemsSource = baseData.Select(b => b.Split("/")[0]).GroupBy(s => s).Select(g => new BattleContentRow() { Enemy = g.Key, Count = g.Count() }); //.ForEach(newBattleContents.Add);
            Scenes.ItemsSource = plandomizer.battleData[selection].Charasets.Select(c => new DisplayableLabel() { Value = c, DisplayName = c });
        } catch
        {
            BattleContents.ItemsSource = new List<BattleContentRow>();
            NewBattleContents.ItemsSource = new List<BattleContentRow>();
        }
    }

    public struct BattleContentRow
    {
        public string Enemy { get; set; }
        public int Count { get; set; }
    }

    public struct DisplayableLabel
    {
        public string DisplayName { get; set; }
        public string Value { get; set; }
    }

    private void RegionSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        activeRegion = (sender as ComboBox).SelectedItem as string;
        UpdateData();
    }

    private void UpdateData()
    {
        Battles.UnselectAll();
        Scenes.UnselectAll();
        Battles.ItemsSource = battleNames;
        Scenes.ItemsSource = sceneNames;
        BattleContents.ItemsSource = new List<BattleContentRow>();
        NewBattleContents.ItemsSource = new List<BattleContentRow>();
    }
}

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
            return state.btscs == null ? new() : state.btscs.Keys.Where(k => activeRegion == null || activeRegion == "None" || plandomizer?.battleData[k].Location == activeRegion).Select(b =>
            {
                var name = plandomizer?.battleData[b].Name;
                var displayName = b;
                if (name != null && name.Length > 0)
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
            return state.btscs == null ? new() : state.charasets.Keys.Select(k => new DisplayableLabel() { DisplayName = k, Value = k }).ToList();
        }
    }

    public event Action<object> OnComplete;

    public List<BattleContentRow> battleContents;
    public List<BattleContentRow> newBattleContents;

    public (string, List<string>)? activeSceneIntersection;

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
        var imported = JsonConvert.DeserializeObject<BattleRandoState>(File.ReadAllText("packs\\battle-plando.json"));
        state.btscs = imported.btscs;
        state.charasets = imported.charasets;
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
            var sceneContentsKeys = UpdateSceneContents(selection);
            EnemySelector.ItemsSource = plandomizer.battleData.Keys
                .Where(k => !sceneContentsKeys.Contains(k))
                .Select(e => new DisplayableLabel() { DisplayName = plandomizer.enemyData[e].Name, Value = e });
            AddCharaspecEntryButton.IsEnabled = true;
        }
        catch
        {
            SceneContents.ItemsSource = new List<DisplayableLabel>();
        }
    }

    private IEnumerable<string> UpdateSceneContents(string selection)
    {
        var baseSceneContents = state.charasets[selection];
        var sceneContentsKeys = baseSceneContents.SelectMany(scene => state.btToCharaSpec.Where(s => s.Value == scene)
            .Select(kvp => kvp.Key));
        SceneContents.ItemsSource = sceneContentsKeys.Select(e => plandomizer.enemyData[e].Name)
            .Select(s =>
            {
                if (activeSceneIntersection != null)
                {
                    return activeSceneIntersection.Value.Item2.Contains(s)
                        ? new DisplayableLabel() { Value = s, DisplayName = s }
                        : new DisplayableLabel() { Value = null, DisplayName = $"({s})" };
                }
                return new DisplayableLabel() { Value = null, DisplayName = s };
            });
        return sceneContentsKeys;
    }

    private void Battles_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selection = ((sender as ListBox).SelectedItem as DisplayableLabel?)?.Value;
        if (selection == null)
        {
            return;
        }
        try
        {
            var baseData = state.btscs[selection];
            var baseData_orig = state_orig.btscs[selection];
            var origTotals = new BattleContentRow() { Id = null, Enemy = "Total", Count = baseData_orig.Count };
            var newTotals = new BattleContentRow() { Id = null, Enemy = "Total", Count = baseData.Count };
            BattleContents.ItemsSource = baseData_orig.Select(b => b.Split("/")[0])
                .GroupBy(s => s)
                .Select(g => new BattleContentRow() { Id = g.Key, Enemy = plandomizer.enemyData[g.Key].Name, Count = g.Count() })
                .Append(origTotals);
            NewBattleContents.ItemsSource = baseData.Select(b => b.Split("/")[0])
                .GroupBy(s => s)
                .Select(g => new BattleContentRow() { Id = g.Key, Enemy = plandomizer.enemyData[g.Key].Name, Count = g.Count() })
                .Append(newTotals);
            Scenes.ItemsSource = plandomizer.battleData[selection].Charasets
                .Select(c => new DisplayableLabel() { Value = c, DisplayName = c + $" ({plandomizer.charasetData[c].Limit})" });
            activeSceneIntersection = (selection, plandomizer.battleData[selection].Charasets
                .Select(cs => state.btToCharaSpec.Where(s => s.Value == cs)
                    .Select(kvp => kvp.Key).ToList())
                .Aggregate(state.btToCharaSpec.Keys, (IEnumerable<string> a, IEnumerable<string> b) => a.Intersect(b)).ToList());
        }
        catch
        {
            BattleContents.ItemsSource = new List<BattleContentRow>();
            NewBattleContents.ItemsSource = new List<BattleContentRow>();
        }
    }

    public struct BattleContentRow
    {
        public string Id { get; set; }
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
        AddButton.IsEnabled = false;
        RemoveButton.IsEnabled = false;
        AddCharaspecEntryButton.IsEnabled = false;
        RemoveCharaspecEntryButton.IsEnabled = false;
    }

    private void SceneContents_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selection = ((sender as ListBox).SelectedItem as DisplayableLabel?);
        if(selection == null || selection.Value.Value == null)
        {
            return;
        }
        AddButton.IsEnabled = true;
        //Only enable removal if the entry is not in the original charaset (TODO: can be removed once free edit is stable)
        RemoveCharaspecEntryButton.IsEnabled = !state_orig.charasets[GetActiveScene()].Contains(selection.Value.Value);
    }

    private void NewBattleContents_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selection = ((sender as DataGrid).SelectedItem as BattleContentRow?);
        if (selection == null || selection.Value.Id == null)
        {
            return;
        }
        RemoveButton.IsEnabled = true;
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        var selection = NewBattleContents.SelectedItem as BattleContentRow?;
        if(selection == null || selection.Value.Id == null)
        {
            return;
        }
        //Remove selected enemy from battle (decrease count until 1 then remove entirely)
        //Update state
        var baseData = state.btscs[activeSceneIntersection.Value.Item1];
        baseData.Remove(selection.Value.Id);
        //Update UI display
        var newTotals = new BattleContentRow() { Id = null, Enemy = "Total", Count = baseData.Count };
        NewBattleContents.ItemsSource = baseData.Select(b => b.Split("/")[0])
            .GroupBy(s => s)
            .Select(g => new BattleContentRow() { Id = g.Key, Enemy = plandomizer.enemyData[g.Key].Name, Count = g.Count() })
            .Append(newTotals);
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var selection = SceneContents.SelectedItem as DisplayableLabel?;
        if (selection == null || selection.Value.Value == null)
        {
            return;
        }
        //Add selected enemy from charaset into battle
        var baseData = state.btscs[activeSceneIntersection.Value.Item1];
        baseData.Add(selection.Value.Value);
        //Update UI display
        var newTotals = new BattleContentRow() { Id = null, Enemy = "Total", Count = baseData.Count };
        NewBattleContents.ItemsSource = baseData.Select(b => b.Split("/")[0])
            .GroupBy(s => s)
            .Select(g => new BattleContentRow() { Id = g.Key, Enemy = plandomizer.enemyData[g.Key].Name, Count = g.Count() })
            .Append(newTotals);
    }

    private string GetActiveScene()
    {
        var selection = Scenes.SelectedItem as DisplayableLabel?;
        return selection?.Value;
    }

    private void Validate_Click(object sender, RoutedEventArgs e)
    {
        //TODO: put in validation to check battle contents etc.
        //Check sizes of all battles
        //Check content sizes of charasets
        //Check intersection battles are correct
    }

    private void AddCharaspecEntryButton_Click(object sender, RoutedEventArgs e)
    {
        var activeEnemy = EnemySelector.SelectedItem as DisplayableLabel?;
        if(activeEnemy == null || activeEnemy.Value.Value == null)
        {
            return;
        }
        state.charasets[GetActiveScene()].Add(activeEnemy.Value.Value);
        UpdateSceneContents(GetActiveScene());
    }

    private void RemoveCharaspecEntryButton_Click(object sender, RoutedEventArgs e)
    {
        var selection = SceneContents.SelectedItem as DisplayableLabel?;
        if (selection == null || selection.Value.Value == null)
        {
            return;
        }
        state.charasets[GetActiveScene()].Remove(selection?.Value);
        UpdateSceneContents(GetActiveScene());
    }
}

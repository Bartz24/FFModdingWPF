using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FF13Rando;

public struct BattleRandoState
{
    public Dictionary<string, List<string>> charasets;
    public Dictionary<string, List<string>> btscs;
    public Dictionary<string, string> btToCharaSpec;
}

public class BattlePlandomizer : BattleRando, IPlandomizer
{
    private readonly DataStoreWDB<DataStoreCharaSet> charaSets = new();
    private Dictionary<string, List<string>> charaSetsOrig = new();

    public BattleRandoState? plandoState;

    public BattlePlandomizer(RandomizerManager randomizers) : base(randomizers) { }

    public void SetState(object state)
    {
        plandoState = (BattleRandoState)state;
    }

    protected override void RandomizeBattles()
    {
        if(plandoState == null)
        {
            throw new Exception();
        }
        //Take state from struct and update scenes etc.
        if(plandoState is BattleRandoState plandoOutput)
        {
            //TODO: LYB replacement support as well as just charaset shuffling
            foreach(var charaset in plandoOutput.charasets)
            {
                charaSets[charaset.Key].SetCharaSpecs(charaset.Value);
            }
            foreach(var battleScene in plandoOutput.btscs)
            {
                var scene = btscs[battleScene.Key];
                scene.Values.Where(e => enemyData.ContainsKey(e.sEntryBtChSpec_string)).ForEach(e => scene.Values.Remove(e));
                //Not currently sure how to reconstruct the BtSc entry.
                // Current rando implementation just shuffles ids around based on index which I guess I can keep doing?
                //But we are able to go to more distinct enemies from a lesser set so not sure...
                //Just index match it and ignore anything out of range maybe?
                //scene.Values.AddRange(battleScene.Value.Distinct().Select(e => {
                //    var split = e.Split("/");
                //    var store = new DataStoreBtSc();
                //    store.sEntryBtChSpec_string = split[0];
                //    store.sEntryBtChSpec_pointer = uint.Parse(split[1]);
                //    return store;
                //}));
                //Index match it for now. Has the limitation that enemies in MUST match enemies out for now.
                for(var i = 0; i < scene.Values.Count(); i++)
                {
                    if (scene.Values[i].sEntryBtChSpec_string.StartsWith("pc"))
                    {
                        continue;
                    }
                    scene.Values[i].sEntryBtChSpec_string = battleScene.Value[i];
                }
            }
        }
    }

    public UserControl GetPlandoPage()
    {
        if (!FF13Flags.Enemies.EnemiesFlag.FlagEnabled)
        {
            return null;
        }
        EnemyRando enemyRando = Randomizers.Get<EnemyRando>();
        //Setup reverse mapping for all randomisable enemies
        var btToCharaSpec = enemyRando.btCharaSpec.EntryList.Where(kvp=>enemyData.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.sCharaSpec_string);
        plandoState = new BattleRandoState
        {
            btscs = btscsOrigString,
            charasets = charaSetsOrig,
            btToCharaSpec = btToCharaSpec
        };
        var battlePlandoPage = new BattlePlando();
        battlePlandoPage.Setup((BattleRandoState)plandoState, this);
        return battlePlandoPage;
    }
}

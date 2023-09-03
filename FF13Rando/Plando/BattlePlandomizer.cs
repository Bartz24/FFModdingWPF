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
}

//TODO: should this be a whole separate class or should plando behaviour be put into randos as an option directly. Separate for now to make testing faster/easier
public class BattlePlandomizer : Plandomizer
{
    public DataStoreWDB<DataStoreBtScene> btscene = new();
    public DataStoreWDB<DataStoreBtScene> btsceneOrig = new();
    private readonly DataStoreWDB<DataStoreCharaSet> charaSets = new();
    private Dictionary<string, List<string>> charaSetsOrig = new();

    public DataStoreWDB<DataStoreBtConstant> battleConsts = new();

    public ConcurrentDictionary<string, DataStoreWDB<DataStoreBtSc>> btscs = new();
    public Dictionary<string, List<string>> btscsOrig = new();

    public Dictionary<string, BattleData> battleData = new();
    public Dictionary<string, EnemyData> enemyData = new();
    public Dictionary<string, CharasetData> charasetData = new();

    public BattleRandoState? plandoState;

    public BattlePlandomizer(RandomizerManager randomizers) : base(randomizers) { }

    public override void SetState(object state)
    {
        plandoState = (BattleRandoState)state;
    }

    public override void Load()
    {
        Randomizers.SetUIProgress("Loading Battle Data...", 0, 100);
        btscene.LoadWDB("13", @"\db\resident\bt_scene.wdb");
        btsceneOrig.LoadWDB("13", @"\db\resident\bt_scene.wdb");

        charaSets.LoadWDB("13", @"\db\resident\charaset.wdb");

        battleConsts.LoadWDB("13", @"\db\resident\bt_constants.wdb");

        string btscWDBPath = Nova.GetNovaFile("13", @"btscene\wdb\btsc_wdb.bin", SetupData.Paths["Nova"], SetupData.Paths["13"]);
        string btscWDBOutPath = SetupData.OutputFolder + @"\btscene\wdb\btsc_wdb.bin";
        FileHelpers.CopyFile(btscWDBPath, btscWDBOutPath);
        Nova.UnpackWPD(btscWDBOutPath, SetupData.Paths["Nova"]);

        Randomizers.SetUIProgress("Loading Battle Data...", 10, 100);

        FileHelpers.ReadCSVFile(@"data\battlescenes.csv", row =>
        {
            BattleData b = new(row);
            battleData.Add(b.ID, b);
        }, FileHelpers.CSVFileHeader.HasHeader);

        Randomizers.SetUIProgress("Loading Battle Data...", 20, 100);

        if (FF13Flags.Other.Enemies.FlagEnabled)
        {
            IEnumerable<string> files = Directory.GetFiles(SetupData.OutputFolder + @"\btscene\wdb\_btsc_wdb.bin").Where(path => battleData.ContainsKey(Path.GetFileNameWithoutExtension(path)));
            int maxCount = files.Count();
            int count = 0;
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 8 }, path =>
            {
                count++;
                Randomizers.SetUIProgress($"Loading Encounter... ({count} of {maxCount})", count, maxCount);
                DataStoreWDB<DataStoreBtSc> btsc = new();
                btsc.Load("13", path, SetupData.Paths["Nova"]);
                btscs.TryAdd(Path.GetFileNameWithoutExtension(path), btsc);
            });
        }

        Randomizers.SetUIProgress("Loading Battle Data...", 90, 100);
        FileHelpers.ReadCSVFile(@"data\enemies.csv", row =>
        {
            EnemyData e = new(row);
            enemyData.Add(e.ID, e);
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\charasets.csv", row =>
        {
            CharasetData c = new(row);
            charasetData.Add(c.ID, c);
        }, FileHelpers.CSVFileHeader.HasHeader);

        charaSets.Values.Where(c => c.ID.Contains("z030")).ForEach(c =>
        {
            List<string> list = c.GetCharaSpecs();
            list.Remove("m193");
            list.Remove("m106");
            list.Remove("m110");
            c.SetCharaSpecs(list);
        });
        List<string> charaSetOrigKeys = charaSets.Keys;
        charaSetsOrig = charaSetOrigKeys.ToDictionary(k => k, k => charaSets[k].GetCharaSpecs());
        btscsOrig = btscs.ToDictionary(k => k.Key, k => k.Value.Values.Where(e => !e.sEntryBtChSpec_string.StartsWith("pc")).Select(e => e.sEntryBtChSpec_string+"/"+e.sEntryBtChSpec_pointer).ToList());
    }

    private List<string> resolvePossibleCandidates(string oldEnemy, IEnumerable<string> basePool)
    {
        EnemyData lookup = enemyData[oldEnemy];
        int rangeMin = lookup.Rank - FF13Flags.Other.EnemyRank.Value;
        int rangeMax = lookup.Rank + FF13Flags.Other.EnemyRank.Value;
        return basePool.Where(next =>
        {
            if (enemyData[next].Traits.Contains("Ignore"))
            {
                return false; //Ignore summoned weapons and Syphax
            }

            return lookup.Traits.Contains("Event")
|| (lookup.Traits.Contains("Flying")
                ? enemyData[next].Traits.Contains("Flying")
                : lookup.Traits.Contains("Turtle")
                ? enemyData[next].Traits.Contains("Turtle")
                : !enemyData[next].Traits.Contains("Flying") && !enemyData[next].Traits.Contains("Turtle"));
        }).Where(next =>
        {
            return enemyData[next].Rank >= rangeMin && enemyData[next].Rank <= rangeMax;
        }).ToList();
    }

    public override void Randomize()
    {
        Randomizers.SetUIProgress("Randomizing Battle Data...", -1, 100);
        if(plandoState == null)
        {
            throw new Exception();
        }
        EnemyRando enemyRando = Randomizers.Get<EnemyRando>();
        //Take state from struct and update scenes etc.
        if(plandoState is BattleRandoState plandoOutput)
        {
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

        if (FF13Flags.Stats.RandTPBorders.FlagEnabled)
        {
            FF13Flags.Stats.RandTPBorders.SetRand();
            int max = 21;
            if (FF13Flags.Stats.RandTPMax.Enabled)
            {
                max = RandomNum.RandInt(10, 40);
            }

            int[] newValues = new int[5];

            string type = FF13Flags.Stats.TPBorderType.SelectedValue == "Random Type" ? FF13Flags.Stats.TPBorderType.Values.Take(FF13Flags.Stats.TPBorderType.Values.Count - 1).Shuffle().First() : FF13Flags.Stats.TPBorderType.SelectedValue;

            if (type == "Equal")
            {
                for (int i = 0; i < 5; i++)
                {
                    newValues[i] = max * 100 / 5 * (i + 1);
                }
            }
            else
            {
                List<int> vals = Enumerable.Range(1, max - 1).Shuffle().Take(5).OrderBy(i => i).ToList();
                vals[vals.Count - 1] = max;

                List<int> sizes = Enumerable.Range(0, 5).Select(i => i == 0 ? vals[i] : vals[i] - vals[i - 1]).ToList();
                switch (type)
                {
                    case "Increasing":
                        sizes = sizes.OrderBy(i => i).ToList();
                        break;
                    case "Decreasing":
                        sizes = sizes.OrderByDescending(i => i).ToList();
                        break;
                    case "Random Borders":
                        sizes = sizes.Shuffle();
                        break;
                }

                int total = 0;
                for (int i = 0; i < 5; i++)
                {
                    newValues[i] = (sizes[i] + total) * 100;
                    total += sizes[i];
                }
            }

            for (int i = 0; i < 5; i++)
            {
                battleConsts[$"iTpBorder{i + 1}"].u16UintValue = (uint)newValues[i];
            }

            RandomNum.ClearRand();
        }
    }

    private int GetMaxCountAllowed()
    {
        return FF13Flags.Other.EnemyVariety.SelectedIndex switch
        {
            0 => 0,
            1 => 16,
            2 => 30,
            3 => 44,
            _ => 16,
        };
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        HTMLPage page = new("Encounters", "template/documentation.html");
        //TODO: fix this to account for slightly different battle data view
        page.HTMLElements.Add(new Table("Encounters", (new string[] { "ID", "Region / Name (If known)", "New Enemies", "Old Enemies" }).ToList(), (new int[] { 5, 15, 40, 40 }).ToList(), btscs.Keys.OrderBy(b => b).Select(b =>
        {
            List<string> names = btscs[b].Values.Where(e => !e.sEntryBtChSpec_string.StartsWith("pc")).Select(e => enemyData.ContainsKey(e.sEntryBtChSpec_string) ? enemyData[e.sEntryBtChSpec_string].Name : e.sEntryBtChSpec_string + " (???)").GroupBy(e => e).Select(g => $"{g.Key} x {g.Count()}").ToList();
            List<string> oldNames = btscsOrig[b];
            string region = battleData[b].Location + " - " + battleData[b].Name;
            return new string[] { b, region, string.Join(", ", names), string.Join(", ", oldNames) }.ToList();
        }).ToList()));

        page.HTMLElements.Add(new Table("Charasets", (new string[] { "ID", "Original contents", "New contents" }).ToList(), (new int[] { 10, 50, 30 }).ToList(), charasetData.Keys.OrderBy(b => b).Select(b =>
        {
            List<string> origContents = charaSetsOrig[b].Where(spec => enemyData.ContainsKey(spec)).Select(spec => enemyData[spec].Name).ToList();
            List<string> newContents = charaSets[b].GetCharaSpecs().Where(spec => enemyData.ContainsKey(spec)).Select(spec => enemyData[spec].Name).ToList();
            return new string[] { b, string.Join(", ", origContents), string.Join(", ", newContents) }.ToList();
        }).ToList()));
        pages.Add("encounters", page);
        return pages;
    }

    public override void Save()
    {
        Randomizers.SetUIProgress("Saving Battle Data...", 0, 100);
        btscene.SaveWDB(@"\db\resident\bt_scene.wdb");

        charaSets.SaveWDB(@"\db\resident\charaset.wdb");

        battleConsts.SaveWDB(@"\db\resident\bt_constants.wdb");

        Randomizers.SetUIProgress("Saving Battle Data...", 10, 100);

        if (FF13Flags.Other.Enemies.FlagEnabled)
        {
            IEnumerable<string> files = Directory.GetFiles(SetupData.OutputFolder + @"\btscene\wdb\_btsc_wdb.bin").Where(path => battleData.ContainsKey(Path.GetFileNameWithoutExtension(path)));
            int maxCount = files.Count();
            int count = 0;
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 8 }, path =>
            {
                count++;
                Randomizers.SetUIProgress($"Saving Encounter... ({count} of {maxCount})", count, maxCount);
                btscs[Path.GetFileNameWithoutExtension(path)].Save(path, SetupData.Paths["Nova"]);
            });
        }

        Randomizers.SetUIProgress("Saving Battle Data...", 90, 100);
        Nova.RepackWPD(SetupData.OutputFolder + @"\btscene\wdb\btsc_wdb.bin", SetupData.Paths["Nova"]);
    }

    public override UserControl GetPlandoPage()
    {
        plandoState = new BattleRandoState
        {
            btscs = btscsOrig,
            charasets = charaSetsOrig
        };
        var battlePlandoPage = new BattlePlando();
        battlePlandoPage.Setup((BattleRandoState)plandoState, this);
        return battlePlandoPage;
    }

    public class EnemyData : CSVDataRow
    {
        [RowIndex(0)]
        public string ID { get; set; }
        [RowIndex(1)]
        public string Name { get; set; }
        [RowIndex(2)]
        public List<string> Traits { get; set; }
        [RowIndex(3)]
        public int Rank { get; set; }
        public EnemyData(string[] row) : base(row)
        {
        }
    }

    public class BattleData : CSVDataRow
    {
        [RowIndex(0)]
        public string ID { get; set; }
        [RowIndex(1)]
        public string Name { get; set; }
        [RowIndex(2)]
        public string Location { get; set; }
        [RowIndex(3)]
        public List<string> Charasets { get; set; }
        [RowIndex(4)]
        public List<string> Traits { get; set; }
        public BattleData(string[] row) : base(row)
        {
        }
    }

    public class CharasetData : CSVDataRow
    {
        [RowIndex(0)]
        public string ID { get; set; }
        [RowIndex(1)]
        public int Limit { get; set; }
        public CharasetData(string[] row) : base(row)
        {
        }
    }
}

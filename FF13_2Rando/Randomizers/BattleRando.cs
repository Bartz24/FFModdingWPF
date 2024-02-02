using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FF13_2Rando;

public partial class BattleRando : Randomizer
{
    public DataStoreDB3<DataStoreBtScene> btScenes = new();
    private readonly DataStoreDB3<DataStoreRCharaSet> charaSets = new();

    public Dictionary<string, DataStoreDB3<DataStoreBtSTable>> btTables = new();
    private Dictionary<string, EnemyData> enemyData = new();
    private readonly Dictionary<string, Dictionary<string, BossData>> bossData = new();
    public Dictionary<string, BattleData> battleData = new();
    private Dictionary<string, string> shuffledBosses = new();
    public Dictionary<string, (int, int)> areaBounds = new();
    private Dictionary<string, (int, int)> areaBoundsOrig = new();

    public BattleRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Battle Data...");
        btScenes.LoadDB3(Generator, "13-2", @"\db\resident\bt_scene.wdb");
        enemyData = File.ReadAllLines(@"data\enemies.csv").Select(s => new EnemyData(s.Split(","))).ToDictionary(e => e.ID, e => e);

        charaSets.LoadDB3(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_charaset.wdb", false);

        bossData.Clear();
        using (CsvParser csv = new(new StreamReader(@"data\bosses.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
        {
            while (csv.Read())
            {
                if (csv.Row > 1)
                {
                    BossData b = new(csv.Record);
                    if (!bossData.ContainsKey(b.Group))
                    {
                        bossData.Add(b.Group, new Dictionary<string, BossData>());
                    }

                    bossData[b.Group].Add(b.ID, b);
                }
            }
        }

        FileHelpers.ReadCSVFile(@"data\battlescenes.csv", row =>
        {
            BattleData b = new(row);
            battleData.Add(b.ID, b);
        }, FileHelpers.CSVFileHeader.HasHeader);

        HistoriaCruxRando historiaCruxRando = Generator.Get<HistoriaCruxRando>();
        historiaCruxRando.areaData.Values.Where(a => !string.IsNullOrEmpty(a.BattleTableID)).ForEach(a =>
        {
            DataStoreDB3<DataStoreBtSTable> table = new();
            table.LoadDB3(Generator, "13-2", @"\db\btscenetable\" + a.BattleTableID + ".wdb");
            btTables.Add(a.BattleTableID, table);
        });
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Battle Data...");
        EnemyRando enemyRando = Generator.Get<EnemyRando>();
        if (FF13_2Flags.Enemies.EnemyLocations.FlagEnabled)
        {
            FF13_2Flags.Enemies.EnemyLocations.SetRand();

            areaBounds = GetAreaRankBounds();
            areaBoundsOrig = new Dictionary<string, (int, int)>(areaBounds);

            TreasureRando treasureRando = Generator.Get<TreasureRando>();
            List<string> areaUnlockOrder = new();//treasureRando.PlacementAlgo.Logic.GetPropValue<List<string>>("AreaUnlockOrder");
            areaUnlockOrder = areaUnlockOrder.Where(a => areaBounds.ContainsKey(a)).ToList();
            areaUnlockOrder.AddRange(areaBounds.Keys.Where(a => !areaUnlockOrder.Contains(a)));

            areaUnlockOrder = RandomNum.ShuffleWeightedOrder(areaUnlockOrder, (i1, a1, i2, a2) =>
            {
                return i1 == i2 ? 1 : (i1 < 5 || i2 < 5 ? 0 : Math.Abs(i1 - i2) < 3 ? 1 : 0);
            });
            List<int> newMins = areaBounds.Values.Select(t => t.Item1).OrderBy(i => i).ToList();
            int enemyMaxRank = enemyData.Values.Where(e => !e.Traits.Contains("Boss")).Max(e => e.Rank);
            for (int i = 0; i < areaBounds.Count; i++)
            {
                string area = areaUnlockOrder[i];
                int newMax = newMins[i] + areaBoundsOrig[area].Item2 - areaBoundsOrig[area].Item1;
                areaBounds[area] = (newMins[i], Math.Min(newMax, enemyMaxRank));
            }

            if (FF13_2Flags.Enemies.Bosses.SelectedValues.Count > 0)
            {
                List<string> list = bossData.Keys
                    .Where(g => FF13_2Flags.Enemies.Bosses.SelectedValues.Contains(g))
                    .Where(g => !bossData[g].Values.First(b => b.Traits.Contains("Main")).Traits.Contains("NoShuffle"))
                    .ToList();
                List<string> shuffled = list.Shuffle().ToList();
                shuffledBosses = Enumerable.Range(0, list.Count).ToDictionary(i => list[i], i => shuffled[i]);
            }

            btScenes.Values.Shuffle().ForEach(b =>
            {
                List<EnemyData> oldEnemies = b.GetCharSpecs().Where(s => enemyData.ContainsKey(s)).Select(s => enemyData[s]).ToList();
                int count = oldEnemies.Count;
                if (!FF13_2Flags.Enemies.LargeEnc.Enabled)
                {
                    count = Math.Min(4, count);
                }

                if (count > oldEnemies.Count)
                {
                    for (int i = oldEnemies.Count; i < count; i++)
                    {
                        oldEnemies.Add(oldEnemies[RandomNum.RandInt(0, oldEnemies.Count - 1)]);
                    }
                }

                if (count < oldEnemies.Count)
                {
                    for (int i = oldEnemies.Count; i > count; i--)
                    {
                        oldEnemies.RemoveAt(RandomNum.RandInt(0, oldEnemies.Count - 1));
                    }
                }

                if (count > 0)
                {
                    if (!oldEnemies[0].Traits.Contains("Boss") || FF13_2Flags.Enemies.Bosses.SelectedValues.Count > 0)
                    {
                        List<EnemyData> validEnemies = enemyData.Values.Where(e => !e.Traits.Contains("Boss")).ToList();
                        if (battleData.ContainsKey(b.name))
                        {
                            validEnemies = validEnemies.Where(e => e.Parts.Count == 0 || oldEnemies.Contains(e)).ToList();
                        }

                        UpdateEnemyLists(oldEnemies, validEnemies, b.name, b.name.StartsWith("btsc011"));
                    }
                }
            });
            RandomNum.ClearRand();
        }
    }

    private Dictionary<string, (int, int)> GetAreaRankBounds()
    {
        Dictionary<string, HashSet<int>> areaRanks = new();
        btScenes.Values.ForEach(b =>
        {
            int rank = GetBattleRank(b);
            if (rank > 0)
            {
                List<string> areas = GetAreasWithBattle(b.name);
                areas.ForEach(a =>
                {
                    if (!areaRanks.ContainsKey(a))
                    {
                        areaRanks.Add(a, new HashSet<int>());
                    }

                    areaRanks[a].Add(rank);
                });
            }
        });
        return areaRanks.ToDictionary(p => p.Key, p => (p.Value.Min(), p.Value.Max()));
    }

    private int GetBattleRank(DataStoreBtScene b)
    {
        List<EnemyData> enemies = b.GetCharSpecs().Where(s => enemyData.ContainsKey(s)).Select(s => enemyData[s]).ToList();
        return enemies.Count > 0 ? enemies.Select(e => e.Rank).Max() : 0;
    }

    private List<string> GetAreasWithBattle(string btsceneName)
    {
        // Dummy garuda battle
        if (btsceneName == "btsc99000")
        {
            return new List<string>();
        }

        HistoriaCruxRando historiaCruxRando = Generator.Get<HistoriaCruxRando>();
        List<string> list = btTables.Keys
            .Where(id => btTables[id].Values
                .SelectMany(bt => bt.GetBattleIDs()).Distinct()
                .Where(i => btsceneName == "btsc" + i.ToString("D5")).Any())
            .Select(id => historiaCruxRando.areaData.Values.First(a => a.BattleTableID == id).ID)
            .ToList();

        if (battleData.ContainsKey(btsceneName))
        {
            foreach (string id in battleData[btsceneName].LocationIDs)
            {
                if (!list.Contains(id))
                {
                    list.Add(id);
                }
            }
        }

        return list;
    }

    private void UpdateBossStats(BossData newBoss, BossData origBoss)
    {
        EnemyRando enemyRando = Generator.Get<EnemyRando>();
        DataStoreBtCharaSpec newEnemy = enemyRando.GetEnemy(newBoss.ID);
        DataStoreBtCharaSpec origEnemy = enemyRando.GetEnemy(origBoss.ID, true);

        if (newBoss.HPMult != -1)
        {
            newEnemy.u24MaxHp = (int)(origEnemy.u24MaxHp / origBoss.HPMult * newBoss.HPMult);
        }

        if (newBoss.STRMult != -1)
        {
            newEnemy.u16StatusStr = (int)(origEnemy.u16StatusStr / origBoss.STRMult * newBoss.STRMult);
        }

        if (newBoss.MAGMult != -1)
        {
            newEnemy.u16StatusMgk = (int)(origEnemy.u16StatusMgk / origBoss.MAGMult * newBoss.MAGMult);
        }

        if (newBoss.HPMult != -1)
        {
            newEnemy.u24MaxHp = (int)(origEnemy.u24MaxHp / origBoss.HPMult * newBoss.HPMult);
        }

        if (newBoss.StaggerPointMult != -1 && newEnemy.u12BrChainBonus != 1000 && origEnemy.u12BrChainBonus != 1000)
        {
            newEnemy.u12BrChainBonus = Math.Min(999, (int)((origEnemy.u12BrChainBonus - 100) / origBoss.StaggerPointMult * newBoss.StaggerPointMult) + 100);
        }

        if (newBoss.ChainResMult != -1)
        {
            newEnemy.u12MaxBp = Math.Min(100, (int)(origEnemy.u12MaxBp / origBoss.ChainResMult * newBoss.ChainResMult));
        }

        if (newBoss.CPGilMult != -1)
        {
            newEnemy.u24AbilityPoint = (int)(origEnemy.u24AbilityPoint / origBoss.CPGilMult * newBoss.CPGilMult);
            newEnemy.u16DropGil = (int)(origEnemy.u16DropGil / origBoss.CPGilMult * newBoss.CPGilMult);
        }

        newEnemy.u12KeepVal = origEnemy.u12KeepVal;
        newEnemy.s10DropItem0_string = origEnemy.s10DropItem0_string;
        newEnemy.s10DropItem1_string = origEnemy.s10DropItem1_string;
        newEnemy.s10DropItem2_string = origEnemy.s10DropItem2_string;
        newEnemy.u8NumDrop0 = origEnemy.u8NumDrop0;
        newEnemy.u8NumDrop1 = origEnemy.u8NumDrop1;
        newEnemy.u8NumDrop2 = origEnemy.u8NumDrop2;
        newEnemy.u14DropProb0 = origEnemy.u14DropProb0;
        newEnemy.u14DropProb1 = origEnemy.u14DropProb1;
        newEnemy.u14DropProb2 = origEnemy.u14DropProb2;
    }

    private void UpdateEnemyLists(List<EnemyData> oldEnemies, List<EnemyData> allowed, string btsceneName, bool sameRank)
    {
        EnemyRando enemyRando = Generator.Get<EnemyRando>();
        List<EnemyData> newEnemies = new();
        if (oldEnemies[0].Traits.Contains("Boss"))
        {
            bool noEntry = true;
            foreach (EnemyData e in oldEnemies)
            {
                if (!bossData.Values.SelectMany(d => d.Values).Where(b => b.ID == e.ID).Any())
                {
                    continue;
                }

                BossData oldBoss = bossData.Values.SelectMany(d => d.Values).First(b => b.ID == e.ID && b.Traits.Contains("Main"));
                if (!shuffledBosses.ContainsKey(oldBoss.Group))
                {
                    return;
                }

                string newGroup = shuffledBosses[oldBoss.Group];
                if (oldBoss.Group == newGroup)
                {
                    return;
                }

                if (oldBoss.Group != newGroup)
                {
                    newEnemies.Add(enemyData[bossData[newGroup].Values.First(b => b.Traits.Contains("Main")).ID]);
                    bossData[newGroup].Values.ForEach(b => UpdateBossStats(b, oldBoss));
                    if (!bossData[newGroup].Values.First(b => b.Traits.Contains("Main")).Traits.Contains("NoEntry"))
                    {
                        noEntry = false;
                    }

                    if (bossData[oldBoss.Group].Values.First(b => b.Traits.Contains("Main")).Traits.Contains("ForceEntry"))
                    {
                        noEntry = false;
                    }
                }
            }

            if (newEnemies.Count > 0)
            {
                if (noEntry)
                {
                    btScenes[btsceneName].s10BtChEntryId_string = "";
                    btScenes[btsceneName].s10PartyEntryId_string = "";
                }
                else
                {
                    btScenes[btsceneName].s10BtChEntryId_string = "btsc_def_e00";
                    btScenes[btsceneName].s10PartyEntryId_string = "btsc_def_p00";
                }
            }
        }
        else
        {
            List<string> areas = GetAreasWithBattle(btsceneName);
            if (areas.Count == 0)
            {
                return;
            }

            int oldRankMin = areaBoundsOrig.Keys.Where(a => areas.Contains(a)).Select(a => areaBoundsOrig[a].Item1).Min();
            int oldRankMax = areaBoundsOrig.Keys.Where(a => areas.Contains(a)).Select(a => areaBoundsOrig[a].Item2).Max();
            int newRankMin = areaBounds.Keys.Where(a => areas.Contains(a)).Select(a => areaBounds[a].Item1).Min();
            int newRankMax = areaBounds.Keys.Where(a => areas.Contains(a)).Select(a => areaBounds[a].Item2).Max();
            newEnemies.Clear();

            oldEnemies.ForEach(oldEnemy =>
            {

                bool canAdd = false;
                int attempts = -1;
                EnemyData newEnemy = null;

                List<string> ignored = new();

                do
                {

                    attempts++;
                    canAdd = false;
                    int newRank = oldRankMax > oldRankMin
                        ? (int)Math.Round((oldEnemy.Rank - oldRankMin) * ((float)newRankMax - newRankMin) / ((float)oldRankMax - oldRankMin)) + newRankMin
                        : (int)Math.Round((oldEnemy.Rank - oldRankMin) * ((float)newRankMax - newRankMin)) + newRankMin;
                    int range = attempts + FF13_2Flags.Enemies.EnemyRank.Value;
                    if (sameRank)
                    {
                        range -= 2;
                    }

                    List<EnemyData> possible = allowed.Where(e => !ignored.Contains(e.ID)).Where(newE =>
                    {
                        return newE.Rank >= newRank - range && newE.Rank <= newRank + range;
                    }).ToList();

                    if (possible.Count == 0)
                    {
                        continue;
                    }

                    canAdd = true;
                    newEnemy = RandomNum.SelectRandomWeighted(possible, _ => 1);
                    if (battleData.ContainsKey(btsceneName))
                    {
                        if (oldEnemies.Contains(newEnemy))
                        {
                            break;
                        }

                        battleData[btsceneName].Charasets.ForEach(c =>
                        {
                            List<string> list = charaSets[c].CharaSpecs;

                            string spec = enemyRando.HasEnemy(newEnemy.ID) ? enemyRando.GetEnemy(newEnemy.ID).sCharaSpec_string : newEnemy.ID;
                            if (!list.Contains(spec))
                            {
                                list.Add(spec);
                            }

                            newEnemy.Parts.ForEach(id =>
                            {
                                string spec = enemyRando.HasEnemy(newEnemy.ID) ? enemyRando.GetEnemy(newEnemy.ID).sCharaSpec_string : newEnemy.ID;
                                if (!list.Contains(spec))
                                {
                                    list.Add(spec);
                                }
                            });

                            if (list.Count > battleData[btsceneName].CharasetLimit && list.Count > charaSets[c].CharaSpecs.Count)
                            {
                                canAdd = false;
                                ignored.Add(newEnemy.ID);
                                if (possible.Count == 0)
                                {
                                    newEnemy = oldEnemy;
                                }
                            }
                        });
                    }
                    else
                    {
                        List<EnemyData> enemies = new(newEnemies)
                        {
                            newEnemy
                        };
                        // Variety limit is 3 or the vanilla variety + 1
                        if (GetCharaSpecs(enemies).Distinct().Count() > Math.Min(3, GetCharaSpecs(oldEnemies).Distinct().Count() + 1))
                        {
                            canAdd = false;
                            ignored.Add(newEnemy.ID);
                            if (possible.Count == 0)
                            {
                                newEnemy = oldEnemy;
                            }
                        }
                    }
                } while (!canAdd);

                if (newEnemy == null)
                {
                    throw new Exception("Failed to add an enemy to " + btsceneName);
                }

                newEnemies.Add(newEnemy);
            });
        }

        if (newEnemies.Count == 0)
        {
            return;
        }

        List<string> charaSpecs = GetCharaSpecs(newEnemies);
        btScenes[btsceneName].SetCharSpecs(charaSpecs);

        if (battleData.ContainsKey(btsceneName))
        {
            charaSpecs.Select(spec => enemyRando.HasEnemy(spec) ? enemyRando.GetEnemy(spec).sCharaSpec_string : spec).ForEach(spec =>
            {
                battleData[btsceneName].Charasets.ForEach(c =>
                {
                    List<string> list = charaSets[c].CharaSpecs;

                    if (!list.Contains(spec))
                    {
                        list.Add(spec);
                    }

                    charaSets[c].CharaSpecs = list;
                });
            });
        }

        btScenes[btsceneName].u4BtChInitSetNum = charaSpecs.Count > newEnemies.Count ? newEnemies.Sum(e => e.Size) : 0;
    }

    private static List<string> GetCharaSpecs(List<EnemyData> newEnemies)
    {
        List<string> charaSpecs = newEnemies.Select(e => e.ID).ToList();
        charaSpecs.AddRange(newEnemies.SelectMany(e => e.Parts).Distinct().Where(s => !charaSpecs.Contains(s)));
        return charaSpecs;
    }

    public Dictionary<string, int> GetAreaDifficulties()
    {
        Dictionary<string, List<int>> diffs = new();
        btScenes.Keys.ForEach(id =>
        {
            List<string> areas = GetAreasWithBattle(id);
            if (areas.Count > 0)
            {
                List<EnemyData> oldEnemies = btScenes[id].GetCharSpecs().Where(s => enemyData.ContainsKey(s)).Select(s => enemyData[s]).ToList();

                if (oldEnemies.Count > 0)
                {
                    int diff = (int)(oldEnemies.Max(e => e.Rank) * Math.Pow(1.05, oldEnemies.Count));
                    areas.ForEach(a =>
                    {
                        if (!diffs.ContainsKey(a))
                        {
                            diffs.Add(a, new List<int>());
                        }

                        diffs[a].Add(diff);
                    });
                }
            }
        });

        return diffs.ToDictionary(p => p.Key, p => (int)Math.Ceiling(p.Value.Average()));
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        HistoriaCruxRando historiaCruxRando = Generator.Get<HistoriaCruxRando>();
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        HTMLPage page = new("Encounters", "template/documentation.html");

        page.HTMLElements.Add(new Table("Bosses", (new string[] { "Original Boss", "New Boss", }).ToList(), (new int[] { 50, 50 }).ToList(), shuffledBosses.Select(p =>
        {
            return new string[] { p.Key, p.Value }.ToList();
        }).ToList()));

        page.HTMLElements.Add(new Table("Encounters", (new string[] { "ID", "Location", "New Enemies" }).ToList(), (new int[] { 20, 20, 60 }).ToList(), btScenes.Values.Where(b => GetAreasWithBattle(b.name).Count > 0).Select(b =>
          {
              List<string> names = b.GetCharSpecs().Take(b.u4BtChInitSetNum > 0 ? b.u4BtChInitSetNum : int.MaxValue).Select(e => enemyData.ContainsKey(e) ? enemyData[e].Name : e + " (???)").GroupBy(e => e).Select(g => $"{g.Key} x {g.Count()}").ToList();
              return new string[] { b.name, string.Join("/", GetAreasWithBattle(b.name).Select(a => historiaCruxRando.areaData[a].Name)), string.Join(", ", names) }.ToList();
          }).ToList()));
        pages.Add("encounters", page);
        return pages;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Battle Data...");
        btScenes.SaveDB3(Generator, @"\db\resident\bt_scene.wdb");

        charaSets.SaveDB3(Generator, @"\db\resident\_wdbpack.bin\r_charaset.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_charaset.wdb");

        btTables.Keys.ForEach(id =>
        {
            btTables[id].DeleteDB3(Generator, @"\db\btscenetable\" + id + ".db3");
        });
    }
}

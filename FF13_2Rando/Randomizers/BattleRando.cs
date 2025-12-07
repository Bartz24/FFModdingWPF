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
    public DataStoreWDB<DataStoreBtScene> btScenes = new();
    private readonly DataStoreWDB<DataStoreRCharaSet> charaSets = new();

    public Dictionary<string, DataStoreWDB<DataStoreBtSTable>> btTables = new();

    public Dictionary<string, EnemyData> enemyData = new();
    private readonly Dictionary<string, Dictionary<string, BossData>> bossData = new();
    public Dictionary<string, BattleData> battleData = new();
    public Dictionary<int, BossScalingData> bossScalingData = new();

    private Dictionary<string, string> shuffledBosses = new();
    public Dictionary<string, (int, int)> areaBounds = new();
    private Dictionary<string, (int, int)> areaBoundsOrig = new();

    private Dictionary<string, int> newBossRanks = new();

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

        FileHelpers.ReadCSVFile(@"data\bossscaling.csv", row =>
        {
            BossScalingData b = new(row);
            bossScalingData.Add(b.Rank, b);
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\battlescenes.csv", row =>
        {
            BattleData b = new(row);
            battleData.Add(b.ID, b);
        }, FileHelpers.CSVFileHeader.HasHeader);

        HistoriaCruxRando historiaCruxRando = Generator.Get<HistoriaCruxRando>();
        historiaCruxRando.areaData.Values.Where(a => !string.IsNullOrEmpty(a.BattleTableID)).ForEach(a =>
        {
            DataStoreWDB<DataStoreBtSTable> table = new();
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

            // TODO:
            // Make smoother scaling based on area depth after crux rando
            // review enemy/boss data for scaling purposes

            areaBounds = GetAreaRankBounds();
            areaBoundsOrig = new Dictionary<string, (int, int)>(areaBounds);

            DetermineAreaBounds();

            if (FF13_2Flags.Enemies.Bosses.SelectedValues.Count > 0)
            {
                ShuffleBosses();
            }

            ApplyEnemyBossPlacementUpdates();

            if (FF13_2Flags.Enemies.BossScaling.Enabled)
            {
                ApplyBossScalingUpdates();
            }

            RandomNum.ClearRand();
        }
    }

    private void DetermineAreaBounds()
    {
        HistoriaCruxRando cruxRando = Generator.Get<HistoriaCruxRando>();
        //List<string> areaUnlockOrder = new();//treasureRando.PlacementAlgo.Logic.GetPropValue<List<string>>("AreaUnlockOrder");
        //areaUnlockOrder = areaUnlockOrder.Where(a => areaBounds.ContainsKey(a)).ToList();
        //areaUnlockOrder.AddRange(areaBounds.Keys.Where(a => !areaUnlockOrder.Contains(a)));

        //areaUnlockOrder = RandomNum.ShuffleWeightedOrder(areaUnlockOrder, (i1, a1, i2, a2) =>
        //{
        //    return i1 == i2 ? 1 : (i1 < 5 || i2 < 5 ? 0 : Math.Abs(i1 - i2) < 3 ? 1 : 0);
        //});
        List<int> newMins = areaBounds.Values.Select(t => t.Item1).OrderBy(i => i).ToList();
        int enemyMaxRank = enemyData.Values.Where(e => !e.Traits.Contains("Boss")).Max(e => e.Rank);
        var maxAreaDepth = cruxRando.areaDepths.Max(kvp => kvp.Value);
        float ratio = (float)enemyMaxRank / (float)maxAreaDepth;
        foreach (var (area, range) in areaBounds)
        {
            // This is working out ok, can potentially bump up more after the first couple of ranks
            // Especially since the endgame is locked and adds like 6 nodes to depth currently
            var areaDepth = cruxRando.areaDepths[area];
            // Scale upper bound higher once we're more than 3 locations in
            var offset = areaDepth > 3 ? 2 : 0;
            var adjusted = (areaDepth + offset) * ratio;
            // Overall floor of 5 for max, scales up with areaDepth*0.75
            int newMax = Math.Max((int)adjusted + 1, 5);
            // Min is 1 or scaled rank*0.5
            var mult = 0.5;
            int newMin = Math.Max(1, (int)(adjusted * mult));
            areaBounds[area] = (newMin, Math.Min(newMax, enemyMaxRank));
        }
    }

    private void ShuffleBosses()
    {
        HistoriaCruxRando cruxRando = Generator.Get<HistoriaCruxRando>();
        Dictionary<string, BossData> reducedBossDataForShuffle = bossData.Keys.Distinct()
                            .Where(g => FF13_2Flags.Enemies.Bosses.SelectedValues.Contains(g))
                            .ToDictionary(g => g, g => bossData[g].Values.First(b => b.Traits.Contains("Main")))
                            .Where(kvp => !kvp.Value.Traits.Contains("NoShuffle"))
                            .ToDictionary();
        List<string> list = bossData.Keys
            .Where(g => FF13_2Flags.Enemies.Bosses.SelectedValues.Contains(g))
            .Where(g => !bossData[g].Values.First(b => b.Traits.Contains("Main")).Traits.Contains("NoShuffle"))
            .ToList();

        // Earlier in this list is an easier boss by rank. Bosses of equivalent rank are randomised in order
        List<string> bossesByTheirRank = reducedBossDataForShuffle
            .GroupBy(kvp => kvp.Value.Rank)
            .SelectMany(group => group.Shuffle())
            .Select(kvp => kvp.Key)
            .ToList();

        // Ordered by where you'll encounter the area, with some variance and then randomised by rank
        List<string> locationsByTheirDepth = reducedBossDataForShuffle
            .GroupBy(kvp =>
            {
                // This basically leaves gog vanilla always - check if I did something silly...
                var location = kvp.Value.Location;
                var areaDepth = cruxRando.areaDepths[location];
                // Randomise the area ranks to shuffle things up a little
                return RandomNum.NextInt(areaDepth - 1, areaDepth + 1);
            })
            .SelectMany(group => group.Shuffle())
            .Select(kvp => kvp.Key)
            .ToList();

        // shuffled Bosses should now pick bosses based on a rough mapping of [vanilla boss] => [location depth in shuffled areas] => [boss of equivalent rank to depth]
        List<string> shuffled = new();
        for (var i = 0; i < list.Count; i++)
        {
            var originalBossName = list[i];
            var locationDepth = locationsByTheirDepth.IndexOf(originalBossName);
            var newBoss = bossesByTheirRank[locationDepth];
            shuffled.Add(newBoss);
        }
        shuffledBosses = Enumerable.Range(0, list.Count).ToDictionary(i => list[i], i => shuffled[i]);
    }

    private void ApplyEnemyBossPlacementUpdates()
    {
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
                    oldEnemies.Add(oldEnemies[RandomNum.NextInt(0, oldEnemies.Count)]);
                }
            }

            if (count < oldEnemies.Count)
            {
                for (int i = oldEnemies.Count; i > count; i--)
                {
                    oldEnemies.RemoveAt(RandomNum.NextInt(0, oldEnemies.Count));
                }
            }

            if (count > 0)
            {
                if (!oldEnemies[0].Traits.Contains("Boss") || FF13_2Flags.Enemies.Bosses.SelectedValues.Count > 0)
                {
                    List<EnemyData> validEnemies = enemyData.Values.Where(e => !e.Traits.Contains("Boss")).ToList();
                    if (battleData.ContainsKey(b.record))
                    {
                        validEnemies = validEnemies.Where(e => e.Parts.Count == 0 || oldEnemies.Contains(e)).ToList();
                    }

                    UpdateEnemyLists(oldEnemies, validEnemies, b.record, b.record.StartsWith("btsc011"));
                }
            }
        });
    }

    private Dictionary<string, (int, int)> GetAreaRankBounds()
    {
        Dictionary<string, HashSet<int>> areaRanks = new();
        btScenes.Values.ForEach(b =>
        {
            int rank = GetBattleRank(b);
            if (rank > 0)
            {
                List<string> areas = GetAreasWithBattle(b.record);
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
        newEnemy.s10DropItem0 = origEnemy.s10DropItem0;
        newEnemy.s10DropItem1 = origEnemy.s10DropItem1;
        newEnemy.s10DropItem2 = origEnemy.s10DropItem2;
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
                    btScenes[btsceneName].s10BtChEntryId = "";
                    btScenes[btsceneName].s10PartyEntryId = "";
                }
                else
                {
                    btScenes[btsceneName].s10BtChEntryId = "btsc_def_e00";
                    btScenes[btsceneName].s10PartyEntryId = "btsc_def_p00";
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

                    // Variety limit is 3 or the vanilla variety + 1, or if specified in battle data
                    int varietyLimit = Math.Min(3, GetCharaSpecs(oldEnemies).Distinct().Count() + 1);
                    if (battleData.ContainsKey(btsceneName) && battleData[btsceneName].VarietyLimit != CSVDataRow.CSV_INVALID_VALUE)
                    {
                        varietyLimit = battleData[btsceneName].VarietyLimit;
                    }

                    if (GetCharaSpecs(newEnemies).Distinct().Count() > varietyLimit)
                    {
                        // Pick a enemy from the already selected ones
                        newEnemy = RandomNum.SelectRandom(newEnemies);
                    }
                    else
                    {
                        newEnemy = RandomNum.SelectRandom(possible);
                    }

                    if (battleData.ContainsKey(btsceneName))
                    {
                        if (oldEnemies.Contains(newEnemy))
                        {
                            break;
                        }

                        battleData[btsceneName].Charasets.ForEach(c =>
                        {
                            List<string> list = charaSets[c].CharaSpecs;

                            string spec = enemyRando.HasEnemy(newEnemy.ID) ? enemyRando.GetEnemy(newEnemy.ID).sCharaSpec : newEnemy.ID;
                            if (!list.Contains(spec))
                            {
                                list.Add(spec);
                            }

                            newEnemy.Parts.ForEach(id =>
                            {
                                string spec = enemyRando.HasEnemy(newEnemy.ID) ? enemyRando.GetEnemy(newEnemy.ID).sCharaSpec : newEnemy.ID;
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
            charaSpecs.Select(spec => enemyRando.HasEnemy(spec) ? enemyRando.GetEnemy(spec).sCharaSpec : spec).ForEach(spec =>
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
        foreach (string id in btScenes.Keys)
        {
            // Skip any battles that have bosses in them
            if (btScenes[id].GetCharSpecs().Where(s => enemyData.ContainsKey(s)).Select(s => enemyData[s]).Any(e => e.Traits.Contains("Boss")))
            {
                continue;
            }

            List<string> areas = GetAreasWithBattle(id);
            if (areas.Count > 0)
            {
                List<EnemyData> oldEnemies = btScenes[id].GetCharSpecs().Where(s => enemyData.ContainsKey(s)).Select(s => enemyData[s]).ToList();

                if (oldEnemies.Count > 0)
                {
                    int diff = (int)(oldEnemies.Max(e => e.Rank) * Math.Pow(1.05, oldEnemies.Count));
                    foreach (string a in areas)
                    {
                        if (!diffs.ContainsKey(a))
                        {
                            diffs.Add(a, new List<int>());
                        }

                        diffs[a].Add(diff);
                    }
                }
            }
        }

        return diffs.ToDictionary(p => p.Key, p => (int)Math.Ceiling(p.Value.Average()));
    }

    private void ApplyBossScalingUpdates()
    {
        var areaDifficulties = GetAreaDifficulties();
        EnemyRando enemyRando = Generator.Get<EnemyRando>();
        foreach (var bossGroups in bossData)
        {
            var mainBoss = bossGroups.Value.Values.FirstOrDefault(b => b.Traits.Contains("Main"));
            if (mainBoss.Traits.Contains("NoScaling") || mainBoss == null)
            {
                continue;
            }

            // Determine the new boss replacing this one
            string newBossGroup = shuffledBosses.ContainsKey(bossGroups.Key) ? shuffledBosses[bossGroups.Key] : bossGroups.Key;
            var newMainBoss = bossData[newBossGroup].Values.FirstOrDefault(b => b.Traits.Contains("Main"));

            // Get the location avg rank for the original boss
            int locationAvgRank = areaDifficulties.GetValueOrDefault(mainBoss.Location, -1);
            if (locationAvgRank == -1)
            {
                continue;
            }

            // Determine the new rank from the original location avg
            int newRank = locationAvgRank + mainBoss.RankOffsetToLocationAvg;
            int oldRank = mainBoss.Rank;

            if (oldRank == newRank)
            {
                continue;
            }

            // Clamp new rank to available scaling data
            newRank = Math.Max(bossScalingData.Keys.Min(), Math.Min(bossScalingData.Keys.Max(), newRank));

            // Apply scaling to HP/STR/MAG based on the rank difference
            double hpMult = (double)bossScalingData[newRank].HP / bossScalingData[oldRank].HP;
            double strMagMult = (double)bossScalingData[newRank].STRMAG / bossScalingData[oldRank].STRMAG;

            foreach (var boss in bossData[newBossGroup].Values)
            {
                if (!enemyRando.HasEnemy(boss.ID))
                {
                    continue;
                }

                var enemy = enemyRando.GetEnemy(boss.ID);
                enemy.u24MaxHp = (int)(enemy.u24MaxHp * hpMult);
                enemy.u16StatusStr = (int)(enemy.u16StatusStr * strMagMult);
                enemy.u16StatusMgk = (int)(enemy.u16StatusMgk * strMagMult);
            }              
            
            newBossRanks[newMainBoss.Group] = newRank;
        }
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        HistoriaCruxRando historiaCruxRando = Generator.Get<HistoriaCruxRando>();
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        HTMLPage page = new("Encounters", "template/documentation.html");

        page.HTMLElements.Add(new Table("Bosses", (new string[] { "Original Boss", "New Boss", "New Boss Rank" }).ToList(), (new int[] { 35, 35, 30 }).ToList(), bossData.Keys.Select(name =>
        {
            string original = name;
            string newName = shuffledBosses.ContainsKey(name) ? shuffledBosses[name] : name;
            return new string[] { original, newName, newBossRanks.ContainsKey(newName) ? newBossRanks[newName].ToString() : "N/A" }.ToList();
        }).ToList()));

        page.HTMLElements.Add(new Table("Encounters", (new string[] { "ID", "Location", "New Enemies" }).ToList(), (new int[] { 20, 20, 60 }).ToList(), btScenes.Values.Where(b => GetAreasWithBattle(b.record).Count > 0).Select(b =>
          {
              List<string> names = b.GetCharSpecs().Take(b.u4BtChInitSetNum > 0 ? b.u4BtChInitSetNum : int.MaxValue).Select(e => enemyData.ContainsKey(e) ? enemyData[e].Name : e + " (???)").GroupBy(e => e).Select(g => $"{g.Key} x {g.Count()}").ToList();
              return new string[] { b.record, string.Join("/", GetAreasWithBattle(b.record).Select(a => historiaCruxRando.areaData[a].Name)), string.Join(", ", names) }.ToList();
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

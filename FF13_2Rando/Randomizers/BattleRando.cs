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

namespace FF13_2Rando
{
    public class BattleRando : Randomizer
    {
        public DataStoreDB3<DataStoreBtScene> btScenes = new DataStoreDB3<DataStoreBtScene>();
        DataStoreDB3<DataStoreRCharaSet> charaSets = new DataStoreDB3<DataStoreRCharaSet>();

        public Dictionary<string, DataStoreDB3<DataStoreBtSTable>> btTables = new Dictionary<string, DataStoreDB3<DataStoreBtSTable>>();

        Dictionary<string, EnemyData> enemyData = new Dictionary<string, EnemyData>();
        Dictionary<string, Dictionary<string, BossData>> bossData = new Dictionary<string, Dictionary<string, BossData>>();
        public Dictionary<string, BattleData> battleData = new Dictionary<string, BattleData>();

        Dictionary<string, string> shuffledBosses = new Dictionary<string, string>();
        public Dictionary<string, (int, int)> areaBounds = new Dictionary<string, (int, int)>();
        Dictionary<string, (int, int)> areaBoundsOrig = new Dictionary<string, (int, int)>();

        public BattleRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetProgressFunc("Loading Battle Data...", 0, -1);
            btScenes.LoadDB3("13-2", @"\db\resident\bt_scene.wdb");
            enemyData = File.ReadAllLines(@"data\enemies.csv").Select(s => new EnemyData(s.Split(","))).ToDictionary(e => e.ID, e => e);

            charaSets.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_charaset.wdb", false);

            bossData.Clear();
            using (CsvParser csv = new CsvParser(new StreamReader(@"data\bosses.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
            {
                while (csv.Read())
                {
                    if (csv.Row > 1)
                    {
                        BossData b = new BossData(csv.Record);
                        if (!bossData.ContainsKey(b.Group))
                            bossData.Add(b.Group, new Dictionary<string, BossData>());

                        bossData[b.Group].Add(b.ID, b);
                    }
                }
            }

            FileHelpers.ReadCSVFile(@"data\battlescenes.csv", row =>
            {
                BattleData b = new BattleData(row);
                battleData.Add(b.ID, b);
            }, FileHelpers.CSVFileHeader.HasHeader);

            HistoriaCruxRando historiaCruxRando = Randomizers.Get<HistoriaCruxRando>();
            historiaCruxRando.areaData.Values.Where(a => !string.IsNullOrEmpty(a.BattleTableID)).ForEach(a =>
            {
                DataStoreDB3<DataStoreBtSTable> table = new DataStoreDB3<DataStoreBtSTable>();
                table.LoadDB3("13-2", @"\db\btscenetable\" + a.BattleTableID + ".wdb");
                btTables.Add(a.BattleTableID, table);
            });
        }
        public override void Randomize(Action<int> progressSetter)
        {
            Randomizers.SetProgressFunc("Randomizing Battle Data...", 0, -1);
            EnemyRando enemyRando = Randomizers.Get<EnemyRando>();
            if (FF13_2Flags.Enemies.EnemyLocations.FlagEnabled)
            {
                FF13_2Flags.Enemies.EnemyLocations.SetRand();

                areaBounds = GetAreaRankBounds();
                areaBoundsOrig = new Dictionary<string, (int, int)>(areaBounds);

                TreasureRando treasureRando = Randomizers.Get<TreasureRando>();
                List<string> areaUnlockOrder = treasureRando.PlacementAlgo.Logic.GetPropValue<List<string>>("AreaUnlockOrder");
                areaUnlockOrder = areaUnlockOrder.Where(a => areaBounds.ContainsKey(a)).ToList();
                areaUnlockOrder.AddRange(areaBounds.Keys.Where(a => !areaUnlockOrder.Contains(a)));

                RandomNum.ShuffleWeightedOrder(areaUnlockOrder, (i1, a1, i2, a2) => {
                    if (i1 < 5 || i2 < 5)
                        return 0;
                    return Math.Abs(i1 - i2) < 3 ? 1 : 0;
                });
                List<int> newMins = areaBounds.Values.Select(t => t.Item1).OrderBy(i => i).ToList();
                for (int i = 0; i < areaBounds.Count; i++)
                {
                    string area = areaUnlockOrder[i];
                    areaBounds[area] = (newMins[i], (int)(newMins[i] * ((float)areaBounds[area].Item2 / areaBounds[area].Item1)));
                }

                if (FF13_2Flags.Enemies.Bosses.Enabled)
                {
                    List<string> list = bossData.Keys.Where(g => !bossData[g].Values.First(b => b.Traits.Contains("Main")).Traits.Contains("NoShuffle")).ToList();
                    // DLC fights require entry
                    if (!FF13_2Flags.Enemies.DLCBosses.Enabled)
                        list = list.Where(g => !bossData[g].Values.First(b => b.Traits.Contains("Main")).Traits.Contains("ForceEntry")).ToList();
                    List<string> shuffled = list.Shuffle().ToList();
                    shuffledBosses = Enumerable.Range(0, list.Count).ToDictionary(i => list[i], i => shuffled[i]);
                }


                btScenes.Values.ForEach(b =>
                {
                    List<EnemyData> oldEnemies = b.GetCharSpecs().Where(s => enemyData.Keys.Contains(s)).Select(s => enemyData[s]).ToList();
                    int count = oldEnemies.Count;
                    if (!FF13_2Flags.Enemies.LargeEnc.Enabled)
                        count = Math.Min(4, count);
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
                        if (!oldEnemies[0].Traits.Contains("Boss") || FF13_2Flags.Enemies.Bosses.Enabled)
                        {
                            List<EnemyData> validEnemies = enemyData.Values.Where(e => !e.Traits.Contains("Boss")).ToList();
                            if (battleData.ContainsKey(b.name))
                            {
                                validEnemies = validEnemies.Where(e => e.Parts.Count() == 0).ToList();
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
            Dictionary<string, (int, int)> areaBounds = new Dictionary<string, (int, int)>();
            HistoriaCruxRando historiaCruxRando = Randomizers.Get<HistoriaCruxRando>();
            historiaCruxRando.areaData.Values.ForEach(a =>
            {
                HashSet<int> ranks = new HashSet<int>();

                if (!string.IsNullOrEmpty(a.BattleTableID))
                {
                    btTables[a.BattleTableID].Values
                    .SelectMany(bt => bt.GetBattleIDs()
                        .Where(id => btScenes.Keys.Contains($"btsc{id.ToString("D5")}"))
                        .Select(id => btScenes[$"btsc{id.ToString("D5")}"]))
                    .Distinct()
                    .Select(b => GetBattleRank(b))
                    .Where(i => i > 0)
                    .ForEach(i => ranks.Add(i));
                }

                if (ranks.Count > 0)
                    areaBounds.Add(a.ID, (ranks.Min(), ranks.Max()));
            });
            return areaBounds;
        }

        private int GetBattleRank(DataStoreBtScene b)
        {
            List<EnemyData> enemies = b.GetCharSpecs().Where(s => enemyData.Keys.Contains(s)).Select(s => enemyData[s]).ToList();
            return enemies.Count > 0 ? enemies.Select(e => e.Rank).Max() : 0;
        }

        private List<string> GetAreasWithBattle(string btsceneName)
        {
            HistoriaCruxRando historiaCruxRando = Randomizers.Get<HistoriaCruxRando>();
            List<string> list = btTables.Keys
                .Where(id => btTables[id].Values
                    .SelectMany(bt => bt.GetBattleIDs()).Distinct()
                    .Where(i => btsceneName == "btsc" + i.ToString("D5")).Count() > 0)
                .Select(id => historiaCruxRando.areaData.Values.First(a => a.BattleTableID == id).ID)
                .ToList();

            if (battleData.ContainsKey(btsceneName) && !list.Contains(battleData[btsceneName].LocationID))
            {
                list.Add(battleData[btsceneName].LocationID);
            }

            return list;
        }

        private void UpdateBossStats(BossData newBoss, BossData origBoss)
        {
            EnemyRando enemyRando = Randomizers.Get<EnemyRando>();
            DataStoreBtCharaSpec newEnemy = enemyRando.GetEnemy(newBoss.ID);
            DataStoreBtCharaSpec origEnemy = enemyRando.GetEnemy(origBoss.ID, true);

            if (newBoss.HPMult != -1)
                newEnemy.u24MaxHp = (int)(origEnemy.u24MaxHp / origBoss.HPMult * newBoss.HPMult);

            if (newBoss.STRMult != -1)
                newEnemy.u16StatusStr = (int)(origEnemy.u16StatusStr / origBoss.STRMult * newBoss.STRMult);

            if (newBoss.MAGMult != -1)
                newEnemy.u16StatusMgk = (int)(origEnemy.u16StatusMgk / origBoss.MAGMult * newBoss.MAGMult);

            if (newBoss.HPMult != -1)
                newEnemy.u24MaxHp = (int)(origEnemy.u24MaxHp / origBoss.HPMult * newBoss.HPMult);

            if (newBoss.StaggerPointMult != -1 && newEnemy.u12BrChainBonus != 1000 && origEnemy.u12BrChainBonus != 1000)
                newEnemy.u12BrChainBonus = Math.Min(999, (int)((origEnemy.u12BrChainBonus - 100) / origBoss.StaggerPointMult * newBoss.StaggerPointMult) + 100);

            if (newBoss.ChainResMult != -1)
                newEnemy.u12MaxBp = Math.Min(100, (int)(origEnemy.u12MaxBp / origBoss.ChainResMult * newBoss.ChainResMult));

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
            EnemyRando enemyRando = Randomizers.Get<EnemyRando>();
            List<EnemyData> newEnemies = new List<EnemyData>();
            if (oldEnemies[0].Traits.Contains("Boss"))
            {
                bool noEntry = true;
                foreach (EnemyData e in oldEnemies)
                {
                    if (bossData.Values.SelectMany(d => d.Values).Where(b => b.ID == e.ID).Count() == 0)
                        continue;
                    BossData oldBoss = bossData.Values.SelectMany(d => d.Values).First(b => b.ID == e.ID && b.Traits.Contains("Main"));
                    if (!shuffledBosses.ContainsKey(oldBoss.Group))
                        return;
                    string newGroup = shuffledBosses[oldBoss.Group];
                    if (oldBoss.Group != newGroup)
                    {
                        newEnemies.Add(enemyData[bossData[newGroup].Values.First(b => b.Traits.Contains("Main")).ID]);
                        bossData[newGroup].Values.ForEach(b => UpdateBossStats(b, oldBoss));
                        if (!bossData[newGroup].Values.First(b => b.Traits.Contains("Main")).Traits.Contains("NoEntry"))
                            noEntry = false;
                        if (bossData[oldBoss.Group].Values.First(b => b.Traits.Contains("Main")).Traits.Contains("ForceEntry"))
                            noEntry = false;
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
                    return;

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

                    List<string> ignored = new List<string>();

                    do
                    {

                        attempts++;
                        canAdd = false;
                        int newRank;
                        if (oldRankMax > oldRankMin)
                            newRank = (int)Math.Round((oldEnemy.Rank - oldRankMin) * ((float)newRankMax - newRankMin) / ((float)oldRankMax - oldRankMin)) + newRankMin;
                        else
                            newRank = (int)Math.Round((oldEnemy.Rank - oldRankMin) * ((float)newRankMax - newRankMin)) + newRankMin;

                        int range = attempts + FF13_2Flags.Enemies.EnemyRank.Value;
                        if (sameRank)
                            range -= 2;
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
                        if (oldEnemies.Contains(newEnemy))
                        {
                            break;
                        }
                        if (battleData.ContainsKey(btsceneName))
                        {
                            battleData[btsceneName].Charasets.ForEach(c =>
                            {
                                List<string> list = charaSets[c].GetCharaSpecs();

                                string spec = enemyRando.HasEnemy(newEnemy.ID) ? enemyRando.GetEnemy(newEnemy.ID).sCharaSpec_string : newEnemy.ID;
                                if (!list.Contains(spec))
                                    list.Add(spec);
                                newEnemy.Parts.ForEach(id =>
                                {
                                    string spec = enemyRando.HasEnemy(newEnemy.ID) ? enemyRando.GetEnemy(newEnemy.ID).sCharaSpec_string : newEnemy.ID;
                                    if (!list.Contains(spec))
                                        list.Add(spec);
                                });

                                if (list.Count > battleData[btsceneName].CharasetLimit && list.Count > charaSets[c].GetCharaSpecs().Count)
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
                    } while (!canAdd);

                    if (newEnemy == null)
                        throw new Exception("Failed to add an enemy to " + btsceneName);
                    newEnemies.Add(newEnemy);
                });
            }

            List<string> charaSpecs = newEnemies.Select(e => e.ID).ToList();
            charaSpecs.AddRange(newEnemies.SelectMany(e => e.Parts).Distinct().Where(s => !charaSpecs.Contains(s)));
            btScenes[btsceneName].SetCharSpecs(charaSpecs);

            if (battleData.ContainsKey(btsceneName))
            {
                charaSpecs.Select(spec => enemyRando.HasEnemy(spec) ? enemyRando.GetEnemy(spec).sCharaSpec_string : spec).ForEach(spec =>
                {
                    battleData[btsceneName].Charasets.ForEach(c =>
                    {
                        List<string> list = charaSets[c].GetCharaSpecs();

                        if (!list.Contains(spec))
                            list.Add(spec);

                        charaSets[c].SetCharaSpecs(list);
                    });
                });
            }

            if (charaSpecs.Count > newEnemies.Count)
                btScenes[btsceneName].u4BtChInitSetNum = newEnemies.Sum(e => e.Size);
            else
                btScenes[btsceneName].u4BtChInitSetNum = 0;
        }

        public Dictionary<string, int> GetAreaDifficulties()
        {
            Dictionary<string, List<int>> diffs = new Dictionary<string, List<int>>();
            btScenes.Keys.ForEach(id =>
            {
                List<string> areas = GetAreasWithBattle(id);
                if (areas.Count > 0)
                {
                    List<EnemyData> oldEnemies = btScenes[id].GetCharSpecs().Where(s => enemyData.Keys.Contains(s)).Select(s => enemyData[s]).ToList();

                    if (oldEnemies.Count > 0)
                    {
                        int diff = (int)(oldEnemies.Max(e => e.Rank) * Math.Pow(1.05, oldEnemies.Count));
                        areas.ForEach(a =>
                        {
                            if (!diffs.ContainsKey(a))
                                diffs.Add(a, new List<int>());
                            diffs[a].Add(diff);
                        });
                    }
                }
            });

            return diffs.ToDictionary(p => p.Key, p => (int)Math.Ceiling(p.Value.Average()));
        }

        public override Dictionary<string, HTMLPage> GetDocumentation()
        {
            Dictionary<string, HTMLPage> pages = base.GetDocumentation();
            HTMLPage page = new HTMLPage("Encounters", "template/documentation.html");

            page.HTMLElements.Add(new Table("Bosses", (new string[] { "Original Boss", "New Boss", }).ToList(), (new int[] { 50, 50 }).ToList(), shuffledBosses.Select(p =>
            {
                return new string[] { p.Key, p.Value }.ToList();
            }).ToList()));

            page.HTMLElements.Add(new Table("Encounters", (new string[] { "ID (Actual Location TBD)", "New Enemies" }).ToList(), (new int[] { 60, 40 }).ToList(), btScenes.Values.Where(b => int.Parse(b.name.Substring(4)) >= 1100).Select(b =>
              {
                  List<string> names = b.GetCharSpecs().Select(e => enemyData.ContainsKey(e) ? enemyData[e].Name : (e + " (???)")).GroupBy(e => e).Select(g => $"{g.Key} x {g.Count()}").ToList();
                  return new string[] { b.name, string.Join(",", names) }.ToList();
              }).ToList()));
            pages.Add("encounters", page);
            return pages;
        }

        public override void Save()
        {
            Randomizers.SetProgressFunc("Saving Battle Data...", 0, -1);
            btScenes.SaveDB3(@"\db\resident\bt_scene.wdb");

            charaSets.SaveDB3(@"\db\resident\_wdbpack.bin\r_charaset.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_charaset.wdb");

            btTables.Keys.ForEach(id =>
            {
                btTables[id].DeleteDB3(@"\db\btscenetable\" + id + ".db3");
            });
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
            [RowIndex(4)]
            public int Size { get; set; }
            [RowIndex(5)]
            public List<string> Parts { get; set; }

            public EnemyData(string[] row) : base(row)
            {
            }
        }
        public class BossData : CSVDataRow
        {
            [RowIndex(0)]
            public string Group { get; set; }
            [RowIndex(1)]
            public int Rank { get; set; }
            [RowIndex(2)]
            public string ID { get; set; }
            [RowIndex(3)]
            public float HPMult { get; set; }
            [RowIndex(4)]
            public float STRMult { get; set; }
            [RowIndex(5)]
            public float MAGMult { get; set; }
            [RowIndex(6)]
            public float StaggerPointMult { get; set; }
            [RowIndex(7)]
            public float ChainResMult { get; set; }
            [RowIndex(8)]
            public float CPGilMult { get; set; }
            [RowIndex(9)]
            public List<string> Traits { get; set; }

            public BossData(string[] row) : base(row)
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
            public string LocationID { get; set; }
            [RowIndex(4)]
            public List<string> Charasets { get; set; }
            [RowIndex(5)]
            public List<string> Traits { get; set; }
            [RowIndex(6)]
            public int CharasetLimit { get; set; }
            public BattleData(string[] row) : base(row)
            {
            }
        }
    }
}

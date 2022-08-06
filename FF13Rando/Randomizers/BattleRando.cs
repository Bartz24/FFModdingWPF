using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FF13Rando
{
    public class BattleRando : Randomizer
    {
        public DataStoreWDB<DataStoreBtScene> btscene = new DataStoreWDB<DataStoreBtScene>();
        public DataStoreWDB<DataStoreBtScene> btsceneOrig = new DataStoreWDB<DataStoreBtScene>();

        DataStoreWDB<DataStoreCharaSet> charaSets = new DataStoreWDB<DataStoreCharaSet>();

        public ConcurrentDictionary<string, DataStoreWDB<DataStoreBtSc>> btscs = new ConcurrentDictionary<string, DataStoreWDB<DataStoreBtSc>>();

        public Dictionary<string, BattleData> battleData = new Dictionary<string, BattleData>();
        public Dictionary<string, EnemyData> enemyData = new Dictionary<string, EnemyData>();

        public BattleRando(RandomizerManager randomizers) : base(randomizers) { }

        public override string GetProgressMessage()
        {
            return "Randomizing Battles...";
        }
        public override string GetID()
        {
            return "Battles";
        }

        public override void Load()
        {
            Randomizers.SetProgressFunc("Loading Battle Data...", 0, 100);
            btscene.LoadWDB("13", @"\db\resident\bt_scene.wdb");
            btsceneOrig.LoadWDB("13", @"\db\resident\bt_scene.wdb");

            charaSets.LoadWDB("13", @"\db\resident\charaset.wdb");

            string btscWDBPath = Nova.GetNovaFile("13", @"btscene\wdb\btsc_wdb.bin", SetupData.Paths["Nova"], SetupData.Paths["13"]);
            string btscWDBOutPath = SetupData.OutputFolder + @"\btscene\wdb\btsc_wdb.bin";
            FileHelpers.CopyFile(btscWDBPath, btscWDBOutPath);
            Nova.UnpackWPD(btscWDBOutPath, SetupData.Paths["Nova"]);

            Randomizers.SetProgressFunc("Loading Battle Data...", 10, 100);

            FileHelpers.ReadCSVFile(@"data\battlescenes.csv", row =>
            {
                BattleData b = new BattleData(row);
                battleData.Add(b.ID, b);
            }, FileHelpers.CSVFileHeader.HasHeader);

            Randomizers.SetProgressFunc("Loading Battle Data...", 20, 100);

            if (FF13Flags.Other.Enemies.FlagEnabled)
            {
                IEnumerable<string> files = Directory.GetFiles(SetupData.OutputFolder + @"\btscene\wdb\_btsc_wdb.bin").Where(path => battleData.ContainsKey(Path.GetFileNameWithoutExtension(path)));
                int maxCount = files.Count();
                int count = 0;
                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 8 }, path =>
                {
                    count++;
                    Randomizers.SetProgressFunc($"Loading Encounter... ({count} of {maxCount})", count, maxCount);
                    DataStoreWDB<DataStoreBtSc> btsc = new DataStoreWDB<DataStoreBtSc>();
                    btsc.Load("13", path, SetupData.Paths["Nova"]);
                    btscs.TryAdd(Path.GetFileNameWithoutExtension(path), btsc);
                });
            }

            Randomizers.SetProgressFunc("Loading Battle Data...", 90, 100);
            FileHelpers.ReadCSVFile(@"data\enemies.csv", row =>
            {
                EnemyData e = new EnemyData(row);
                enemyData.Add(e.ID, e);
            }, FileHelpers.CSVFileHeader.HasHeader);
        }
        public override void Randomize(Action<int> progressSetter)
        {
            Randomizers.SetProgressFunc("Randomizing Battle Data...", 0, 100);
            EnemyRando enemyRando = Randomizers.Get<EnemyRando>("Enemies");
            if (FF13Flags.Other.Enemies.FlagEnabled)
            {
                FF13Flags.Other.Enemies.SetRand();

                List<string> enemies = battleData.Keys.Select(id => btscs[id]).SelectMany(wdb => wdb.Values.Select(b => b.sEntryBtChSpec_string)).Distinct().Where(e => !enemyData.Keys.Contains(e)).ToList();
                enemies.Sort();

                //List<string> setEnemies = charaSets.Values.SelectMany(c => c.GetCharaSpecs()).Distinct().ToList();
                //setEnemies.Sort();

                battleData.Keys.ToList().Shuffle().ForEach(id =>
                {
                    btscs[id].Values.Shuffle().Where(e => enemyData.ContainsKey(e.sEntryBtChSpec_string)).ForEach(e =>
                      {
                          string oldEnemy = e.sEntryBtChSpec_string;
                          bool canAdd = true;
                          List<string> possible = enemyData.Keys.Where(next =>
                          {
                              if (enemyData[oldEnemy].Traits.Contains("Event"))
                                  return true;
                              if (enemyData[oldEnemy].Traits.Contains("Flying"))
                                  return enemyData[next].Traits.Contains("Flying");
                              else
                                  return !enemyData[next].Traits.Contains("Flying");
                          }).Where(next =>
                          {
                              return enemyData[next].Rank >= enemyData[oldEnemy].Rank - FF13Flags.Other.EnemyRank.Value && enemyData[next].Rank <= enemyData[oldEnemy].Rank + FF13Flags.Other.EnemyRank.Value;
                          }).ToList();

                          do
                          {
                              canAdd = true;
                              e.sEntryBtChSpec_string = RandomNum.SelectRandomWeighted(possible, _ => 1);
                              if (e.sEntryBtChSpec_string == oldEnemy)
                              {
                                  break;
                              }
                              battleData[id].Charasets.ForEach(c =>
                              {
                                  List<string> list = charaSets[c].GetCharaSpecs();

                                  string charaspec = enemyRando.charaSpec[e.sEntryBtChSpec_string].sCharaSpec_string;
                                  if (!list.Contains(charaspec))
                                      list.Add(charaspec);

                                  if (list.Count > Math.Min(FF13Flags.Other.EnemyNoLimit.Enabled ? 60 : 16, battleData[id].CharasetLimit) && list.Count > charaSets[c].GetCharaSpecs().Count)
                                  {
                                      canAdd = false;
                                      possible.Remove(e.sEntryBtChSpec_string);
                                      if (possible.Count == 0)
                                      {
                                          canAdd = true;
                                          // If it hit the soft cap, it's ok to add
                                          if (FF13Flags.Other.EnemyNoLimit.Enabled && battleData[id].CharasetLimit >= 60 && list.Count <= 64)
                                          {
                                              possible.Add(e.sEntryBtChSpec_string);
                                          }
                                          else
                                          {
                                              e.sEntryBtChSpec_string = oldEnemy;
                                          }
                                      }
                                  }
                              });
                          } while (!canAdd);

                          if (e.sEntryBtChSpec_string != oldEnemy)
                          {
                              battleData[id].Charasets.ForEach(c =>
                              {
                                  List<string> list = charaSets[c].GetCharaSpecs();

                                  string charaspec = enemyRando.charaSpec[e.sEntryBtChSpec_string].sCharaSpec_string;
                                  if (!list.Contains(charaspec))
                                      list.Add(charaspec);

                                  charaSets[c].SetCharaSpecs(list);
                              });
                          }
                      });
                });

                RandomNum.ClearRand();
            }
        }

        private bool IsVarietyLimited(string id)
        {
            return battleData[id].Traits.Contains("LimitChara") || !FF13Flags.Other.EnemyNoLimit.Enabled;
        }

        public override HTMLPage GetDocumentation()
        {
            HTMLPage page = new HTMLPage("Encounters", "template/documentation.html");

            page.HTMLElements.Add(new Table("Encounters", (new string[] { "ID (Actual Location TBD)", "New Enemies" }).ToList(), (new int[] { 20, 80 }).ToList(), btscs.Keys.OrderBy(b => b).Select(b =>
              {
                  List<string> names = btscs[b].Values.Where(e => !e.sEntryBtChSpec_string.StartsWith("pc")).Select(e => enemyData.ContainsKey(e.sEntryBtChSpec_string) ? enemyData[e.sEntryBtChSpec_string].Name : (e.sEntryBtChSpec_string + " (???)")).GroupBy(e => e).Select(g => $"{g.Key} x {g.Count()}").ToList();
                  return new string[] { b, string.Join(",", names) }.ToList();
              }).ToList()));
            return page;
        }

        public override void Save()
        {
            Randomizers.SetProgressFunc("Saving Battle Data...", 0, 100);
            btscene.SaveWDB(@"\db\resident\bt_scene.wdb");

            charaSets.SaveWDB(@"\db\resident\charaset.wdb");

            Randomizers.SetProgressFunc("Saving Battle Data...", 10, 100);

            if (FF13Flags.Other.Enemies.FlagEnabled)
            {
                IEnumerable<string> files = Directory.GetFiles(SetupData.OutputFolder + @"\btscene\wdb\_btsc_wdb.bin").Where(path => battleData.ContainsKey(Path.GetFileNameWithoutExtension(path)));
                int maxCount = files.Count();
                int count = 0;
                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 8 }, path =>
                {
                    count++;
                    Randomizers.SetProgressFunc($"Saving Encounter... ({count} of {maxCount})", count, maxCount);
                    btscs[Path.GetFileNameWithoutExtension(path)].Save(path, SetupData.Paths["Nova"]);
                });
            }
            Randomizers.SetProgressFunc("Saving Battle Data...", 90, 100);
            Nova.RepackWPD(SetupData.OutputFolder + @"\btscene\wdb\btsc_wdb.bin", SetupData.Paths["Nova"]);
        }

        public class EnemyData
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public List<string> Traits { get; set; }
            public int Rank { get; set; }
            public EnemyData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Traits = row[2].Split("|").Where(s => !String.IsNullOrEmpty(s)).ToList();
                Rank = int.Parse(row[3]);
            }
        }

        public class BattleData
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }
            public List<string> Charasets { get; set; }
            public List<string> Traits { get; set; }
            public int CharasetLimit { get; set; }
            public BattleData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Location = row[2];
                Charasets = row[3].Split("|").Where(s => !String.IsNullOrEmpty(s)).ToList();
                Traits = row[4].Split("|").ToList();
                CharasetLimit = int.Parse(row[5]);
            }
        }
    }
}

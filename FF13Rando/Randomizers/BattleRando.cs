using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using FF13Rando;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FF13Rando;

public class BattleRando : Randomizer
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

    public BattleRando(RandomizerManager randomizers) : base(randomizers) { }

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
        btscsOrig = btscs.ToDictionary(k => k.Key, k => k.Value.Values.Where(e => !e.sEntryBtChSpec_string.StartsWith("pc")).Select(e => enemyData.ContainsKey(e.sEntryBtChSpec_string) ? enemyData[e.sEntryBtChSpec_string].Name : e.sEntryBtChSpec_string + " (???)").GroupBy(e => e).Select(g => $"{g.Key} x {g.Count()}").ToList());
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
        EnemyRando enemyRando = Randomizers.Get<EnemyRando>();
        if (FF13Flags.Other.Enemies.FlagEnabled)
        {
            FF13Flags.Other.Enemies.SetRand();

            List<string> enemies = battleData.Keys.Select(id => btscs[id]).SelectMany(wdb => wdb.Values.Select(b => b.sEntryBtChSpec_string)).Distinct().Where(e => !enemyData.Keys.Contains(e)).ToList();
            enemies.Sort();

            //List<string> setEnemies = charaSets.Values.SelectMany(c => c.GetCharaSpecs()).Distinct().ToList();
            //setEnemies.Sort();
            if (FF13Flags.Other.GroupShuffle.Enabled)
            {
                /*
                swap to per group rather than encounter.
                Iterate all display models in a group, save count
                (total-count) shuffled enemies in allowed range based on limit
                shuffle all enemies back into encounters based on enemy count, with a bias towards shuffled enemies slightly (depending on proportion)
                */

                List<string> charasets = battleData.Values.SelectMany(battle => battle.Charasets).Distinct().ToList();

                List<string> processedCharasets = new();

                Dictionary<string, int> charasetWithAvailable = charasets.ToDictionary(cs => cs, cs =>
                {
                    List<string> list = charaSets[cs].GetCharaSpecs();

                    List<string> battles = battleData.Where(battle => battle.Value.Charasets.Contains(cs)).Select(b => b.Key).ToList();
                    List<string> vanillaEnemies = battles.SelectMany(id => btscs[id].Values.Select(e => e.sEntryBtChSpec_string)).Distinct().ToList();
                    int reservedCapacity = vanillaEnemies.Count;

                    int nonBattleCharacters = list.Where(cha => !enemyData.ContainsKey(cha)).Count();
                    int standardLimit = charasetData[cs].Limit;

                    int totalCapacity = Math.Min(GetMaxCountAllowed(), standardLimit);

                    int availableSlots = totalCapacity - nonBattleCharacters - reservedCapacity;
                    if (availableSlots < 0)
                    {
                        Console.WriteLine($"Negative available slots resolved for set {cs}. Not sure what happened here? ({totalCapacity}, {nonBattleCharacters}, {reservedCapacity})");
                        availableSlots = 0;
                    }

                    return availableSlots;
                });

                Randomizers.SetUIProgress("Randomizing battle character sets", 0, 3);
                //Step 1: shuffle all charasets regardless of shared fights
                charasetWithAvailable.ForEach(charasetKVP =>
                {
                    string charaset = charasetKVP.Key;
                    int availableSlots = charasetKVP.Value;
                    List<string> list = charaSets[charaset].GetCharaSpecs();

                    //Extract all battles for a given charaset.
                    List<string> battles = battleData.Where(battle => battle.Value.Charasets.Contains(charaset)).Select(b => b.Key).ToList();
                    //Extract all vanilla enemies across all battles for the charaset
                    List<string> vanillaEnemies = battles.SelectMany(id => btscs[id].Values.Select(e => e.sEntryBtChSpec_string)).Distinct().ToList();
                    int reservedCapacity = vanillaEnemies.Count;

                    IEnumerable<string> singleSetFights = battles.Where(id => battleData[id].Charasets.Count == 1);
                    IEnumerable<string> sharedSetFights = battles.Where(id => battleData[id].Charasets.Count > 1);

                    IEnumerable<int> standardFights = singleSetFights.Select(id => battleData[id].Charasets.Min(c => charasetData[c].Limit));
                    List<string> peerCharasets = sharedSetFights.SelectMany(id => battleData[id].Charasets).Where(c => c != charaset).ToList();

                    //For each enemy in the group, generate the list of available candidates to shuffle in.
                    Dictionary<string, List<string>> shuffleCandidates = vanillaEnemies.ToDictionary(key => key, key =>
                    {
                        return !enemyData.ContainsKey(key) ? new List<string>() : resolvePossibleCandidates(key, enemyData.Keys);
                    });

                    List<string> enemiesToAddToSet = new();
                    //Only shuffle fights which actually have candidate enemies we can add to it.
                    List<string> fightsToShuffle = shuffleCandidates.Keys.Where(k => shuffleCandidates[k].Count > 0).ToList();
                    for (int i = availableSlots; i > 0; i--)
                    {
                        //Select a new random enemy to add to the pool of enemies for this group.
                        //TODO: need to take some care about enemy ranges here, probably have to do some kind of stepped distribution and select enemies in the relevant rank pools.
                        string shuffleIdx = RandomNum.SelectRandom(fightsToShuffle);
                        string newCandidateEnemy = RandomNum.SelectRandomWeighted(shuffleCandidates[shuffleIdx], enemy => vanillaEnemies.Contains(enemy) ? reservedCapacity : 2 * availableSlots);
                        if (newCandidateEnemy != null)
                        {
                            enemiesToAddToSet.Add(newCandidateEnemy);
                        }
                    }

                    foreach (string enemyToAdd in enemiesToAddToSet)
                    {
                        string charaspecToAdd = enemyRando.btCharaSpec[enemyToAdd].sCharaSpec_string;
                        if (!list.Contains(charaspecToAdd))
                        {
                            list.Add(charaspecToAdd);
                        }
                    }

                    charaSets[charaset].SetCharaSpecs(list);
                    processedCharasets.Add(charaset);

                    foreach (string peer in peerCharasets)
                    {
                        if (processedCharasets.Contains(peer))
                        {
                            continue;
                        }

                        int sharedFightCount = sharedSetFights.Where(s => battleData[s].Charasets.Contains(peer)).Count();
                        int peerTotalFightCount = battleData.Where(battle => battle.Value.Charasets.Contains(peer)).Count();
                        float peerProportion = (float)sharedFightCount / peerTotalFightCount;
                        float sharedProportion = (float)sharedFightCount / battles.Count;
                        //Resolve how many fights we share with this other charaset and what proportion of the overlap is from the other side.
                        //e.g. set 1 has 10 fights, set 2 has 15 fights, 5 overlap.
                        //In this case approx. half the added enemies in this group should be added to the other group to aid with possible shuffling. (cap dependent)
                        //If a set fully overlaps, then as many enemies as possible in the added set should be copied over
                        //take minimum ratio of shared fights from either cross over
                        //in example above, 5/10 = 1/2, 5/15 = 1/3
                        //Take the final ratio (1/3) and multiply by the minimum of enemies to add or available slots in the peer
                        float approxCopyCount = Math.Min(peerProportion, sharedProportion) * Math.Min(enemiesToAddToSet.Count, charasetWithAvailable[peer]);
                        //Take the floor of this
                        int availablePeerSlotsToFill = (int)Math.Floor(approxCopyCount);
                        List<string> peerList = charaSets[peer].GetCharaSpecs();
                        for (int i = availablePeerSlotsToFill; i > 0; i--)
                        {
                            string peerEnemy = RandomNum.SelectRandom(enemiesToAddToSet);
                            string charaspecToAdd = enemyRando.btCharaSpec[peerEnemy].sCharaSpec_string;
                            if (!peerList.Contains(peerEnemy))
                            {
                                peerList.Add(peerEnemy);
                            }
                        }
                        // Make sure we update the available amount remaining for a peer fill
                        charasetWithAvailable[peer] -= availablePeerSlotsToFill;
                        charaSets[peer].SetCharaSpecs(peerList);
                    }
                });

                //Step 2: shuffle single charaset fights
                Randomizers.SetUIProgress("Randomizing single character set battles", 1, 3);
                charasets.ForEach(charaset =>
                {
                    List<string> candidates = charaSets[charaset].GetCharaSpecs();
                    //Extract all battles for a given charaset.
                    List<string> battles = battleData.Where(battle => battle.Value.Charasets.Contains(charaset)).Select(b => b.Key).ToList();
                    battles.Where(id => battleData[id].Charasets.Count == 1).ForEach(id =>
                    {
                        btscs[id].Values.Shuffle().Where(e => enemyData.ContainsKey(e.sEntryBtChSpec_string)).ForEach(e =>
                        {
                            // List the old enemy
                            string oldEnemy = e.sEntryBtChSpec_string;
                            List<string> possible = resolvePossibleCandidates(oldEnemy, enemyData.Keys.Where(enemy => candidates.Contains(enemyRando.btCharaSpec[enemy].sCharaSpec_string)));

                            if (possible.Count > 0)
                            {
                                //Select a new random enemy from the list
                                e.sEntryBtChSpec_string = RandomNum.SelectRandomWeighted(possible, _ => 1);
                            }
                            else
                            {
                                Console.WriteLine($"Unable to resolve possible enemy shuffle for encounter {id}");
                            }
                        });
                    });
                });

                List<string> multiCharasetBattles = battleData.Where(battle => battle.Value.Charasets.Count > 1).Select(battle => battle.Key).ToList();
                multiCharasetBattles.Shuffle().ForEach(id =>
                {
                    BattleData data = battleData[id];
                    List<string> dataCharsets = data.Charasets;
                    //Resolve all modified charasets available for this battle and take the intersection as enemy candidates.
                    List<List<string>> charasetEnemyGroups = dataCharsets.Select(cs => charaSets[cs].GetCharaSpecs()).ToList();
                    List<string> intersectionGroup = charasetEnemyGroups.Aggregate(charasetEnemyGroups[0], (a, b) => a.Intersect(b).ToList());
                    btscs[id].Values.Shuffle().Where(e => intersectionGroup.Contains(e.sEntryBtChSpec_string)).ForEach(e =>
                    {
                        // List the old enemy
                        string oldEnemy = e.sEntryBtChSpec_string;
                        List<string> possible = resolvePossibleCandidates(oldEnemy, enemyData.Keys.Where(enemy => intersectionGroup.Contains(enemyRando.btCharaSpec[enemy].sCharaSpec_string)));

                        if (possible.Count > 0)
                        {
                            //Select a new random enemy from the list if we have any to save
                            e.sEntryBtChSpec_string = RandomNum.SelectRandomWeighted(possible, _ => 1);
                        }
                        else
                        {
                            Console.WriteLine($"Unable to resolve possible enemy shuffle for encounter {id}");
                        }
                    });
                });
            }
            else
            {

                //For each battle scene
                battleData.Keys.Shuffle().ForEach(id =>
                {
                    //Obtain vanilla enemy list from game battle scene internal data
                    List<string> vanillaEnemies = btscs[id].Values.Select(e => e.sEntryBtChSpec_string).ToList();
                    //For each enemy in the scene
                    btscs[id].Values.Shuffle().Where(e => enemyData.ContainsKey(e.sEntryBtChSpec_string)).ForEach(e =>
                      {
                          // List the old enemy
                          string oldEnemy = e.sEntryBtChSpec_string;
                          bool canAdd = true; //Assume we can add something new
                                              //Obtain list of possible enemies to replace it with based on rank and type
                          List<string> possible = resolvePossibleCandidates(oldEnemy, enemyData.Keys);

                          do
                          {
                              canAdd = true;
                              //Select a new random enemy from the list
                              e.sEntryBtChSpec_string = RandomNum.SelectRandomWeighted(possible, _ => 1);
                              //If its in the vanilla set then we good, move on.
                              if (vanillaEnemies.Contains(e.sEntryBtChSpec_string))
                              {
                                  break;
                              }
                              //Otherwise, check all the charasets this fight is a part of and iterate
                              battleData[id].Charasets.ForEach(c =>
                              {
                                  //Get the list of scene models for this set as modified so far
                                  List<string> list = charaSets[c].GetCharaSpecs();

                                  //Retrieve "randomised" enemy data (no changes currently)
                                  string charaspec = enemyRando.btCharaSpec[e.sEntryBtChSpec_string].sCharaSpec_string;
                                  //Add spec to the list if not already containing
                                  if (!list.Contains(charaspec))
                                  {
                                      list.Add(charaspec);
                                  }

                                  //Check if the number of things in the list is greater than the limit
                                  if (list.Count > Math.Min(GetMaxCountAllowed(), battleData[id].Charasets.Min(c => charasetData[c].Limit)) && list.Count > charaSets[c].GetCharaSpecs().Count)
                                  {
                                      canAdd = false;
                                      possible.Remove(e.sEntryBtChSpec_string); //Take it back out of the list
                                      if (possible.Count == 0) //If there's no more enemies it can be...
                                      {
                                          canAdd = true;
                                          // If it hit the soft cap, it's ok to add
                                          if (FF13Flags.Other.EnemyVariety.SelectedIndex == FF13Flags.Other.EnemyVariety.Values.Count - 1 && battleData[id].Charasets.Min(c => charasetData[c].Limit) >= 44 && list.Count <= 48)
                                          {
                                              possible.Add(e.sEntryBtChSpec_string);
                                          }
                                          else
                                          {
                                              //Fallback to vanilla
                                              e.sEntryBtChSpec_string = oldEnemy;
                                          }
                                      }
                                  }
                              });
                          } while (!canAdd);

                          //Once we have a valid shuffled enemy
                          if (!vanillaEnemies.Contains(e.sEntryBtChSpec_string))
                          {
                              //Ensure all charaspecs are added to the list
                              battleData[id].Charasets.ForEach(c =>
                              {
                                  List<string> list = charaSets[c].GetCharaSpecs();

                                  string charaspec = enemyRando.btCharaSpec[e.sEntryBtChSpec_string].sCharaSpec_string;
                                  if (!list.Contains(charaspec))
                                  {
                                      list.Add(charaspec);
                                  }

                                  charaSets[c].SetCharaSpecs(list);
                              });
                          }
                      });
                });
            }

            RandomNum.ClearRand();
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
            List<string> newContents = charaSets[b].GetCharaSpecs().Where(c => !charaSetsOrig[b].Contains(c)).Where(spec => enemyData.ContainsKey(spec)).Select(spec => enemyData[spec].Name).ToList();
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

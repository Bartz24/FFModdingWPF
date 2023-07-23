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
using System.Windows.Forms;
using System.Windows.Input;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace FF13Rando;

public class BattleRando : Randomizer
{
    public DataStoreWDB<DataStoreBtScene> btscene = new();
    public DataStoreWDB<DataStoreBtScene> btsceneOrig = new();
    private readonly DataStoreWDB<DataStoreCharaSet> charaSets = new();
    private Dictionary<string, List<string>> charaSetsOrig = new();
    public Dictionary<string, DataStoreLYB> lybs = new();

    public DataStoreWDB<DataStoreMission> missions = new();

    public DataStoreWDB<DataStoreBtConstant> battleConsts = new();

    public SortedDictionary<string, DataStoreWDB<DataStoreBtSc>> btscs = new();
    public SortedDictionary<string, DataStoreWDB<DataStoreBtSc>> btscsOrig = new();

    public Dictionary<string, BattleData> battleData = new();
    public Dictionary<string, EnemyData> enemyData = new();
    public Dictionary<string, CharasetData> charasetData = new();

    public BattleRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        Randomizers.SetUIProgress("Loading Battle Data...", 0, 100);
        btscene.LoadWDB("13", @"\db\resident\bt_scene.wdb");
        btsceneOrig.LoadWDB("13", @"\db\resident\bt_scene.wdb");

        charaSets.LoadWDB("13", @"\db\resident\charaset.wdb");

        missions.LoadWDB("13", @"\db\resident\mission.wdb");

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

        btscs = LoadBtscs();
        btscsOrig = LoadBtscs();

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

        charaSetsOrig = charaSets.Keys.ToDictionary(k => k, k => charaSets[k].GetCharaSpecs());

        charasetData.Keys
            .Where(c=>c.StartsWith("chset_z"))
            .Select(c=> GetLybId(c)).Distinct()
            .ForEach(lybId =>
        {
            string relative = @"scene\lay\" + lybId + @"\bin\" + lybId + ".win32.lyb";
            string path = Nova.GetNovaFile("13", relative, SetupData.Paths["Nova"], SetupData.Paths["13"]);
            string outPath = SetupData.OutputFolder + "\\" + relative;
            FileHelpers.CopyFile(path, outPath);

            DataStoreLYB lyb = new ();
            lyb.LoadData(File.ReadAllBytes(outPath));
            lybs.Add(lybId, lyb);
        });
    }

    private SortedDictionary<string, DataStoreWDB<DataStoreBtSc>> LoadBtscs()
    {
        IEnumerable<string> files = Directory.GetFiles(SetupData.OutputFolder + @"\btscene\wdb\_btsc_wdb.bin").Where(path => battleData.ContainsKey(Path.GetFileNameWithoutExtension(path)));
        int maxCount = files.Count();
        int count = 0;
        ConcurrentDictionary<string, DataStoreWDB<DataStoreBtSc>> dict = new();
        Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 8 }, path =>
        {
            count++;
            Randomizers.SetUIProgress($"Loading Encounter... ({count} of {maxCount})", count, maxCount);
            DataStoreWDB<DataStoreBtSc> btsc = new()
            {
                ID = Path.GetFileNameWithoutExtension(path)
            };
            btsc.Load(path);
            dict.TryAdd(Path.GetFileNameWithoutExtension(path), btsc);
        });

        //Encounter containing chieftain and 4 _Display_ goblins - seems to be unused
        dict.Remove("btsc11457", out _);
        return new(dict);
    }

    private string GetLybId(string charasetId)
    {
        string num = charasetId.Substring(7, 3); // Get chset_z{023}...
        return $"scene00{num}";
    }

    private enum BattleType
    {
        NonEvent,
        Event
    }

    private List<string> ResolvePossibleCandidates(string oldEnemyID, IEnumerable<string> basePool, BattleType battleType)
    {
        EnemyData oldEnemy = enemyData[oldEnemyID];
        int diff = FF13Flags.Enemies.EnemyRank.Value;
        int rangeMin = oldEnemy.Rank - diff;
        int rangeMax = oldEnemy.Rank + diff;

        return basePool.Where(next =>
        {
            if (enemyData[next].Traits.Contains("Ignore"))
            {
                return false; //Ignore summoned weapons and Syphax
            }

            if (battleType == BattleType.Event)
            {
                return true; // Events can be replaced by anything
            }

            // SmallHover enemies can be replaced by LargeHover and SmallHover enemies
            if (oldEnemy.Traits.Contains("SmallHover"))
            {
                return enemyData[next].Traits.Contains("LargeHover") || enemyData[next].Traits.Contains("SmallHover");
            }

            // Flying - can move up and down
            // Turtle - too big and too hard if there's more than one
            // SmallHover - taken care of above, but keep in the list to remove from the "normal" pool
            // LargeHover - cannot move up and down and cannot be replaced by SmallHover as the hitbox will be too high
            List<string> groups = new() { "Flying", "Turtle", "SmallHover", "LargeHover" };

            return groups.Intersect(oldEnemy.Traits).Any()
                ? groups.Intersect(enemyData[next].Traits).Intersect(oldEnemy.Traits).Any()
                : !groups.Intersect(enemyData[next].Traits).Any();

        }).Where(next =>
        {
            return enemyData[next].Rank >= rangeMin && enemyData[next].Rank <= rangeMax;
        }).ToList();
    }

    public override void Randomize()
    {
        Randomizers.SetUIProgress("Randomizing Battle Data...", -1, 100);
        if (FF13Flags.Enemies.EnemiesFlag.FlagEnabled)
        {
            FF13Flags.Enemies.EnemiesFlag.SetRand();

            ReplaceLYBEnemies();

            if (FF13Flags.Enemies.EnemyVariety.SelectedIndex > 0)
            {
                RandomizeGroupShuffle();

                RemoveMissionOnlyCharasets();
            }

            RandomNum.ClearRand();
        }

        if (FF13Flags.Stats.RandTPBorders.FlagEnabled)
        {
            FF13Flags.Stats.RandTPBorders.SetRand();
            RandomizeTPBorders();
            RandomNum.ClearRand();
        }
    }

    private void RemoveMissionOnlyCharasets()
    {
        EnemyRando enemyRando = Randomizers.Get<EnemyRando>();

        // Remove charaspecs that only appear in missions from charasets
        charaSets.Values.ForEach(c =>
        {
            List<string> charaSpecs = c.GetCharaSpecs();
            charaSpecs.RemoveAll(spec =>
            {
                // Skip specs not in enemy rando
                if (!enemyRando.btCharaSpec.Values.Select(e => e.sCharaSpec_string).Contains(spec))
                {
                    return false;
                }

                // Get battles with enemies with this charaspec
                List<string> battles = btscs.Values
                .Where(btsc =>
                    battleData[btsc.ID].Charasets.Contains(c.ID) &&
                    btsc.Values
                        .Select(e => enemyRando.btCharaSpec[e.sEntryBtChSpec_string].sCharaSpec_string)
                        .Contains(spec))
                .Select(btsc => btsc.ID)
                .ToList();

                // If the battles list is not empty and all battles are missions which load the charaspec already, remove the charaspec
                return battles.Count > 0 && battles.All(b => battleData[b].Traits.Contains("Mission") && missions[battleData[b].MissionID].GetCharaSpecs().Contains(spec));
            });
            c.SetCharaSpecs(charaSpecs);
        });
    }

    private Dictionary<string, Dictionary<string, string>> ReplaceLYBEnemies()
    {
        EnemyRando enemyRando = Randomizers.Get<EnemyRando>();

        Dictionary<string, Dictionary<string, string>> lybMappings = new();
        lybs.Keys.ForEach(lybId =>
        {
            Randomizers.SetUIProgress($"Replacing enemies per zone ({lybs.Keys.ToList().IndexOf(lybId) + 1} out of {lybs.Count})", lybs.Keys.ToList().IndexOf(lybId) + 1, lybs.Count);
            List<DataStoreWDB<DataStoreBtSc>> btscsForLyb = btscs.Values.Where(
                            b => battleData[b.ID].Charasets.Where(
                                c => GetLybId(c) == lybId).Any()).ToList();

            List<string> enemies = btscsForLyb
            .SelectMany(
                b => b.Values.Select(
                    spec => spec.sEntryBtChSpec_string))
            .Distinct()
            .Where(e => enemyData.ContainsKey(e))
            .ToList();

            // Map each enemy to a distinct charaset ID within its rank range and add to lybMappings
            Dictionary<string, string> mapping = new();
            do
            {
                mapping.Clear();
                enemies.ForEach(enemyId =>
                {
                    List<string> possible = ResolvePossibleCandidates(enemyId, enemyData.Keys.Where(id=>!mapping.Values.Contains(id)), btscsForLyb
                        .Where(b => btscs[b.ID].Values.Select(e => e.sEntryBtChSpec_string).Contains(enemyId))
                        .All(b => battleData[b.ID].Traits.Contains("Event")) ? BattleType.Event : BattleType.NonEvent);

                    if (possible.Count == 0)
                    {
                        return;
                    }

                    string next = RandomNum.SelectRandom(possible);
                    mapping.Add(enemyId, next);
                });
            } while (mapping.Count != enemies.Count || mapping.Values.Select(e => enemyRando.btCharaSpec[e].sCharaSpec_string).Distinct().Count() != mapping.Count());

            lybMappings.Add(lybId, mapping);

            // Replace enemies in lyb looping through the EnemyCharasets
            lybs[lybId].EnemyCharasets.ForEach(pair =>
            {
                string enemy = mapping.Keys.FirstOrDefault(old => enemyRando.btCharaSpec[old].sCharaSpec_string == pair.Value);

                // Replace the charaset if it exists in the mapping
                if (enemy != null)
                {
                    lybs[lybId].EnemyCharasets[pair.Key] = enemyRando.btCharaSpec[mapping[enemy]].sCharaSpec_string;
                }
            });

            // Replace charasets
            charaSets.Values.Where(c=>GetLybId(c.ID) == lybId).ForEach(c =>
            {
                List<string> charaSpecs = c.GetCharaSpecs();

                // Get forced enemies
                List<string> forcedSpecs = enemies.Where(e =>
                        enemyData[e].LYBForced
                            .Select(l => $"scene{l.ToString("00000")}")
                            .Contains(lybId))
                .Select(e => enemyRando.btCharaSpec[e].sCharaSpec_string)
                .Distinct().Where(spec => charaSpecs.Contains(spec)).ToList();

                for (int i = 0; i < charaSpecs.Count; i++)
                {
                    string enemy = mapping.Keys.FirstOrDefault(old => enemyRando.btCharaSpec[old].sCharaSpec_string == charaSpecs[i]);

                    // Replace the charaset if it exists in the mapping
                    if (enemy != null)
                    {
                        charaSpecs[i] = enemyRando.btCharaSpec[mapping[enemy]].sCharaSpec_string;
                    }
                }

                // Add removed forced enemy specs back in
                charaSpecs.AddRange(forcedSpecs.Where(s => !charaSpecs.Contains(s)));

                c.SetCharaSpecs(charaSpecs);
            });

            // Replace enemies in each btsc
            btscsForLyb.ForEach(btsc =>
            {
                btsc.Values
                .Where(spec => mapping.ContainsKey(spec.sEntryBtChSpec_string))
                .ForEach(spec => spec.sEntryBtChSpec_string = mapping[spec.sEntryBtChSpec_string]);

                if (battleData[btsc.ID].Traits.Contains("Mission"))
                {
                    List<string> notInCharasets = btsc.Values
                        .Select(b => b.sEntryBtChSpec_string)
                        .Where(e => !charaSets.Values
                            .Where(c => battleData[btsc.ID].Charasets.Contains(c.ID))
                            .SelectMany(c => c.GetCharaSpecs())
                            .Distinct()
                            .Contains(enemyRando.btCharaSpec[e].sCharaSpec_string))
                        .Distinct()
                        .Where(e => !e.StartsWith("pc"))
                        .Select(e => enemyRando.btCharaSpec[e].sCharaSpec_string)
                        .ToList();

                    if (notInCharasets.Count > 4)
                    {
                        throw new Exception($"Mission {battleData[btsc.ID].MissionID} has more than 4 enemies not in charasets: {string.Join(", ", notInCharasets)}");
                    }

                    missions[battleData[btsc.ID].MissionID].SetCharaSpecs(notInCharasets);
                }
            });
        });

        return lybMappings;
    }

    private void RandomizeTPBorders()
    {
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
    }

    private void RandomizeGroupShuffle()
    {
        EnemyRando enemyRando = Randomizers.Get<EnemyRando>();

        // Save all charasets before group shuffle is ran
        Dictionary<string, List<string>> preGroupShuffleCharasets = charaSets.Keys.ToDictionary(k => k, k => charaSets[k].GetCharaSpecs());

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
            List<string> vanillaEnemies = battles.SelectMany(id => btscsOrig[id].Values.Select(e => e.sEntryBtChSpec_string)).Distinct().ToList();
            int reservedCapacity = vanillaEnemies.Count;

            IEnumerable<string> singleSetFights = battles.Where(id => battleData[id].Charasets.Count == 1);
            if (singleSetFights.Count() == 0)
            {
                //Add no new enemies to a group that only contains shared battles.
                //TODO: is this truly correct? If a group only contains shared battles then it can be shuffled, but the shuffle has to be applied to all sets.
                //Probably fine for now but could be improved.
                return;
            }
            IEnumerable<string> sharedSetFights = battles.Where(id => battleData[id].Charasets.Count > 1);

            IEnumerable<int> standardFights = singleSetFights.Select(id => battleData[id].Charasets.Min(c => charasetData[c].Limit));
            List<string> peerCharasets = sharedSetFights.SelectMany(id => battleData[id].Charasets).Where(c => c != charaset).ToList();

            //For each enemy in the group, generate the list of available candidates to shuffle in.
            Dictionary<string, List<string>> shuffleCandidates = vanillaEnemies.ToDictionary(key => key, key =>
            {
                if (!enemyData.ContainsKey(key))
                {
                    return new();
                }

                return ResolvePossibleCandidates(key, enemyData.Keys, 
                    battles
                        .Where(id => btscsOrig[id].Values.Select(e => e.sEntryBtChSpec_string).Contains(key))
                        .All(id => battleData[id].Traits.Contains("Event")) ? BattleType.Event : BattleType.NonEvent);
            });

            List<string> enemiesToAddToSet = new();
            //Only shuffle fights which actually have candidate enemies we can add to it.
            List<string> fightsToShuffle = shuffleCandidates.Keys.Where(k => shuffleCandidates[k].Count > 0).ToList();
            if (fightsToShuffle.Count > 0)
            {
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
            }

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
                    if (!peerList.Contains(charaspecToAdd))
                    {
                        peerList.Add(charaspecToAdd);
                    }
                }
                // Make sure we update the available amount remaining for a peer fill
                charasetWithAvailable[peer] -= availablePeerSlotsToFill;
                charaSets[peer].SetCharaSpecs(peerList);
            }
        });

        //Step 1b: shuffle sets only containing shared fights with the updated enemy pool
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
            if (singleSetFights.Count() > 0)
            {
                //Only perform this step if the set only contains shared fights
                return;
            }

            //Resolve all other peer charasets for this set
            IEnumerable<string> sharedSetFights = battles.Where(id => battleData[id].Charasets.Count > 1);

            List<string> peerCharasets = sharedSetFights.SelectMany(id => battleData[id].Charasets).Where(c => c != charaset).ToList();

            List<string> intersectionEnemies = peerCharasets.Select(id => charaSets[id].GetCharaSpecs()).Aggregate(null, (List<string>? prev, List<string> next) =>
            {
                if (prev == null)
                {
                    return next;
                }
                return prev.Intersect(next).ToList();
            });
            if (intersectionEnemies.Count > 0)
            {
                Console.WriteLine($"Adding {intersectionEnemies.Count} enemies to charaset {charaset}");
                charaSets[charaset].SetCharaSpecs(list.Union(intersectionEnemies).ToList());
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
                List<string> uniqueMissionEnemies = new();
                btscs[id].Values.Shuffle().Where(e => enemyData.ContainsKey(e.sEntryBtChSpec_string)).ForEach(e =>
                {
                    IEnumerable<string> enemyPool = enemyData.Keys.Where(enemy => candidates.Contains(enemyRando.btCharaSpec[enemy].sCharaSpec_string));

                    // If this is a mission battle, leave one of each unique enemy and randomize the rest
                    if (battleData[id].Traits.Contains("Mission"))
                    {
                        if (!uniqueMissionEnemies.Contains(e.sEntryBtChSpec_string))
                        {
                            uniqueMissionEnemies.Add(e.sEntryBtChSpec_string);
                            return;
                        }

                        // Add mission enemies to the pool
                        enemyPool = enemyPool.Union(missions[battleData[id].MissionID].GetCharaSpecs()
                            .Select(spec => enemyData.Keys.FirstOrDefault(missionEnemy => enemyRando.btCharaSpec[missionEnemy].sCharaSpec_string == spec))
                            .Where(e => !string.IsNullOrEmpty(e)));
                    }

                    // Use the old vanilla enemy as its rank should be the center instead of the currently replaced enemy
                    string oldEnemy = btscsOrig[id].Values.First(eOrig => eOrig.ID == e.ID).sEntryBtChSpec_string;
                    List<string> possible = ResolvePossibleCandidates(oldEnemy, enemyPool, battleData[id].Traits.Contains("Event") ? BattleType.Event : BattleType.NonEvent);

                    if (possible.Count > 0)
                    {
                        //Select a new random enemy from the list
                        e.sEntryBtChSpec_string = RandomNum.SelectRandom(possible);
                    }
                    else
                    {
                        Console.WriteLine($"Unable to resolve possible enemy shuffle for encounter {id}");
                    }
                });
            });
        });

        Randomizers.SetUIProgress("Randomizing multiple character set battles", 2, 3);
        List<string> multiCharasetBattles = battleData.Where(battle => battle.Value.Charasets.Count > 1).Select(battle => battle.Key).ToList();
        multiCharasetBattles.Shuffle().ForEach(id =>
        {
            // Battle is unused and contains invalid data.
            if (id == "btsc11457")
            {
                return;
            }
            BattleData data = battleData[id];
            List<string> dataCharsets = data.Charasets;
            //Resolve all modified charasets available for this battle and take the intersection as enemy candidates.
            List<List<string>> charasetEnemyGroups = dataCharsets.Select(cs => charaSets[cs].GetCharaSpecs()).ToList();
            List<string> intersectionGroup = charasetEnemyGroups.Aggregate((IEnumerable<string>)charasetEnemyGroups[0], (a, b) => a.Intersect(b)).Where(e => enemyData.ContainsKey(e)).ToList();

            List<string> uniqueMissionEnemies = new();
            btscs[id].Values.Shuffle().Where(e => intersectionGroup.Contains(enemyRando.btCharaSpec[e.sEntryBtChSpec_string].sCharaSpec_string)).ForEach(e =>
            {

                IEnumerable<string> enemyPool = enemyData.Keys.Where(enemy => intersectionGroup.Contains(enemyRando.btCharaSpec[enemy].sCharaSpec_string));

                // If this is a mission battle, leave one of each unique enemy and randomize the rest
                if (battleData[id].Traits.Contains("Mission"))
                {
                    if (!uniqueMissionEnemies.Contains(e.sEntryBtChSpec_string))
                    {
                        uniqueMissionEnemies.Add(e.sEntryBtChSpec_string);
                        return;
                    }

                    // Add mission enemies to the pool
                    enemyPool = enemyPool.Union(missions[battleData[id].MissionID].GetCharaSpecs()
                        .Select(spec => enemyData.Keys.FirstOrDefault(missionEnemy => enemyRando.btCharaSpec[missionEnemy].sCharaSpec_string == spec))
                        .Where(e => !string.IsNullOrEmpty(e)));
                }

                // Use the old vanilla enemy as its rank should be the center instead of the currently replaced enemy
                string oldEnemy = btscsOrig[id].Values.First(eOrig => eOrig.ID == e.ID).sEntryBtChSpec_string;
                List<string> possible = ResolvePossibleCandidates(oldEnemy, enemyPool, battleData[id].Traits.Contains("Event") ? BattleType.Event : BattleType.NonEvent);

                if (possible.Count > 0)
                {
                    //Select a new random enemy from the list if we have any to save
                    e.sEntryBtChSpec_string = RandomNum.SelectRandom(possible);
                }
                else
                {
                    Console.WriteLine($"Unable to resolve possible enemy shuffle for encounter {id}");
                }
            });
        });

        //Step 3: Remove any unused enemies from the character set to reduce memory overhead.
        Randomizers.SetUIProgress("Cleaning up unused enemies from character sets", 3, 3);
        charasets.ForEach(charaset =>
        {
            //TODO: Filter this to be non-battle characters only?
            List<string> original = preGroupShuffleCharasets[charaset];
            //Extract all battles for a given charaset.
            List<string> battles = battleData.Where(battle => battle.Value.Charasets.Contains(charaset)).Select(b => b.Key).ToList();
            //Extract all used enemies from battles
            List<string> used = battles.SelectMany(b => btscs[b].Values.Select(e => enemyRando.btCharaSpec[e.sEntryBtChSpec_string].sCharaSpec_string)).Distinct().ToList();
            //Union all required enemies from battles with the original contents of the character set
            //TODO: When we can update field models, this can be more aggressive to remove unused enemies.
            charaSets[charaset].SetCharaSpecs(used.Union(original).Distinct().ToList());
        });
    }

    private int GetMaxCountAllowed()
    {
        return FF13Flags.Enemies.EnemyVariety.SelectedIndex switch
        {
            0 => 0,
            1 => 0,
            2 => 36,
            _ => 16,
        };
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        HTMLPage page = new("Encounters", "template/documentation.html");

        Dictionary<string, List<string>> formattedOrig = btscsOrig.ToDictionary(k => k.Key, k => k.Value.Values.Where(e => !e.sEntryBtChSpec_string.StartsWith("pc")).Select(e => enemyData.ContainsKey(e.sEntryBtChSpec_string) ? enemyData[e.sEntryBtChSpec_string].Name : e.sEntryBtChSpec_string + " (???)").GroupBy(e => e).Select(g => $"{g.Key} x {g.Count()}").ToList());

        page.HTMLElements.Add(new Table("Encounters", (new string[] { "ID", "Region / Name (If known)", "New Enemies", "Old Enemies" }).ToList(), (new int[] { 5, 15, 40, 40 }).ToList(), btscs.Keys.OrderBy(b => b).Select(b =>
        {
            List<string> names = btscs[b].Values.Where(e => !e.sEntryBtChSpec_string.StartsWith("pc")).Select(e => enemyData.ContainsKey(e.sEntryBtChSpec_string) ? enemyData[e.sEntryBtChSpec_string].Name : e.sEntryBtChSpec_string + " (???)").GroupBy(e => e).Select(g => $"{g.Key} x {g.Count()}").ToList();
            List<string> oldNames = formattedOrig[b];
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

        missions.SaveWDB(@"\db\resident\mission.wdb");

        battleConsts.SaveWDB(@"\db\resident\bt_constants.wdb");

        Randomizers.SetUIProgress("Saving Battle Data...", 10, 100);

        if (FF13Flags.Enemies.EnemiesFlag.FlagEnabled)
        {
            IEnumerable<string> files = Directory.GetFiles(SetupData.OutputFolder + @"\btscene\wdb\_btsc_wdb.bin").Where(path => battleData.ContainsKey(Path.GetFileNameWithoutExtension(path)));
            int maxCount = files.Count();
            int count = 0;
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 8 }, path =>
            {
                count++;
                Randomizers.SetUIProgress($"Saving Encounter... ({count} of {maxCount})", count, maxCount);
                btscs[Path.GetFileNameWithoutExtension(path)].Save(path);
            });
        }

        Randomizers.SetUIProgress("Saving Battle Data...", 90, 100);
        Nova.RepackWPD(SetupData.OutputFolder + @"\btscene\wdb\btsc_wdb.bin", SetupData.Paths["Nova"]);

        lybs.Keys.ForEach(s =>
        {
            string relative = @"\scene\lay\" + s + @"\bin\" + s + ".win32.lyb";
            string outPath = SetupData.OutputFolder + relative;

            File.WriteAllBytes(outPath, lybs[s].Data);
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
        public List<int> LYBForced { get; set; }
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
        public string MissionID { get; set; }
        [RowIndex(5)]
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

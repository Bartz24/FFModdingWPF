using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando
{
    public class TreasureRando : Randomizer
    {
        public DataStoreBPSection<DataStoreReward> rewards, rewardsOrig;
        public DataStoreBPSection<DataStorePrice> prices;
        public Dictionary<string, DataStoreEBP> ebpAreas = new Dictionary<string, DataStoreEBP>();
        public Dictionary<string, DataStoreEBP> ebpAreasOrig = new Dictionary<string, DataStoreEBP>();

        public List<List<string>> hints = Enumerable.Range(0, 35).Select(_ => new List<string>()).ToList();

        public Dictionary<string, string> areaMapping = new Dictionary<string, string>();
        Dictionary<string, ItemLocation> itemLocations = new Dictionary<string, ItemLocation>();
        public Dictionary<string, string> missableTreasureLinks = new Dictionary<string, string>();

        FF12AssumedItemPlacementAlgorithm placementAlgoNormal;
        ItemPlacementAlgorithm<ItemLocation> placementAlgoBackup;
        private bool usingBackup = false;
        public ItemPlacementAlgorithm<ItemLocation> PlacementAlgo { get => usingBackup ? placementAlgoBackup : placementAlgoNormal; }

        public List<string> treasuresToPlace = new List<string>();
        public List<string> treasuresAllowed = new List<string>();

        public List<string> randomizeItems = new List<string>();
        public List<string> remainingRandomizeItems = new List<string>();

        public TreasureRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetUIProgress("Loading Treasure Data...", 0, -1);
            rewards = new DataStoreBPSection<DataStoreReward>();
            rewards.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_037.bin"));
            rewardsOrig = new DataStoreBPSection<DataStoreReward>();
            rewardsOrig.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_037.bin"));
            prices = new DataStoreBPSection<DataStorePrice>();
            prices.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_028.bin"));

            ebpAreas = File.ReadAllLines("data\\treasureAddresses.csv").ToDictionary(s => s.Split(',')[0], s =>
            {
                string name = s.Split(',')[0];
                string area = name.Substring(0, name.Length - 2);
                int offset = Int32.Parse(s.Split(',')[1]);
                int count = Int32.Parse(s.Split(',')[2]);
                DataStoreEBP ebp = new DataStoreEBP(offset, count);
                ebp.LoadData(File.ReadAllBytes($"data\\ps2data\\plan_master\\us\\plan_map\\{area}\\{name}\\global\\{name}.ebp"));
                return ebp;
            });
            ebpAreasOrig = File.ReadAllLines("data\\treasureAddresses.csv").ToDictionary(s => s.Split(',')[0], s =>
            {
                string name = s.Split(',')[0];
                string area = name.Substring(0, name.Length - 2);
                int offset = Int32.Parse(s.Split(',')[1]);
                int count = Int32.Parse(s.Split(',')[2]);
                DataStoreEBP ebp = new DataStoreEBP(offset, count);
                ebp.LoadData(File.ReadAllBytes($"data\\ps2data\\plan_master\\us\\plan_map\\{area}\\{name}\\global\\{name}.ebp"));
                return ebp;
            });

            PartyRando partyRando = Randomizers.Get<PartyRando>();
            partyRando.Characters = MathHelpers.DecodeNaturalSequence(prices[0x76].Price, 6, 6).Select(l => (int)l).ToArray();

            areaMapping = File.ReadAllLines("data\\mapAreas.csv").ToDictionary(s => s.Split(',')[1], s => s.Split(',')[0]);

            itemLocations.Clear();

            FileHelpers.ReadCSVFile(@"data\treasures.csv", row =>
            {
                int start = int.Parse(row[3]);
                int count = int.Parse(row[4]);
                for (int i = start; i < start + count; i++)
                {
                    TreasureData t = new TreasureData(row, i);
                    itemLocations.Add(t.ID, t);
                }
            }, FileHelpers.CSVFileHeader.HasHeader);

            int fakeID = -1;
            FileHelpers.ReadCSVFile(@"data\rewards.csv", row =>
            {
                RewardData parent = null;
                for (int i = 0; i < 3; i++)
                {
                    RewardData r = new RewardData(row, i, fakeID);
                    if (i == 0)
                    {
                        parent = r;
                    }
                    else
                    {
                        r.Parent = parent;
                    }
                    if (r.Traits.Contains("Fake"))
                    {
                        if (i == 0)
                            fakeID--;
                        else
                            continue;
                    }
                    else if (r.FakeItems.Count > 0)
                    {
                        if (i == 0)
                        {
                            RewardData rFake = new RewardData(row, i, fakeID, true);
                            fakeID--;
                            itemLocations.Add(rFake.ID, rFake);
                            parent = rFake;
                            r.Parent = parent;
                        }
                        r.FakeItems.Clear();
                    }
                    itemLocations.Add(r.ID, r);
                }
            }, FileHelpers.CSVFileHeader.HasHeader);

            if (FF12Flags.Items.KeyStartingInv.Enabled)
            {
                FileHelpers.ReadCSVFile(@"data\startingInvs.csv", row =>
                {
                    StartingInvData first = new StartingInvData(row, 0);
                    itemLocations.Add(first.ID, first);
                    for (int i = 1; i < partyRando.partyOrig[first.IntID].ItemIDs.Count; i++)
                    {
                        StartingInvData s = new StartingInvData(row, i);
                        itemLocations.Add(s.ID, s);
                    }
                }, FileHelpers.CSVFileHeader.HasHeader);
            }

            List<string> hintsNotesLocations = itemLocations.Values.SelectMany(l => l.Areas).Distinct().ToList();

            placementAlgoNormal = new FF12AssumedItemPlacementAlgorithm(itemLocations, hintsNotesLocations, Randomizers, 3)
            {
                SetProgressFunc = Randomizers.SetUIProgress
            };
            placementAlgoNormal.Logic = new FF12ItemPlacementLogic(placementAlgoNormal, Randomizers);

            placementAlgoBackup = new ItemPlacementAlgorithm<ItemLocation>(itemLocations, hintsNotesLocations, -1)
            {
                SetProgressFunc = Randomizers.SetUIProgress
            };
            placementAlgoBackup.Logic = new FF12ItemPlacementLogic(placementAlgoBackup, Randomizers);
        }
        public override void Randomize()
        {
            Randomizers.SetUIProgress("Randomizing Treasure Data...", 0, -1);

            randomizeItems = new List<string>();
            if (FF12Flags.Items.Treasures.FlagEnabled)
                randomizeItems = randomizeItems.Concat(GetRandomizableItems()).Distinct().ToList();
            ShopRando shopRando = Randomizers.Get<ShopRando>();
            if (FF12Flags.Items.Shops.FlagEnabled)
                randomizeItems = randomizeItems.Concat(shopRando.GetRandomizableShopItems()).Distinct().ToList();
            if (FF12Flags.Items.Bazaars.FlagEnabled)
                randomizeItems = randomizeItems.Concat(shopRando.GetRandomizableBazaarItems()).Distinct().ToList();

            remainingRandomizeItems = new List<string>(randomizeItems);

            if (FF12Flags.Items.Treasures.FlagEnabled)
            {
                FF12Flags.Items.Treasures.SetRand();

                CollapseAndSelectTreasures();

                Dictionary<string, double> areaMults = itemLocations.Values.SelectMany(t => t.Areas).Distinct().ToDictionary(s => s, _ => RandomNum.RandInt(10, 200) * 0.01d);
                if (!placementAlgoNormal.Randomize(new List<string>(), areaMults))
                {
                    usingBackup = true;
                    placementAlgoBackup.Randomize(new List<string>(), areaMults);
                }

                int respawnIndex = 0;
                itemLocations.Values.Shuffle().ForEach(l =>
                {
                    if (PlacementAlgo.Logic.GetKeysAllowed().Contains(l.ID))
                    {
                        if (IsEmpty(l.ID))
                        {
                            List<string> list = PlacementAlgo.Placement.Keys.Where(s => IsExtra(s) && PlacementAlgo.Logic.GetLocationItem(s, false) != null && !IsImportantKeyItem(PlacementAlgo.Placement[s]) && PlacementAlgo.Logic.IsAllowed(l.ID, PlacementAlgo.Placement[s])).Shuffle();
                            if (list.Count > 0)
                            {
                                string rep = list.First();
                                (string, int)? item = PlacementAlgo.Logic.GetLocationItem(rep, false);
                                PlacementAlgo.Logic.SetLocationItem(l.ID, item.Value.Item1, item.Value.Item2);
                                PlacementAlgo.Placement.Add(l.ID, PlacementAlgo.Placement[rep]);
                                PlacementAlgo.Placement.Remove(rep);
                                hints.Where(list => list.Contains(rep)).ForEach(list =>
                                {
                                    list.Remove(rep);
                                    list.Add(l.ID);
                                });
                            }
                        }
                        if (l is TreasureData)
                        {
                            DataStoreTreasure t = ebpAreas[((TreasureData)l).MapID].TreasureList[((TreasureData)l).Index];
                            t.Respawn = (byte)respawnIndex;
                            respawnIndex++;
                            t.SpawnChance = 100;
                        }
                    }
                    else if (l is TreasureData)
                    {
                        if (l.Traits.Contains("Missable") && RandomNum.RandInt(0, 99) < 20)
                        {
                            missableTreasureLinks.Add(l.ID, RandomNum.SelectRandomWeighted(treasuresAllowed, _ => 1));
                        }
                    }
                });

                EquipRando equipRando = Randomizers.Get<EquipRando>();
                itemLocations.Values.ForEach(l =>
                {
                    if (PlacementAlgo.Placement.ContainsKey(l.ID))
                    {
                        (string, int)? tuple = PlacementAlgo.Logic.GetLocationItem(l.ID, false);
                        if (tuple != null && randomizeItems.Contains(tuple.Value.Item1))
                        {
                            string newItem = RandomNum.SelectRandomWeighted(remainingRandomizeItems, item =>
                            {
                                if (item == "2000")
                                    return 25;
                                if (item.StartsWith("00") || item.StartsWith("20") || item.StartsWith("21"))
                                    return 8;
                                if ((item.StartsWith("30") || item.StartsWith("40")) && itemLocations[l.ID].Traits.Contains("Missable"))
                                    return 0;
                                if (item.StartsWith("30") || item.StartsWith("40"))
                                    return 1;
                                return 3;
                            });
                            if (newItem.StartsWith("30") || newItem.StartsWith("40"))
                            {
                                PlacementAlgo.Logic.SetLocationItem(l.ID, newItem, 1);
                                if (!tuple.Value.Item1.StartsWith("30") && !tuple.Value.Item1.StartsWith("40"))
                                    hints.Where(list => list.Count() == hints.Select(list => list.Count()).Min()).First().Add(l.ID);
                            }
                            else
                            {
                                PlacementAlgo.Logic.SetLocationItem(l.ID, newItem, tuple.Value.Item2);
                                if (tuple.Value.Item1.StartsWith("30") || tuple.Value.Item1.StartsWith("40"))
                                    hints.Where(list => list.Contains(l.ID)).ForEach(list => list.Remove(l.ID));

                                if (l is TreasureData && equipRando.itemData.ContainsKey(newItem) && equipRando.itemData[newItem].Upgrade != "")
                                {
                                    TreasureData t = (TreasureData)l;
                                    ebpAreas[t.MapID].TreasureList[t.Index].RareItem1ID = (ushort)equipRando.itemData[newItem].IntUpgrade;
                                    ebpAreas[t.MapID].TreasureList[t.Index].RareItem2ID = (ushort)equipRando.itemData[newItem].IntUpgrade;
                                }
                            }

                            if (ShouldRemoveItem(newItem))
                                remainingRandomizeItems.Remove(newItem);
                        }
                    }
                });

                if (!FF12Flags.Items.KeyWrit.Enabled)
                {
                    itemLocations.Values.Where(l => PlacementAlgo.Logic.GetLocationItem(l.ID, false) != null && PlacementAlgo.Logic.GetLocationItem(l.ID, false).Value.Item1 == "8070")
                        .ForEach(l =>
                        {
                            PlacementAlgo.Logic.SetLocationItem(l.ID, "0000", 5);
                            hints.Where(list => list.Contains(l.ID)).ForEach(list => list.Remove(l.ID));
                        });
                }

                missableTreasureLinks.ForEach(p =>
                {
                    (string, int)? item = PlacementAlgo.Logic.GetLocationItem(p.Value, false);
                    PlacementAlgo.Logic.SetLocationItem(p.Key, item.Value.Item1, item.Value.Item2);
                    DataStoreTreasure tMiss = ebpAreas[((TreasureData)itemLocations[p.Key]).MapID].TreasureList[((TreasureData)itemLocations[p.Key]).Index];
                    DataStoreTreasure tLinked = ebpAreas[((TreasureData)itemLocations[p.Value]).MapID].TreasureList[((TreasureData)itemLocations[p.Value]).Index];
                    tMiss.Respawn = tLinked.Respawn;
                    tMiss.SpawnChance = 100;
                });

                itemLocations.Values.ForEach(l =>
                {
                    if (!PlacementAlgo.Placement.ContainsKey(l.ID) && !missableTreasureLinks.ContainsKey(l.ID))
                    {
                        if (l is TreasureData)
                        {
                            DataStoreTreasure t = ebpAreas[((TreasureData)l).MapID].TreasureList[((TreasureData)l).Index];
                            t.Respawn = 255;
                            t.SpawnChance = 0;
                        }
                        else if (l is RewardData)
                        {
                            if (((RewardData)l).Index == 0)
                                PlacementAlgo.Logic.SetLocationItem(l.ID, "Gil", 0);
                            else
                                PlacementAlgo.Logic.SetLocationItem(l.ID, "FFFF", 255);
                        }
                    }
                });
                RandomNum.ClearRand();
            }
        }

        private bool ShouldRemoveItem(string newItem)
        {
            EquipRando equipRando = Randomizers.Get<EquipRando>();
            if (newItem.StartsWith("00") || newItem.StartsWith("20") || newItem.StartsWith("21"))
                return false;
            if (newItem.StartsWith("30") || newItem.StartsWith("40"))
                return true;

            return !equipRando.itemData.ContainsKey(newItem) || RandomNum.RandInt(0, 100) < Math.Pow(equipRando.itemData[newItem].Rank, 2);
        }

        private void CollapseAndSelectTreasures()
        {
            treasuresToPlace.Clear();
            List<int> usedRespawnIDs = new List<int>();
            itemLocations.Values.Where(l => l is TreasureData).Select(l => (TreasureData)l).ForEach(l =>
            {
                DataStoreTreasure t = ebpAreasOrig[((TreasureData)l).MapID].TreasureList[l.Index];
                DataStoreTreasure t2 = ebpAreas[((TreasureData)l).MapID].TreasureList[l.Index];
                if (RandomNum.RandInt(0, 99) < 75)
                {
                    if (t.GilChance > 0 && RandomNum.RandInt(0, 99) < 15)
                    {
                        t.GilChance = 100;
                        t.GilRare = t.GilCommon = (ushort)(t.GilRare * 8);
                        t2.GilChance = 100;
                        t2.GilRare = t2.GilCommon = (ushort)(t2.GilRare * 8);
                    }
                    else
                    {
                        if (RandomNum.RandInt(0, 99) < 30)
                        {
                            t.CommonItem1ID = t.CommonItem2ID = t.RareItem1ID = t.RareItem2ID = t.CommonItem1ID;
                            t2.CommonItem1ID = t2.CommonItem2ID = t2.RareItem1ID = t2.RareItem2ID = t2.CommonItem1ID;
                        }
                        else
                        {
                            t.CommonItem1ID = t.CommonItem2ID = t.RareItem1ID = t.RareItem2ID = t.CommonItem2ID;
                            t2.CommonItem1ID = t2.CommonItem2ID = t2.RareItem1ID = t2.RareItem2ID = t2.CommonItem2ID;
                        }
                        t.GilChance = 0;
                        t2.GilChance = 0;
                    }
                }
                else
                {
                    if (t.GilChance > 0 && RandomNum.RandInt(0, 99) < 15)
                    {
                        t.GilChance = 100;
                        t.GilRare = t.GilCommon = (ushort)(t.GilRare * 8);
                        t2.GilChance = 100;
                        t2.GilRare = t2.GilCommon = (ushort)(t2.GilRare * 8);
                    }
                    else
                    {
                        if (RandomNum.RandInt(0, 99) < 50)
                        {
                            t.CommonItem1ID = t.CommonItem2ID = t.RareItem1ID = t.RareItem2ID = t.RareItem1ID;
                            t2.CommonItem1ID = t2.CommonItem2ID = t2.RareItem1ID = t2.RareItem2ID = t2.RareItem1ID;
                        }
                        else
                        {
                            t.CommonItem1ID = t.CommonItem2ID = t.RareItem1ID = t.RareItem2ID = t.RareItem2ID;
                            t2.CommonItem1ID = t2.CommonItem2ID = t2.RareItem1ID = t2.RareItem2ID = t2.RareItem2ID;
                        }
                        t.GilChance = 0;
                        t2.GilChance = 0;
                    }
                }

                if (IsAbility(l.ID))
                {
                    if (t.Respawn == 255 || !usedRespawnIDs.Contains(t.Respawn))
                    {
                        treasuresToPlace.Add(l.ID);
                    }
                }
            });

            foreach (TreasureData l in itemLocations.Values.Where(l => l is TreasureData && !treasuresToPlace.Contains(l.ID)).Select(l => (TreasureData)l).Shuffle())
            {
                DataStoreTreasure t = ebpAreasOrig[((TreasureData)l).MapID].TreasureList[l.Index];
                if (t.Respawn == 255 || !usedRespawnIDs.Contains(t.Respawn))
                {
                    treasuresToPlace.Add(l.ID);
                    if (t.Respawn < 255)
                        usedRespawnIDs.Add(t.Respawn);
                    if (treasuresToPlace.Count == 255)
                        break;
                }
            }

            treasuresAllowed = itemLocations.Values.Where(l => l is TreasureData && !l.Traits.Contains("Missable")).Select(l => l.ID).Shuffle().Take(255).ToList();
        }

        private bool IsEmpty(string id)
        {
            if (itemLocations[id] is TreasureData)
            {
                return !PlacementAlgo.Placement.ContainsKey(id);
            }
            if (itemLocations[id] is RewardData)
            {
                string rewardId = id.Split(":")[0];
                return PlacementAlgo.Placement.Keys.Where(s => s.StartsWith(rewardId)).Count() == 0;
            }
            return false;
        }

        private bool IsExtra(string id)
        {
            if (itemLocations[id] is RewardData)
            {
                string rewardId = id.Split(":")[0];
                return PlacementAlgo.Placement.Keys.Where(s => s.StartsWith(rewardId)).Count() > 1;
            }
            return false;
        }

        public bool IsImportantKeyItem(string location)
        {
            return (IsMainKeyItem(location) || IsSideKeyItem(location) || IsHuntKeyItem(location) || IsGrindyKeyItem(location) || IsBlackOrbKeyItem(location) || IsWoTItem(location) || IsHuntClubKeyItem(location)) && (PlacementAlgo.Logic.GetLocationItem(location) != null || PlacementAlgo.Iterations > PlacementAlgo.Placement.Count * 1.5f + 1) && (PlacementAlgo.Logic.GetLocationItem(location) == null || PlacementAlgo.Logic.GetLocationItem(location).Value.Item1 != "Gil");
        }

        public bool IsAbility(string t, bool orig = true)
        {
            return PlacementAlgo.Logic.GetLocationItem(t, orig) != null && (PlacementAlgo.Logic.GetLocationItem(t, orig).Value.Item1.StartsWith("30") || PlacementAlgo.Logic.GetLocationItem(t, orig).Value.Item1.StartsWith("40"));
        }

        public bool IsWoTItem(string t)
        {
            return PlacementAlgo.ItemLocations[t].Traits.Contains("Writ");
        }

        public bool IsMainKeyItem(string t)
        {
            return PlacementAlgo.ItemLocations[t].Traits.Contains("MainKey");
        }

        public bool IsSideKeyItem(string t)
        {
            return PlacementAlgo.ItemLocations[t].Traits.Contains("SideKey");
        }

        public bool IsHuntKeyItem(string t)
        {
            return PlacementAlgo.ItemLocations[t].Traits.Contains("HuntKey");
        }

        public bool IsGrindyKeyItem(string t)
        {
            return PlacementAlgo.ItemLocations[t].Traits.Contains("GrindyKey");
        }

        public bool IsBlackOrbKeyItem(string t)
        {
            return PlacementAlgo.ItemLocations[t].Traits.Contains("BlackOrb");
        }

        public bool IsHuntClubKeyItem(string t)
        {
            return PlacementAlgo.ItemLocations[t].Traits.Contains("HuntClubKey");
        }

        private List<string> GetRandomizableItems()
        {
            return itemLocations.Values.Where(l => !(l is TreasureData) || treasuresToPlace.Contains(l.ID))
                .Select(l =>
                {
                    (string, int)? tuple = PlacementAlgo.Logic.GetLocationItem(l.ID);
                    if (tuple != null && IsRandomizableItem(tuple.Value.Item1))
                    {
                        return tuple.Value.Item1;
                    }
                    return null;
                }).Where(s => s != null).Distinct().ToList();
        }

        public static bool IsRandomizableItem(string item)
        {
            if (item.StartsWith("10") || item.StartsWith("11"))
                return true;
            if (item.StartsWith("30") || item.StartsWith("40"))
                return true;
            return false;
        }

        public void SaveHints()
        {
            List<string> output = new List<string>();
            for (int i = 0; i < hints.Count; i++)
            {
                output.Add($"[Hint {i + 1}]");
                if (hints[i].Count > 0)
                {
                    foreach (string l in hints[i])
                    {
                        output.AddRange(GetHintText(l));
                    }
                }
                else
                {
                    output.Add("Nothing to hint.");
                }
            }
            File.WriteAllLines($"outdata\\hints.txt", output);
        }


        private List<string> GetHintText(string l)
        {
            List<string> list = new List<string>();
            int index = FF12Flags.Other.HintsSpecific.Values.IndexOf(FF12Flags.Other.HintsSpecific.SelectedValue);
            if (index == FF12Flags.Other.HintsSpecific.Values.Count - 1)
            {
                FF12Flags.Other.HintsMain.SetRand();
                index = RandomNum.RandInt(0, FF12Flags.Other.HintsSpecific.Values.Count - 2);
                RandomNum.ClearRand();
            }
            switch (index)
            {
                case 0:
                default:
                    {
                        list.Add($"{itemLocations[l].Name} has {GetItemName(PlacementAlgo.Logic.GetLocationItem(l, false).Value.Item1)}");
                        list.Add("");
                        break;
                    }
                case 1:
                    {
                        string type = "Other";
                        if (IsMainKeyItem(PlacementAlgo.Placement[l]))
                            type = "a Story Key Item";
                        if (IsSideKeyItem(PlacementAlgo.Placement[l]))
                            type = "a Side Key Item";
                        if (IsHuntKeyItem(PlacementAlgo.Placement[l]))
                            type = "a Hunt Key Item";
                        if (IsGrindyKeyItem(PlacementAlgo.Placement[l]))
                            type = "a Chop";
                        if (IsBlackOrbKeyItem(PlacementAlgo.Placement[l]))
                            type = "a Black Orb";
                        if (IsHuntClubKeyItem(PlacementAlgo.Placement[l]))
                            type = "a Trophy";
                        if (IsWoTItem(PlacementAlgo.Placement[l]))
                            type = "the Writ of Transit";
                        (string, int)? item = PlacementAlgo.Logic.GetLocationItem(l, false);
                        if (item != null && (item.Value.Item1.StartsWith("30") || item.Value.Item1.StartsWith("40")))
                            type = "an Ability";

                        list.Add($"{itemLocations[l].Name} has {type}");
                        list.Add("");
                        break;
                    }
                case 2:
                    {
                        list.Add($"{itemLocations[l].Areas[0]} has {GetItemName(PlacementAlgo.Logic.GetLocationItem(l, false).Value.Item1)}");
                        list.Add("");
                        break;
                    }
                case 3:
                    {
                        list.Add($"{itemLocations[l].Name} has ????");
                        list.Add("");
                        break;
                    }
            }
            return list;
        }

        public void SaveTreasureTracker()
        {
            Dictionary<string, List<int>> areaRespawns = new Dictionary<string, List<int>>();
            IEnumerable<TreasureData> treasures = PlacementAlgo.Placement.Keys.Where(l => itemLocations[l] is TreasureData).Select(l => (TreasureData)itemLocations[l]);
            treasures = treasures.Concat(missableTreasureLinks.Keys.Select(l => (TreasureData)itemLocations[l]));
            foreach (TreasureData l in treasures)
            {
                DataStoreTreasure t = ebpAreas[l.MapID].TreasureList[l.Index];
                if (!areaRespawns.ContainsKey(l.MapID))
                    areaRespawns.Add(l.MapID, new List<int>());
                areaRespawns[l.MapID].Add(t.Respawn);
            }
            File.WriteAllLines($"outdata\\treasureTracker.txt", areaRespawns.Select(p => $"{areaMapping[p.Key]},{String.Join(",", p.Value)}"));
        }

        public override void Save()
        {
            Randomizers.SetUIProgress("Saving Treasure Data...", 0, -1);
            File.WriteAllBytes($"outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_037.bin", rewards.Data);
            File.WriteAllBytes($"outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_028.bin", prices.Data);

            ebpAreas.ForEach(p =>
            {
                string name = p.Key.Split(',')[0];
                string area = name.Substring(0, name.Length - 2);
                File.WriteAllBytes($"outdata\\ps2data\\plan_master\\us\\plan_map\\{area}\\{name}\\global\\{name}.ebp", p.Value.Data);
            });

            SaveHints();
            SaveTreasureTracker();
        }


        public override Dictionary<string, HTMLPage> GetDocumentation()
        {
            PartyRando partyRando = Randomizers.Get<PartyRando>();
            Dictionary<string, HTMLPage> pages = base.GetDocumentation();
            HTMLPage page = new HTMLPage("Item Locations", "template/documentation.html");

            page.HTMLElements.Add(new Table("Item Locations", (new string[] { "Name", "New Contents", "Difficulty" }).ToList(), (new int[] { 45, 45, 10 }).ToList(), itemLocations.Values.Select(l =>
            {
                string display = "";
                if (l is TreasureData)
                {
                    TreasureData t = (TreasureData)l;
                    DataStoreTreasure treasure = ebpAreas[t.MapID].TreasureList[t.Index];
                    if (treasure.SpawnChance == 0)
                        return null;
                    display = GetTreasureDisplay(treasure);
                }
                else if (l is RewardData)
                {
                    RewardData r = (RewardData)l;
                    if (r.Index > 0 || r.Traits.Contains("Fake"))
                        return null;
                    DataStoreReward reward = rewards[r.IntID - 0x9000];
                    display = GetRewardDisplay(reward);
                }
                else if (l is StartingInvData)
                {
                    StartingInvData s = (StartingInvData)l;
                    if (s.Index > 0)
                        return null;
                    DataStorePartyMember chara = partyRando.party[s.IntID];
                    display = GetPartyMemberDisplay(chara);
                }
                else
                    throw new Exception("Unsupported item location type found");
                string reqsDisplay = l.Requirements.GetDisplay(GetItemName);
                if (reqsDisplay.StartsWith("(") && reqsDisplay.EndsWith(")"))
                    reqsDisplay = reqsDisplay.Substring(1, reqsDisplay.Length - 2);

                TableCellMultiple nameCell = new TableCellMultiple(new List<string>());
                nameCell.Elements.Add($"<div style=\"margin-right: auto\">{l.Name}</div>");
                if (reqsDisplay != ItemReq.Empty.GetDisplay())
                    nameCell.Elements.Add(new IconTooltip("common/images/lock_white_48dp.svg", "Requires: " + reqsDisplay).ToString());

                return new object[] { nameCell, display, l.Difficulty.ToString() }.ToList();
            }).Where(l => l != null).ToList(), "itemlocations"));
            pages.Add("item_locations", page);
            return pages;
        }

        public string GetItemName(string id)
        {
            TextRando textRando = Randomizers.Get<TextRando>();
            if (id == "Gil")
                return "Gil";
            try
            {
                ushort intId = Convert.ToUInt16(id, 16);
                if (intId >= 0x3000 && intId < 0x4000)
                    return textRando.TextAbilities[intId - 0x3000].Text;
                if (intId >= 0x4000 && intId < 0x5000)
                    return textRando.TextAbilities[intId - 0x4000 + 158].Text;
                if (intId < 0x1000)
                    return textRando.TextAbilities[intId + 82].Text;
                if (intId >= 0x1000 && intId < 0x2000)
                    return textRando.TextEquipment[intId - 0x1000].Text;
                if (intId >= 0x2000 && intId < 0x3000)
                    return textRando.TextLoot[intId - 0x2000].Text;
                if (intId >= 0x8000 && intId < 0x9000)
                    return textRando.TextKeyItems[intId - 0x8000].Text;
            }
            catch
            {
            }
            return id;
        }

        private string GetRewardDisplay(DataStoreReward reward, bool hintableOnly = false)
        {
            List<string> stringList = new List<string>();
            if (reward.Gil > 0)
                stringList.Add($"{reward.Gil} Gil");
            if (reward.Item1ID != 0xFFFF)
                stringList.Add($"{GetItemName(reward.Item1ID.ToString("X4"))} x {reward.Item1Amount}");
            if (reward.Item2ID != 0xFFFF)
                stringList.Add($"{GetItemName(reward.Item2ID.ToString("X4"))} x {reward.Item2Amount}");
            return String.Join(", ", stringList);
        }

        private string GetPartyMemberDisplay(DataStorePartyMember chara, bool hintableOnly = false)
        {
            List<string> stringList = new List<string>();
            for (int i = 0; i < chara.ItemIDs.Count; i++)
            {
                if (chara.ItemIDs[i] != 0xFFFF)
                    stringList.Add($"{GetItemName(chara.ItemIDs[i].ToString("X4"))} x {chara.ItemAmounts[i]}");
            }
            return String.Join(", ", stringList);
        }

        private string GetTreasureDisplay(DataStoreTreasure treasure, bool hintableOnly = false)
        {
            List<string> stringList = new List<string>();
            if (treasure.GilChance > 0)
                stringList.Add($"{treasure.GilCommon} Gil" + (treasure.GilCommon != treasure.GilRare ? $" or {treasure.GilRare} Gil (with DA)" : ""));
            else
            {
                stringList.Add($"{GetItemName(treasure.CommonItem1ID.ToString("X4"))} x 1" + (treasure.CommonItem1ID != treasure.RareItem1ID ? $" or {GetItemName(treasure.RareItem1ID.ToString("X4"))} x 1 (with DA)" : ""));
            }
            return String.Join(", ", stringList);
        }

        public class TreasureData : ItemLocation
        {
            public override string ID { get; set; }
            public int Index { get; set; }
            public override string Name { get; set; }
            public override string LocationImagePath { get; set; }
            [RowIndex(1)]
            public string Subarea { get; set; }
            [RowIndex(2)]
            public string MapID { get; set; }
            [RowIndex(5)]
            public override ItemReq Requirements { get; set; }
            [RowIndex(7)]
            public override List<string> Traits { get; set; }
            [RowIndex(0)]
            public override List<string> Areas { get; set; }
            [RowIndex(6)]
            public override int Difficulty { get; set; }

            public TreasureData(string[] row, int index) : base(row)
            {
                Name = row[0] + " - " + row[1] + " Treasure";
                ID = row[2] + ":" + index;
                Index = index;
            }

            public override bool IsValid(Dictionary<string, int> items)
            {
                if (!Requirements.IsValid(items))
                    return false;
                return true;
            }

            public override void SetData(dynamic obj, string newItem, int newCount)
            {
                DataStoreTreasure t = (DataStoreTreasure)obj;
                if (newItem == "Gil")
                {
                    t.GilChance = 100;
                    t.GilCommon = (ushort)Math.Min(newCount, 65535);
                    t.GilRare = (ushort)Math.Min(newCount * 2, 65535);
                }
                else
                {
                    ushort id = Convert.ToUInt16(newItem, 16);
                    t.GilChance = 0;
                    t.CommonItem1ID = id;
                    t.CommonItem2ID = id;
                    t.RareItem1ID = id;
                    t.RareItem2ID = id;
                }
            }

            public override (string, int)? GetData(dynamic obj)
            {
                DataStoreTreasure t = (DataStoreTreasure)obj;
                if (t.GilChance == 100)
                {
                    return ("Gil", (int)t.GilCommon);
                }
                else
                {
                    return (t.CommonItem1ID.ToString("X4"), 1);
                }
            }
        }

        public class RewardData : ItemLocation
        {
            public override string ID { get; set; }
            public int IntID { get; set; }
            public int Index { get; set; }
            [RowIndex(1)]
            public override string Name { get; set; }
            public override string LocationImagePath { get; set; }
            [RowIndex(3)]
            public override ItemReq Requirements { get; set; }
            [RowIndex(5)]
            public override List<string> Traits { get; set; }
            [RowIndex(0)]
            public override List<string> Areas { get; set; }
            [RowIndex(4)]
            public override int Difficulty { get; set; }
            [RowIndex(6)]
            public List<string> FakeItems { get; set; }

            public RewardData Parent { get; set; }

            public RewardData(string[] row, int index, int fakeID, bool forceFake = false) : base(row)
            {
                if (forceFake && !Traits.Contains("Fake"))
                    Traits.Add("Fake");
                if (!Traits.Contains("Fake"))
                    IntID = Convert.ToInt32(row[2], 16);
                else
                    IntID = fakeID;
                ID = (forceFake ? "_" : "") + row[2] + ":" + index;
                Index = index;
                Parent = this;
            }

            public override bool IsValid(Dictionary<string, int> items)
            {
                if (!Requirements.IsValid(items))
                    return false;
                return true;
            }

            public override void SetData(dynamic obj, string newItem, int newCount)
            {
                DataStoreReward r = (DataStoreReward)obj;
                if (Index == 0)
                {
                    r.Gil = (uint)newCount;
                }
                else
                {
                    ushort id = Convert.ToUInt16(newItem, 16);
                    if (Index == 1)
                    {
                        r.Item1ID = id;
                        r.Item1Amount = (ushort)newCount;
                    }
                    else if (Index == 2)
                    {
                        r.Item2ID = id;
                        r.Item2Amount = (ushort)newCount;
                    }
                }
            }

            public override (string, int)? GetData(dynamic obj)
            {
                DataStoreReward r = (DataStoreReward)obj;
                if (Index == 0)
                {
                    if (r.Gil == 0)
                        return null;
                    return ("Gil", (int)r.Gil);
                }
                else if (Index == 1)
                {
                    if (r.Item1ID == 0xFFFF)
                        return null;
                    return (r.Item1ID.ToString("X4"), (int)r.Item1Amount);
                }
                else
                {
                    if (r.Item2ID == 0xFFFF)
                        return null;
                    return (r.Item2ID.ToString("X4"), (int)r.Item2Amount);
                }
            }
        }

        public class StartingInvData : ItemLocation
        {
            public override string ID { get; set; }
            [RowIndex(2), FieldTypeOverride(FieldType.HexInt)]
            public int IntID { get; set; }
            public int Index { get; set; }
            [RowIndex(1)]
            public override string Name { get; set; }
            public override string LocationImagePath { get; set; }
            [RowIndex(3)]
            public override ItemReq Requirements { get; set; }
            [RowIndex(5)]
            public override List<string> Traits { get; set; }
            [RowIndex(0)]
            public override List<string> Areas { get; set; }
            [RowIndex(4)]
            public override int Difficulty { get; set; }

            public StartingInvData(string[] row, int index) : base(row)
            {
                ID = row[2] + "::" + index;
                Index = index;
            }

            public override bool IsValid(Dictionary<string, int> items)
            {
                if (!Requirements.IsValid(items))
                    return false;
                return true;
            }

            public override void SetData(dynamic obj, string newItem, int newCount)
            {
                DataStorePartyMember c = (DataStorePartyMember)obj;
                List<ushort> itemIDs = c.ItemIDs;
                List<byte> itemAmounts = c.ItemAmounts;

                ushort id = Convert.ToUInt16(newItem, 16);
                itemIDs[Index] = id;
                itemAmounts[Index] = (byte)newCount;

                c.ItemIDs = itemIDs;
                c.ItemAmounts = itemAmounts;
            }

            public override (string, int)? GetData(dynamic obj)
            {
                DataStorePartyMember c = (DataStorePartyMember)obj;
                return (c.ItemIDs[Index].ToString("X4"), (int)c.ItemAmounts[Index]);
            }
        }
    }
}

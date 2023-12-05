using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FF12Rando;

public class TreasureRando : Randomizer
{
    public DataStoreBPSection<DataStoreReward> rewards, rewardsOrig;
    public DataStoreBPSection<DataStorePrice> prices;
    public Dictionary<string, DataStoreEBP> ebpAreas = new();
    public Dictionary<string, DataStoreEBP> ebpAreasOrig = new();

    public List<List<string>> hints = Enumerable.Range(0, 35).Select(_ => new List<string>()).ToList();

    public Dictionary<string, string> areaMapping = new();
    public Dictionary<string, ItemLocation> ItemLocations = new();
    public Dictionary<string, string> missableTreasureLinks = new();
    private FF12AssumedItemPlacementAlgorithm placementAlgoNormal;
    private ItemPlacementAlgorithm<ItemLocation> placementAlgoBackup;
    private bool usingBackup = false;
    public ItemPlacementAlgorithm<ItemLocation> PlacementAlgo => usingBackup ? placementAlgoBackup : placementAlgoNormal;

    public List<string> treasuresToPlace = new();
    public List<string> treasuresAllowed = new();

    public List<string> randomizeItems = new();
    public List<string> remainingRandomizeItems = new();

    public Dictionary<string, int> LocationSpheres { get; set; } = new();

    public TreasureRando(SeedGenerator randomizers) : base(randomizers) { }

    public static int FakeId = -1;

    // Used for grouping/sorting by type
    public enum ItemType
    {
        Consumable, // 0x0000 to 0x0FFF
        Equipment, // 0x1000 to 0x1FFF
        Ability // 0x3000 to 0x4FFF
    }

    public override void Load()
    {
        Generator.SetUIProgress("Loading Treasure Data...", 0, -1);
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
            int offset = int.Parse(s.Split(',')[1]);
            int count = int.Parse(s.Split(',')[2]);
            string path = $"data\\ps2data\\plan_master\\us\\plan_map\\{area}\\{name}\\global\\{name}.ebp";
            DataStoreEBP ebp = new(offset, count, path);
            ebp.LoadData(File.ReadAllBytes(path));
            return ebp;
        });
        ebpAreasOrig = File.ReadAllLines("data\\treasureAddresses.csv").ToDictionary(s => s.Split(',')[0], s =>
        {
            string name = s.Split(',')[0];
            string area = name.Substring(0, name.Length - 2);
            int offset = int.Parse(s.Split(',')[1]);
            int count = int.Parse(s.Split(',')[2]);
            string path = $"data\\ps2data\\plan_master\\us\\plan_map\\{area}\\{name}\\global\\{name}.ebp";
            DataStoreEBP ebp = new(offset, count, path);
            ebp.LoadData(File.ReadAllBytes(path));
            return ebp;
        });

        PartyRando partyRando = Generator.Get<PartyRando>();
        partyRando.Characters = MathHelpers.DecodeNaturalSequence(prices[0x76].Price, 6, 6).Select(l => (int)l).ToArray();

        areaMapping = File.ReadAllLines("data\\mapAreas.csv").ToDictionary(s => s.Split(',')[1], s => s.Split(',')[0]);

        ItemLocations.Clear();

        FileHelpers.ReadCSVFile(@"data\treasures.csv", row =>
        {
            int start = int.Parse(row[3]);
            int count = int.Parse(row[4]);
            for (int i = start; i < start + count; i++)
            {
                TreasureData t = new(row, i);
                ItemLocations.Add(t.ID, t);
            }
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\rewards.csv", row =>
        {
            RewardData parent = null;
            for (int i = 0; i < 3; i++)
            {
                RewardData r = new(row, i, FakeId);
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
                    {
                        FakeId--;
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (r.FakeItems.Count > 0)
                {
                    if (i == 0)
                    {
                        RewardData rFake = new(row, i, FakeId, true);
                        FakeId--;
                        ItemLocations.Add(rFake.ID, rFake);
                        parent = rFake;
                        r.Parent = parent;
                    }

                    r.FakeItems.Clear();
                }

                ItemLocations.Add(r.ID, r);
            }
        }, FileHelpers.CSVFileHeader.HasHeader);

        if (FF12Flags.Items.KeyStartingInv.Enabled)
        {
            FileHelpers.ReadCSVFile(@"data\startingInvs.csv", row =>
            {
                StartingInvData first = new(row, 0);
                ItemLocations.Add(first.ID, first);
                // Keep the last slot empty for tp stones
                for (int i = 1; i < 10; i++)
                {
                    StartingInvData s = new(row, i);
                    ItemLocations.Add(s.ID, s);
                }
            }, FileHelpers.CSVFileHeader.HasHeader);
        }

        List<string> hintsNotesLocations = ItemLocations.Values.SelectMany(l => l.Areas).Distinct().ToList();

        placementAlgoNormal = new FF12AssumedItemPlacementAlgorithm(ItemLocations, hintsNotesLocations, Generator, 3)
        {
            SetProgressFunc = Generator.SetUIProgress
        };
        placementAlgoNormal.Logic = new FF12ItemPlacementLogic(placementAlgoNormal, Generator);

        placementAlgoBackup = new ItemPlacementAlgorithm<ItemLocation>(ItemLocations, hintsNotesLocations, Generator, -1)
        {
            SetProgressFunc = Generator.SetUIProgress
        };
        placementAlgoBackup.Logic = new FF12ItemPlacementLogic(placementAlgoBackup, Generator);
    }
    public override void Randomize()
    {
        Generator.SetUIProgress("Randomizing Treasure Data...", 0, -1);

        randomizeItems = new List<string>();
        if (FF12Flags.Items.Treasures.FlagEnabled)
        {
            randomizeItems = randomizeItems.Concat(GetRandomizableItems()).Distinct().ToList();
        }

        ShopRando shopRando = Generator.Get<ShopRando>();
        if (FF12Flags.Items.Shops.FlagEnabled)
        {
            randomizeItems = randomizeItems.Concat(shopRando.GetRandomizableShopItems()).Distinct().ToList();
        }

        if (FF12Flags.Items.Bazaars.FlagEnabled)
        {
            randomizeItems = randomizeItems.Concat(shopRando.GetRandomizableBazaarItems()).Distinct().ToList();
        }

        remainingRandomizeItems = new List<string>(randomizeItems);

        if (FF12Flags.Items.Treasures.FlagEnabled)
        {
            FF12Flags.Items.Treasures.SetRand();

            CollapseAndSelectTreasures();

            Dictionary<string, double> areaMults = ItemLocations.Values.SelectMany(t => t.Areas).Distinct().ToDictionary(s => s, _ => RandomNum.RandInt(10, 200) * 0.01d);

            // Backward algorithm does not well yet for this game
            usingBackup = true;
            placementAlgoBackup.Randomize(new List<string>(), areaMults);

            Generator.SetUIProgress("Filling empty and missable locations...", 30, 100);
            int respawnIndex = 0;
            ItemLocations.Values.Shuffle().ForEach(l =>
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

                    if (l is TreasureData data)
                    {
                        DataStoreTreasure t = ebpAreas[data.MapID].TreasureList[data.Index];
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

            Generator.SetUIProgress("Randomizing \"junk\" items...", 60, 100);
            EquipRando equipRando = Generator.Get<EquipRando>();
            ItemLocations.Values.ForEach(l =>
            {
                if (PlacementAlgo.Placement.ContainsKey(l.ID))
                {
                    (string, int)? tuple = PlacementAlgo.Logic.GetLocationItem(l.ID, false);
                    if (tuple != null && randomizeItems.Contains(tuple.Value.Item1))
                    {
                        string newItem = RandomNum.SelectRandomWeighted(remainingRandomizeItems, item =>
                        {
                            return item == "2000"
                                ? 25
                                : item.StartsWith("00") || item.StartsWith("20") || item.StartsWith("21")
                                ? 8
                                : (item.StartsWith("30") || item.StartsWith("40")) && ItemLocations[l.ID].Traits.Contains("Missable")
                                ? 0
                                : item.StartsWith("30") || item.StartsWith("40") ? 1 : 3;
                        });
                        if (newItem.StartsWith("30") || newItem.StartsWith("40"))
                        {
                            PlacementAlgo.Logic.SetLocationItem(l.ID, newItem, 1);
                            if (!tuple.Value.Item1.StartsWith("30") && !tuple.Value.Item1.StartsWith("40"))
                            {
                                hints.Where(list => list.Count == hints.Select(list => list.Count).Min()).First().Add(l.ID);
                            }
                        }
                        else
                        {
                            PlacementAlgo.Logic.SetLocationItem(l.ID, newItem, tuple.Value.Item2);
                            if (tuple.Value.Item1.StartsWith("30") || tuple.Value.Item1.StartsWith("40"))
                            {
                                hints.Where(list => list.Contains(l.ID)).ForEach(list => list.Remove(l.ID));
                            }

                            if (l is TreasureData treasure && equipRando.itemData.ContainsKey(newItem) && equipRando.itemData[newItem].Upgrade != "")
                            {
                                ebpAreas[treasure.MapID].TreasureList[treasure.Index].RareItem1ID = (ushort)equipRando.itemData[newItem].IntUpgrade;
                                ebpAreas[treasure.MapID].TreasureList[treasure.Index].RareItem2ID = (ushort)equipRando.itemData[newItem].IntUpgrade;
                            }
                        }

                        if (ShouldRemoveItem(newItem))
                        {
                            remainingRandomizeItems.Remove(newItem);
                        }
                    }
                }
            });

            List<ItemLocation> writLocations = ItemLocations.Values.Where(l => PlacementAlgo.Logic.GetLocationItem(l.ID, false)?.Item1 == "8070").ToList();

            writLocations.ForEach(l =>
            {
                PlacementAlgo.Logic.SetLocationItem(l.ID, "0000", 5);
                hints.Where(list => list.Contains(l.ID)).ForEach(list => list.Remove(l.ID));
            });

            if (FF12Flags.Items.WritGoals.SelectedValues.Contains(FF12Flags.Items.WritGoalCid2))
            {
                ItemLocation cid2 = writLocations.First(l => l.Traits.Contains("WritCid2"));
                PlacementAlgo.Logic.SetLocationItem(cid2.ID, "8070", 1);
                AddHint(cid2.ID);
                writLocations.Remove(cid2);
            }

            if (FF12Flags.Items.WritGoals.SelectedValues.Contains(FF12Flags.Items.WritGoalAny))
            {
                ItemLocation l = RandomNum.SelectRandom(writLocations);
                PlacementAlgo.Logic.SetLocationItem(l.ID, "8070", 1);
                AddHint(l.ID);
            }

            missableTreasureLinks.ForEach(p =>
            {
                (string, int)? item = PlacementAlgo.Logic.GetLocationItem(p.Value, false);
                PlacementAlgo.Logic.SetLocationItem(p.Key, item.Value.Item1, item.Value.Item2);
                DataStoreTreasure tMiss = ebpAreas[((TreasureData)ItemLocations[p.Key]).MapID].TreasureList[((TreasureData)ItemLocations[p.Key]).Index];
                DataStoreTreasure tLinked = ebpAreas[((TreasureData)ItemLocations[p.Value]).MapID].TreasureList[((TreasureData)ItemLocations[p.Value]).Index];
                tMiss.Respawn = tLinked.Respawn;
                tMiss.SpawnChance = 100;
            });

            // Clear empty items
            ItemLocations.Values.ForEach(l =>
            {
                if (!PlacementAlgo.Placement.ContainsKey(l.ID))
                {
                    if (l is TreasureData treasure)
                    {
                        DataStoreTreasure t = ebpAreas[treasure.MapID].TreasureList[treasure.Index];
                        t.Respawn = 255;
                        t.SpawnChance = 0;
                    }

                    if (PlacementAlgo.Logic.GetKeysAllowed().Contains(l.ID))
                    {
                        if (l is RewardData)
                        {
                            PlacementAlgo.Logic.SetLocationItem(l.ID, null, 0);
                        }
                        else if (l is StartingInvData)
                        {
                            PlacementAlgo.Logic.SetLocationItem(l.ID, null, 0);
                        }
                        else
                        {
                            throw new Exception("Missing implemention for clearing");
                        }
                    }
                }
            });

            RandomNum.ClearRand();
        }

        // Generate spheres and anything based on it
        Generator.SetUIProgress("Calculating spheres...", 70, 100);
        LocationSpheres = SphereCalculator.CalculateSpheres(PlacementAlgo.Logic);

        if (FF12Flags.Items.Treasures.FlagEnabled)
        {
            FF12Flags.Items.Treasures.SetRand();

            if (FF12Flags.Items.WritGoals.SelectedValues.Contains(FF12Flags.Items.WritGoalMaxSphere))
            {
                int sphere = LocationSpheres.Values.Max();
                bool placed = false;
                while (!placed)
                {
                    List<ItemLocation> maxSphere = ItemLocations.Values.Where(l => PlacementAlgo.Placement.ContainsKey(l.ID)).Where(l =>
                    {
                        ItemLocation orig = ItemLocations[PlacementAlgo.Placement[l.ID]];
                        return LocationSpheres.GetValueOrDefault(l.ID, 0) == sphere
                                && !l.Traits.Contains("Fake")
                                && !l.Traits.Contains("Missable")
                                && !IsKeyItem(orig.ID)
                                && !IsAbility(orig.ID)
                                && PlacementAlgo.Logic.GetLocationItem(l.ID, false)?.Item1 != "8070"
                                && PlacementAlgo.Logic.GetLocationItem(l.ID, false)?.Item1 != "Gil";
                    }).ToList();

                    if (maxSphere.Count > 0)
                    {
                        ItemLocation l = RandomNum.SelectRandom(maxSphere);
                        PlacementAlgo.Logic.SetLocationItem(l.ID, "8070", 1);
                        AddHint(l.ID);
                        placed = true;
                    }
                    else
                    {
                        sphere--;
                    }
                }
            }

            Generator.SetUIProgress("Reordering junk items...", 80, 100);
            if (FF12Flags.Items.JunkRankScale.Enabled)
            {
                // Get all the locations with consumables, equipment, and abilities and group by their item type
                var grouping = ItemLocations.Values.Where(l =>
                {
                    string id = PlacementAlgo.Logic.GetLocationItem(l.ID, false)?.Item1;
                    if (id == null)
                    {
                        return false;
                    }

                    int intId;
                    try
                    {
                        intId = Convert.ToInt32(id, 16);
                    }
                    catch
                    {
                        return false;
                    }

                    return intId < 0x2000 || intId is >= 0x3000 and < 0x5000;
                }).GroupBy(l =>
                {
                    string id = PlacementAlgo.Logic.GetLocationItem(l.ID, false)?.Item1;
                    int intId = Convert.ToInt32(id, 16);
                    return intId < 0x1000 ? ItemType.Consumable : intId is >= 0x3000 and < 0x5000 ? ItemType.Ability : ItemType.Equipment;
                });

                // Group by type and sort the items by its item rank
                EquipRando equipRando = Generator.Get<EquipRando>();
                foreach (var group in grouping)
                {
                    List<(string id, int count)> items = group.Shuffle().Select(l => PlacementAlgo.Logic.GetLocationItem(l.ID, false).Value).OrderBy(pair =>
                    {
                        return equipRando.itemData[pair.Item1].Rank;
                    }).ToList();
                    items = RandomNum.ShuffleLocalized(items, 5);

                    // Sort the junk locations by sphere
                    List<ItemLocation> junk = group.Shuffle().OrderBy(l => LocationSpheres.GetValueOrDefault(l.ID, 0)).ToList();

                    // Go in order and set the junk items
                    for (int i = 0; i < items.Count; i++)
                    {
                        (string id, int count) = items[i];
                        PlacementAlgo.Logic.SetLocationItem(junk[i].ID, id, count);
                    }
                }
            }

            RandomNum.ClearRand();
        }

        if (!FF12Flags.Items.AllowSeitengrat.FlagEnabled)
        {
            // Replace any Seitengrat with the Dhanusha
            ItemLocations.Values.Where(l => PlacementAlgo.Logic.GetLocationItem(l.ID, false)?.Item1 == "10B2").ForEach(l =>
            {
                PlacementAlgo.Logic.SetLocationItem(l.ID, "10C7", PlacementAlgo.Logic.GetLocationItem(l.ID, false).Value.Item2);
            });
        }
    }

    public int AddHint(string location)
    {
        int index = Enumerable.Range(0, hints.Count).First(i => hints[i].Count == hints.Select(l => l.Count).Min());
        hints[index].Add(location);
        return index;
    }

    private bool ShouldRemoveItem(string newItem)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        return !newItem.StartsWith("00") && !newItem.StartsWith("20") && !newItem.StartsWith("21")
&& (newItem.StartsWith("30") || newItem.StartsWith("40")
|| !equipRando.itemData.ContainsKey(newItem) || RandomNum.RandInt(0, 100) < Math.Pow(equipRando.itemData[newItem].Rank, 2));
    }

    private void CollapseAndSelectTreasures()
    {
        treasuresToPlace.Clear();
        List<int> usedRespawnIDs = new();
        ItemLocations.Values.Where(l => l is TreasureData).Select(l => (TreasureData)l).ForEach(l =>
        {
            DataStoreTreasure t = ebpAreasOrig[l.MapID].TreasureList[l.Index];
            DataStoreTreasure t2 = ebpAreas[l.MapID].TreasureList[l.Index];
            if (RandomNum.RandInt(0, 99) < 75)
            {
                if (t.GilChance > 0 && RandomNum.RandInt(0, 99) < 60)
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

        foreach (TreasureData l in ItemLocations.Values.Where(l => l is TreasureData && !treasuresToPlace.Contains(l.ID)).Select(l => (TreasureData)l).Shuffle())
        {
            DataStoreTreasure t = ebpAreasOrig[l.MapID].TreasureList[l.Index];
            if (t.Respawn == 255 || !usedRespawnIDs.Contains(t.Respawn))
            {
                treasuresToPlace.Add(l.ID);
                if (t.Respawn < 255)
                {
                    usedRespawnIDs.Add(t.Respawn);
                }

                if (treasuresToPlace.Count == 255)
                {
                    break;
                }
            }
        }

        treasuresAllowed = ItemLocations.Values.Where(l => l is TreasureData && !l.Traits.Contains("Missable")).Select(l => l.ID).Shuffle().Take(255).ToList();
    }

    private bool IsEmpty(string id)
    {
        if (ItemLocations[id] is TreasureData)
        {
            return !PlacementAlgo.Placement.ContainsKey(id);
        }

        if (ItemLocations[id] is RewardData)
        {
            string rewardId = id.Split(":")[0];
            return PlacementAlgo.Placement.Keys.Where(s => s.StartsWith(rewardId)).Count() == 0;
        }

        return false;
    }

    private bool IsExtra(string id)
    {
        if (ItemLocations[id] is RewardData)
        {
            string rewardId = id.Split(":")[0];
            return PlacementAlgo.Placement.Keys.Where(s => s.StartsWith(rewardId)).Count() > 1;
        }

        return false;
    }

    public bool IsImportantKeyItem(string location)
    {
        return IsKeyItem(location) && (PlacementAlgo.Logic.GetLocationItem(location) != null || PlacementAlgo.Iterations > (PlacementAlgo.Placement.Count * 1.5f) + 1) && (PlacementAlgo.Logic.GetLocationItem(location) == null || PlacementAlgo.Logic.GetLocationItem(location).Value.Item1 != "Gil");
    }

    public bool IsAbility(string t, bool orig = true)
    {
        return PlacementAlgo.Logic.GetLocationItem(t, orig) != null && (PlacementAlgo.Logic.GetLocationItem(t, orig).Value.Item1.StartsWith("30") || PlacementAlgo.Logic.GetLocationItem(t, orig).Value.Item1.StartsWith("40"));
    }

    public bool IsWoT(string t, bool orig = true)
    {
        return PlacementAlgo.Logic.GetLocationItem(t, orig) != null && PlacementAlgo.Logic.GetLocationItem(t, orig).Value.Item1 == "8070";
    }

    public bool IsChopKeyItem(string t)
    {
        return PlacementAlgo.ItemLocations[t].Traits.Any(s => s.StartsWith("Chop"));
    }

    public bool IsBlackOrbKeyItem(string t)
    {
        return PlacementAlgo.ItemLocations[t].Traits.Any(s => s.StartsWith("BlackOrb"));
    }

    public bool IsKeyItem(string t)
    {
        return PlacementAlgo.Logic.GetLocationItem(t) != null && (FF12Flags.Items.KeyItems.DictValues.Keys.Contains(PlacementAlgo.Logic.GetLocationItem(t)?.Item1) || IsChopKeyItem(t) || IsBlackOrbKeyItem(t));
    }

    private List<string> GetRandomizableItems()
    {
        return ItemLocations.Values.Where(l => l is not TreasureData || treasuresToPlace.Contains(l.ID))
            .Select(l =>
            {
                (string, int)? tuple = PlacementAlgo.Logic.GetLocationItem(l.ID);
                return tuple != null && IsRandomizableItem(tuple.Value.Item1) ? tuple.Value.Item1 : null;
            }).Where(s => s != null).Distinct().ToList();
    }

    public static bool IsRandomizableItem(string item)
    {
        return item.StartsWith("10") || item.StartsWith("11") || item.StartsWith("30") || item.StartsWith("40");
    }

    public void SaveHints()
    {
        TextRando textRando = Generator.Get<TextRando>();
        if (hints.Select(l => l.Count).Sum() > 0)
        {
            for (int i = 0; i < hints.Count; i++)
            {
                List<string> lines = new();
                if (hints[i].Count > 0)
                {
                    foreach (string l in hints[i])
                    {
                        lines.Add(GetHintText(l));
                    }
                }
                else
                {
                    lines.Add("There is nothing left to hint.");
                }

                textRando.TextKeyDescriptions[352 + i].Text = string.Join("\n", lines);
            }
        }
    }

    private string GetHintText(string l)
    {
        string val;
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
                    val = $"{ItemLocations[l].Name} has {GetItemName(PlacementAlgo.Logic.GetLocationItem(l, false)?.Item1)}";
                    break;
                }
            case 1:
                {

                    string type = "Other";
                    if (IsKeyItem(PlacementAlgo.Placement[l]))
                    {
                        type = "a Unique Key Item";
                    }

                    if (IsChopKeyItem(PlacementAlgo.Placement[l]))
                    {
                        type = "a Chop";
                    }

                    if (IsBlackOrbKeyItem(PlacementAlgo.Placement[l]))
                    {
                        type = "a Black Orb";
                    }

                    (string, int)? item = PlacementAlgo.Logic.GetLocationItem(l, false);
                    int intId = -1;
                    if (item != null)
                    {
                        try
                        {
                            intId = Convert.ToInt32(item.Value.Item1, 16);
                        }
                        catch { }
                    }

                    if (item?.Item1 == "8070")
                    {
                        type = "a Writ of Transit";
                    }

                    if (intId is >= 0x80B9 and <= 0x80D6)
                    {
                        type = "a Useless Trophy";
                    }

                    if (item != null && (item.Value.Item1.StartsWith("30") || item.Value.Item1.StartsWith("40")))
                    {
                        type = "an Ability";
                    }

                    val = $"{ItemLocations[l].Name} has {type}";
                    break;
                }
            case 2:
                {
                    val = $"{ItemLocations[l].Areas[0]} has {GetItemName(PlacementAlgo.Logic.GetLocationItem(l, false)?.Item1)}";
                    break;
                }
            case 3:
                {
                    val = $"{ItemLocations[l].Name} has ????";
                    break;
                }
        }

        return val;
    }

    public void SaveTreasureTracker()
    {
        Dictionary<string, List<int>> areaRespawns = new();
        IEnumerable<TreasureData> treasures = PlacementAlgo.Placement.Keys.Where(l => ItemLocations[l] is TreasureData).Select(l => (TreasureData)ItemLocations[l]);
        treasures = treasures.Concat(missableTreasureLinks.Keys.Select(l => (TreasureData)ItemLocations[l]));
        foreach (TreasureData l in treasures)
        {
            DataStoreTreasure t = ebpAreas[l.MapID].TreasureList[l.Index];
            if (!areaRespawns.ContainsKey(l.MapID))
            {
                areaRespawns.Add(l.MapID, new List<int>());
            }

            areaRespawns[l.MapID].Add(t.Respawn);
        }

        File.WriteAllLines($"{Generator.OutFolder}\\treasureTracker.txt", areaRespawns.Select(p => $"{areaMapping[p.Key]},{string.Join(",", p.Value)}"));
    }

    public override void Save()
    {
        Generator.SetUIProgress("Saving Treasure Data...", 0, -1);
        File.WriteAllBytes($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_037.bin", rewards.Data);
        File.WriteAllBytes($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_028.bin", prices.Data);

        ebpAreas.ForEach(p =>
        {
            string name = p.Key.Split(',')[0];
            string area = name.Substring(0, name.Length - 2);
            File.WriteAllBytes($"{Generator.DataOutFolder}\\plan_master\\us\\plan_map\\{area}\\{name}\\global\\{name}.ebp", p.Value.Data);
        });

        SaveHints();
        SaveTreasureTracker();
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        PartyRando partyRando = Generator.Get<PartyRando>();
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        HTMLPage page = new("Item Locations", "template/documentation.html");

        page.HTMLElements.Add(new Table("Item Locations", (new string[] { "Name", "New Contents", "Sphere" }).ToList(), (new int[] { 45, 45, 10 }).ToList(), ItemLocations.Values.Select(l =>
        {
            string display = "";
            if (l is TreasureData t)
            {
                DataStoreTreasure treasure = ebpAreas[t.MapID].TreasureList[t.Index];
                if (treasure.SpawnChance == 0)
                {
                    return null;
                }

                display = GetTreasureDisplay(treasure);
            }
            else if (l is RewardData r)
            {
                if (r.Index > 0 || r.Traits.Contains("Fake"))
                {
                    return null;
                }

                DataStoreReward reward = rewards[r.IntID - 0x9000];
                display = GetRewardDisplay(reward);
            }
            else if (l is StartingInvData s)
            {
                if (s.Index > 0)
                {
                    return null;
                }

                DataStorePartyMember chara = partyRando.party[s.IntID];
                display = GetPartyMemberDisplay(chara);
            }
            else
            {
                throw new Exception("Unsupported item location type found");
            }

            string reqsDisplay = l.Requirements.GetDisplay(GetItemName);
            if (reqsDisplay.StartsWith("(") && reqsDisplay.EndsWith(")"))
            {
                reqsDisplay = reqsDisplay.Substring(1, reqsDisplay.Length - 2);
            }

            TableCellMultiple nameCell = new(new List<string>());
            nameCell.Elements.Add($"<div style=\"margin-right: auto\">{l.Name}</div>");
            if (reqsDisplay != ItemReq.TRUE.GetDisplay())
            {
                nameCell.Elements.Add(new IconTooltip("common/images/lock_white_48dp.svg", "Requires: " + reqsDisplay).ToString());
            }

            return new object[] { nameCell, display, LocationSpheres.ContainsKey(l.ID) ? LocationSpheres[l.ID] : "N/A" }.ToList();
        }).Where(l => l != null).ToList(), "itemlocations"));
        pages.Add("item_locations", page);

        // Add hints page
        page = new("Hints", "template/documentation.html");
        page.HTMLElements.Add(new Table("Hints", (new string[] { "Hint" }).ToList(), (new int[] { 100 }).ToList(), hints.Select(list => new object[] { string.Join("\n", list.Select(line => GetHintText(line))) }.ToList()).ToList(), "hints"));
        pages.Add("hints", page);

        return pages;
    }

    public string GetItemName(string id)
    {
        return GetItemName(id, true);
    }

    public string GetItemName(string id, bool removeFormatting = true)
    {
        TextRando textRando = Generator.Get<TextRando>();
        if (id == "Gil")
        {
            return "Gil";
        }

        ushort intId;
        try
        {
            intId = Convert.ToUInt16(id, 16);
        }
        catch
        {
            return id;
        }

        string output = "";
        if (intId is >= 0x3000 and < 0x4000)
        {
            if (!textRando.TextAbilities.Keys.Contains(intId - 0x3000))
            {
                return "Unknown Magick";
            }

            output = textRando.TextAbilities[intId - 0x3000].Text;
        }

        if (intId is >= 0x4000 and < 0x5000)
        {
            if (!textRando.TextAbilities.Keys.Contains(intId - 0x4000 + 158))
            {
                return "Unknown Technick";
            }

            output = textRando.TextAbilities[intId - 0x4000 + 158].Text;
        }

        if (intId < 0x1000)
        {
            if (!textRando.TextAbilities.Keys.Contains(intId + 82))
            {
                return "Unknown Consumable";
            }

            output = textRando.TextAbilities[intId + 82].Text;
        }

        if (intId is >= 0x1000 and < 0x2000)
        {
            if (!textRando.TextEquipment.Keys.Contains(intId - 0x1000))
            {
                return "Unknown Equipment";
            }

            output = textRando.TextEquipment[intId - 0x1000].Text;
        }

        if (intId is >= 0x2000 and < 0x3000)
        {
            if (!textRando.TextLoot.Keys.Contains(intId - 0x2000))
            {
                return "Unknown Loot";
            }

            output = textRando.TextLoot[intId - 0x2000].Text;
        }

        if (intId is >= 0x8000 and < 0x9000)
        {
            if (!textRando.TextLoot.Keys.Contains(intId - 0x8000))
            {
                return "Unknown Key Item";
            }

            output = textRando.TextKeyItems[intId - 0x8000].Text;
        }
        
        if (!string.IsNullOrEmpty(output))
        {
            if (removeFormatting)
            {
                // Remove tags within {}
                output = Regex.Replace(output, @"{.*?}", "");
            }

            return output;
        }

        return id;
    }

    private string GetRewardDisplay(DataStoreReward reward, bool hintableOnly = false)
    {
        List<string> stringList = new();
        if (reward.Gil > 0)
        {
            stringList.Add($"{reward.Gil} Gil");
        }

        if (reward.Item1ID != 0xFFFF)
        {
            stringList.Add($"{GetItemName(reward.Item1ID.ToString("X4"))} x {reward.Item1Amount}");
        }

        if (reward.Item2ID != 0xFFFF)
        {
            stringList.Add($"{GetItemName(reward.Item2ID.ToString("X4"))} x {reward.Item2Amount}");
        }

        return string.Join(", ", stringList);
    }

    private string GetPartyMemberDisplay(DataStorePartyMember chara, bool hintableOnly = false)
    {
        List<string> stringList = new();
        for (int i = 0; i < chara.ItemIDs.Count; i++)
        {
            if (chara.ItemIDs[i] != 0xFFFF)
            {
                stringList.Add($"{GetItemName(chara.ItemIDs[i].ToString("X4"))} x {chara.ItemAmounts[i]}");
            }
        }

        return string.Join(", ", stringList);
    }

    private string GetTreasureDisplay(DataStoreTreasure treasure, bool hintableOnly = false)
    {
        List<string> stringList = new();
        if (treasure.GilChance > 0)
        {
            stringList.Add($"{treasure.GilCommon} Gil" + (treasure.GilCommon != treasure.GilRare ? $" or {treasure.GilRare} Gil (with DA)" : ""));
        }
        else
        {
            stringList.Add($"{GetItemName(treasure.CommonItem1ID.ToString("X4"))} x 1" + (treasure.CommonItem1ID != treasure.RareItem1ID ? $" or {GetItemName(treasure.RareItem1ID.ToString("X4"))} x 1 (with DA)" : ""));
        }

        return string.Join(", ", stringList);
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
        public override int BaseDifficulty { get; set; }

        public TreasureData(string[] row, int index) : base(row)
        {
            Name = row[0] + " - " + row[1] + " Treasure";
            ID = row[2] + ":" + index;
            Index = index;
        }

        public override bool IsValid(Dictionary<string, int> items)
        {
            return Requirements.IsValid(items);
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
            return t.GilChance == 100 ? ((string, int)?)("Gil", t.GilCommon) : (t.CommonItem1ID.ToString("X4"), 1);
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
        public override int BaseDifficulty { get; set; }
        [RowIndex(6)]
        public List<string> FakeItems { get; set; }

        public bool IsFakeOnly { get; set; }

        public RewardData Parent { get; set; }

        public RewardData(string[] row, int index, int fakeID, bool isAttachedFake = false) : base(row)
        {
            if (isAttachedFake && !Traits.Contains("Fake"))
            {
                Traits.Add("Fake");
            }
            else if (!isAttachedFake && Traits.Contains("Fake"))
            {
                // Fake only rewards are explicitly defined as Fake, attached fakes are added automatically
                IsFakeOnly = true;
            }    

            IntID = !Traits.Contains("Fake") ? Convert.ToInt32(row[2], 16) : fakeID;
            ID = (isAttachedFake ? "_" : "") + row[2] + ":" + index;
            Index = index;
            Parent = this;

            if (Traits.Contains("WritTomaj"))
            {
                if (Index != 1)
                {
                    Traits.Remove("WritTomaj");
                }
                else
                {
                    Traits.Remove("MainKey");
                }
            }

            if (Traits.Contains("WritCid2") && Index != 1)
            {
                Traits.Remove("WritCid2");
            }
        }

        public override bool IsValid(Dictionary<string, int> items)
        {
            return Requirements.IsValid(items);
        }

        public override void SetData(dynamic obj, string newItem, int newCount)
        {
            DataStoreReward r = (DataStoreReward)obj;
            if (Index == 0)
            {
                if (newItem == null)
                {
                    r.Gil = 0;                    
                }
                else
                {
                    r.Gil = (uint)newCount;
                }
            }
            else
            {
                if (newItem == null || newItem == "FFFF")
                {
                    if (Index == 1)
                    {
                        r.Item1ID = 0xFFFF;
                        r.Item1Amount = 255;
                    }
                    else if (Index == 2)
                    {
                        r.Item2ID = 0xFFFF;
                        r.Item2Amount = 255;
                    }
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
        }

        public override (string, int)? GetData(dynamic obj)
        {
            DataStoreReward r = (DataStoreReward)obj;
            return Index == 0
                ? r.Gil == 0 ? null : ("Gil", (int)r.Gil)
                : Index == 1
                    ? r.Item1ID == 0xFFFF ? null : (r.Item1ID.ToString("X4"), r.Item1Amount)
                    : r.Item2ID == 0xFFFF ? null : (r.Item2ID.ToString("X4"), r.Item2Amount);
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
        public override int BaseDifficulty { get; set; }

        public StartingInvData(string[] row, int index) : base(row)
        {
            ID = row[2] + "::" + index;
            Index = index;
        }

        public override bool IsValid(Dictionary<string, int> items)
        {
            return Requirements.IsValid(items);
        }

        public override void SetData(dynamic obj, string newItem, int newCount)
        {
            DataStorePartyMember c = (DataStorePartyMember)obj;
            List<ushort> itemIDs = c.ItemIDs;
            List<byte> itemAmounts = c.ItemAmounts;

            if (newItem == null)
            {
                itemIDs[Index] = 0xFFFF;
                itemAmounts[Index] = 0;
            }
            else
            {
                ushort id = Convert.ToUInt16(newItem, 16);
                itemIDs[Index] = id;
                itemAmounts[Index] = (byte)newCount;
            }

            c.ItemIDs = itemIDs;
            c.ItemAmounts = itemAmounts;
        }

        public override (string, int)? GetData(dynamic obj)
        {
            DataStorePartyMember c = (DataStorePartyMember)obj;
            if (c.ItemIDs[Index] == 0xFFFF)
            {
                return null;
            }
            else
            {
                return (c.ItemIDs[Index].ToString("X4"), c.ItemAmounts[Index]);
            }
        }
    }
}

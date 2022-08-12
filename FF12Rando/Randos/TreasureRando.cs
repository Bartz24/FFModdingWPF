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
        public int[] characters = new int[6];
        public string[] characterMapping = new string[] { "Vaan", "Ashe", "Fran", "Balthier", "Basch", "Penelo" };

        public List<List<string>> hints = Enumerable.Range(0, 35).Select(_ => new List<string>()).ToList();

        public Dictionary<string, string> areaMapping = new Dictionary<string, string>();
        Dictionary<string, ItemLocation> itemLocations = new Dictionary<string, ItemLocation>();
        public Dictionary<string, string> missableTreasureLinks = new Dictionary<string, string>();

        FF12AssumedItemPlacementAlgorithm placementAlgo;
        FF12ItemPlacementAlgorithm placementAlgoBackup;
        private bool usingBackup = false;
        public ItemPlacementAlgorithm<ItemLocation> PlacementAlgo { get => usingBackup ? placementAlgoBackup : placementAlgo; }

        public List<string> treasuresToPlace = new List<string>();
        public List<string> treasuresAllowed = new List<string>();

        public List<string> randomizeItems = new List<string>();
        public List<string> remainingRandomizeItems = new List<string>();

        public TreasureRando(RandomizerManager randomizers) : base(randomizers) { }

        public override string GetID()
        {
            return "Treasures";
        }

        public override void Load()
        {
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

            characters = MathExtensions.DecodeNaturalSequence(prices[0x76].Price, 6, 6).Select(l => (int)l).ToArray();

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
                for (int i = 0; i < 3; i++)
                {
                    RewardData r = new RewardData(row, i, fakeID);
                    if (r.Traits.Contains("Fake") && i == 2)
                        fakeID--;
                    itemLocations.Add(r.ID, r);
                }
            }, FileHelpers.CSVFileHeader.HasHeader);

            List<string> hintsNotesLocations = itemLocations.Values.SelectMany(l => l.Areas).Distinct().Where(l => l != "Fake").ToList();

            placementAlgo = new FF12AssumedItemPlacementAlgorithm(itemLocations, hintsNotesLocations, Randomizers, 3);
            placementAlgoBackup = new FF12ItemPlacementAlgorithm(itemLocations, hintsNotesLocations, Randomizers, -1);
        }
        public override void Randomize(Action<int> progressSetter)
        {
            if (FF12Flags.Other.Party.FlagEnabled)
            {
                FF12Flags.Other.Party.SetRand();
                characters.SetSubArray(1, characters.SubArray(1, 5).Shuffle().ToArray());
                prices[0x76].Price = (uint)MathExtensions.EncodeNaturalSequence(characters.Select(i => (long)i).ToArray(), 6);
                RandomNum.ClearRand();
            }

            randomizeItems = new List<string>();
            if (FF12Flags.Items.Treasures.FlagEnabled)
                randomizeItems = randomizeItems.Concat(GetRandomizableItems()).Distinct().ToList();
            ShopRando shopRando = Randomizers.Get<ShopRando>("Shops");
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
                if (!placementAlgo.Randomize(new List<string>(), areaMults))
                {
                    usingBackup = true;
                    placementAlgoBackup.Randomize(new List<string>(), areaMults);
                }

                int respawnIndex = 0;
                itemLocations.Values.Shuffle().ForEach(l =>
                {
                    if (PlacementAlgo.GetKeysAllowed().Contains(l.ID))
                    {
                        if (IsEmpty(l.ID))
                        {
                            string rep = PlacementAlgo.Placement.Keys.Where(s => IsExtra(s) && PlacementAlgo.GetLocationItem(s, false) != null && !IsImportantKeyItem(PlacementAlgo.Placement[s]) && PlacementAlgo.IsAllowed(l.ID, PlacementAlgo.Placement[s])).Shuffle().First();
                            Tuple<string, int> item = PlacementAlgo.GetLocationItem(rep, false);
                            PlacementAlgo.SetLocationItem(l.ID, item.Item1, item.Item2);
                            PlacementAlgo.Placement.Add(l.ID, PlacementAlgo.Placement[rep]);
                            PlacementAlgo.Placement.Remove(rep);
                            hints.Where(list => list.Contains(rep)).ForEach(list =>
                            {
                                list.Remove(rep);
                                list.Add(l.ID);
                            });
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

                EquipRando equipRando = Randomizers.Get<EquipRando>("Equip");
                itemLocations.Values.ForEach(l =>
                {
                    if (PlacementAlgo.Placement.ContainsKey(l.ID))
                    {
                        Tuple<string, int> tuple = PlacementAlgo.GetLocationItem(l.ID, false);
                        if (tuple != null && randomizeItems.Contains(tuple.Item1))
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
                                PlacementAlgo.SetLocationItem(l.ID, newItem, 1);
                                if (!tuple.Item1.StartsWith("30") && !tuple.Item1.StartsWith("40"))
                                    hints.Where(list => list.Count() == hints.Select(list => list.Count()).Min()).First().Add(l.ID);
                            }
                            else
                            {
                                PlacementAlgo.SetLocationItem(l.ID, newItem, tuple.Item2);
                                if (tuple.Item1.StartsWith("30") || tuple.Item1.StartsWith("40"))
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
                    itemLocations.Values.Where(l => PlacementAlgo.GetLocationItem(l.ID, false) != null && PlacementAlgo.GetLocationItem(l.ID, false).Item1 == "8070")
                        .ForEach(l =>
                        {
                            PlacementAlgo.SetLocationItem(l.ID, "0000", 5);
                            hints.Where(list => list.Contains(l.ID)).ForEach(list => list.Remove(l.ID));
                        });
                }

                missableTreasureLinks.ForEach(p =>
                {
                    Tuple<string, int> item = PlacementAlgo.GetLocationItem(p.Value, false);
                    PlacementAlgo.SetLocationItem(p.Key, item.Item1, item.Item2);
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
                                PlacementAlgo.SetLocationItem(l.ID, "Gil", 0);
                            else
                                PlacementAlgo.SetLocationItem(l.ID, "FFFF", 255);
                        }
                    }
                });
                RandomNum.ClearRand();
            }
        }

        private bool ShouldRemoveItem(string newItem)
        {
            EquipRando equipRando = Randomizers.Get<EquipRando>("Equip");
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
            return IsMainKeyItem(location) || IsSideKeyItem(location) || IsHuntKeyItem(location) || IsGrindyKeyItem(location) || IsBlackOrbKeyItem(location) || IsWoTItem(location) || IsHuntClubKeyItem(location);
        }

        public bool IsAbility(string t, bool orig = true)
        {
            return PlacementAlgo.GetLocationItem(t, orig) != null && (PlacementAlgo.GetLocationItem(t, orig).Item1.StartsWith("30") || PlacementAlgo.GetLocationItem(t, orig).Item1.StartsWith("40"));
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
                    Tuple<string, int> tuple = PlacementAlgo.GetLocationItem(l.ID);
                    if (tuple != null && IsRandomizableItem(tuple.Item1))
                    {
                        return tuple.Item1;
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
                        list.Add($"{itemLocations[l].Name} has {GetItemName(PlacementAlgo.GetLocationItem(l, false).Item1)}");
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
                        Tuple<string, int> item = PlacementAlgo.GetLocationItem(l, false);
                        if (item != null && (item.Item1.StartsWith("30") || item.Item1.StartsWith("40")))
                            type = "an Ability";

                        list.Add($"{itemLocations[l].Name} has {type}");
                        list.Add("");
                        break;
                    }
                case 2:
                    {
                        list.Add($"{itemLocations[l].Areas[0]} has {GetItemName(PlacementAlgo.GetLocationItem(l, false).Item1)}");
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


        public override HTMLPage GetDocumentation()
        {
            HTMLPage page = new HTMLPage("Item Locations", "template/documentation.html");

            page.HTMLElements.Add(new Button("document.getElementById(\"itemlocations\").classList.toggle(\"hide4\")", null, "Hide/Show Requirements"));

            page.HTMLElements.Add(new Table("Item Locations", (new string[] { "Name", "New Contents", "Location", "Requirements", "'Difficulty'" }).ToList(), (new int[] { 30, 20, 10, 35, 5 }).ToList(), itemLocations.Values.Select(l =>
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
                    RewardData t = (RewardData)l;
                    if (t.Index > 0 || t.Traits.Contains("Fake"))
                        return null;
                    DataStoreReward reward = rewards[t.IntID - 0x9000];
                    display = GetRewardDisplay(reward);
                }
                string reqsDisplay = l.Requirements.GetDisplay(GetItemName);
                if (reqsDisplay.StartsWith("(") && reqsDisplay.EndsWith(")"))
                    reqsDisplay = reqsDisplay.Substring(1, reqsDisplay.Length - 2);
                return new string[] { l.Name, display, l.Areas[0], reqsDisplay, l.Difficulty.ToString() }.ToList();
            }).Where(l => l != null).ToList(), "itemlocations"));
            return page;
        }

        public string GetItemName(string id)
        {
            TextRando textRando = Randomizers.Get<TextRando>("Text");
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
            public override string ID { get; }
            public int Index { get; }
            public override string Name { get; }
            public string Subarea { get; }
            public string MapID { get; }
            public override ItemReq Requirements { get; }
            public override List<string> Traits { get; }
            public override List<string> Areas { get; }
            public override int Difficulty { get; }

            public TreasureData(string[] row, int index)
            {
                Areas = new List<string>() { row[0] };
                Name = row[0] + " - " + row[1] + " Treasure";
                Subarea = row[1];
                MapID = row[2];
                ID = row[2] + ":" + index;
                Index = index;
                Requirements = ItemReq.Parse(row[5]);
                Difficulty = int.Parse(row[6]);
                Traits = row[7].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
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

            public override Tuple<string, int> GetData(dynamic obj)
            {
                DataStoreTreasure t = (DataStoreTreasure)obj;
                if (t.GilChance == 100)
                {
                    return new Tuple<string, int>("Gil", t.GilCommon);
                }
                else
                {
                    return new Tuple<string, int>(t.CommonItem1ID.ToString("X4"), 1);
                }
            }
        }

        public class RewardData : ItemLocation
        {
            public override string ID { get; }
            public int IntID { get; }
            public int Index { get; }
            public override string Name { get; }
            public override ItemReq Requirements { get; }
            public override List<string> Traits { get; }
            public override List<string> Areas { get; }
            public override int Difficulty { get; }
            public List<string> FakeItems { get; }

            public RewardData(string[] row, int index, int fakeID)
            {
                Traits = row[5].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                Areas = new List<string>() { row[0] };
                Name = row[1];
                if (!Traits.Contains("Fake"))
                    IntID = Convert.ToInt32(row[2], 16);
                else
                    IntID = fakeID;
                ID = row[2] + ":" + index;
                Index = index;
                Requirements = ItemReq.Parse(row[3]);
                Difficulty = int.Parse(row[4]);
                FakeItems = row[6].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
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

            public override Tuple<string, int> GetData(dynamic obj)
            {
                DataStoreReward r = (DataStoreReward)obj;
                if (Index == 0)
                {
                    if (r.Gil == 0)
                        return null;
                    return new Tuple<string, int>("Gil", (int)r.Gil);
                }
                else if (Index == 1)
                {
                    if (r.Item1ID == 0xFFFF)
                        return null;
                    return new Tuple<string, int>(r.Item1ID.ToString("X4"), r.Item1Amount);
                }
                else
                {
                    if (r.Item2ID == 0xFFFF)
                        return null;
                    return new Tuple<string, int>(r.Item2ID.ToString("X4"), r.Item2Amount);
                }
            }
        }
    }
}

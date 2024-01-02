using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using FF12Rando.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FF12Rando;

public partial class TreasureRando : Randomizer
{
    public DataStoreBPSection<DataStoreReward> rewards, rewardsOrig;
    public DataStoreBPSection<DataStorePrice> prices;
    public Dictionary<string, DataStoreEBP> ebpAreas = new();
    public Dictionary<string, DataStoreEBP> ebpAreasOrig = new();

    public Dictionary<string, string> areaMapping = new();
    public Dictionary<string, ItemLocation> ItemLocations = new();
    public FF12ItemPlacer ItemPlacer { get; set; }
    public FF12HintPlacer HintPlacer { get; set; }

    public List<string> treasuresToPlace = new();
    public List<string> treasuresAllowed = new();

    public TreasureRando(SeedGenerator randomizers) : base(randomizers) { }

    // Used for grouping/sorting by type
    public enum ItemType
    {
        Consumable, // 0x0000 to 0x0FFF
        Equipment, // 0x1000 to 0x1FFF
        Ability // 0x3000 to 0x4FFF
    }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Treasure Data...");
        rewards = new DataStoreBPSection<DataStoreReward>();
        rewards.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_037.bin"));
        rewards.DataList.ForEach(r => r.IntID = rewards.DataList.IndexOf(r) + 0x9000);
        rewardsOrig = new DataStoreBPSection<DataStoreReward>();
        rewardsOrig.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_037.bin"));
        rewardsOrig.DataList.ForEach(r => r.IntID = rewardsOrig.DataList.IndexOf(r) + 0x9000);
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
                TreasureLocation t = new(Generator, row, i);
                ItemLocations.Add(t.ID, t);
            }
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\rewards.csv", row =>
        {
            for (int i = 0; i < 3; i++)
            {
                RewardLocation r = new(Generator, row, i);
                ItemLocations.Add(r.ID, r);
            }
        }, FileHelpers.CSVFileHeader.HasHeader);

        if (FF12Flags.Items.KeyStartingInv.Enabled)
        {
            FileHelpers.ReadCSVFile(@"data\startingInvs.csv", row =>
            {
                StartingInvLocation first = new(Generator, row, 0);
                ItemLocations.Add(first.ID, first);
                // Keep the last slot empty for tp stones
                for (int i = 1; i < 10; i++)
                {
                    StartingInvLocation s = new(Generator, row, i);
                    ItemLocations.Add(s.ID, s);
                }
            }, FileHelpers.CSVFileHeader.HasHeader);
        }

        FileHelpers.ReadCSVFile(@"data\fakeChecks.csv", row =>
        {
            string[] fakeItems = row[6].Split('|');
            int index = 0;
            foreach (string fakeItem in fakeItems)
            {
                FF12FakeLocation f = new(Generator, row, fakeItem);
                f.ID = f.ID + ":" + index;
                ItemLocations.Add(f.ID, f);
                index++;
            }
        }, FileHelpers.CSVFileHeader.HasHeader);

        List<string> hintsNotesLocations = ItemLocations.Values.SelectMany(l => l.Areas).Distinct().ToList();
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Treasure Data...");

        if (FF12Flags.Items.Treasures.FlagEnabled)
        {
            FF12Flags.Items.Treasures.SetRand();

            CollapseAndSelectTreasures();

            Dictionary<string, double> areaMults = ItemLocations.Values.SelectMany(t => t.Areas).Distinct().ToDictionary(s => s, _ => RandomNum.RandInt(10, 200) * 0.01d);

            ItemPlacer = new(Generator);
            ItemPlacer.LocationsToPlace = ItemLocations.Values
                .Where(l => (l is not TreasureLocation || treasuresToPlace.Contains(l.ID)) && l.GetItem(true) != null).ToHashSet();
            ItemPlacer.PossibleLocations = ItemLocations.Values
                .Where(l => l is not TreasureLocation || treasuresAllowed.Contains(l.ID)).ToHashSet();
            ItemPlacer.PlaceItems();
            ItemPlacer.ApplyToGameData();
            
            // Clear null treasures
            foreach (var location in ItemLocations.Values)
            {
                if (!ItemPlacer.FinalPlacement.ContainsKey(location) && location is TreasureLocation)
                {
                    DataStoreTreasure t = ebpAreas[((TreasureLocation)location).MapID].TreasureList[((TreasureLocation)location).Index];
                    t.SpawnChance = 0;
                    t.Respawn = 255;
                    t.GilChance = 0;
                    t.CommonItem1ID = t.CommonItem2ID = t.RareItem1ID = t.RareItem2ID = 0xFFFF;
                    t.GilCommon = t.GilRare = 0;
                }
            }

            // Set treasure respawn IDs
            int respawnIndex = 0;
            foreach (var location in ItemPlacer.FinalPlacement.Keys.Shuffle())
            {
                if (respawnIndex >= 255)
                {
                    break;
                }

                if (location is TreasureLocation t)
                {
                    DataStoreTreasure treasure = ebpAreas[t.MapID].TreasureList[t.Index];
                    treasure.Respawn = (byte)respawnIndex;
                    respawnIndex++;
                }
            }

            // Set random linked missable chests
            foreach (var location in ItemLocations.Values.Where(l => l is TreasureLocation && l.Traits.Contains("Missable")))
            {
                if (RandomNum.RandInt(0, 99) < 20)
                {
                    continue;
                }

                TreasureLocation missable = (TreasureLocation)location;

                // Find another non missable chest
                TreasureLocation other = RandomNum.SelectRandom(ItemLocations.Values.Where(l => l is TreasureLocation && !l.Traits.Contains("Missable") && l.GetItem(false) != null).Select(l => (TreasureLocation)l));

                // Copy item
                var item = other.GetItem(false);
                missable.SetItem(item.Value.Item, item.Value.Amount);

                // Copy respawn and spawn chance
                DataStoreTreasure treasureMissable = ebpAreas[missable.MapID].TreasureList[missable.Index];
                DataStoreTreasure treasureOther = ebpAreas[other.MapID].TreasureList[other.Index];
                treasureMissable.Respawn = treasureOther.Respawn;
                treasureMissable.SpawnChance = treasureOther.SpawnChance;
            }

            HintPlacer = new(Generator, ItemPlacer, Enumerable.Range(0, 35).ToHashSet());
            HintPlacer.PlaceHints();

            RandomNum.ClearRand();
        }

        if (!FF12Flags.Items.AllowSeitengrat.FlagEnabled)
        {
            // Replace any Seitengrat with the Dhanusha
            ItemLocations.Values.Where(l => ItemLocations[l.ID].GetItem(false)?.Item1 == "10B2").ForEach(l =>
            {
                ItemLocations[l.ID].SetItem("10C7", ItemLocations[l.ID].GetItem(false).Value.Item2);
            });
        }
    }

    private void CollapseAndSelectTreasures()
    {
        treasuresToPlace.Clear();
        List<int> usedRespawnIDs = new();
        ItemLocations.Values.Where(l => l is TreasureLocation).Select(l => (TreasureLocation)l).ForEach(l =>
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

            if (l.GetItem(true) != null && (l.GetItem(true).Value.Item.StartsWith("30") || l.GetItem(true).Value.Item.StartsWith("40")))
            {
                if (t.Respawn == 255 || !usedRespawnIDs.Contains(t.Respawn))
                {
                    treasuresToPlace.Add(l.ID);
                }
            }
        });

        foreach (TreasureLocation l in ItemLocations.Values.Where(l => l is TreasureLocation && !treasuresToPlace.Contains(l.ID)).Select(l => (TreasureLocation)l).Shuffle())
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

        treasuresAllowed = ItemLocations.Values.Where(l => l is TreasureLocation && !l.Traits.Contains("Missable")).Select(l => l.ID).Shuffle().Take(255).ToList();
    }

    private List<string> GetRandomizableItems()
    {
        return ItemLocations.Values.Where(l => l is not TreasureLocation || treasuresToPlace.Contains(l.ID))
            .Select(l =>
            {
                (string, int)? tuple = ItemLocations[l.ID].GetItem(true);
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
        if (HintPlacer.Hints.Values.Select(l => l.Count).Sum() > 0)
        {
            foreach (int num in HintPlacer.Hints.Keys)
            {
                List<string> lines = new();
                if (HintPlacer.Hints[num].Count > 0)
                {
                    foreach (var l in HintPlacer.Hints[num])
                    {
                        lines.Add(HintPlacer.GetHintText(l));
                    }
                }
                else
                {
                    lines.Add("There is nothing left to hint.");
                }

                textRando.TextKeyDescriptions[352 + num].Text = string.Join("\n", lines);
            }
        }
    }

    public void SaveTreasureTracker()
    {
        Dictionary<string, List<int>> areaRespawns = new();
        IEnumerable<TreasureLocation> treasures = ItemLocations.Values
            .Where(l => l is TreasureLocation && l.GetItem(false) != null)
            .Select(l => (TreasureLocation)l);
        foreach (TreasureLocation l in treasures)
        {
            DataStoreTreasure t = ebpAreas[l.MapID].TreasureList[l.Index];
            if (t.SpawnChance > 0 && t.Respawn < 255)
            {
                if (!areaRespawns.ContainsKey(l.MapID))
                {
                    areaRespawns.Add(l.MapID, new List<int>());
                }

                areaRespawns[l.MapID].Add(t.Respawn);
            }
        }

        File.WriteAllLines($"{Generator.OutFolder}\\treasureTracker.txt", areaRespawns.Select(p => $"{areaMapping[p.Key]},{string.Join(",", p.Value)}"));
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Treasure Data...");
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

        page.HTMLElements.Add(new Table("Item Locations", (new string[] { "Name", "New Contents", "Sphere" }).ToList(), (new int[] { 45, 45, 10 }).ToList(), ItemLocations.Values.Where(l => l is not FakeLocation).Select(l =>
        {
            string display = "";
            if (l is TreasureLocation t)
            {
                DataStoreTreasure treasure = ebpAreas[t.MapID].TreasureList[t.Index];
                if (treasure.SpawnChance == 0)
                {
                    return null;
                }

                display = GetTreasureDisplay(treasure);
            }
            else if (l is RewardLocation r)
            {
                if (r.Index > 0)
                {
                    return null;
                }

                DataStoreReward reward = rewards[r.IntID - 0x9000];
                display = GetRewardDisplay(reward);
            }
            else if (l is StartingInvLocation s)
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

            return new object[] { nameCell, display, ItemPlacer.SphereCalculator.Spheres.ContainsKey(l) ? ItemPlacer.SphereCalculator.Spheres[l] : "N/A" }.ToList();
        }).Where(l => l != null).ToList(), "itemlocations"));
        pages.Add("item_locations", page);

        // Add hints page
        page = new("Hints", "template/documentation.html");
        page.HTMLElements.Add(new Table("Hints", (new string[] { "Hint Number", "Hint" }).ToList(), (new int[] { 20, 80 }).ToList(), HintPlacer.Hints.Select(hint => new object[] { $"Hint Number {hint.Key + 1}", string.Join("\n", hint.Value.Select(line => HintPlacer.GetHintText(line))) }.ToList()).ToList(), "hints"));
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
}

using FF12Rando;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using static System.Windows.Forms.AxHost;
using System.Xml.Linq;

namespace AutoDataGenerator;
internal class FF12MultiworldGenerator
{
    public string OutputDir { get; }

    TreasureRando TreasureRando { get; }
    EquipRando EquipRando { get; }
    PartyRando PartyRando { get; }
    ShopRando ShopRando { get; }

    Dictionary<ItemLocation, string> locations = new();

    public FF12MultiworldGenerator(string inputDir, string outputDir)
    {
        SetupData.Paths["12"] = "G:\\SteamLibrary\\steamapps\\common\\FINAL FANTASY XII THE ZODIAC AGE\\x64\\FFXII_TZA.exe";
        DataExtensions.Mode = ByteMode.LittleEndian;
        FF12Flags.Init();

        OutputDir = outputDir;
        var seedGenerator = new FF12SeedGenerator();
        TreasureRando = seedGenerator.Get<TreasureRando>();
        EquipRando = seedGenerator.Get<EquipRando>();
        PartyRando = seedGenerator.Get<PartyRando>();
        ShopRando = seedGenerator.Get<ShopRando>();

        // Set working directory to the input directory
        Directory.SetCurrentDirectory(inputDir);

        // Enable starting inv flag
        FF12Flags.Items.KeyStartingInv.Enabled = true;
        PartyRando.Load();
        EquipRando.Load();
        TreasureRando.Load();
        ShopRando.Load();
    }

    public void Generate()
    {
        GenerateItemsScript();
        GenerateLocationsScript();
        GenerateEventsScript();
        GenerateRulesScript();
        GenerateRegionsScript();
    }

    private void GenerateItemsScript()
    {        
        // Auto generate the Items.py script
        string script =
            "from typing import Dict, NamedTuple, Optional\n" +
            "from BaseClasses import Item, ItemClassification\n" +
            "\n" +
            "\n" +
            "class FF12OpenWorldItem(Item):\n" +
            "    game: str = \"Final Fantasy 12 Open World\"\n" +
            "\n" +
            "\n" +
            "class FF12OpenWorldItemData(NamedTuple):\n" +
            "    code: Optional[int] = None\n" +
            "    classification: ItemClassification = ItemClassification.filler\n" +
            "    category: str = \"\"\n" +
            "    weight: int = 0\n" +
            "    amount: int = 1\n" +
            "    duplicateAmount: int = 1\n" +
            "\n" +
            "\n" +
            "item_data_table: Dict[str, FF12OpenWorldItemData] = {\n";

        EquipRando.itemData.Values.ForEach(i =>
        {
            if (!i.Traits.Contains("Ignore"))
            {
                string type = "filler";
                int weight = 0;
                int duplicates = 1;
                if ((i.Category == "Key" || i.Category == "Esper" || i.Category == "Board") && !i.Traits.Contains("Trophy"))
                {
                    type = "progression";
                    int count = TreasureRando.ItemLocations.Values.Where(l => l.GetItem(true) != null && l.GetItem(true)?.Item == i.ID).Count();
                    // Allow duplicates except for writ of transit
                    if (count > 1 && i.IntID != 0x8070)
                    {
                        duplicates = count;
                        type = "progression_skip_balancing";
                    }
                }
                else if (i.Category == "Ability")
                {
                    type = "useful";
                }
                else if (i.Category == "Loot")
                {
                    weight = 10;
                }
                else if (i.Traits.Contains("Trophy"))
                {
                    weight = 0;
                }
                else
                {
                    // Weight is a value based on rank using exponential decay from 200 to 10
                    // Rank goes up to 10 using this formula
                    // Weapons and armor are weighted lower for ranks 0-3
                    weight = i.Rank > 10 ? 1 : 
                    i.Rank <= 3 && (i.Category == "Weapon" || i.Category == "Armor") ? 5 :
                    (int)Math.Ceiling(20 * Math.Pow(0.7, i.Rank) * 10);

                    if (i.Category == "Item")
                    {
                        weight *= 2;
                    }
                }

                script = AddItemToItemsScript(script, i.Name, i.IntID + 1, type, i.Category, weight, 1, duplicates);
            }
        });

        int[] gilAmounts = new[] { 1, 500, 1000, 5000, 10000, 25000 };
        int[] gilWeights = new[] { 250, 900, 1150, 800, 500, 200 };
        for (int i = 0; i < gilAmounts.Length; i++)
        {
            script = AddItemToItemsScript(script, $"{gilAmounts[i]} Gil", 0x18000 + i + 1, "filler", "Gil", gilWeights[i], gilAmounts[i], 0);
        }

        script += "}\n";

        script += "\n" +
            "item_table = {name: data.code for name, data in item_data_table.items()}\n" +
            "inv_item_table = {data.code: name for name, data in item_data_table.items()}\n" +
            "\n" +
            "filler_items = [name for name, data in item_data_table.items()\n" +
            "                if data.classification == ItemClassification.filler and data.weight > 0]\n" +
            "filler_weights = [item_data_table[name].weight for name in filler_items]\n";

        File.WriteAllText(Path.Combine(OutputDir, "Items.py"), script);
    }

    private static string AddItemToItemsScript(string script, string name, int id, string type, string category, int weight, int amount, int duplicates)
    {
        script += 
            $"    \"{name}\": FF12OpenWorldItemData(\n" +
            $"        code={id},\n" +
            $"        classification=ItemClassification.{type},\n" +
            $"        category=\"{category}\"";
        if (weight > 0)
        {
            script += $",\n" +
                $"        weight={weight}";
        }


        if (amount != 1)
        {
            script += $",\n" +
                $"        amount={amount}";
        }

        if (duplicates != 1)
        {
            script += $",\n" +
                $"        duplicateAmount={duplicates}";
        }

        script += "\n    ),\n";
        return script;
    }

    private void GenerateLocationsScript()
    {
        string script =
            "from typing import Dict, NamedTuple, Optional\n" +
            "from BaseClasses import Location, LocationProgressType\n" +
            "\n" +
            "\n" +
            "class FF12OpenWorldLocation(Location):\n" +
            "    game: str = \"Final Fantasy 12 Open World\"\n" +
            "\n" +
            "\n" +
            "class FF12OpenWorldLocationData(NamedTuple):\n" +
            "    region: str\n" +
            "    type: str\n" +
            "    str_id: str\n" +
            "    address: Optional[int] = None\n" +
            "    classification: LocationProgressType = LocationProgressType.DEFAULT\n" +
            "    secondary_index: int = 0\n" +
            "    difficulty: int = 0" +
            "\n" +
            "\n" +
            "location_data_table: Dict[str, FF12OpenWorldLocationData] = {\n";

        locations.Clear();

        int nextIndex = 1;
        TreasureRando.ItemLocations.Values
            .Where(l => (!l.Traits.Contains("Missable") || l is not TreasureLocation) && l is not FakeLocation)
            .ToList()
            .ForEach(l =>
            {
                string classification = l.Traits.Contains("Missable") ? "EXCLUDED" : "DEFAULT";
                string regionName = l.Areas != null && l.Areas.Count > 0 ? l.Areas[0] : "Initial";

                string name;
                switch (l)
                {
                    case RewardLocation r:
                        name = $"{r.Name} ({r.Index + 1})";
                        script = AddLocationToLocationsScript(script, name, regionName, nextIndex, classification, "reward", r.IntID.ToString("X4"), r.Index, l.BaseDifficulty);
                        break;
                    case TreasureLocation t:
                        name = $"{t.Name} {t.Index + 1}";
                        script = AddLocationToLocationsScript(script, name, regionName, nextIndex, classification, "treasure", t.MapID, t.Index, l.BaseDifficulty);
                        break;
                    case StartingInvLocation s:
                        name = $"{s.Name} ({s.Index + 1})";
                        script = AddLocationToLocationsScript(script, name, regionName, nextIndex, classification, "inventory", s.IntID.ToString(), s.Index, l.BaseDifficulty);
                        break;
                    default:
                        throw new Exception("Unknown location type");
                }

                locations.Add(l, name);

                nextIndex++;
            });

        script += "}\n";

        script += "\nlocation_table = {location_name: location_data.address for location_name, location_data in location_data_table.items()}";

        File.WriteAllText(Path.Combine(OutputDir, "Locations.py"), script);
    }

    private string AddLocationToLocationsScript(string script, string name, string region, int id, string classification, string type, string strId, int index, int difficulty)
    {
        // Map ID and secondary index are optional
        script +=
            $"    \"{name}\": FF12OpenWorldLocationData(\n" +
            $"        region=\"{region}\",\n" +
            $"        address={id},\n" +
            $"        classification=LocationProgressType.{classification},\n" +
            $"        type=\"{type}\"";
        if (!string.IsNullOrEmpty(strId))
        {
            script += $",\n" +
                $"        str_id=\"{strId}\"";
        }

        if (index > 0)
        {
            script += $",\n" +
                $"        secondary_index={index}";
        }

        if (difficulty > 0)
        {
            script += $",\n" +
                $"        difficulty={difficulty}";
        }

        script += "\n    ),\n";
        return script;
    }

    private void GenerateEventsScript()
    {
        string script =
            "from typing import Dict, NamedTuple\n" +
            "\n" +
            "\n" +
            "class FF12OpenWorldEventData(NamedTuple):\n" +
            "    region: str\n" +
            "    item: str\n" +
            "    difficulty: int = 0\n" +
            "\n" +
            "\n" +
            "event_data_table: Dict[str, FF12OpenWorldEventData] = {\n";

        Dictionary<string, int> usedNames = new();
        TreasureRando.ItemLocations.Values.Where(l => l is FakeLocation).ForEach(l =>
        {
            FakeLocation fake = (FakeLocation)l;
            if (usedNames.ContainsKey(fake.Name))
            {
                usedNames[fake.Name]++;
            }
            else
            {
                usedNames.Add(fake.Name, 1);
            }
            string newName = $"{fake.Name} Event ({usedNames[fake.Name]})";
            string regionName = l.Areas != null && l.Areas.Count > 0 ? l.Areas[0] : "Initial";

            script += $"    \"{newName}\": FF12OpenWorldEventData(\n" +
                      $"        region=\"{regionName}\",\n" +
                      $"        item=\"{fake.FakeItem}\",\n" +
                      $"        difficulty={l.BaseDifficulty}\n" +
                      $"    ),\n";

            locations.Add(l, newName);
        });

        script += "}\n";
        File.WriteAllText(Path.Combine(OutputDir, "Events.py"), script);
    }

    private void GenerateRulesScript()
    {
        string script =
            "from typing import Callable, Dict, List, Tuple\n" +
            "from BaseClasses import CollectionState\n" +
            "from .RuleLogic import state_has_at_least, state_has_category" +
            "\n" +
            "\n" +
            "rule_data_list: List[Callable[[CollectionState, int], bool]] = [\n";

        Dictionary<string, string> locationToRules = new();
        Dictionary<(string From, string To), string> entranceToRules = new();
        List<string> rules = new();

        locations.Keys.ForEach(l =>
        {
            string ruleStr = l.GetArchipelagoRule(TreasureRando.GetItemName);
            if (!rules.Contains(ruleStr))
            {
                rules.Add(ruleStr);
            }

            locationToRules.Add(locations[l], ruleStr);
        });

        // Build entrance rules from AreaGraph connections BEFORE emitting rule_data_list
        TreasureRando.AreaGraph.Connections.ForEach(c =>
        {
            string ruleStr = c.Requirements.GetArchipelagoRule(TreasureRando.GetItemName);
            List<string> ruleLines = ruleStr.Split('\n').ToList();
            for (int i = 0; i < ruleLines.Count; i++)
            {
                int indent = i == 0 ? 4 : ruleLines[i - 1].TakeWhile(ch => ch == ' ' || ch == '(').Count();
                ruleLines[i] = new string(' ', indent) + ruleLines[i];
            }

            ruleStr = $"lambda state, player:\n{string.Join("\n", ruleLines)}";

            if (!rules.Contains(ruleStr))
            {
                rules.Add(ruleStr);
            }

            entranceToRules[(c.FromAreaName, c.ToAreaName)] = ruleStr;
        });

        for (int i = 0; i < rules.Count; i++)
        {
            script += $"    {rules[i]},  # Rule {i}\n";
        }

        script += "]\n\n";

        script += "rule_data_table: Dict[str, Callable[[CollectionState, int], bool]] = {\n";
        locationToRules.Keys.ForEach(l =>
        {
            script += $"    \"{l}\": rule_data_list[{rules.IndexOf(locationToRules[l])}],\n";
        });

        script += "}\n";

        // Emit entrance rules table
        script += "\nentrance_rule_data_table: Dict[Tuple[str, str], Callable[[CollectionState, int], bool]] = {\n";
        entranceToRules.Keys.ForEach(k =>
        {
            var ruleStr = entranceToRules[k];
            var idx = rules.IndexOf(ruleStr);
            script += $"    (\"{k.From}\", \"{k.To}\"): rule_data_list[{idx}],\n";
        });
        script += "}\n";

        // Write entrance rule difficulty table
        script += "\nentrance_rule_difficulty_table: Dict[Tuple[str, str], int] = {\n";
        TreasureRando.AreaGraph.Connections.ForEach(c =>
        {
            FF12AreaConnection conn = (FF12AreaConnection)c;
            script += $"    (\"{conn.FromAreaName}\", \"{conn.ToAreaName}\"): {conn.BaseDifficulty},\n";
        });
        script += "}\n";

        // Write table of indirect entrances (area -> entrance tuples)
        script += "\nindirect_entrance_table: Dict[str, List[Tuple[str, str]]] = {\n";
        Dictionary<string, List<(string From, string To)>> indirects = new();
        TreasureRando.AreaGraph.Connections.Where(c=>c.Traits.Contains("Indirect")).ForEach(c =>
        {
            // Get the indirect areas of the connection from the area req components
            var indirectAreas = c.Requirements.GetOf<AreaItemReq>().Select(r => r.Area).Distinct().ToList();

            indirectAreas.ForEach(area =>
            {
                if (!indirects.ContainsKey(area))
                {
                    indirects[area] = new();
                }

                indirects[area].Add((c.FromAreaName, c.ToAreaName));
            });
        });

        foreach (var kvp in indirects)
        {
            script += $"    \"{kvp.Key}\": [";
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (i > 0)
                {
                    script += ", ";
                }

                script += $"(\"{kvp.Value[i].From}\", \"{kvp.Value[i].To}\")";
            }

            script += "],\n";
        }

        script += "}\n";

        File.WriteAllText(Path.Combine(OutputDir, "Rules.py"), script);

    }
    private void GenerateRegionsScript()
    {
        StringBuilder sb = new();
        sb.Append(
            "from typing import Dict, List, NamedTuple, Optional\n" +
            "from BaseClasses import Region\n\n" +
            "class FF12OpenWorldRegion(Region):\n" +
            "    game: str = \"Final Fantasy 12 Open World\"\n\n" +
            "class FF12OpenWorldRegionData(NamedTuple):\n" +
            "    connecting_regions: List[str]\n" +
            "    map_id: Optional[int] = None\n" +
            "    secondary_index: Optional[int] = None\n\n" +
            "region_data_table: Dict[str, FF12OpenWorldRegionData] = {\n");

        var areas = TreasureRando.AreaGraph.Areas.Keys.ToList();
        areas.Sort(StringComparer.Ordinal);
        for (int i = 0; i < areas.Count; i++)
        {
            var areaName = areas[i];
            var connections = TreasureRando.AreaGraph.Connections
                .Where(c => c.FromAreaName == areaName)
                .Select(c => c.ToAreaName)
                .Distinct()
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToList();

            sb.Append($"    \"{areaName}\": FF12OpenWorldRegionData(connecting_regions=[");
            for (int j = 0; j < connections.Count; j++)
            {
                if (j > 0)
                {
                    sb.Append(", ");
                }

                sb.Append($"\"{connections[j]}\"");
            }

            sb.Append("]),\n");
        }

        sb.Append("}\n");

        File.WriteAllText(Path.Combine(OutputDir, "Regions.py"), sb.ToString());
    }
}

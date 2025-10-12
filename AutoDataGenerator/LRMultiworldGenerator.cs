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
using LRRando;
using System.IO;

namespace AutoDataGenerator;
internal class LRMultiworldGenerator
{
    public string OutputDir { get; }

    TreasureRando TreasureRando { get; }
    EquipRando EquipRando { get; }

    Dictionary<ItemLocation, string> locations = new();
    List<(ItemLocation Location, string Name)> extraLocations = new();

    public LRMultiworldGenerator(string inputDir, string outputDir)
    {
        SetupData.Paths["LR"] = "G:\\SteamLibrary\\steamapps\\common\\LIGHTNING RETURNS FINAL FANTASY XIII";
        SetupData.Paths["Nova"] = "S:\\Games\\FF13Series\\Nova Chrysalia v2.0.3\\NovaChrysalia.exe";
        DataExtensions.Mode = ByteMode.BigEndian;
        LRFlags.Init();
        SetupData.Seed = "1234567890";

        OutputDir = outputDir;
        var seedGenerator = new LRSeedGenerator();
        TreasureRando = seedGenerator.Get<TreasureRando>();
        EquipRando = seedGenerator.Get<EquipRando>();

        // Set working directory to the input directory
        Directory.SetCurrentDirectory(inputDir);

        seedGenerator.PrepareData();
        seedGenerator.Load();
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
            "class LRFF13Item(Item):\n" +
            "    game: str = \"Lightning Returns: Final Fantasy XIII\"\n" +
            "\n" +
            "\n" +
            "class LRFF13ItemData(NamedTuple):\n" +
            "    code: Optional[int] = None\n" +
            "    str_id: str = \"\"\n" +
            "    classification: ItemClassification = ItemClassification.filler\n" +
            "    category: str = \"\"\n" +
            "    weight: int = 0\n" +
            "    amount: int = 1\n" +
            "    duplicate_amount: int = 1\n" +
            "    traits: list = []\n" +
            "\n" +
            "\n" +
            "item_data_table: Dict[str, LRFF13ItemData] = {\n";

        int nextIndex = 0;
        EquipRando.itemData.Values.ForEach(i =>
        {
            if (!i.Traits.Contains("Ignore") && !i.Traits.Contains("Remove"))
            {
                string type = "filler";
                int weight = 0;
                int duplicates = i.OverrideCount > 0 ? i.OverrideCount : 1;

                // Trait-driven classification first
                if (i.Traits.Contains("Key") || i.Traits.Contains("Progression"))
                {
                    type = "progression";
                }
                else if (i.Traits.Contains("Useful"))
                {
                    type = "useful";
                }
                // Fallbacks based on category
                else if (i.Category == "Key")
                {
                    type = "progression";
                }
                else if (i.Category == "EP Ability" || i.Category == "Garb" || i.Category == "Weapon" || i.Category == "Shield" || i.Category == "Accessory")
                {
                    type = "useful";
                }
                else if (i.Category == "Adornment")
                {
                    weight = 5;
                }
                else
                {
                    // Weight is based on rank using exponential decay from 20 to ~1
                    weight = i.Rank > 10 ? 1 : (int)Math.Ceiling(20 * Math.Pow(0.7, i.Rank) * 10);
                    if (i.Category == "Item")
                    {
                        weight = (int)(weight * 90);
                    }
                    else if (i.Category == "Material")
                    {
                        weight = (int)(weight * 160);
                    }
                }

                script = AddItemToItemsScript(script, i.Name, i.ID, nextIndex, type, i.Category, weight, 1, duplicates, i.Traits);
                nextIndex++;
            }
        });

        int[] gilAmounts = new[] { 10, 500, 1000, 2500, 7500, 20000 };
        int[] gilWeights = new[] { 50, 700, 900, 600, 400, 100 };
        for (int i = 0; i < gilAmounts.Length; i++)
        {
            script = AddItemToItemsScript(script, $"{gilAmounts[i]} Gil", "", nextIndex, "filler", "Gil", gilWeights[i], gilAmounts[i], 0, new List<string>());
            nextIndex++;
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

    private string AddItemToItemsScript(string script, string name, string id, int intIndex, string type, string category, int weight, int amount, int duplicates, List<string> traits)
    {
        script +=
            $"    \"{name}\": LRFF13ItemData(\n" +
            $"        code={intIndex},\n" +
            $"        str_id=\"{id}\",\n" +
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
                $"        duplicate_amount={duplicates}";
        }

        if (traits != null && traits.Count > 0)
        {
            string traitList = string.Join(", ", traits.Select(t => $"\"{t}\""));
            script += $",\n" +
                $"        traits=[{traitList}]";
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
            "class LRFF13Location(Location):\n" +
            "    game: str = \"Lightning Returns: Final Fantasy XIII\"\n" +
            "\n" +
            "\n" +
            "class LRFF13LocationData(NamedTuple):\n" +
            "    region: str\n" +
            "    type: str\n" +
            "    str_id: str\n" +
            "    address: Optional[int] = None\n" +
            "    classification: LocationProgressType = LocationProgressType.DEFAULT\n" +
            "\n" +
            "\n" +
            "location_data_table: Dict[str, LRFF13LocationData] = {\n";

        locations.Clear();

        int nextIndex = 0;
        Dictionary<string, int> nameCounts = TreasureRando.ItemLocations.Values.Select(l => l.Name).GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());
        Dictionary<string, int> usedNames = new();

        TreasureRando.ItemLocations.Values.Where(l => l is not FakeLocation).ToList().ForEach(l =>
        {
            string classification = "DEFAULT";
            // Max EP randomization isn't implemented for AP yet, so exclude EP locations
            if (l.Traits.Contains("Missable") || l.Traits.Contains("EP"))
            {
                classification = "EXCLUDED";
            }

            string name;
            if (nameCounts[l.Name] == 1)
            {
                name = l.Name;
            }
            else
            {
                if (usedNames.ContainsKey(l.Name))
                {
                    usedNames[l.Name]++;
                }
                else
                {
                    usedNames.Add(l.Name, 1);
                }

                name = $"{l.Name} ({usedNames[l.Name]})";
            }

            // Use the first area as the region name for this location
            var regionName = l.Areas != null && l.Areas.Count > 0 ? l.Areas[0] : "Initial";

            name = $"{regionName} - {name}";

            switch (l)
            {
                case TreasureLocation t:
                    script = AddLocationToLocationsScript(script, name, regionName, nextIndex, classification, "treasure", t.ID);
                    break;
                case BattleDropLocation b:
                    script = AddLocationToLocationsScript(script, name, regionName, nextIndex, classification, "battle", b.ID);
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

    private string AddLocationToLocationsScript(string script, string name, string region, int intIndex, string classification, string type, string strId)
    {
        // Map ID and secondary index are optional
        script +=
            $"    \"{name}\": LRFF13LocationData(\n" +
            $"        region=\"{region}\",\n" +
            $"        address={intIndex},\n" +
            $"        classification=LocationProgressType.{classification},\n" +
            $"        type=\"{type}\"";
        if (!string.IsNullOrEmpty(strId))
        {
            script += $",\n" +
                $"        str_id=\"{strId}\"";
        }

        script += "\n    ),\n";

        return script;
    }

    private void GenerateRegionsScript()
    {
        StringBuilder sb = new();
        sb.Append(
            "from typing import Dict, List, NamedTuple, Optional\n" +
            "from BaseClasses import Region\n\n" +
            "class LRFF13Region(Region):\n" +
            "    game: str = \"Lightning Returns: Final Fantasy XIII\"\n\n" +
            "class LRFF13RegionData(NamedTuple):\n" +
            "    connecting_regions: List[str]\n" +
            "    map_id: Optional[int] = None\n" +
            "    secondary_index: Optional[int] = None\n\n" +
            "region_data_table: Dict[str, LRFF13RegionData] = {\n");

        // Build regions dynamically from AreaGraph
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

            sb.Append($"    \"{areaName}\": LRFF13RegionData(connecting_regions=[");
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

    private void GenerateEventsScript()
    {
        extraLocations.Clear();

        string script =
            "from typing import Dict, NamedTuple\n" +
            "\n" +
            "\n" +
            "class LRFF13EventData(NamedTuple):\n" +
            "    region: str\n" +
            "    item: str\n" +
            "\n" +
            "\n" +
            "event_data_table: Dict[str, LRFF13EventData] = {\n";

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

            string baseName = $"{fake.Name} Event ({usedNames[fake.Name]})";

            int count = Math.Max(1, fake.Amount);
            var regionName = fake.Areas != null && fake.Areas.Count > 0 ? fake.Areas[0] : "Initial";
            for (int i = 1; i <= count; i++)
            {
                string newName = count == 1 ? baseName : $"{baseName} [{i}]";

                script += $"    \"{newName}\": LRFF13EventData(\n" +
                          $"        region=\"{regionName}\",\n" +
                          $"        item=\"{fake.FakeItem}\"\n" +
                          $"    ),\n";

                extraLocations.Add((l, newName));
            }
        });

        script += "}\n";
        File.WriteAllText(Path.Combine(OutputDir, "Events.py"), script);
    }

    private void GenerateRulesScript()
    {
        string script =
            "from typing import Callable, Dict, List, Tuple\n" +
            "from BaseClasses import CollectionState, Item\n" +
            "from .RuleLogic import state_has_at_least, item_is_category, state_has_category" +
            "\n" +
            "\n";

    Dictionary<string, string> locationToRules = new();
    Dictionary<string, string> itemRules = new();
        Dictionary<(string From, string To), string> entranceToRules = new();
        List<string> rules = new();

        locations.Keys.ForEach(l =>
        {
            string ruleStr = l.GetArchipelagoRule(EquipRando.GetItemName);
            if (!rules.Contains(ruleStr))
            {
                rules.Add(ruleStr);
            }

            locationToRules.Add(locations[l], ruleStr);

            // Build item rule for Same-type restricted locations
            if (l.Traits != null && l.Traits.Contains("Same"))
            {
                var tItem = l?.GetItem(true);
                string origId = tItem?.Item1;

                if (!string.IsNullOrEmpty(origId) && EquipRando.itemData.ContainsKey(origId))
                {
                    string category = EquipRando.itemData[origId].Category;
                    // Lambda that checks item's category using RuleLogic.item_is_category
                    string itemRule = $"lambda item: item_is_category(item.name, \"{category}\")";
                    itemRules[locations[l]] = itemRule;
                }
            }
        });

        // Add any extra locations (e.g., event splits for amounts)
        extraLocations.ForEach(tuple =>
        {
            var l = tuple.Location;
            var name = tuple.Name;
            string ruleStr = l.GetArchipelagoRule(EquipRando.GetItemName);
            if (!rules.Contains(ruleStr))
            {
                rules.Add(ruleStr);
            }

            locationToRules[name] = ruleStr;
        });

        // Build entrance rules from AreaGraph connections BEFORE emitting rule_data_list
        TreasureRando.AreaGraph.Connections.ForEach(c =>
        {
            // Format rule similar to ItemLocation.GetArchipelagoRule
            string ruleStr = c.Requirements.GetArchipelagoRule(EquipRando.GetItemName);
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

        // Emit full rule_data_list now that we've collected both location and entrance rules
        script += "rule_data_list: List[Callable[[CollectionState, int], bool]] = [\n";
        for (int i = 0; i < rules.Count; i++)
        {
            script += $"    {rules[i]},  # Rule {i}\n";
        }

        script += "]\n\n";

        script += "location_rule_data_table: Dict[str, Callable[[CollectionState, int], bool]] = {\n";
        locationToRules.Keys.ForEach(l =>
        {
            script += $"    \"{l}\": rule_data_list[{rules.IndexOf(locationToRules[l])}],\n";
        });

        script += "}\n";

        // Emit item rules for Same-type restrictions
        script += "\nitem_rule_data_table: Dict[str, Callable[[Item], bool]] = {\n";
        foreach (var kvp in itemRules)
        {
            script += $"    \"{kvp.Key}\": {kvp.Value},\n";
        }
        script += "}\n";

        script += "\nentrance_rule_data_table: Dict[Tuple[str, str], Callable[[CollectionState, int], bool]] = {\n";
        entranceToRules.Keys.ForEach(k =>
        {
            var ruleStr = entranceToRules[k];
            var idx = rules.IndexOf(ruleStr);
            script += $"    (\"{k.From}\", \"{k.To}\"): rule_data_list[{idx}],\n";
        });
        script += "}\n";

        File.WriteAllText(Path.Combine(OutputDir, "Rules.py"), script);

    }
}

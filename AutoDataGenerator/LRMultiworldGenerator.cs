using Bartz24.Data;
using Bartz24.RandoWPF;
using LRRando;

namespace AutoDataGenerator;
internal class LRMultiworldGenerator : BaseMultiworldGenerator
{
    TreasureRando TreasureRando { get; }
    EquipRando EquipRando { get; }

    Dictionary<ItemLocation, string> locations = new();
    List<(ItemLocation Location, string Name)> extraLocations = new();

    public LRMultiworldGenerator(string inputDir, string outputDir) : base(outputDir)
    {
        SetupData.Paths["LR"] = "G:\\SteamLibrary\\steamapps\\common\\LIGHTNING RETURNS FINAL FANTASY XIII";
        SetupData.Paths["Nova"] = "S:\\Games\\FF13Series\\Nova Chrysalia v2.0.3\\NovaChrysalia.exe";
        DataExtensions.Mode = ByteMode.BigEndian;
        LRFlags.Init();
        SetupData.Seed = "1234567890";
        var seedGenerator = new LRSeedGenerator();
        TreasureRando = seedGenerator.Get<TreasureRando>();
        EquipRando = seedGenerator.Get<EquipRando>();

        // Set working directory to the input directory
        Directory.SetCurrentDirectory(inputDir);

        seedGenerator.PrepareData();
        seedGenerator.Load();
    }

    protected override void GenerateItemsScript()
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

        int nextIndex = 1;
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
                else if (i.Category == "EP Ability")
                {
                    type = "useful";
                }
                else if (i.Category == "Adornment" || i.Category == "Garb" || i.Category == "Weapon" || i.Category == "Shield" || i.Category == "Accessory")
                {
                    weight = 50;
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

                if (i.ID.StartsWith("libra_"))
                {
                    type = "filler";
                    weight = 0;
                }

                if (i.Category == "Adornment")
                {
                    type = "progression_deprioritized_skip_balancing";
                }

                script = AddItemToItemsScript(script, i.Name, i.ID, nextIndex, type, i.Category, weight, 1, duplicates, i.Traits);
                nextIndex++;
            }
        });

        int[] gilAmounts = [10, 500, 1000, 2500, 7500, 20000];
        int[] gilWeights = [500, 7000, 9000, 6000, 4000, 1000];
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

    protected override void GenerateLocationsScript()
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

        int nextIndex = 1;
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

    protected override void GenerateRegionsScript()
    {
        File.WriteAllText(
            Path.Combine(OutputDir, "Regions.py"),
            BuildRegionScript("LRFF13RegionData", "LRFF13Region", "Lightning Returns: Final Fantasy XIII", TreasureRando.AreaGraph));
    }

    protected override void GenerateEventsScript()
    {
        extraLocations.Clear();

        string script =
            "from typing import Dict, NamedTuple\n" +
            "\n" +
            "\n" +
            "class LRFF13EventData(NamedTuple):\n" +
            "    region: str\n" +
            "    item: str\n" +
            "    traits: list = []\n" +
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
                          $"        item=\"{fake.FakeItem}\",\n" +
                          $"        traits=[{string.Join(", ", fake.Traits.Select(t => $"\"{t}\""))}]\n" +
                          $"    ),\n";

                extraLocations.Add((l, newName));
            }
        });

        script += "}\n";
        File.WriteAllText(Path.Combine(OutputDir, "Events.py"), script);
    }

    protected override void GenerateRulesScript()
    {
        string gameName = "Lightning Returns: Final Fantasy XIII";
        RuleModuleData data = CreateRuleModuleData();
        Dictionary<string, string> itemRules = new();
        bool needsItemCategoryHelper = false;

        locations.Keys.ForEach(l =>
        {
            AddLocationRule(data, gameName, l, locations[l], EquipRando.GetItemName);

            // Build item rule for Same-type restricted locations
            if (l.Traits != null && l.Traits.Contains("Same"))
            {
                var tItem = l?.GetItem(true);
                string origId = tItem?.Item1;

                if (!string.IsNullOrEmpty(origId) && EquipRando.itemData.ContainsKey(origId))
                {
                    string category = EquipRando.itemData[origId].Category;
                    // Lambda that checks item's category using RuleLogic.item_is_category
                    string itemRule = $"lambda item: item_is_category(item.name, \"{ItemReq.EscapePythonString(category)}\")";
                    itemRules[locations[l]] = itemRule;
                    needsItemCategoryHelper = true;
                }
            }
        });

        // Add any extra locations (e.g., event splits for amounts)
        extraLocations.ForEach(tuple =>
        {
            var l = tuple.Location;
            var name = tuple.Name;
            AddLocationRule(data, gameName, l, name, EquipRando.GetItemName);
        });

        AddEntranceRules(data, gameName, TreasureRando.AreaGraph, EquipRando.GetItemName);

        string script = BuildRulesModule(data, "location_rule_data_table", needsItemCategoryHelper, (sb, _) =>
        {
            sb.Append("\nitem_rule_data_table: Dict[str, Callable[[Item], bool]] = {\n");
            foreach (var kvp in itemRules)
            {
                sb.Append($"    \"{ItemReq.EscapePythonString(kvp.Key)}\": {kvp.Value},\n");
            }
            sb.Append("}\n");
        });

        File.WriteAllText(Path.Combine(OutputDir, "Rules.py"), script);
    }
}

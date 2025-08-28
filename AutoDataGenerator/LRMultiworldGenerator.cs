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

namespace AutoDataGenerator;
internal class LRMultiworldGenerator
{
    public string OutputDir { get; }

    TreasureRando TreasureRando { get; }
    EquipRando EquipRando { get; }

    Dictionary<ItemLocation, string> locations = new();

    public LRMultiworldGenerator(string inputDir, string outputDir)
    {
        SetupData.Paths["LR"] = "G:\\SteamLibrary\\steamapps\\common\\LIGHTNING RETURNS FINAL FANTASY XIII";
        SetupData.Paths["Nova"] = "S:\\Games\\FF13Series\\Nova Chrysalia v1.0.1\\NovaChrysalia.exe";
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
            "    str_id: str\n" +
            "    classification: ItemClassification = ItemClassification.filler\n" +
            "    weight: int = 0\n" +
            "    amount: int = 1\n" +
            "    duplicate_amount: int = 1\n" +
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
                int duplicates = 1;
                if (i.Category == "Key")
                {
                    type = "progression";
                }
                else if (i.Category == "EP Ability")
                {
                    type = "useful";
                }
                else if (i.Category == "Material")
                {
                    weight = 15;
                }
                else if (i.Category == "Adornment")
                {
                    weight = 5;
                }
                else
                {
                    // Weight is a value based on rank using exponential decay from 20 to 1
                    // Rank goes up to 10 using this formula
                    weight = i.Rank > 10 ? 1 : (int)Math.Ceiling(20 * Math.Pow(0.7, i.Rank) * 10);

                    if (i.Category == "Item")
                    {
                        weight = (int)(weight * 2.5);
                    }
                }

                script = AddItemToItemsScript(script, i.Name, i.ID, nextIndex, type, weight, 1, duplicates);
                nextIndex++;
            }
        });

        int[] gilAmounts = new[] { 10, 500, 1000, 2500, 7500, 20000 };
        int[] gilWeights = new[] { 50, 700, 900, 600, 400, 100 };
        for (int i = 0; i < gilAmounts.Length; i++)
        {
            script = AddItemToItemsScript(script, $"{gilAmounts[i]} Gil", "", nextIndex, "filler", gilWeights[i], gilAmounts[i], 0);
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

    private string AddItemToItemsScript(string script, string name, string id, int intIndex, string type, int weight, int amount, int duplicates)
    {
        script +=
            $"    \"{name}\": LRFF13ItemData(\n" +
            $"        code={intIndex},\n" +
            $"        str_id=\"{id}\",\n" +
            $"        classification=ItemClassification.{type}";
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
            "    game: str = \"Lightning Returns Final Fantasy XIII\"\n" +
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
            if (l.Traits.Contains("Missable"))
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

            switch (l)
            {
                case TreasureLocation t:
                    script = AddLocationToLocationsScript(script, name, nextIndex, classification, "treasure", t.ID);
                    break;
                case BattleDropLocation b:
                    script = AddLocationToLocationsScript(script, name, nextIndex, classification, "battle", b.ID);
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

    private string AddLocationToLocationsScript(string script, string name, int intIndex, string classification, string type, string strId)
    {
        // Map ID and secondary index are optional
        script +=
            $"    \"{name}\": LRFF13LocationData(\n" +
            $"        region=\"Nova\",\n" +
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

    private void GenerateEventsScript()
    {
        string script =
            "from typing import Dict, NamedTuple\n" +
            "\n" +
            "\n" +
            "class LRFF13EventData(NamedTuple):\n" +
            "    item: str\n" +
            "    amount: int = 1\n" +
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

            string newName = $"{fake.Name} Event ({usedNames[fake.Name]})";

            script += $"    \"{newName}\": LRFF13EventData(\n" +
            $"        item=\"{fake.FakeItem}\",\n" +
            $"        amount={fake.Amount}\n" +
            $"    ),\n";

            locations.Add(l, newName);
        });

        script += "}\n";
        File.WriteAllText(Path.Combine(OutputDir, "Events.py"), script);
    }

    private void GenerateRulesScript()
    {
        string script =
            "from typing import Callable, Dict, List\n" +
            "from BaseClasses import CollectionState\n" +
            "from .RuleLogic import state_has_at_least" +
            "\n" +
            "\n" +
            "rule_data_list: List[Callable[[CollectionState, int], bool]] = [\n";

        Dictionary<string, string> locationToRules = new();
        List<string> rules = new();

        locations.Keys.ForEach(l =>
        {
            string ruleStr = l.GetArchipelagoRule(EquipRando.GetItemName);
            if (!rules.Contains(ruleStr))
            {
                rules.Add(ruleStr);
            }

            locationToRules.Add(locations[l], ruleStr);
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

        File.WriteAllText(Path.Combine(OutputDir, "Rules.py"), script);

    }
}

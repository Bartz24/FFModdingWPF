using Bartz24.Data;
using Bartz24.FF13_2;
using Bartz24.RandoWPF;
using FF13_2Rando;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AutoDataGenerator;

internal class FF13_2MultiworldGenerator: BaseMultiworldGenerator
{
    TreasureRando TreasureRando { get; }
    EquipRando EquipRando { get; }
    HistoriaCruxRando HistoriaCruxRando { get; }

    Dictionary<ItemLocation, string> locations = new();

    List<(ItemLocation Location, string Name)> extraLocations = new();

    Dictionary<(string From, string Gate), string> ExitToRules { get; } = new();

    Dictionary<string, string> regions = new();

    List<string> fakeLocationKeys = new();

    public FF13_2MultiworldGenerator(string inputDir, string outputDir): base(outputDir)
    {
        // TODO: paths only set locally :)
        SetupData.Paths["13-2"] = "E:\\Programs\\Steam\\steamapps\\common\\FINAL FANTASY XIII-2";
        SetupData.Paths["Nova"] = "E:\\Programs\\Nova Chrysalia 2\\NovaChrysalia.exe";
        DataExtensions.Mode = ByteMode.BigEndian;
        FF13_2Flags.Init();
        SetupData.Seed = "1234567890";
        var seedGenerator = new FF13_2SeedGenerator();
        TreasureRando = seedGenerator.Get<TreasureRando>();
        EquipRando = seedGenerator.Get<EquipRando>();
        HistoriaCruxRando = seedGenerator.Get<HistoriaCruxRando>();

        // Always use wild artefact replacement for archi
        FF13_2Flags.Items.ReplaceWildArtefacts.Enabled = true;

        // Set working directory to the input directory
        Directory.SetCurrentDirectory(inputDir);

        seedGenerator.PrepareData();
        seedGenerator.Load();
    }

    protected override void Prepare()
    {
        TreasureRando.ItemLocations.Values.Where(l => l is FF13_2FakeItemLocation)
            .ForEach(l =>
            {
                FF13_2FakeItemLocation fake = (FF13_2FakeItemLocation)l;
                fakeLocationKeys.Add(fake.ID.Split(":")[0]);
            });
    }

    protected override void GenerateItemsScript()
    {
        // Auto generate the Items.py script
        string script =
            "from typing import Dict, NamedTuple, Optional\n" +
            "from BaseClasses import Item, ItemClassification\n" +
            "\n" +
            "\n" +
            "class FF132Item(Item):\n" +
            "    game: str = \"Final Fantasy XIII-2\"\n" +
            "\n" +
            "\n" +
            "class FF132ItemData(NamedTuple):\n" +
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
            "item_data_table: Dict[str, FF132ItemData] = {\n";

        int nextIndex = 1;

        EquipRando.itemData.Values.ForEach(i =>
        {
            if (!i.Traits.Contains("Ignore") && !i.Traits.Contains("Remove") && !i.Traits.Contains("Fixed"))
            {
                string type = "filler";
                int weight = 0;
                // TODO: does anything have >1-arity in 13-2? capsule?
                int duplicates = i.OverrideCount > 0 ? i.OverrideCount : 1;

                // Trait-driven classification first
                if (i.Traits.Contains("SideKey") || i.Traits.Contains("Artefact") || i.Traits.Contains("Fragment") || i.Traits.Contains("Graviton") || i.Traits.Contains("FragmentSkill"))
                {
                    type = "progression";
                }
                else if (i.Traits.Contains("GateSeal"))
                {
                    type = "useful";
                }
                // Fallbacks based on category
                else if (i.Category == "Key" || i.Category == "Fragment")
                {
                    type = "progression";
                }
                else if (i.Category == "Monster Crystal")
                {
                    type = "useful";
                }
                else if (i.Category == "Adornment" || i.Category == "Map" || i.Category == "Weapon" || i.Category == "Shield" || i.Category == "Accessory")
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

                script = AddItemToItemsScript(script, i.Name, i.ID, nextIndex, type, i.Category, weight, 1, duplicates, i.Traits);
                nextIndex++;
            }
        });

        // Include gil in pool? Might break because it doesn't have a noSync item grant?
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
            $"    \"{name}\": FF132ItemData(\n" +
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
            "class FF132Location(Location):\n" +
            "    game: str = \"Final Fantasy XIII-2\"\n" +
            "\n" +
            "\n" +
            "class FF132LocationData(NamedTuple):\n" +
            "    region: str\n" +
            "    type: str\n" +
            "    str_id: str\n" +
            "    address: Optional[int] = None\n" +
            "    classification: LocationProgressType = LocationProgressType.DEFAULT\n" +
            "    fixed_item: Optional[str] = None\n"+
            "\n" +
            "\n" +
            "location_data_table: Dict[str, FF132LocationData] = {\n";

        locations.Clear();

        int nextIndex = 1;
        Dictionary<string, int> nameCounts = TreasureRando.ItemLocations.Values.Select(l => l.Name).GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());
        Dictionary<string, int> usedNames = new();

        TreasureRando.ItemLocations.Values.Where(l => l is not FF13_2FakeItemLocation).ToList().ForEach(l =>
        {
            string classification = "DEFAULT";
            // Exclude missable locations
            if (l.Traits.Contains("Missable") || l.Traits.Contains("APSkip"))
            {
                classification = "EXCLUDED";
            }

            if (l is SearchItemData)
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

            // Build region from the area union the treasure is available in
            if(l.Areas == null)
            {
                throw new Exception("Location has no area set!");
            }
            var regionName = l.Areas.Count == 0 ? l.Areas[0] : buildUnionRegion(l.Areas);
            if(regionName == "Initial")
            {
                regionName = "Historia Crux";
            }
            var displayRegion = buildDisplayRegion(l.Areas);
            if (!regions.ContainsKey(regionName))
            {
                regions.Add(regionName, displayRegion);
            }

            name = $"{displayRegion} - {name}";

            string fixedItem = null;

            if (l.Traits.Contains("Fixed"))
            {
                var content = l.GetItem(true);
                if (content != null)
                {
                    // Assume always 1-count in these
                    fixedItem = content.Value.Item;
                }
            }

            switch (l)
            {
                case FF13_2ItemLocation t:
                    script = AddLocationToLocationsScript(script, name, regionName, nextIndex, classification, "treasure", t.ID, fixedItem);
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

    private string buildDisplayRegion(List<string> areas)
    {
        if (areas.Contains("Initial"))
        {
            return "Historia Crux";
        }
        if (areas.Count == 1)
        {
            string area = areas[0];
            string areaRegionCode = area.Split("_")[1];
            if (areaRegionCode == "sp")
            {
                return "Void Beyond";
            }
            string areaName = HistoriaCruxConstants.AREA_PREFIX_LOOKUP[areaRegionCode];
            var parts = area.Split("_");
            var timeCode = parts[2];
            string modifiedTime = timeCode;
            if (HistoriaCruxConstants.DATE_SPECIAL_CASES.ContainsKey(area))
            {
                modifiedTime = HistoriaCruxConstants.DATE_SPECIAL_CASES[area] + "AF";
            }
            else if (timeCode.StartsWith("NA"))
            {
                modifiedTime = "???AF";
            }
            else
            {
                modifiedTime = int.Parse(timeCode.Substring(2)).ToString() + "AF";
            }
            return areaName + ": " + modifiedTime;
        }
        else
        {
            string area = areas[0];
            string areaRegionCode = area.Split("_")[1];
            string areaName = HistoriaCruxConstants.AREA_PREFIX_LOOKUP[areaRegionCode];
            if(areaRegionCode == "sp")
            {
                return "Void Beyond";
            }
            var times = areas.Select(area =>
            {
                var parts = area.Split("_");
                var timeCode = parts[2];
                string modifiedTime = timeCode;
                if (HistoriaCruxConstants.DATE_SPECIAL_CASES.ContainsKey(area))
                {
                    modifiedTime = HistoriaCruxConstants.DATE_SPECIAL_CASES[area] + "AF";
                } else if (timeCode.StartsWith("NA")){
                    modifiedTime = "???AF";
                } else
                {
                    modifiedTime = int.Parse(timeCode.Substring(2)).ToString() + "AF";
                }
                return modifiedTime;
            });
            return areaName + ": " + string.Join("/", times);
        }
    }

    private string buildUnionRegion(List<string> areas)
    {
        return string.Join("|", areas.Order());
    }

    private string AddLocationToLocationsScript(string script, string name, string region, int intIndex, string classification, string type, string strId, string fixedItem)
    {
        // Map ID and secondary index are optional
        script +=
            $"    \"{name}\": FF132LocationData(\n" +
            $"        region=\"{region}\",\n" +
            $"        address={intIndex},\n" +
            $"        classification=LocationProgressType.{classification},\n" +
            $"        type=\"{type}\"";
        if (!string.IsNullOrEmpty(strId))
        {
            script += $",\n" +
                $"        str_id=\"{strId}\"";
        }
        if (!string.IsNullOrEmpty(fixedItem))
        {
            script += $",\n" +
                $"        fixed_item=\"{fixedItem}\"";
        }

        script += "\n    ),\n";

        return script;
    }

    protected override void GenerateEventsScript()
    {
        extraLocations.Clear();

        string script =
            "from typing import Dict, NamedTuple\n" +
            "\n" +
            "\n" +
            "class FF132EventData(NamedTuple):\n" +
            "    region: str\n" +
            "    item: str\n" +
            "\n" +
            "\n" +
            "event_data_table: Dict[str, FF132EventData] = {\n";

        Dictionary<string, int> usedNames = new();
        TreasureRando.ItemLocations.Values.Where(l => l is FF13_2FakeItemLocation)
            .ForEach(l =>
        {
            FF13_2FakeItemLocation fake = (FF13_2FakeItemLocation)l;
            if (fake.Traits.Contains("Gate"))
            {
                return;
            }
            if (usedNames.ContainsKey(fake.Name))
            {
                usedNames[fake.Name]++;
            }
            else
            {
                usedNames.Add(fake.Name, 1);
            }

            var idx = usedNames[fake.Name];
            // This always has to be here for the client output to use annoyingly.
            string baseName = $"{fake.Name} Event ({idx})";

            int count = Math.Max(1, fake.Amount);
            var regionName = fake.Areas != null && fake.Areas.Count > 0 ? fake.Areas[0] : "Historia Crux";
            if(regionName == "Initial")
            {
                regionName = "Historia Crux";
            }
            for (int i = 1; i <= count; i++)
            {
                string newName = count == 1 ? baseName : $"{baseName} [{i}]";

                script += $"    \"{newName}\": FF132EventData(\n" +
                          $"        region=\"{regionName}\",\n" +
                          $"        item=\"{fake.FakeItem}\"\n" +
                          $"    ),\n";

                extraLocations.Add((l, newName));
            }
        });

        script += "}\n";
        File.WriteAllText(Path.Combine(OutputDir, "Events.py"), script);
    }

    protected override void GenerateRegionsScript()
    {
        // base regions for each area
        // composite regions for "treasure accessibility groups"
        // empty crux nodes
        // dlc areas
        // etc
        string script =
            "from typing import Dict, NamedTuple\n" +
            "from .RegionTypes import FF132RegionData, FF132RegionType\n" +
            "\n" +
            "\n" +
            "region_data_table: Dict[str, FF132RegionData] = {\n";

        var areaKeys = HistoriaCruxRando.areaData.Keys;

        foreach(var (region, alias) in regions)
        {
            if (areaKeys.Contains(region))
            {
                continue;
            }
            script += $"    \"{region}\": FF132RegionData(connecting_regions=[], type=FF132RegionType.TreasureGroup, alias=\"{alias}\"),\n";
        }

        foreach(var region in areaKeys)
        {
            if(region == "Initial") {
                continue;
            }
            string type = "CruxLocation";
            if (region.Contains("_zz_"))
            {
                type = "EmptyNode";
            }
            var areaData = HistoriaCruxRando.areaData[region];
            if (areaData.Traits.Contains("Paradox"))
            {
                type = "ParadoxEndingLocation";
            } else if (areaData.Traits.Contains("DLC"))
            {
                type = "DLCCruxLocation";
            }

            var connectedRegions = regions.Keys.Where(x => x.Contains(region) && x != region).Select(k => $"\"{k}\"").ToList();

            var alias = regions.ContainsKey(region) ? regions[region] : areaData.Name;

            script += $"    \"{region}\": FF132RegionData(connecting_regions=[{string.Join(", ",connectedRegions)}], type=FF132RegionType.{type}, alias=\"{alias}\"),\n";
        }

        script += "}\n";

        File.WriteAllText(Path.Combine(OutputDir, "Regions.py"), script);

        // logic to join up areas based on area graph from known exists - do this manually for now in EntranceShuffle.py?

        return;
    }

    private string GetItemName(string itemName)
    {
        if (fakeLocationKeys.Contains(itemName))
        {
            return itemName;
        }
        // fake check intercept here
        return EquipRando.GetItemName(itemName);
    }

    protected override void GenerateRulesScript()
    {
        string gameName = "Final Fantasy XIII-2";

        RuleModuleData data = CreateRuleModuleData();
        Dictionary<string, string> itemRules = new();
        bool needsItemCategoryHelper = false;

        locations.Keys.ForEach(l =>
        {
            // TODO: mog level not coming across properly
            // TODO: if the requirement is a fake check item it needs to retain the fake id not the "true" item id.
            AddLocationRule(data, gameName, l, locations[l], GetItemName);

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

        // Add any extra locations
        extraLocations.ForEach(tuple =>
        {
            var l = tuple.Location;
            var name = tuple.Name;
            // Mog levels not coming across here - check fake check stuff
            AddLocationRule(data, gameName, l, name, GetItemName);
        });

        // Entrance rules (skip and place elsewhere?)
        foreach(var location in HistoriaCruxRando.areaData.Keys)
        {
            // resolve outgoing location links
            // resolve requirements on links
            // build map of outgoing link (i.e. gate) to requirements
            // these will be linked together in EntranceShuffle.py separately.
            // TODO: ensure all of the outgoing rules accurately reflect state including fake checks (e.g. sunleth 300 out to VB)
            // Just hack in sunleth/yaschas out links here manually?

            var outgoingLinks = HistoriaCruxRando.areaData[location].OutgoingGates;
            foreach (var outgoingLink in outgoingLinks)
            {
                var gateLocation = TreasureRando.ItemLocations[outgoingLink+":0"];
                AddRequirementPreambles(data.PreambleParts, gateLocation.Requirements, gameName);

                string ruleStr = gateLocation.Requirements.GetArchipelagoRule(GetItemName);
                AddUniqueRule(data.Rules, ruleStr);

                ExitToRules[(location, outgoingLink)] = ruleStr;
            }

            if(location == "h_sn_AD0300")
            {
                var gateLocation = TreasureRando.ItemLocations["hs_snda03_ac:0"];
                AddRequirementPreambles(data.PreambleParts, gateLocation.Requirements, gameName);
                string ruleStr = gateLocation.Requirements.GetArchipelagoRule(GetItemName);
                AddUniqueRule(data.Rules, ruleStr);
                ExitToRules[("h_sn_AD0300", "hs_snda03_ac")] = ruleStr;
            }

            if(location == "h_gh_AD0010")
            {
                var gateLocation = TreasureRando.ItemLocations["hs_ghaa01_cs:0"];
                AddRequirementPreambles(data.PreambleParts, gateLocation.Requirements, gameName);
                string ruleStr = gateLocation.Requirements.GetArchipelagoRule(GetItemName);
                AddUniqueRule(data.Rules, ruleStr);
                ExitToRules[("h_sn_AD0300", "hs_ghaa01_cs")] = ruleStr;
            }

            // Then also add any composite regions that this location links to
            // e.g. bresha 5 links to "bresha (5/100/300)" with no requirement
            foreach (var additionalRegion in regions.Keys)
            {
                if(additionalRegion.Contains(location) && additionalRegion != location)
                {
                    string ruleStr = new BoolItemReq(true).GetArchipelagoRule(GetItemName);
                    ExitToRules[(location, additionalRegion)] = ruleStr;
                }
            }

            // DO NOT populate EntranceToRules to allow for later manipulation
        }

        // location rules table
        string script = BuildRulesModule(data, "location_rule_data_table", needsItemCategoryHelper, (sb, _) =>
        {
            sb.Append("\nitem_rule_data_table: Dict[str, Callable[[Item], bool]] = {\n");
            foreach (var kvp in itemRules)
            {
                sb.Append($"    \"{ItemReq.EscapePythonString(kvp.Key)}\": {kvp.Value},\n");
            }
            sb.Append("}\n");
        });

        script += buildExitRuleTable(data.Rules);

        File.WriteAllText(Path.Combine(OutputDir, "Rules.py"), script);
    }

    private string buildExitRuleTable(List<string> rules)
    {
        if (ExitToRules.Count == 0)
        {
            return "";
        }
        StringBuilder script = new();
        script.Append("\nentrance_rule_data_table: Dict[Tuple[str, str], Rule[Any]] = {\n");
        foreach (var key in ExitToRules.Keys)
        {
            var ruleStr = ExitToRules[key];
            int idx = rules.IndexOf(ruleStr);
            var area = key.From == "Initial" ? "Historia Crux" : key.From;
            // Initial -> Historia Crux
            script.Append($"    (\"{ItemReq.EscapePythonString(area)}\", \"{ItemReq.EscapePythonString(key.Gate)}\"): rule_data_list[{idx}],\n");
        }
        script.Append("}\n");
        return script.ToString();
    }

}

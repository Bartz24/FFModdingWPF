using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data.Areas;
using System.Text;

namespace AutoDataGenerator;

abstract class BaseMultiworldGenerator
{
    protected class RuleModuleData
    {
        public Dictionary<string, string> LocationToRules { get; } = new();
        public Dictionary<string, List<string>> LocationTraits { get; } = new();
        public Dictionary<(string From, string To), string> EntranceToRules { get; } = new();
        public List<string> Rules { get; } = new();
        public HashSet<string> PreambleParts { get; } = new();
    }

    public string OutputDir { get; }

    protected BaseMultiworldGenerator(string outputDir)
    {
        OutputDir = outputDir;
    }

    public void Generate()
    {
        GenerateItemsScript();
        GenerateLocationsScript();
        GenerateEventsScript();
        GenerateRulesScript();
        GenerateRegionsScript();
    }

    protected abstract void GenerateItemsScript();
    protected abstract void GenerateLocationsScript();
    protected abstract void GenerateEventsScript();
    protected abstract void GenerateRulesScript();
    protected abstract void GenerateRegionsScript();

    protected static void AddUniqueRule(List<string> rules, string ruleStr)
    {
        if (!rules.Contains(ruleStr))
        {
            rules.Add(ruleStr);
        }
    }

    protected static void AddRequirementPreambles(HashSet<string> preambleParts, ItemReq requirement, string gameName)
    {
        foreach (string preamble in requirement.GetArchipelagoPreamble(gameName))
        {
            preambleParts.Add(preamble);
        }
    }

    protected static string BuildRulesModulePreamble(
        IEnumerable<string> rules,
        IEnumerable<string> preambleParts,
        bool needsItemCategoryHelper,
        params string[] extraTypingImports)
    {
        StringBuilder script = new();
        script.Append("from __future__ import annotations\n");

        HashSet<string> typingImports = new() { "Any", "Dict", "Tuple" };
        foreach (string importName in extraTypingImports)
        {
            typingImports.Add(importName);
        }

        HashSet<string> baseClassImports = new();
        HashSet<string> ruleImports = new() { "Rule" };

        List<string> preambleList = preambleParts.ToList();
        IEnumerable<string> allRuleText = rules.Concat(preambleList);
        if (allRuleText.Any(r => r.Contains("Has(")))
        {
            ruleImports.Add("Has");
        }
        if (allRuleText.Any(r => r.Contains("CanReachRegion(")))
        {
            ruleImports.Add("CanReachRegion");
        }
        if (allRuleText.Any(r => r.Contains("True_(")))
        {
            ruleImports.Add("True_");
        }
        if (allRuleText.Any(r => r.Contains("False_(")))
        {
            ruleImports.Add("False_");
        }
        if (preambleList.Any(r => r.Contains("CollectionState")))
        {
            baseClassImports.Add("CollectionState");
        }
        if (needsItemCategoryHelper)
        {
            baseClassImports.Add("Item");
        }

        if (needsItemCategoryHelper || preambleList.Any(r => r.Contains("item_data_table")))
        {
            script.Append("from .Items import item_data_table\n");
        }
        if (preambleList.Count > 0)
        {
            script.Append("import dataclasses\n");
        }

        script.Append($"from typing import {string.Join(", ", typingImports.OrderBy(s => s, StringComparer.Ordinal))}\n");
        if (baseClassImports.Count > 0)
        {
            script.Append($"from BaseClasses import {string.Join(", ", baseClassImports.OrderBy(s => s, StringComparer.Ordinal))}\n");
        }
        script.Append($"from rule_builder.rules import {string.Join(", ", ruleImports.OrderBy(s => s, StringComparer.Ordinal))}\n");
        script.Append('\n');

        if (needsItemCategoryHelper)
        {
            script.Append(
                "def item_is_category(item_name: str, category: str) -> bool:\n" +
                "    if item_name not in item_data_table:\n" +
                "        return False\n" +
                "    return item_data_table[item_name].category == category\n" +
                "\n");
        }

        foreach (string preamble in preambleList.OrderBy(s => s, StringComparer.Ordinal))
        {
            script.Append(preamble).Append('\n');
        }

        return script.ToString();
    }

    protected static RuleModuleData CreateRuleModuleData() => new();

    protected static void AddLocationRule(
        RuleModuleData data,
        string gameName,
        ItemLocation location,
        string locationName,
        Func<string, string> itemNameFunc)
    {
        AddRequirementPreambles(data.PreambleParts, location.Requirements, gameName);

        string ruleStr = location.GetArchipelagoRule(itemNameFunc);
        AddUniqueRule(data.Rules, ruleStr);

        data.LocationToRules[locationName] = ruleStr;
        data.LocationTraits[locationName] = location.Traits?.OrderBy(t => t, StringComparer.Ordinal).ToList() ?? new List<string>();
    }

    protected static void AddEntranceRules(
        RuleModuleData data,
        string gameName,
        AreaGraph areaGraph,
        Func<string, string> itemNameFunc)
    {
        areaGraph.Connections.ForEach(c =>
        {
            AddRequirementPreambles(data.PreambleParts, c.Requirements, gameName);

            string ruleStr = c.Requirements.GetArchipelagoRule(itemNameFunc);
            AddUniqueRule(data.Rules, ruleStr);

            data.EntranceToRules[(c.FromAreaName, c.ToAreaName)] = ruleStr;
        });
    }

    protected static string BuildRulesModule(
        RuleModuleData data,
        string locationTableName,
        bool needsItemCategoryHelper,
        Action<StringBuilder, RuleModuleData>? appendExtra = null,
        params string[] extraTypingImports)
    {
        StringBuilder script = new();
        script.Append(BuildRulesModulePreamble(data.Rules, data.PreambleParts, needsItemCategoryHelper, extraTypingImports));
        script.Append(BuildRuleDataList(data.Rules));
        script.Append(BuildLocationTraitDataTable(data.LocationTraits));
        script.Append(BuildLocationRuleTable(locationTableName, data.LocationToRules, data.Rules));
        appendExtra?.Invoke(script, data);
        script.Append(BuildEntranceRuleTable(data.EntranceToRules, data.Rules));
        return script.ToString();
    }

    protected static string BuildRegionScript(string regionImportName, string regionClassName, string gameName, AreaGraph areaGraph)
    {
        StringBuilder script = new();
        script.Append(
            "from typing import Dict, List, NamedTuple, Optional\n" +
            "from BaseClasses import Region\n\n" +
            $"class {regionClassName}(Region):\n" +
            $"    game: str = \"{gameName}\"\n\n" +
            $"class {regionImportName}(NamedTuple):\n" +
            "    connecting_regions: List[str]\n" +
            "    map_id: Optional[int] = None\n" +
            "    secondary_index: Optional[int] = None\n\n" +
            $"region_data_table: Dict[str, {regionImportName}] = {{\n");

        List<string> areas = areaGraph.Areas.Keys.OrderBy(n => n, StringComparer.Ordinal).ToList();
        foreach (string areaName in areas)
        {
            List<string> connections = areaGraph.Connections
                .Where(c => c.FromAreaName == areaName)
                .Select(c => c.ToAreaName)
                .Distinct()
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToList();

            script.Append($"    \"{areaName}\": {regionImportName}(connecting_regions=[");
            for (int i = 0; i < connections.Count; i++)
            {
                if (i > 0)
                {
                    script.Append(", ");
                }

                script.Append($"\"{connections[i]}\"");
            }

            script.Append("]),\n");
        }

        script.Append("}\n");
        return script.ToString();
    }

    protected static string BuildRuleDataList(List<string> rules)
    {
        StringBuilder script = new();
        script.Append("rule_data_list: list[Rule[Any]] = [\n");
        for (int i = 0; i < rules.Count; i++)
        {
            script.Append($"    {rules[i]},  # Rule {i}\n");
        }
        script.Append("]\n\n");
        return script.ToString();
    }

    protected static string BuildLocationTraitDataTable(Dictionary<string, List<string>> locationTraits)
    {
        StringBuilder script = new();
        script.Append("location_trait_data_table = {\n");
        foreach (var kvp in locationTraits.OrderBy(k => k.Key, StringComparer.Ordinal))
        {
            List<string> escapedTraits = kvp.Value.Select(t => $"\"{ItemReq.EscapePythonString(t)}\"").ToList();
            string traits = escapedTraits.Count switch
            {
                0 => "()",
                1 => $"({escapedTraits[0]})",
                _ => $"({string.Join(", ", escapedTraits)})",
            };
            script.Append($"    \"{ItemReq.EscapePythonString(kvp.Key)}\": {traits},\n");
        }
        script.Append("}\n\n");
        return script.ToString();
    }

    protected static string BuildLocationRuleTable(string tableName, Dictionary<string, string> locationToRules, List<string> rules)
    {
        StringBuilder script = new();
        script.Append($"{tableName}: Dict[str, Rule[Any]] = {{\n");
        foreach (string locationName in locationToRules.Keys)
        {
            script.Append($"    \"{ItemReq.EscapePythonString(locationName)}\": rule_data_list[{rules.IndexOf(locationToRules[locationName])}],\n");
        }
        script.Append("}\n");
        return script.ToString();
    }

    protected static string BuildEntranceRuleTable(Dictionary<(string From, string To), string> entranceToRules, List<string> rules)
    {
        if(entranceToRules.Count == 0)
        {
            return "";
        }
        StringBuilder script = new();
        script.Append("\nentrance_rule_data_table: Dict[Tuple[str, str], Rule[Any]] = {\n");
        foreach (var key in entranceToRules.Keys)
        {
            int idx = rules.IndexOf(entranceToRules[key]);
            script.Append($"    (\"{ItemReq.EscapePythonString(key.From)}\", \"{ItemReq.EscapePythonString(key.To)}\"): rule_data_list[{idx}],\n");
        }
        script.Append("}\n");
        return script.ToString();
    }
}

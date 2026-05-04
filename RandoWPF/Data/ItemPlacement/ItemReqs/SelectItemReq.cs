using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;
public class SelectItemReq : ItemReq
{
    private readonly int count;
    private readonly List<ItemReq> reqs = new();
    public SelectItemReq(int count, List<ItemReq> reqs)
    {
        this.reqs = reqs;
        this.count = count;
    }
    protected override bool IsMet(ProgressionState state)
    {
        return reqs.Where(r => r.IsValid(state)).Count() >= count;
    }

    public override bool HasUpperBound()
    {
        foreach (ItemReq req in reqs)
        {
            if (req.HasUpperBound())
            {
                return true;
            }
        }
        return false;
    }

    protected override List<string> GetPossibleRequirementsImpl()
    {
        return reqs.SelectMany(r => r.GetPossibleRequirements()).Distinct().ToList();
    }
    public override int GetPossibleRequirementsCount() { return count; }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        return $"At least {count} of ({string.Join(", ", reqs.Select(r => r.GetDisplay(itemNameFunc)))})";
    }

    public override int GetDifficulty(ProgressionState state)
    {
        int minDiff = int.MaxValue;
        foreach (List<ItemReq> reqSubset in reqs.GetAllSubsets(count))
        {
            ItemReq and = ItemReq.And(reqSubset.ToArray());
            int diff = and.GetDifficulty(state);

            if (and.IsValid(state) && diff >= 0)
            {
                minDiff = Math.Min(minDiff, diff);
            }
        }

        if (minDiff == int.MaxValue)
        {
            return -1;
        }

        return base.GetDifficulty(state) + minDiff;
    }

    public override bool Equals(object obj)
    {
        return obj is SelectItemReq req &&
               count == req.count &&
               Enumerable.SequenceEqual(reqs, req.reqs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(count, reqs);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        return $"AtLeastRule({count}, {string.Join(", ", reqs.Select(r => r.GetArchipelagoRule(itemNameFunc)))})";
    }

    public override IEnumerable<string> GetArchipelagoPreamble(string gameName)
    {
        string escapedGameName = EscapePythonString(gameName);
        yield return
            "@dataclasses.dataclass(init=False)\n" +
            $"class AtLeastRule(Rule[Any], game=\"{escapedGameName}\"):\n" +
            "    count: int\n" +
            "    children: tuple[Rule[Any], ...]\n" +
            "\n" +
            "    def __init__(self, count: int, *children: Rule[Any]):\n" +
            "        super().__init__()\n" +
            "        self.count = count\n" +
            "        self.children = children\n" +
            "\n" +
            "    def _instantiate(self, world):\n" +
            "        resolved_children = tuple(child.resolve(world) for child in self.children)\n" +
            "        if self.count <= 0:\n" +
            "            return True_().resolve(world)\n" +
            "        if self.count > len(resolved_children):\n" +
            "            return False_().resolve(world)\n" +
            "        return self.Resolved(\n" +
            "            self.count,\n" +
            "            resolved_children,\n" +
            "            player=world.player,\n" +
            "            caching_enabled=getattr(world, \"rule_caching_enabled\", False),\n" +
            "        )\n" +
            "\n" +
            "    class Resolved(Rule.Resolved):\n" +
            "        count: int\n" +
            "        children: tuple[Rule.Resolved, ...]\n" +
            "\n" +
            "        def _evaluate(self, state: CollectionState) -> bool:\n" +
            "            found = 0\n" +
            "            for child in self.children:\n" +
            "                if child(state):\n" +
            "                    found += 1\n" +
            "                    if found >= self.count:\n" +
            "                        return True\n" +
            "            return False\n" +
            "\n" +
            "        def item_dependencies(self) -> dict[str, set[int]]:\n" +
            "            deps: dict[str, set[int]] = {}\n" +
            "            for child in self.children:\n" +
            "                for item_name, rules in child.item_dependencies().items():\n" +
            "                    deps.setdefault(item_name, {id(self)}).update(rules)\n" +
            "            return deps\n" +
            "\n" +
            "        def region_dependencies(self) -> dict[str, set[int]]:\n" +
            "            deps: dict[str, set[int]] = {}\n" +
            "            for child in self.children:\n" +
            "                for region_name, rules in child.region_dependencies().items():\n" +
            "                    deps.setdefault(region_name, {id(self)}).update(rules)\n" +
            "            return deps\n" +
            "\n" +
            "        def location_dependencies(self) -> dict[str, set[int]]:\n" +
            "            deps: dict[str, set[int]] = {}\n" +
            "            for child in self.children:\n" +
            "                for location_name, rules in child.location_dependencies().items():\n" +
            "                    deps.setdefault(location_name, {id(self)}).update(rules)\n" +
            "            return deps\n" +
            "\n" +
            "        def entrance_dependencies(self) -> dict[str, set[int]]:\n" +
            "            deps: dict[str, set[int]] = {}\n" +
            "            for child in self.children:\n" +
            "                for entrance_name, rules in child.entrance_dependencies().items():\n" +
            "                    deps.setdefault(entrance_name, {id(self)}).update(rules)\n" +
            "            return deps\n";

        foreach (string preamble in reqs.SelectMany(r => r.GetArchipelagoPreamble(gameName)).Distinct())
        {
            yield return preamble;
        }
    }

    public override List<T> GetOf<T>()
    {
        List<T> list = new();
        foreach (ItemReq req in reqs)
        {
            if (req is T tReq)
            {
                list.Add(tReq);
            }

            list.AddRange(req.GetOf<T>());
        }

        return list;
    }
}

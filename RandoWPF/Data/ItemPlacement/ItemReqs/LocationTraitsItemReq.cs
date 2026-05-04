using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class LocationTraitsItemReq : ItemReq
{
    private readonly string trait;
    private readonly int amount;
    public LocationTraitsItemReq(string trait, int amount)
    {
        this.trait = trait;
        this.amount = amount;
    }
    protected override bool IsMet(ProgressionState state)
    {
        var dict = GetItemLocations();
        return state.LocationsCompleted.Where(name => dict[name].Traits.Contains(trait)).Count() >= amount;
    }

    protected override List<string> GetPossibleRequirementsImpl()
    {
        var dict = GetItemLocations();
        return dict.Where(kv => kv.Value != null && kv.Value.Traits.Contains(trait)).Select(kv => kv.Key).ToList();
    }
    public override int GetPossibleRequirementsCount() { return amount; }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        return $"{amount} {trait}(s)";
    }

    public override int GetDifficulty(ProgressionState state)
    {
        if (!IsValid(state))
        {
            return -1;
        }

        return base.GetDifficulty(state) + amount;
    }

    public override bool Equals(object obj)
    {
        return obj is LocationTraitsItemReq req &&
               trait == req.trait &&
               amount == req.amount;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(trait, amount);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        return $"HasLocationTraitRule(\"{EscapePythonString(trait)}\", {amount})";
    }

    public override IEnumerable<string> GetArchipelagoPreamble(string gameName)
    {
        string escapedGameName = EscapePythonString(gameName);
        yield return
            "@dataclasses.dataclass()\n" +
            $"class HasLocationTraitRule(Rule[Any], game=\"{escapedGameName}\"):\n" +
            "    trait: str\n" +
            "    count: int = 1\n" +
            "\n" +
            "    def _instantiate(self, world):\n" +
            "        location_names = tuple(sorted(name for name, traits in location_trait_data_table.items() if self.trait in traits))\n" +
            "        return self.Resolved(\n" +
            "            self.trait,\n" +
            "            location_names,\n" +
            "            count=self.count,\n" +
            "            player=world.player,\n" +
            "            caching_enabled=getattr(world, \"rule_caching_enabled\", False),\n" +
            "        )\n" +
            "\n" +
            "    class Resolved(Rule.Resolved):\n" +
            "        trait: str\n" +
            "        location_names: tuple[str, ...]\n" +
            "        count: int = 1\n" +
            "\n" +
            "        def _evaluate(self, state: CollectionState) -> bool:\n" +
            "            found = 0\n" +
            "            for location in state.locations_checked:\n" +
            "                if location.player == self.player and location.name in self.location_names:\n" +
            "                    found += 1\n" +
            "                    if found >= self.count:\n" +
            "                        return True\n" +
            "            return False\n" +
            "\n" +
            "        def location_dependencies(self) -> dict[str, set[int]]:\n" +
            "            return {location_name: {id(self)} for location_name in self.location_names}\n";
    }

    public override List<T> GetOf<T>()
    {
        return new List<T>();
    }
}

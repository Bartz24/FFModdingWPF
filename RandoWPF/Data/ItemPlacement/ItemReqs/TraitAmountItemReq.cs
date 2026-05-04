using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class TraitAmountItemReq : ItemReq
{
    private readonly string trait;
    private readonly int amount;
    public TraitAmountItemReq(string trait, int amount)
    {
        this.trait = trait;
        this.amount = amount;
    }
    protected override bool IsMet(ProgressionState state)
    {
        return state.ItemsAvailable.Where(kv =>
        {
            var dict = ItemReq.GetItems();
            return dict.ContainsKey(kv.Key) && dict[kv.Key].Traits.Contains(trait);
        }).Sum(kv => kv.Value) >= amount;
    }

    protected override List<string> GetPossibleRequirementsImpl()
    {
        var dict = ItemReq.GetItems();
        return dict.Where(kv => kv.Value?.Traits.Contains(trait) == true).Select(kv => kv.Key).ToList();
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
        return obj is TraitAmountItemReq req &&
               trait == req.trait &&
               amount == req.amount;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(trait, amount);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        return $"HasTraitRule(\"{EscapePythonString(trait)}\", {amount})";
    }

    public override IEnumerable<string> GetArchipelagoPreamble(string gameName)
    {
        string escapedGameName = EscapePythonString(gameName);
        yield return
            "@dataclasses.dataclass()\n" +
            $"class HasTraitRule(Rule[Any], game=\"{escapedGameName}\"):\n" +
            "    trait: str\n" +
            "    count: int = 1\n" +
            "\n" +
            "    def _instantiate(self, world):\n" +
            "        item_names = tuple(sorted(name for name, data in item_data_table.items() if self.trait in data.traits))\n" +
            "        return self.Resolved(\n" +
            "            self.trait,\n" +
            "            item_names,\n" +
            "            count=self.count,\n" +
            "            player=world.player,\n" +
            "            caching_enabled=getattr(world, \"rule_caching_enabled\", False),\n" +
            "        )\n" +
            "\n" +
            "    class Resolved(Rule.Resolved):\n" +
            "        trait: str\n" +
            "        item_names: tuple[str, ...]\n" +
            "        count: int = 1\n" +
            "\n" +
            "        def _evaluate(self, state: CollectionState) -> bool:\n" +
            "            return state.count_from_list(self.item_names, self.player) >= self.count\n" +
            "\n" +
            "        def item_dependencies(self) -> dict[str, set[int]]:\n" +
            "            return {item_name: {id(self)} for item_name in self.item_names}\n";
    }
    public override List<T> GetOf<T>()
    {
        return new List<T>();
    }
}

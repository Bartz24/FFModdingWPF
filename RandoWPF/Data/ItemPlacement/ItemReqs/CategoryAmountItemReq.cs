using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class CategoryAmountItemReq : ItemReq
{
    private readonly string category;
    private readonly int amount;
    public CategoryAmountItemReq(string category, int amount)
    {
        this.category = category;
        this.amount = amount;
    }
    protected override bool IsMet(ProgressionState state)
    {
        return state.ItemsAvailable.Where(kv =>
        {
            var dict = ItemReq.GetItems();
            return dict.ContainsKey(kv.Key) && dict[kv.Key].Category == category;
        }).Sum(kv => kv.Value) >= amount;
    }

    protected override List<string> GetPossibleRequirementsImpl()
    {
        var dict = ItemReq.GetItems();
        return dict.Where(kv => kv.Value?.Category == category).Select(kv => kv.Key).ToList();
    }
    public override int GetPossibleRequirementsCount() { return amount; }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        return $"{amount} {category}(s)";
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
        return obj is CategoryAmountItemReq req &&
               category == req.category &&
               amount == req.amount;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(category, amount);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        return $"HasCategoryRule(\"{EscapePythonString(category)}\", {amount})";
    }

    public override IEnumerable<string> GetArchipelagoPreamble(string gameName)
    {
        string escapedGameName = EscapePythonString(gameName);
        yield return
            "@dataclasses.dataclass()\n" +
            $"class HasCategoryRule(Rule[Any], game=\"{escapedGameName}\"):\n" +
            "    category: str\n" +
            "    count: int = 1\n" +
            "\n" +
            "    def _instantiate(self, world):\n" +
            "        item_names = tuple(sorted(name for name, data in item_data_table.items() if data.category == self.category))\n" +
            "        return self.Resolved(\n" +
            "            self.category,\n" +
            "            item_names,\n" +
            "            count=self.count,\n" +
            "            player=world.player,\n" +
            "            caching_enabled=getattr(world, \"rule_caching_enabled\", False),\n" +
            "        )\n" +
            "\n" +
            "    class Resolved(Rule.Resolved):\n" +
            "        category: str\n" +
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

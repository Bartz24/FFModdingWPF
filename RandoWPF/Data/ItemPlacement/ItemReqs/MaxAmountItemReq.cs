using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class MaxAmountItemReq : ItemReq
{
    private readonly string item;
    private readonly int amount;
    public MaxAmountItemReq(string item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
    protected override bool IsMet(ProgressionState state)
    {
        return state.ItemsAvailable.ContainsKey(item) && state.ItemsAvailable[item] < amount;
    }

    public override bool HasUpperBound()
    {
        return true;
    }

    protected override List<string> GetPossibleRequirementsImpl()
    {
        return new string[] { item }.ToList();
    }
    public override int GetPossibleRequirementsCount() { return amount; }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        if (amount == 1)
        {
            return $"No {itemNameFunc(item)}";
        }

        return $"{itemNameFunc(item)} < {amount}";
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
        return obj is MaxAmountItemReq req &&
               item == req.item &&
               amount == req.amount;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(item, amount);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        return $"ItemCountLessThanRule(\"{EscapePythonString(itemNameFunc(item))}\", {amount})";
    }

    public override IEnumerable<string> GetArchipelagoPreamble(string gameName)
    {
        string escapedGameName = EscapePythonString(gameName);
        yield return
            "@dataclasses.dataclass()\n" +
            $"class ItemCountLessThanRule(Rule[Any], game=\"{escapedGameName}\"):\n" +
            "    item_name: str\n" +
            "    count: int = 1\n" +
            "\n" +
            "    def _instantiate(self, world):\n" +
            "        return self.Resolved(\n" +
            "            self.item_name,\n" +
            "            count=self.count,\n" +
            "            player=world.player,\n" +
            "            caching_enabled=getattr(world, \"rule_caching_enabled\", False),\n" +
            "        )\n" +
            "\n" +
            "    class Resolved(Rule.Resolved):\n" +
            "        item_name: str\n" +
            "        count: int = 1\n" +
            "\n" +
            "        def _evaluate(self, state: CollectionState) -> bool:\n" +
            "            return state.count(self.item_name, self.player) < self.count\n" +
            "\n" +
            "        def item_dependencies(self) -> dict[str, set[int]]:\n" +
            "            return {self.item_name: set()}\n";
    }

    public override List<T> GetOf<T>()
    {
        return new List<T>();
    }
}

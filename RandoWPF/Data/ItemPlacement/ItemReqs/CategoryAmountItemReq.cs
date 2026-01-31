using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        return $"state_has_category(state, player, \"{category}\", {amount})";
    }
    public override List<T> GetOf<T>()
    {
        return new List<T>();
    }
}

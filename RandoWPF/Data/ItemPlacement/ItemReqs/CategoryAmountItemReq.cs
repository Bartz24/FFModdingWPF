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
    protected override bool IsMet(Dictionary<string, int> itemsAvailable)
    {
        return itemsAvailable.Where(kv =>
        {
            var dict = ItemReq.ItemProvider();
            return dict.ContainsKey(kv.Key) && dict[kv.Key].Category == category;
        }).Sum(kv => kv.Value) >= amount;
    }

    protected override List<string> GetPossibleRequirementsImpl()
    {
        var dict = ItemReq.ItemProvider();
        return dict.Where(kv => kv.Value?.Category == category).Select(kv => kv.Key).ToList();
    }
    public override int GetPossibleRequirementsCount() { return amount; }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        return $"{amount} {category}(s)";
    }

    public override int GetDifficulty(Dictionary<string, int> itemsAvailable)
    {
        if (!IsValid(itemsAvailable))
        {
            return -1;
        }

        return base.GetDifficulty(itemsAvailable) + amount;
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
}

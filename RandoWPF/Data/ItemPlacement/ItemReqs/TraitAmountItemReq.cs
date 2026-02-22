using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        throw new NotImplementedException("TODO: Implement this");
    }
    public override List<T> GetOf<T>()
    {
        return new List<T>();
    }
}

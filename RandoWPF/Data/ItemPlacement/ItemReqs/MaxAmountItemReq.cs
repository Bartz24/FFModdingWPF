using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    protected override bool IsMet(Dictionary<string, int> itemsAvailable)
    {
        return itemsAvailable.ContainsKey(item) && itemsAvailable[item] < amount;
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
        // TODO: Archipelago doen't support not state.has, so until then...just return true
        return "True";

        /*if (amount == 1)
        {
            return $"not state.has(\"{itemNameFunc(item)}\", player)";
        }
        else
        {
            return $"not state.has(\"{itemNameFunc(item)}\", player, {amount})";
        }*/
    }
}

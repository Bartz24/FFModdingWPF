using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class AmountItemReq : ItemReq
{
    private readonly string item;
    private readonly int amount;
    public AmountItemReq(string item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
    protected override bool IsMet(ProgressionState state)
    {
        return state.ItemsAvailable.ContainsKey(item) && state.ItemsAvailable[item] >= amount;
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
            return itemNameFunc(item);
        }

        return $"{itemNameFunc(item)} x {amount}";
    }

    public override int GetDifficulty(ProgressionState state)
    {
        if (!IsValid(state))
        {
            return -1;
        }

        return base.GetDifficulty(state) + (int)Math.Round(Math.Sqrt(amount));
    }

    public override bool Equals(object obj)
    {
        return obj is AmountItemReq req &&
               item == req.item &&
               amount == req.amount;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(item, amount);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        if (amount == 1)
        {
            return $"state.has(\"{itemNameFunc(item)}\", player)";
        }
        else
        {
            return $"state.has(\"{itemNameFunc(item)}\", player, {amount})";
        }
    }

    public override List<T> GetOf<T>()
    {
        return new List<T>();
    }
}

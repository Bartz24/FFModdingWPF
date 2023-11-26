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
    protected override bool IsValidImpl(Dictionary<string, int> itemsAvailable)
    {
        return itemsAvailable.ContainsKey(item) && itemsAvailable[item] >= amount;
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
}

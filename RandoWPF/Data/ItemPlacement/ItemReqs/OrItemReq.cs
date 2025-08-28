using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class OrItemReq : ItemReq
{
    private readonly List<ItemReq> reqs = new();
    public OrItemReq(List<ItemReq> reqs)
    {
        this.reqs = reqs;
    }
    protected override bool IsMet(Dictionary<string, int> itemsAvailable)
    {
        foreach (ItemReq req in reqs)
        {
            if (req.IsValid(itemsAvailable))
            {
                return true;
            }
        }

        return false;
    }

    protected override List<string> GetPossibleRequirementsImpl()
    {
        return reqs.SelectMany(r => r.GetPossibleRequirements()).Distinct().ToList();
    }
    public override int GetPossibleRequirementsCount() { return reqs.Select(r => r.GetPossibleRequirementsCount()).Sum(); }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        return $"({string.Join(" OR ", reqs.Select(r => r.GetDisplay(itemNameFunc)))})";
    }

    public override int GetDifficulty(Dictionary<string, int> itemsAvailable)
    {
        int minDiff = int.MaxValue;
        foreach (ItemReq req in reqs)
        {
            int diff = req.GetDifficulty(itemsAvailable);
            if (req.IsValid(itemsAvailable) && diff >= 0)
            {
                minDiff = Math.Min(minDiff, diff);
            }
        }

        if (minDiff == int.MaxValue)
        {
            return -1;
        }

        return base.GetDifficulty(itemsAvailable) + minDiff;
    }

    public override bool Equals(object obj)
    {
        return obj is OrItemReq req &&
               Enumerable.SequenceEqual(reqs, req.reqs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(reqs);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        return $"({string.Join(" or\n", reqs.Select(r => r.GetArchipelagoRule(itemNameFunc)))})";
    }
}

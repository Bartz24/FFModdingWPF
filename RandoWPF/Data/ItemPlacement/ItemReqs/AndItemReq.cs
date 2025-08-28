using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class AndItemReq : ItemReq
{
    private readonly List<ItemReq> reqs = new();
    public AndItemReq(List<ItemReq> reqs)
    {
        this.reqs = reqs;
    }
    protected override bool IsMet(Dictionary<string, int> itemsAvailable)
    {
        foreach (ItemReq req in reqs)
        {
            if (!req.IsValid(itemsAvailable))
            {
                return false;
            }
        }

        return true;
    }

    protected override List<string> GetPossibleRequirementsImpl()
    {
        return reqs.SelectMany(r => r.GetPossibleRequirements()).Distinct().ToList();
    }
    public override int GetPossibleRequirementsCount() { return reqs.Select(r => r.GetPossibleRequirementsCount()).Sum(); }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        return $"({string.Join(" AND ", reqs.Select(r => r.GetDisplay(itemNameFunc)))})";
    }

    public override int GetDifficulty(Dictionary<string, int> itemsAvailable)
    {
        List<int> diffs = new();
        foreach (ItemReq req in reqs)
        {
            int diff = req.GetDifficulty(itemsAvailable);
            if (!req.IsValid(itemsAvailable) || diff < 0)
            {
                return -1;
            }
        }

        return base.GetDifficulty(itemsAvailable) + diffs.DefaultIfEmpty(0).Sum();
    }

    public override bool Equals(object obj)
    {
        return obj is AndItemReq req &&
               Enumerable.SequenceEqual(reqs, req.reqs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(reqs);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        return $"({string.Join(" and\n", reqs.Select(r => r.GetArchipelagoRule(itemNameFunc)))})";
    }
}

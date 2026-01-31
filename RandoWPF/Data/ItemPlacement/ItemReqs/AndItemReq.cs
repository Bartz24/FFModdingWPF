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
    protected override bool IsMet(ProgressionState state)
    {
        foreach (ItemReq req in reqs)
        {
            if (!req.IsValid(state))
            {
                return false;
            }
        }

        return true;
    }

    public override bool HasUpperBound()
    {
        foreach(ItemReq req in reqs)
        {
            if (req.HasUpperBound())
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
        return $"({string.Join(" AND ", reqs.Select(r => r.GetDisplay(itemNameFunc)))})";
    }

    public override int GetDifficulty(ProgressionState state)
    {
        List<int> diffs = new();
        foreach (ItemReq req in reqs)
        {
            int diff = req.GetDifficulty(state);
            if (!req.IsValid(state) || diff < 0)
            {
                return -1;
            }
        }

        return base.GetDifficulty(state) + diffs.DefaultIfEmpty(0).Sum();
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

    public override List<T> GetOf<T>()
    {
        List<T> list = new();
        foreach (ItemReq req in reqs)
        {
            if (req is T tReq)
            {
                list.Add(tReq);
            }

            list.AddRange(req.GetOf<T>());
        }

        return list;
    }
}

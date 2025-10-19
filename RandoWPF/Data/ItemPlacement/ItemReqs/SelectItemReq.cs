using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public class SelectItemReq : ItemReq
{
    private readonly int count;
    private readonly List<ItemReq> reqs = new();
    public SelectItemReq(int count, List<ItemReq> reqs)
    {
        this.reqs = reqs;
        this.count = count;
    }
    protected override bool IsMet(ProgressionState state)
    {
        return reqs.Where(r => r.IsValid(state)).Count() >= count;
    }

    public override bool HasUpperBound()
    {
        foreach (ItemReq req in reqs)
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
    public override int GetPossibleRequirementsCount() { return count; }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        return $"At least {count} of ({string.Join(", ", reqs.Select(r => r.GetDisplay(itemNameFunc)))})";
    }

    public override int GetDifficulty(ProgressionState state)
    {
        int minDiff = int.MaxValue;
        foreach (List<ItemReq> reqSubset in reqs.GetAllSubsets(count))
        {
            ItemReq and = ItemReq.And(reqSubset.ToArray());
            int diff = and.GetDifficulty(state);

            if (and.IsValid(state) && diff >= 0)
            {
                minDiff = Math.Min(minDiff, diff);
            }
        }

        if (minDiff == int.MaxValue)
        {
            return -1;
        }

        return base.GetDifficulty(state) + minDiff;
    }

    public override bool Equals(object obj)
    {
        return obj is SelectItemReq req &&
               count == req.count &&
               Enumerable.SequenceEqual(reqs, req.reqs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(count, reqs);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        string list = $"[{string.Join(",\n", reqs.Select(r => r.GetArchipelagoRule(itemNameFunc)))}]";
        return $"state_has_at_least({list}, {count})";
    }
}

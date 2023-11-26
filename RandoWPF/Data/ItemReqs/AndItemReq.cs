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
    protected override bool IsValidImpl(Dictionary<string, int> itemsAvailable)
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
}

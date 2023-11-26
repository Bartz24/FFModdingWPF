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
    protected override bool IsValidImpl(Dictionary<string, int> itemsAvailable)
    {
        foreach (ItemReq req in reqs)
        {
            if (!req.IsValid(itemsAvailable))
            {
                return false;
            }
        }

        return reqs.Where(r => r.IsValid(itemsAvailable)).Count() >= count;
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
}

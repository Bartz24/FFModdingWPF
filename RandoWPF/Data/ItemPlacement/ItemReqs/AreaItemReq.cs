using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class AreaItemReq : ItemReq
{
    private readonly string area;
    public AreaItemReq(string area)
    {
        this.area = area;
    }
    protected override bool IsMet(ProgressionState state)
    {
        return state.AreasAccessible.Contains(area);
    }

    protected override List<string> GetPossibleRequirementsImpl()
    {
        return new string[] { area }.ToList();
    }
    public override int GetPossibleRequirementsCount() { return 1; }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        return $"{area} Access";
    }

    public override int GetDifficulty(ProgressionState state)
    {
        if (!IsValid(state))
        {
            return -1;
        }

        return base.GetDifficulty(state);
    }

    public override bool Equals(object obj)
    {
        return obj is AreaItemReq req &&
               area == req.area;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(area);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        return $"state.can_reach_region(\"{area}\", player)";
    }
}

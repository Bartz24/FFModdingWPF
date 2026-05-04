using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class AreaItemReq : ItemReq
{
    public string Area { get; }
    public AreaItemReq(string area)
    {
        this.Area = area;
    }
    protected override bool IsMet(ProgressionState state)
    {
        return state.AreasAccessible.Contains(Area);
    }

    protected override List<string> GetPossibleRequirementsImpl()
    {
        return new string[] { Area }.ToList();
    }
    public override int GetPossibleRequirementsCount() { return 1; }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        return $"{Area} Access";
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
               Area == req.Area;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Area);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        return $"CanReachRegion(\"{EscapePythonString(Area)}\")";
    }

    public override List<T> GetOf<T>()
    {
        return new List<T>();
    }
}

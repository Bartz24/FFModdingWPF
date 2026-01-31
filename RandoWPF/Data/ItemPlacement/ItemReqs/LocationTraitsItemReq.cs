using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;

public class LocationTraitsItemReq : ItemReq
{
    private readonly string trait;
    private readonly int amount;
    public LocationTraitsItemReq(string trait, int amount)
    {
        this.trait = trait;
        this.amount = amount;
    }
    protected override bool IsMet(ProgressionState state)
    {
        var dict = GetItemLocations();
        return state.LocationsCompleted.Where(name => dict[name].Traits.Contains(trait)).Count() >= amount;
    }

    protected override List<string> GetPossibleRequirementsImpl()
    {
        var dict = GetItemLocations();
        return dict.Where(kv => kv.Value != null && kv.Value.Traits.Contains(trait)).Select(kv => kv.Key).ToList();
    }
    public override int GetPossibleRequirementsCount() { return amount; }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        return $"{amount} {trait}(s)";
    }

    public override int GetDifficulty(ProgressionState state)
    {
        if (!IsValid(state))
        {
            return -1;
        }

        return base.GetDifficulty(state) + amount;
    }

    public override bool Equals(object obj)
    {
        return obj is LocationTraitsItemReq req &&
               trait == req.trait &&
               amount == req.amount;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(trait, amount);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        return $"state_has_location_trait(state, player, \"{trait}\", {amount})";
    }

    public override List<T> GetOf<T>()
    {
        return new List<T>();
    }
}

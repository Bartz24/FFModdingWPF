using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public abstract class ItemLocation : CSVDataRow
{
    public SeedGenerator Generator { get; set; }
    public abstract string ID { get; set; }
    public abstract string Name { get; set; }
    public abstract string LocationImagePath { get; set; }
    public abstract ItemReq Requirements { get; set; }
    public abstract List<string> Traits { get; set; }
    public abstract List<string> Areas { get; set; }
    public abstract int BaseDifficulty { get; set; }
    public virtual List<ItemLocationReqComponent> GetComponents()
    {
        var components = new List<ItemLocationReqComponent>
        {
            new ItemReqComponent(Requirements)
        };
        return components;
    }

    public bool AreItemReqsMet(ProgressionState state)
    {
        return state.AreasAccessible.Intersect(Areas).Count() > 0 && GetComponents().All(c => c.AreItemReqsMet(state));
    }

    public abstract bool CanReplace(ItemLocation location);

    public abstract void SetItem(string newItem, int newCount);
    public abstract (string Item, int Amount)? GetItem(bool orig);

    public ItemLocation(SeedGenerator generator, string[] row) : base(row)
    {
        Generator = generator;
    }

    public int GetDifficulty(ProgressionState state)
    {
        int reqDiff = Requirements.GetDifficulty(state);
        if (reqDiff < 0)
        {
            reqDiff = 0;
        }

        return BaseDifficulty + reqDiff;
    }

    protected virtual void LogSetItem(string item, int count)
    {
        Generator.Logger.LogDebug("Set Item Location \"" + ID + "\" to [" + item + " x" + count + "]");
    }

    public virtual string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        return Requirements.GetArchipelagoRule(itemNameFunc);
    }

    public virtual string GetRequirementString()
    {
        return Requirements.ToString();
    }
}

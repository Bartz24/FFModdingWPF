using Bartz24.RandoWPF;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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
    public abstract bool IsValid(Dictionary<string, int> items);

    public abstract void SetItem(string newItem, int newCount);
    public abstract (string Item, int Amount)? GetItem(bool orig);

    public ItemLocation(SeedGenerator generator, string[] row) : base(row)
    {
        Generator = generator;
    }

    public int GetDifficulty(Dictionary<string, int> items)
    {
        int reqDiff = Requirements.GetDifficulty(items);
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
}

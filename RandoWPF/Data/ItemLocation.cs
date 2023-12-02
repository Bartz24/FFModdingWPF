using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace Bartz24.RandoWPF;

public abstract class ItemLocation : CSVDataRow
{
    public abstract string ID { get; set; }
    public abstract string Name { get; set; }
    public abstract string LocationImagePath { get; set; }
    public abstract ItemReq Requirements { get; set; }
    public abstract List<string> Traits { get; set; }
    public abstract List<string> Areas { get; set; }
    public abstract int BaseDifficulty { get; set; }
    public abstract bool IsValid(Dictionary<string, int> items);

    public abstract void SetData(dynamic obj, string newItem, int newCount);
    public abstract (string, int)? GetData(dynamic obj);

    public ItemLocation(string[] row) : base(row)
    {
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
}

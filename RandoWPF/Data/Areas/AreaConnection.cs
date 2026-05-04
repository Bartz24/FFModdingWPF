using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;
public class AreaConnection : CSVDataRow
{
    public SeedGenerator Generator { get; set; }
    [RowIndex(0)]
    public string Name { get; set; }

    [RowIndex(1)]
    public string FromAreaName { get; set; }
    [RowIndex(2)]
    public string ToAreaName { get; set; }

    [RowIndex(3)]
    public ItemReq Requirements { get; set; }
    [RowIndex(4)]
    public List<string> Traits { get; set; }
    [RowIndex(5)]
    public int BaseDifficulty { get; set; }

    public AreaConnection(SeedGenerator generator, string name, string fromAreaName, string toAreaName, ItemReq requirements, List<string> traits, int baseDifficulty) : base()
    {
        Generator = generator;
        Name = name;
        FromAreaName = fromAreaName;
        ToAreaName = toAreaName;
        Requirements = requirements;
        Traits = traits;
        BaseDifficulty = baseDifficulty;
    }

    public AreaConnection(SeedGenerator generator, string[] row) : base(row)
    {
        Generator = generator;
    }

    public virtual AreaConnection CreateReverse()
    {
        AreaConnection reverse = new(Generator, new string[6]);
        reverse.Name = Name + "_Reverse";
        reverse.FromAreaName = ToAreaName;
        reverse.ToAreaName = FromAreaName;
        reverse.Requirements = Requirements;
        reverse.Traits = Traits;
        reverse.BaseDifficulty = BaseDifficulty;
        return reverse;
    }

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
        return GetComponents().All(c => c.AreItemReqsMet(state));
    }
}

using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;

public abstract class FF13_2ItemLocation : ItemLocation
{
    public FF13_2ItemLocation(SeedGenerator generator, string[] row) : base(generator, row)
    {
    }

    public abstract List<string> RequiredAreas { get; set; }
    public abstract int MogLevel { get; set; }


    public override List<ItemLocationReqComponent> GetComponents()
    {
        var list = base.GetComponents();
        if (MogLevel > 0)
        {
            list.Add(new MogLevelReqComponent(Generator, MogLevel));
        }
        if (RequiredAreas.Count > 0)
        {
            list.Add(new RequiredAreasComponent(Generator, RequiredAreas));
        }
        return list;
    }

    public override string GetRequirementString()
    {
        return "Extra Areas: " + string.Join(",", RequiredAreas) + " - Mog level: " + MogLevel + " - " + base.GetRequirementString();
    }
}

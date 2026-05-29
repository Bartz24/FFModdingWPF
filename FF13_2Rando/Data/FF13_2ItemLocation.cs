using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Text;

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

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        // Mog level -> Has()
        // Required area -> CanReachRegion()

        List<string> reqs = new List<string>();

        if (Requirements is BoolItemReq b)
        {
            if (!b.Value)
            {
                return b.GetArchipelagoRule(itemNameFunc);
            }
        }
        else
        {
            reqs.Add(Requirements.GetArchipelagoRule(itemNameFunc));
        }

        if (MogLevel > 0)
        {
            reqs.Add($"Has(\"Progressive Mog Level\", {MogLevel})");
        }

        if (RequiredAreas.Count > 0)
        {
            foreach (var region in RequiredAreas) {
                reqs.Add($"CanReachRegion(\"{region}\")");
            }
        }

        if (reqs.Count > 1)
        {
            return "(" + string.Join(" & ", reqs) + ")";
        }
        else if (reqs.Count == 0)
        {
            return new BoolItemReq(true).GetArchipelagoRule(itemNameFunc);
        }
        return reqs[0];
    }

    public override string GetRequirementString()
    {
        return "Extra Areas: " + string.Join(",", RequiredAreas) + " - Mog level: " + MogLevel + " - " + base.GetRequirementString();
    }
}

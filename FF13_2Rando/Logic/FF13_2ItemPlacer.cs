using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data.Areas;
using Bartz24.RandoWPF.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando.Logic;

public class FF13_2ItemPlacer: CombinedItemPlacer<FF13_2ItemLocation, ItemData>
{
    public ProgressionItemPlacer<FF13_2ItemLocation> ProgressionPlacer { get; set; }

    public FF13_2UsefulItemPlacer UsefulPlacer { get; set; }

    public FF13_2JunkItemPlacer JunkPlacer { get; set; }

    public FF13_2ItemPlacer(SeedGenerator seedGenerator, AreaGraph areaGraph): base(seedGenerator, areaGraph)
    {

    }

    protected override int GetDifficultyIndex()
    {
        return FF13_2Flags.Items.KeyDepth.SelectedIndex;
    }

    public override bool IsFixedLocation(FF13_2ItemLocation location)
    {
        if(location is FF13_2FakeItemLocation)
        {
            return true;
        }

        if (location.Traits.Contains("Same") || location.Traits.Contains("Fixed"))
        {
            return true;
        }

        // TODO flag exclusions?

        return false;
    }

    protected override HashSet<FF13_2ItemLocation> GetLocationsForPlacer(HashSet<FF13_2ItemLocation> usedLocations, ItemPlacer<FF13_2ItemLocation> placer)
    {
        var possible = PossibleLocations.Except(usedLocations).ToHashSet();

        if (placer == ProgressionPlacer)
        {
            return GetProgressionLocations(possible);
        }
        else if (placer == UsefulPlacer)
        {
            return possible.Where(l => !l.Traits.Contains("Missable")).ToHashSet();
        }
        else if (placer == JunkPlacer)
        {
            return possible;
        }
        else
        {
            throw new Exception("Unknown placer");
        }
    }

    private HashSet<FF13_2ItemLocation> GetProgressionLocations(HashSet<FF13_2ItemLocation> possible)
    {
        return possible.Where(l =>
        {
            if (l.Traits.Contains("Missable") || l.Traits.Contains("Same"))
            {
                return false;
            }

            if (!FF13_2Flags.Items.KeyWild.Enabled && l.Traits.Contains("Wild"))
            {
                return false;
            }
            if (!FF13_2Flags.Items.KeyGraviton.Enabled && l.Traits.Contains("Graviton"))
            {
                return false;
            }
            if (!FF13_2Flags.Items.KeySide.Enabled && l.Traits.Contains("SideKey"))
            {
                return false;
            }
            if (!FF13_2Flags.Items.KeyGateSeal.Enabled && l.Traits.Contains("GateSeal"))
            {
                return false;
            }
            if(l is SearchItemData)
            {
                if(!FF13_2Flags.Items.KeyPlaceThrowCryst.Enabled && l.GetItem(true).Value.Item1.StartsWith("mcr")){
                    return false;
                }
                if (!FF13_2Flags.Items.KeyPlaceThrowJunk.Enabled && !l.GetItem(true).Value.Item1.StartsWith("mcr")){
                    return false;
                }
            }

            return true;
        }).ToHashSet();
    }

    protected override HashSet<FF13_2ItemLocation> GetReplacementsForPlacer(HashSet<FF13_2ItemLocation> usedReplacements, ItemPlacer<FF13_2ItemLocation> placer)
    {
        var remaining = Replacements.Except(usedReplacements).ToHashSet();
        if (placer == ProgressionPlacer)
        {
            return remaining.Where(l =>
            {
                string locationItem = l.GetItem(true)?.Item;
                var itemData = Generator.Get<EquipRando>().itemData.GetValueOrDefault(locationItem, null);
                if (itemData != null)
                {
                    var progressionCategories = new List<string>() { "Graviton", "SideKey", "GateSeal", "Wild", "MogLevel" };
                    foreach (var category in progressionCategories)
                    {
                        if (itemData.Traits.Contains(category))
                        {
                            return true;
                        }
                    }
                }
                // Allow everything to be placed currently?
                return false;
            }).ToHashSet();
        }
        else if (placer == UsefulPlacer)
        {
            // TODO: figure out what counts as useful and filter
            // Monster crystals?
            return new();
        }
        else if (placer == JunkPlacer)
        {
            return remaining;
        }
        else
        {
            throw new Exception("Unknown placer");
        }
    }

    protected override void RebuildPlacers()
    {
        Dictionary<string, double> areaMults = PossibleLocations.SelectMany(t => t.Areas).Distinct().ToDictionary(s => s, _ => RandomNum.RandInt(10, 200) * 0.01d);

        ProgressionPlacer = new(Generator, AreaGraph, GetDifficulty(), areaMults);
        ProgressionPlacer.FixedLocations = GetFixedLocations();
        UsefulPlacer = new(Generator, false);
        JunkPlacer = new(Generator);

        Placers = new() { ProgressionPlacer, UsefulPlacer, JunkPlacer };
    }

    protected override HashSet<string> GetReorderItemCategories()
    {
        return new() { "Accessory", "Weapon", "Monster Crystal", "Weapon" };
    }

    protected override Dictionary<string, ItemData> GetReorderItems()
    {
        // TODO: follow down
        return Generator.Get<EquipRando>().itemData;
    }
}


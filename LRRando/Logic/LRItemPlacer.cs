using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando;
public class LRItemPlacer : CombinedItemPlacer<ItemLocation, ItemData>
{
    public ProgressionItemPlacer<ItemLocation> ProgressionPlacer { get; set; }

    public LRUsefulItemPlacer UsefulPlacer { get; set; }

    public LRJunkItemPlacer JunkPlacer { get; set; }

    public LRItemPlacer(SeedGenerator generator) : base(generator)
    {
    }

    protected override int GetDifficultyIndex()
    {
        return LRFlags.Items.KeyDepth.SelectedIndex;
    }

    protected override HashSet<ItemLocation> GetFixedLocations()
    {
        return Replacements.Where(l =>
        {
            if (l is FakeLocation)
            {
                return true;
            }

            if (l.IsEPAbility())
            {
                if (!LRFlags.Items.IncludeEPAbilities.Enabled)
                {
                    return true;
                }

                if (!LRFlags.StatsAbilities.EPAbilitiesPool.SelectedKeys.Contains(l.GetItem(false).Value.Item))
                {
                    return true;
                }
            }

            foreach (string item in LRFlags.Items.KeyItems.DictValues.Keys)
            {
                if (!LRFlags.Items.KeyItems.SelectedKeys.Contains(item) && l.GetItem(true)?.Item == item)
                {
                    return true;
                }
            }

            return false;
        }).ToHashSet();
    }

    protected override HashSet<ItemLocation> GetLocationsForPlacer(HashSet<ItemLocation> usedLocations, ItemPlacer<ItemLocation> placer)
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

    private HashSet<ItemLocation> GetProgressionLocations(HashSet<ItemLocation> possible)
    {
        return possible.Where(l =>
        {
            if (l.Traits.Contains("Missable") || l.Traits.Contains("Same"))
            {
                return false;
            }

            foreach (string item in LRFlags.Items.KeyItems.DictValues.Keys)
            {
                if (LRFlags.Items.KeyItems.SelectedKeys.Contains(item) && l.GetItem(true)?.Item == item)
                {
                    return true;
                }
            }

            List<string> placeTraits = new() { "CoP", "Grindy", "Superboss", "Quest" };

            if (LRFlags.Items.KeyPlaceCoP.Enabled && l.Traits.Contains("CoP"))
            {
                return true;
            }

            if (LRFlags.Items.KeyPlaceGrindy.Enabled && l.Traits.Contains("Grindy"))
            {
                return true;
            }

            if (LRFlags.Items.KeyPlaceQuest.Enabled && l.Traits.Contains("Quest"))
            {
                return true;
            }

            if (LRFlags.Items.KeyPlaceSuperboss.Enabled && l.Traits.Contains("Superboss"))
            {
                return true;
            }

            if (LRFlags.Items.KeyPlaceTreasure.Enabled && !l.Traits.Intersect(placeTraits).Any())
            {
                return true;
            }

            return false;
        }).ToHashSet();
    }

    protected override HashSet<string> GetReorderItemCategories()
    {
        return new() { "Ability", "Accessory", "Weapon", "Shield", "Garb" };
    }

    protected override Dictionary<string, ItemData> GetReorderItems()
    {
        return Generator.Get<EquipRando>().itemData;
    }

    protected override HashSet<ItemLocation> GetReplacementsForPlacer(HashSet<ItemLocation> usedReplacements, ItemPlacer<ItemLocation> placer)
    {
        var remaining = Replacements.Except(usedReplacements).ToHashSet();
        if (placer == ProgressionPlacer)
        {
            return remaining.Where(l =>
            {
                foreach (string item in LRFlags.Items.KeyItems.DictValues.Keys)
                {
                    if (LRFlags.Items.KeyItems.SelectedKeys.Contains(item) && l.GetItem(true)?.Item == item)
                    {
                        return true;
                    }
                }

                return false;
            }).ToHashSet();
        }
        else if (placer == UsefulPlacer)
        {
            return remaining
                .Where(l => l.IsEPAbility() || l.IsPilgrimKeyItem() || l.IsLibraNote())
                .ToHashSet();
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

        areaMults.Keys.Where(a => a.StartsWith("CoP")).ToList().ForEach(a => areaMults[a] *= 0.1d);

        areaMults.Keys.Where(a => a == "Ultimate Lair").ToList().ForEach(a => areaMults[a] *= 0.4d);

        ProgressionPlacer = new(Generator, GetDifficulty(), areaMults);
        ProgressionPlacer.FixedLocations = GetFixedLocations();
        UsefulPlacer = new(Generator, false);
        JunkPlacer = new(Generator);

        Placers = new() { ProgressionPlacer, UsefulPlacer, JunkPlacer };
    }
}

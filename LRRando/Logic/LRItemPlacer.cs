using Bartz24.Data;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data.Areas;
using Bartz24.RandoWPF.Logic;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LRRando;
public class LRItemPlacer : CombinedItemPlacer<ItemLocation, ItemData>
{
    public ProgressionItemPlacer<ItemLocation> ProgressionPlacer { get; set; }

    public LRUsefulItemPlacer UsefulPlacer { get; set; }

    public LRJunkItemPlacer JunkPlacer { get; set; }

    public List<string> ProgressionAdornments { get; set; }

    public LRItemPlacer(SeedGenerator generator, AreaGraph areaGraph) : base(generator, areaGraph)
    {
    }

    private void InitProgressionAdornments()
    {
        var vanillaLocs = Generator.Get<TreasureRando>().ItemLocations;
        // Always add Always adornments to progression pool
        var alwaysAdornments = Generator.Get<EquipRando>().itemData
                    .Where(kvp => kvp.Value.Category == "Adornment" && kvp.Value.Traits.Contains("Always"))
                    .Select(kvp => kvp.Key)
                    .ToList();

        // Determine a set of 70-100 adornments to mark as progression
        // Only choose adornments whose vanilla location is a treasure, not a shop.
        ProgressionAdornments = Generator.Get<EquipRando>().itemData
            .Where(kvp => kvp.Value.Category == "Adornment" && !kvp.Value.Traits.Contains("Remove") && !kvp.Value.Traits.Contains("Always"))
            .Select(kvp => kvp.Key)
            .Where(key => vanillaLocs.Where(kvp => kvp.Value.GetItem(true)?.Item == key).Count() > 0)
            .Shuffle()
            .Take(RandomNum.RandInt(70, 101) - alwaysAdornments.Count)
            .Concat(alwaysAdornments)
            .ToList();

        Generator.Logger.LogDebug("Progression adornments:");
        Generator.Logger.LogDebug(String.Join(",", ProgressionAdornments));
    }

    protected override int GetDifficultyIndex()
    {
        return LRFlags.Items.KeyDepth.SelectedIndex;
    }

    public override bool IsFixedLocation(ItemLocation location)
    {
        if (location is FakeLocation)
        {
            return true;
        }

        if (location.Traits.Contains("Same"))
        {
            return true;
        }

        if (location.IsEPAbility())
        {
            if (!LRFlags.Items.IncludeEPAbilities.Enabled)
            {
                return true;
            }

            if (!LRFlags.StatsAbilities.EPAbilitiesPool.SelectedKeys.Contains(location.GetItem(false).Value.Item))
            {
                return true;
            }
        }

        foreach (string item in LRFlags.Items.KeyItems.DictValues.Keys)
        {
            if (!LRFlags.Items.KeyItems.SelectedKeys.Contains(item) && location.GetItem(true)?.Item == item)
            {
                return true;
            }
        }

        return false;
    }

    protected override OrderedSet<ItemLocation> GetLocationsForPlacer(OrderedSet<ItemLocation> usedLocations, ItemPlacer<ItemLocation> placer)
    {
        var possible = PossibleLocations.Except(usedLocations).ToOrderedSet();

        if (placer == ProgressionPlacer)
        {
            return GetProgressionLocations(possible);
        }
        else if (placer == UsefulPlacer)
        {
            return possible.Where(l => !l.Traits.Contains("Missable")).ToOrderedSet();
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

    private OrderedSet<ItemLocation> GetProgressionLocations(OrderedSet<ItemLocation> possible)
    {
        return possible.Where(l =>
        {
            if (l.Traits.Contains("Missable") || l.Traits.Contains("Same"))
            {
                return false;
            }

            var currentItem = Generator.Get<EquipRando>().itemData.GetValueOrDefault(l.GetItem(true)?.Item, null);

            if (currentItem?.Category == "Adornment")
            {
                return ProgressionAdornments.Contains(l.GetItem(true)?.Item);
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
        }).ToOrderedSet();
    }

    protected override HashSet<string> GetReorderItemCategories()
    {
        return new() { "Ability", "Accessory", "Weapon", "Shield", "Garb" };
    }

    protected override Dictionary<string, ItemData> GetReorderItems()
    {
        return Generator.Get<EquipRando>().itemData;
    }

    protected override OrderedSet<ItemLocation> GetReplacementsForPlacer(OrderedSet<ItemLocation> usedReplacements, ItemPlacer<ItemLocation> placer)
    {
        var remaining = Replacements.Except(usedReplacements).ToOrderedSet();
        if (placer == ProgressionPlacer)
        {
            return remaining.Where(l =>
            {
                string locationItem = l.GetItem(true)?.Item;

                // If it's a progression adornment, allow it
                if (Generator.Get<EquipRando>().itemData.GetValueOrDefault(locationItem, null)?.Category == "Adornment")
                {
                    return ProgressionAdornments.Contains(locationItem);
                }

                foreach (string item in LRFlags.Items.KeyItems.DictValues.Keys)
                {
                    if (LRFlags.Items.KeyItems.SelectedKeys.Contains(item) && locationItem == item)
                    {
                        return true;
                    }
                }

                return false;
            }).ToOrderedSet();
        }
        else if (placer == UsefulPlacer)
        {
            return remaining
                .Where(l => l.IsEPAbility() || l.IsPilgrimKeyItem() || l.IsLibraNote())
                .ToOrderedSet();
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
        InitProgressionAdornments();
        Dictionary<string, double> areaMults = PossibleLocations.SelectMany(t => t.Areas).Distinct().ToDictionary(s => s, _ => RandomNum.RandInt(10, 200) * 0.01d);

        areaMults.Keys.Where(a => a.StartsWith("CoP")).ToList().ForEach(a => areaMults[a] *= 0.1d);

        areaMults.Keys.Where(a => a == "Ultimate Lair").ToList().ForEach(a => areaMults[a] *= 0.4d);

        ProgressionPlacer = new(Generator, AreaGraph, GetDifficulty(), areaMults);
        ProgressionPlacer.FixedLocations = GetFixedLocations();
        UsefulPlacer = new(Generator, false);
        JunkPlacer = new(Generator);

        Placers = new() { ProgressionPlacer, UsefulPlacer, JunkPlacer };
    }
}

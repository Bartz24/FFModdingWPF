using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF.Logic;
public class ProgressionItemPlacer<T> : ItemPlacer<T> where T : ItemLocation
{
    public HashSet<T> FixedLocations { get; set; } = new();

    protected Dictionary<string, int> FoundItems { get; set; } = new();

    protected Dictionary<int, HashSet<T>> UnlockedLocations { get; set; } = new();

    protected HashSet<T> RemainingFixed { get; set; } = new();
    protected Queue<T> RemainingToPlace { get; set; } = new();

    protected int DepthDifficulty { get; set; }

    protected int Attempts { get; set; } = 0;

    protected Dictionary<string, double> AreaMultipliers { get; set; } = new();

    public ProgressionItemPlacer(SeedGenerator generator, int depthDiff, Dictionary<string, double> areaMults) : base(generator)
    {
        DepthDifficulty = depthDiff;
        AreaMultipliers = areaMults;
    }

    public override void PlaceItems()
    {
        bool success;
        do
        {
            Attempts++;
            success = TryPlaceItems();
        }
        while (!success);
    }

    protected virtual bool TryPlaceItems()
    {
        FoundItems = new();
        RemainingFixed = new(FixedLocations);
        RemainingToPlace = new(LocationsToPlace.Where(l => !RemainingFixed.Contains(l)).Shuffle());
        FinalPlacement.Clear();
        UnlockedLocations.Clear();

        T firstFailure = null;
        while (RemainingToPlace.Count > 0 || RemainingFixed.Count > 0)
        {
            // First try to place any fixed locations
            PlaceFixed();

            // Occurs when fixed locations are the last ones to be placed
            if (RemainingToPlace.Count == 0)
            {
                return RemainingFixed.Count == 0;
            }

            UpdatedUnlockedLocations();

            T replacement = RemainingToPlace.Dequeue();

            // Find location and start with depth difficulty
            T location = SelectLocation(replacement, DepthDifficulty);
            if (location != null)
            {
                PlaceItem(location, replacement);

                // Placed an item, so reset failure
                firstFailure = null;
            }
            else
            {
                if (firstFailure == null)
                {
                    firstFailure = replacement;
                }
                else if (firstFailure == replacement)
                {
                    // If we've already failed to place this item, we're stuck
                    return false;
                }

                // If no location found, add to end of queue
                RemainingToPlace.Enqueue(replacement);
            }

            RandoUI.SetUIProgressDeterminate($"Attempt {Attempts}: Placed {FinalPlacement.Count} of {LocationsToPlace.Count + FixedLocations.Count} important items.", FinalPlacement.Count, LocationsToPlace.Count + FixedLocations.Count);
        }

        return true;
    }

    protected virtual void PlaceFixed()
    {
        // Repeat as fixed locations can unlock other fixed locations
        bool placed;
        do
        {
            HashSet<T> toRemove = new();
            placed = false;
            foreach (var loc in RemainingFixed)
            {
                if (loc.AreItemReqsMet(FoundItems))
                {
                    PlaceItem(loc, loc);
                    toRemove.Add(loc);
                    placed = true;
                }
            }

            RemainingFixed.RemoveWhere(l => toRemove.Contains(l));
        }
        while (placed);
    }

    public override void PlaceItem(T location, T replacement)
    {
        base.PlaceItem(location, replacement);

        AddFoundItem(replacement);

        // Remove from UnlockedLocations
        foreach (var group in UnlockedLocations.Values)
        {
            group.Remove(location);
        }
    }

    protected virtual void AddFoundItem(T location)
    {
        var item = location.GetItem(true);
        if (item != null)
        {
            var (itemID, amount) = item.Value;
            if (FoundItems.ContainsKey(itemID))
            {
                FoundItems[itemID] += amount;
            }
            else
            {
                FoundItems.Add(itemID, amount);
            }
        }
    }

    protected virtual void UpdatedUnlockedLocations()
    {
        // Increment all group keys in UnlockedLocations by 1, and move any that are depth 10 or higher into the same group of depth 10
        Dictionary<int, HashSet<T>> newUnlockedLocations = new();
        foreach (var group in UnlockedLocations)
        {
            int newDepth = group.Key + 1;
            if (newDepth < 10)
            {
                newUnlockedLocations.Add(newDepth, group.Value);
            }
            else
            {
                if (newUnlockedLocations.ContainsKey(10))
                {
                    newUnlockedLocations[10].UnionWith(group.Value);
                }
                else
                {
                    newUnlockedLocations.Add(10, group.Value);
                }
            }
        }


        // Then, find any newly accessible locations and add them to UnlockedLocations with depth 0        
        var previouslyFound = newUnlockedLocations.SelectMany(p => p.Value).ToHashSet();
        HashSet<T> newlyAccessible = new();
        foreach (var loc in PossibleLocations)
        {
            if (loc.AreItemReqsMet(FoundItems) && !FinalPlacement.ContainsKey(loc) && !previouslyFound.Contains(loc))
            {
                newlyAccessible.Add(loc);
            }
        }

        newUnlockedLocations.Add(0, newlyAccessible);

        UnlockedLocations = newUnlockedLocations;
    }

    protected T SelectLocation(T replacement, int n)
    {
        // Select the first n groups. If it is empty, grow the search by 1 each time
        var possibleLocations = UnlockedLocations.Keys.OrderBy(k => k).Take(n).SelectMany(k => UnlockedLocations[k]).ToHashSet();       

        // Remove any locations where the replacement cannot be placed
        possibleLocations.RemoveWhere(l => !replacement.CanReplace(l));

        if (possibleLocations.Count == 0)
        {
            if (n > UnlockedLocations.Keys.Count)
            {
                return null;
            }
            else
            {
                return SelectLocation(replacement, n + 1);
            }
        }

        return RandomNum.SelectRandomWeighted(possibleLocations, l => GetAreaWeight(l));

    }
    protected virtual long GetAreaWeight(T location)
    {
        return Math.Max(1, (long)(location.Areas.Select(a => AreaMultipliers[a]).Average() * 100.0));
    }
}

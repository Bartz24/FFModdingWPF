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
        RemainingToPlace = new(GetInitialReplacementOrder());
        FinalPlacement.Clear();
        UnlockedLocations.Clear();

        int initialRemaining = RemainingToPlace.Count;

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

            // The initial depth is based on remaining items. The more items remaining, the higher the depth can be.
            // This allows items early on (first 50%) to be placed in newly unlocked areas more often.
            // Limited by the depth difficulty with a floor of 1
            int depth = 10;
            if (RemainingToPlace.Count > initialRemaining / 2)
            {
                int placedCount = initialRemaining - RemainingToPlace.Count;
                depth = Math.Max(1, (int)Math.Round((double)placedCount / (initialRemaining / 2) * 10));
            }

            depth = Math.Min(depth, DepthDifficulty);

            // Find location and start with depth difficulty
            T location = SelectLocation(replacement, depth);
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

            RandoUI.SetUIProgressDeterminate($"Attempt {Attempts}: Placed {FinalPlacement.Count} of {Replacements.Count + FixedLocations.Count} important items.", FinalPlacement.Count, Replacements.Count + FixedLocations.Count);
        }

        return true;
    }

    /// <summary>
    /// Prioritize placing items that immediately unlock other locations
    /// </summary>
    /// <returns></returns>
    private List<T> GetInitialReplacementOrder()
    {
        List<T> newOrder = new();
        List<T> original = Replacements.Where(l => !RemainingFixed.Contains(l)).ToList();
        while (original.Count > 0)
        {
            T next = RandomNum.SelectRandomWeighted(original, l => Math.Min(GetNewlyAccessibleWithLocation(UnlockedLocations, l).Count * 5, 30) + 1);
            newOrder.Add(next);
            original.Remove(next);
        }
        return newOrder;
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

    protected virtual void AddFoundItem(T location, Dictionary<string, int> foundItems = null)
    {
        if (foundItems == null)
        {
            foundItems = FoundItems;
        }

        var item = location.GetItem(true);
        if (item != null)
        {
            var (itemID, amount) = item.Value;
            if (foundItems.ContainsKey(itemID))
            {
                foundItems[itemID] += amount;
            }
            else
            {
                foundItems.Add(itemID, amount);
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
        HashSet<T> newlyAccessible = GetNewlyAccessible(newUnlockedLocations, FoundItems);

        newUnlockedLocations.Add(0, newlyAccessible);

        UnlockedLocations = newUnlockedLocations;
    }

    private HashSet<T> GetNewlyAccessibleWithLocation(Dictionary<int, HashSet<T>> unlockedLocations, T addLocation)
    {
        var foundItems = new Dictionary<string, int>(FoundItems);
        AddFoundItem(addLocation, foundItems);
        return GetNewlyAccessible(unlockedLocations, foundItems);
    }

    private HashSet<T> GetNewlyAccessible(Dictionary<int, HashSet<T>> unlockedLocations, Dictionary<string, int> foundItems)
    {
        var previouslyFound = unlockedLocations.SelectMany(p => p.Value).ToHashSet();
        HashSet<T> newlyAccessible = new();
        foreach (var loc in PossibleLocations)
        {
            if (!FinalPlacement.ContainsKey(loc) && !previouslyFound.Contains(loc) && loc.AreItemReqsMet(foundItems))
            {
                newlyAccessible.Add(loc);
            }
        }

        return newlyAccessible;
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

        return RandomNum.SelectRandomWeighted(possibleLocations, l => (long)(
                    GetAreaWeight(l) 
                    * Math.Pow(1.2, Math.Max(0, 10 - DepthDifficulty)) 
                    * (l.BaseDifficulty + 1) 
                    * 100));

    }
    protected virtual double GetAreaWeight(T location)
    {
        return Math.Max(1, location.Areas.Select(a => AreaMultipliers[a]).Average());
    }
}

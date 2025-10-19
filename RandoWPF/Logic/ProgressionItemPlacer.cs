using Bartz24.RandoWPF.Data.Areas;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF.Logic;
public class ProgressionItemPlacer<T> : ItemPlacer<T> where T : ItemLocation
{
    public HashSet<T> FixedLocations { get; set; } = new();
    protected AreaGraph AreaGraph { get; set; }

    protected ProgressionState ProgState { get; set; } = new();

    protected List<string> UnlockedAreas { get; set; } = new();

    protected Dictionary<int, HashSet<T>> UnlockedLocations { get; set; } = new();

    protected HashSet<T> RemainingFixed { get; set; } = new();
    protected Queue<T> RemainingToPlace { get; set; } = new();

    protected int DepthDifficulty { get; set; }

    protected int Attempts { get; set; } = 0;

    protected Dictionary<string, double> AreaMultipliers { get; set; } = new();

    public ProgressionItemPlacer(SeedGenerator generator, AreaGraph areaGraph, int depthDiff, Dictionary<string, double> areaMults) : base(generator)
    {
        DepthDifficulty = depthDiff;
        AreaGraph = areaGraph;
        AreaMultipliers = areaMults;
    }

    public override void PlaceItems()
    {
        bool success;
        do
        {
            Attempts++;
            Generator.Logger.LogDebug($"Progression Item Placement Attempt {Attempts}");
            success = TryPlaceItems();
            if (!success)
            {
                const string EMPTY = "empty";
                Generator.Logger.LogDebug($"Failed to place {RemainingToPlace.Count + RemainingFixed.Count} remaining replacements.");
                Generator.Logger.LogDebug($"Remaining to place: {string.Join(",", RemainingToPlace.Select(x => $"[Location: {x.Name}, requires: {x.Requirements}, item: {x.GetItem(true).Value.Item}]"))}{string.Join(",", RemainingFixed.Select(x => $"[Location: {x.Name}, requires: {x.Requirements}, item: {(x.GetItem(true) != null ? x.GetItem(true).Value.Item : EMPTY)}]"))}");
            }
        }
        while (!success);
    }

    protected bool EnsureCompletable(HashSet<T> remainingFixed)
    {
        var trueFixedLocations = remainingFixed.Where(item => !(item is FakeLocation)).ToArray();
        if (trueFixedLocations.Length > 0)
        {
            return false;
        }
        var fakeLocations = remainingFixed.Where(item => item is FakeLocation).ToArray();
        if (fakeLocations.Length > 0)
        {
            // Assume all required locations are marked appropriately (TODO: Game specific logic?)
            // All fake checks have "fake" as a trait so start from 1 not 0
            if(fakeLocations.Any(item => item.Traits.Count > 1 && !item.Traits.Contains("Missable")))
            {
                return false;
            }
        }
        return true;
    }

    protected virtual bool TryPlaceItems()
    {
        ProgState = new();
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
                // This is technically fine if the remaining item set doesn't include critical path items.
                return EnsureCompletable(RemainingFixed);
            }

            // Update unlocked areas and locations
            UpdatedUnlockedAreas();
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
    /// Prioritize placing items that immediately unlock other locations, or that have maximal depth requirements.
    /// </summary>
    /// <returns></returns>
    private List<T> GetInitialReplacementOrder()
    {
        Dictionary<int, T> newOrder = new();
        bool[] usedIndices = new bool[100000];
        List<T> original = Replacements.Where(l => !RemainingFixed.Contains(l)).ToList();
        Dictionary<string, (int min, int max)> itemRanges = new();
        foreach (T next in original)
        {
            int minIndex, maxIndex;
            string similarItemType = GetSimilarItemType(next);
            if (itemRanges.ContainsKey(similarItemType))
            {
                (minIndex, maxIndex) = itemRanges[similarItemType];
            }
            else
            {
                minIndex = RandomNum.RandInt(15, 70);
                var (minAdjust, maxAdjust) = GetLocationOffsets(next, similarItemType);
                minIndex = Math.Clamp(minIndex + minAdjust, 0, 80);
                var rangeCap = Math.Max(100 + maxAdjust, 31);
                int range = RandomNum.RandInt(0, 99) < 30 ? rangeCap : RandomNum.RandInt(30, rangeCap);
                maxIndex = Math.Min(minIndex + range, 100);

                itemRanges.Add(similarItemType, (minIndex, maxIndex));
            }

            int index = -1;
            while (index < 0 || usedIndices[index])
            {
                index = RandomNum.RandInt(minIndex * 1000, Math.Min(maxIndex * 1000, usedIndices.Length - 1));
            }

            newOrder.Add(index, next);
            usedIndices[index] = true;
        }

        return newOrder.Keys.OrderBy(i => i).Select(i => newOrder[i]).ToList();
    }

    protected virtual (int,int) GetLocationOffsets(T location, string itemType)
    {
        var newlyAccessible = GetNewlyAccessibleWithLocation(UnlockedLocations, location);
        var unlockingWeight = newlyAccessible.Count / 10;
        var remainingWithInterest = PossibleLocations.Where(loc => loc.Requirements.GetPossibleRequirements().Contains(itemType)).Count();
        var remainingFixedWithInterest = FixedLocations.Where(loc => loc.Requirements.GetPossibleRequirements().Contains(itemType)).Count();
        // Min bound is adjusted downwards by how many locations are immediately unlocked by this item, as well as the number of overall locations still locked by this item in some way.
        // Max bound as adjusted downwards by the number of overall locations still locked by this item in some way.
        // The idea being that items which unlock large segments of the game are weighted to fall much earlier generally speaking
        // TODO: refine weighting here.
        if(remainingWithInterest > 0 || remainingFixedWithInterest > 0)
        {
            Generator.Logger.LogDebug($"Item {itemType} unlocks {remainingWithInterest} locations ({remainingFixedWithInterest} fixed)");
        }
        return (-unlockingWeight - remainingWithInterest, -remainingFixedWithInterest*5 - remainingWithInterest*5);
    }

    protected virtual void PlaceFixed()
    {
        // Only allow one fixed check marked as NoCascade per iteration.
        bool noCascadeFound = false;
        bool placed;
        // Repeat as fixed locations can unlock other fixed locations
        do
        {
            UpdatedUnlockedAreas();

            HashSet<T> toRemove = new();
            placed = false;
            foreach (var loc in RemainingFixed.OrderBy(i => i.BaseDifficulty).ToList())
            {
                if(loc.Traits.Contains("NoCascade") && noCascadeFound)
                {
                    continue;
                }
                if (loc.AreItemReqsMet(ProgState))
                {
                    PlaceItem(loc, loc);
                    toRemove.Add(loc);
                    placed = true;
                    if (loc.Traits.Contains("NoCascade"))
                    {
                        noCascadeFound = true;
                    }
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

    protected virtual void AddFoundItem(T location, ProgressionState foundItems = null)
    {
        if (foundItems == null)
        {
            foundItems = ProgState;
        }

        var item = location.GetItem(true);
        if (item != null)
        {
            var (itemID, amount) = item.Value;
            if (foundItems.ItemsAvailable.ContainsKey(itemID))
            {
                foundItems.ItemsAvailable[itemID] += amount;
            }
            else
            {
                foundItems.ItemsAvailable.Add(itemID, amount);
            }
        }
    }

    protected virtual void UpdatedUnlockedAreas()
    {
        // Update UnlockedAreas based on FoundItems and AreaGraph
        UnlockedAreas = AreaGraph.GetAllAccessibleAreas("Initial", ProgState).Select(a => a.Name).ToList();
        ProgState.AreasAccessible = new HashSet<string>(UnlockedAreas);
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
        HashSet<T> newlyAccessible = GetNewlyAccessible(newUnlockedLocations, ProgState);

        newUnlockedLocations.Add(0, newlyAccessible);

        // Remove any locations which now cannot be accessed.
        HashSet<T> newlyInaccessible = GetNewlyInaccessible(newUnlockedLocations, ProgState);

        if (newlyInaccessible.Count > 0)
        {
            Generator.Logger.LogDebug($"Removing {newlyInaccessible.Count} locations");
            foreach (var entry in newUnlockedLocations)
            {
                entry.Value.RemoveWhere(e => newlyInaccessible.Contains(e));
            }
        }

        UnlockedLocations = newUnlockedLocations;
    }

    private HashSet<T> GetNewlyAccessibleWithLocation(Dictionary<int, HashSet<T>> unlockedLocations, T addLocation)
    {
        var state = new ProgressionState(ProgState);
        AddFoundItem(addLocation, state);
        return GetNewlyAccessible(unlockedLocations, state);
    }

    private HashSet<T> GetNewlyInaccessibleWithLocation(Dictionary<int, HashSet<T>> unlockedLocations, T addLocation)
    {
        var state = new ProgressionState(ProgState);
        AddFoundItem(addLocation, state);
        return GetNewlyInaccessible(unlockedLocations, state);
    }

    private HashSet<T> GetNewlyInaccessible(Dictionary<int, HashSet<T>> unlockedLocations, ProgressionState state)
    {
        var previouslyFound = unlockedLocations.SelectMany(p => p.Value).ToHashSet();
        HashSet<T> newlyInaccessible = new();
        foreach (var loc in PossibleLocations)
        {
            var finalPlacementContains = FinalPlacement.ContainsKey(loc);
            var prevFound = previouslyFound.Contains(loc);
            var reqMet = loc.AreItemReqsMet(state);
            if (!finalPlacementContains && prevFound && !reqMet)
            {
                newlyInaccessible.Add(loc);
            }
        }

        return newlyInaccessible;
    }

    private HashSet<T> GetNewlyAccessible(Dictionary<int, HashSet<T>> unlockedLocations, ProgressionState state)
    {
        var previouslyFound = unlockedLocations.SelectMany(p => p.Value).ToHashSet();
        HashSet<T> newlyAccessible = new();
        foreach (var loc in PossibleLocations)
        {
            if (!FinalPlacement.ContainsKey(loc) && !previouslyFound.Contains(loc) && loc.AreItemReqsMet(state))
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

    protected virtual string GetSimilarItemType(T location)
    {
        var item = location.GetItem(false);
        if (item == null)
        {
            throw new RandoException("Null item detected for " + location.Name, "Null item");
        }

        return item?.Item;
    }
}

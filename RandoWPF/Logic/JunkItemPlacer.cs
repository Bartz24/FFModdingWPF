using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public abstract class JunkItemPlacer<T> : ItemPlacer<T> where T : ItemLocation
{
    public JunkItemPlacer(SeedGenerator generator) : base(generator)
    {
    }

    public override void PlaceItems()
    {
        HashSet<T> remainingReplacements = new(LocationsToPlace);

        // Fill all possible locations with junk. If the replacements is empty, refill it and continue
        foreach (var loc in PossibleLocations.Shuffle().Take(LocationsToPlace.Count))
        {
            remainingReplacements = SetJunkItem(remainingReplacements, loc);

            RandoUI.SetUIProgressDeterminate($"Placed {FinalPlacement.Count} of {LocationsToPlace.Count} junk items.", FinalPlacement.Count, LocationsToPlace.Count);
        }

        // Fill any multi locations with junk
        HashSet<T> emptyMultis = GetEmptyMultiLocations();
        int count = 0;
        foreach (var loc in emptyMultis)
        {
            remainingReplacements = SetJunkItem(remainingReplacements, loc);
            count++;
            RandoUI.SetUIProgressDeterminate($"Placed {count} of {emptyMultis.Count} empty junk items.", count, emptyMultis.Count);
        }
    }

    private HashSet<T> SetJunkItem(HashSet<T> remainingReplacements, T loc)
    {
        T replacement = null;
        do
        {
            replacement = RandomNum.SelectRandomOrDefault(remainingReplacements.Where(l => l.CanReplace(loc)));
            if (replacement == null)
            {
                remainingReplacements = new(LocationsToPlace);
            }
        } while (replacement == null);

        PlaceItem(loc, replacement);
        remainingReplacements.Remove(replacement);
        return remainingReplacements;
    }

    protected virtual HashSet<T> GetEmptyMultiLocations()
    {
        return new();
    }

    public override void ApplyToGameData()
    {
        foreach (var loc in FinalPlacement.Keys.Shuffle())
        {
            var rep = FinalPlacement[loc];
            var orig = rep.GetItem(true);
            var (item, amount) = GetNewJunkItem(orig.Value);
            loc.SetItem(item, amount);
        }
    }

    public abstract (string Item, int Amount) GetNewJunkItem((string Item, int Amount) orig);
}

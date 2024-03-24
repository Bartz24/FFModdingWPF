using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public abstract class UsefulItemPlacer<T> : ItemPlacer<T> where T : ItemLocation
{
    protected bool LogWarnings { get; set; }
    public UsefulItemPlacer(SeedGenerator generator, bool logWarnings) : base(generator)
    {
        LogWarnings = logWarnings;
    }

    public override void PlaceItems()
    {
        HashSet<T> remainingLocations = new(PossibleLocations);

        foreach (var place in Replacements)
        {
            T location = RandomNum.SelectRandomOrDefault(remainingLocations.Where(l => place.CanReplace(l)));
            if (location == null && LogWarnings)
            {
                Generator.Logger.LogWarning("Could not place location " + place.ID);
            }
            else
            {
                PlaceItem(location, place);
                remainingLocations.Remove(location);
            }

            RandoUI.SetUIProgressDeterminate($"Placed {FinalPlacement.Count} of {Replacements.Count} useful items.", FinalPlacement.Count, Replacements.Count);
        }
    }

    public override void ApplyToGameData()
    {
        foreach (var loc in FinalPlacement.Keys)
        {
            var rep = FinalPlacement[loc];
            var (Item, Amount) = GetNewItem(rep.GetItem(true).Value);
            loc.SetItem(Item, Amount);
        }
    }

    public abstract (string Item, int Amount) GetNewItem((string Item, int Amount) orig);
}

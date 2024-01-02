using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public class UsefulItemPlacer<T> : ItemPlacer<T> where T : ItemLocation
{
    protected bool LogWarnings { get; set; }
    public UsefulItemPlacer(SeedGenerator generator, bool logWarnings) : base(generator)
    {
        LogWarnings = logWarnings;
    }

    public override void PlaceItems()
    {
        HashSet<T> remainingLocations = new(PossibleLocations);

        foreach (var place in LocationsToPlace)
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

            RandoUI.SetUIProgressDeterminate($"Placed {FinalPlacement.Count} of {LocationsToPlace.Count} useful items.", FinalPlacement.Count, LocationsToPlace.Count);
        }
    }
}

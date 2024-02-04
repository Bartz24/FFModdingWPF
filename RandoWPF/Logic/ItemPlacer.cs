using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public abstract class ItemPlacer<T> where T : ItemLocation
{
    /// <summary>
    /// The items that need to be placed
    /// </summary>
    public HashSet<T> Replacements { get; set; } = new();

    /// <summary>
    /// The allowed locations for the items
    /// </summary>
    public HashSet<T> PossibleLocations { get; set; } = new();

    public virtual Dictionary<T, T> FinalPlacement { get; set; } = new();

    protected SeedGenerator Generator { get; set; }

    public ItemPlacer(SeedGenerator generator)
    {
        Generator = generator;
    }
    
    public abstract void PlaceItems();

    public virtual void PlaceItem(T location, T replacement)
    {
        Generator.Logger.LogDebug($"Placed {replacement.ID} [{replacement.GetItem(true)?.Item} x{replacement.GetItem(true)?.Amount}] at {location.ID}");
        FinalPlacement.Add(location, replacement);
    }

    public virtual void ApplyToGameData()
    {
        foreach (var loc in FinalPlacement.Keys)
        {
            var rep = FinalPlacement[loc];
            var orig = rep.GetItem(true);
            loc.SetItem(orig.Value.Item, orig.Value.Amount);
        }
    }

    public virtual void ClearUnsetLocations()
    {
        foreach (var location in PossibleLocations)
        {
            if (!FinalPlacement.ContainsKey(location))
            {
                location.SetItem(null, 0);
            }
        }
    }
}

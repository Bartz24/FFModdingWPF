using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public abstract class HintPlacer<I, T> where T : ItemLocation
{
    public SeedGenerator Generator { get; set; }
    public ItemPlacer<T> ItemPlacer { get; set; }

    public Dictionary<I, List<T>> Hints { get; set; } = new();

    public HintPlacer(SeedGenerator generator, ItemPlacer<T> itemPlacer, HashSet<I> hintLocations)
    {
        Generator = generator;
        ItemPlacer = itemPlacer;
        foreach (var loc in hintLocations)
        {
            Hints.Add(loc, new());
        }
    }

    public void PlaceHints()
    {
        foreach (var loc in ItemPlacer.FinalPlacement.Keys)
        {
            if (IsHintable(loc))
            {
                PlaceHint(loc);
            }
        }
    }

    protected virtual void PlaceHint(T location)
    {
        // Select the hint location to add to which has the lowest amount and select random
        var hintLoc = RandomNum.SelectRandom(Hints.Where(h => h.Value.Count == Hints.Min(h2 => h2.Value.Count)));
        Hints[hintLoc.Key].Add(location);
    }

    protected abstract bool IsHintable(T location);

    public abstract string GetHintText(T location);
}

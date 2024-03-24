using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public abstract class HintPlacer<I, T, P> where T : ItemLocation where P : ItemPlacer<T>
{
    public SeedGenerator Generator { get; set; }
    public P ItemPlacer { get; set; }

    public Dictionary<I, List<T>> Hints { get; set; } = new();

    public HintPlacer(SeedGenerator generator, P itemPlacer, HashSet<I> hintLocations)
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
        IEnumerable<I> possible = GetPossibleLocations(location);
        var hintLoc = RandomNum.SelectRandom(possible.Where(h => Hints[h].Count == possible.Min(h2 => Hints[h2].Count)));
        Hints[hintLoc].Add(location);
    }

    protected virtual IEnumerable<I> GetPossibleLocations(T location)
    {
        return Hints.Keys;
    }

    protected abstract bool IsHintable(T location);

    public abstract string GetHintText(T location);
}

using Bartz24.RandoWPF.Logic;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public abstract class CombinedItemPlacer<L, I> : ItemPlacer<L> where L : ItemLocation where I : IItem
{
    public List<ItemPlacer<L>> Placers { get; set; } = new();

    public SphereCalculator<L> SphereCalculator { get; set; }

    public CombinedItemPlacer(SeedGenerator generator) : base(generator)
    {
        SphereCalculator = new(Generator);
    }

    public override Dictionary<L, L> FinalPlacement
    {
        get
        {
            Dictionary<L, L> final = new();
            foreach (var placer in Placers)
            {
                final = final.Concat(placer.FinalPlacement).ToDictionary(x => x.Key, x => x.Value);
            }

            return final;
        }
    }

    /// <summary>
    /// Each placer should return assuming the previous placers have already used some locations.
    /// This allows later placers to fill in the gaps left by earlier placers and any others.
    /// </summary>
    /// <param name="usedLocations"></param>
    /// <param name="placer"></param>
    /// <returns></returns>
    protected abstract HashSet<L> GetLocationsForPlacer(HashSet<L> usedLocations, ItemPlacer<L> placer);

    /// <summary>
    /// Each placer should return assuming the previous placers have already used some replacements.
    /// The pools should be filtered down for later placers to avoid checking conditions multiple times.
    /// </summary>
    /// <param name="usedReplacements"></param>
    /// <param name="placer"></param>
    /// <returns></returns>
    protected abstract HashSet<L> GetReplacementsForPlacer(HashSet<L> usedReplacements, ItemPlacer<L> placer);

    /// <summary>
    /// Should include any fake locations as well.
    /// </summary>
    /// <returns></returns>
    protected abstract HashSet<L> GetFixedLocations();

    protected abstract void RebuildPlacers();

    public override void PlaceItems()
    {
        Placers.Clear();
        RebuildPlacers();
        if (Placers.Count == 0)
        {
            throw new Exception("No placers found");
        }

        HashSet<L> usedLocations = new();
        usedLocations.UnionWith(GetFixedLocations());

        HashSet<L> usedReplacements = new();
        usedReplacements.UnionWith(Placers.SelectMany(x => x.Replacements));
        usedReplacements.UnionWith(GetFixedLocations());

        foreach (var placer in Placers)
        {
            placer.PossibleLocations = GetLocationsForPlacer(usedLocations, placer);
            placer.Replacements = GetReplacementsForPlacer(usedReplacements, placer);

            Generator.Logger.LogDebug($"Starting placer {placer.GetType().Name} with {placer.PossibleLocations.Count} locations and {placer.Replacements.Count} replacements.");
            placer.PlaceItems();

            usedLocations.UnionWith(placer.FinalPlacement.Keys);
            usedReplacements.UnionWith(placer.FinalPlacement.Values);
        }
    }

    public override void ApplyToGameData()
    {
        base.ApplyToGameData();

        ClearUnsetLocations();
        CalculateSpheres();
        ReorderItems();
    }

    protected virtual void CalculateSpheres()
    {
        SphereCalculator = new SphereCalculator<L>(Generator);
        SphereCalculator.CalculateSpheres(PossibleLocations);
    }

    protected abstract HashSet<string> GetReorderItemCategories();

    protected abstract Dictionary<string, I> GetReorderItems();

    protected virtual void ReorderItems()
    {
        var itemReorderer = new ItemReorderer<L, I>(Generator, GetReorderItemCategories(), GetReorderItems());
        itemReorderer.ReorderItems(PossibleLocations, SphereCalculator);
    }

    protected abstract int GetDifficultyIndex();

    public int GetDifficulty()
    {
        switch (GetDifficultyIndex())
        {
            case 0:
            default:
                return 10;
            case 1:
                return 7;
            case 2:
                return 5;
            case 3:
                return 3;
            case 4:
                return 1;
        }
    }
}

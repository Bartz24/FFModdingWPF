using Bartz24.Data;
using Bartz24.RandoWPF.Data.Areas;
using Bartz24.RandoWPF.Logic;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Bartz24.RandoWPF;
public abstract class CombinedItemPlacer<L, I> : ItemPlacer<L> where L : ItemLocation where I : IItem
{
    public List<ItemPlacer<L>> Placers { get; set; } = new();

    public SphereCalculator<L> SphereCalculator { get; set; }
    public PlaythroughCalculator<L> PlaythroughCalculator { get; set; } = null;

    protected AreaGraph AreaGraph { get; set; }

    public CombinedItemPlacer(SeedGenerator generator, AreaGraph areaGraph) : base(generator)
    {
        SphereCalculator = new(Generator, areaGraph);
        AreaGraph = areaGraph;
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
    protected abstract OrderedSet<L> GetLocationsForPlacer(OrderedSet<L> usedLocations, ItemPlacer<L> placer);

    /// <summary>
    /// Each placer should return assuming the previous placers have already used some replacements.
    /// The pools should be filtered down for later placers to avoid checking conditions multiple times.
    /// </summary>
    /// <param name="usedReplacements"></param>
    /// <param name="placer"></param>
    /// <returns></returns>
    protected abstract OrderedSet<L> GetReplacementsForPlacer(OrderedSet<L> usedReplacements, ItemPlacer<L> placer);

    /// <summary>
    /// Should include any fake locations as well.
    /// </summary>
    /// <returns></returns>
    public abstract bool IsFixedLocation(L location);

    public virtual OrderedSet<L> GetFixedLocations()
    {
        return PossibleLocations.Where(IsFixedLocation).ToOrderedSet();
    }

    protected abstract void RebuildPlacers();

    public override void PlaceItems()
    {
        Placers.Clear();
        RebuildPlacers();
        if (Placers.Count == 0)
        {
            throw new Exception("No placers found");
        }

        OrderedSet<L> usedLocations = new();
        usedLocations.UnionWith(GetFixedLocations());

        OrderedSet<L> usedReplacements = new();
        usedReplacements.UnionWith(Placers.SelectMany(x => x.Replacements));
        usedReplacements.UnionWith(GetFixedLocations());

        RandomNum.AddTestVal("Before Placers");
        foreach (var placer in Placers)
        {
            placer.PossibleLocations = GetLocationsForPlacer(usedLocations, placer);
            placer.Replacements = GetReplacementsForPlacer(usedReplacements, placer);

            Generator.Logger.LogDebug($"Starting placer {placer.GetType().Name} with {placer.PossibleLocations.Count} locations and {placer.Replacements.Count} replacements.");
            if (placer.PossibleLocations.Count < placer.Replacements.Count)
            {
                Generator.Logger.LogDebug("More replacements than locations, likely to fail!");
            }
            placer.PlaceItems();

            RandomNum.AddTestVal(placer.GetType().Name);

            usedLocations.UnionWith(placer.FinalPlacement.Keys);
            usedReplacements.UnionWith(placer.FinalPlacement.Values);
        }
    }

    public override void ApplyToGameData()
    {
        foreach (var placer in Placers)
        {
            placer.ApplyToGameData();
        }

        ClearUnsetLocations();
        PostPlacement();

        CalculateSpheres();
        CalculatePlaythrough();
        ReorderItems();
    }

    protected virtual void CalculateSpheres()
    {
        SphereCalculator = new SphereCalculator<L>(Generator, AreaGraph);
        SphereCalculator.CalculateSpheres(PossibleLocations);
    }

    protected virtual void CalculatePlaythrough()
    {
        try
        {
            PlaythroughCalculator = new PlaythroughCalculator<L>(SphereCalculator);
            ProgressionItemPlacer<L> progressionPlacer = (ProgressionItemPlacer<L>)Placers.FirstOrDefault(x => x is ProgressionItemPlacer<L>);
            PlaythroughCalculator.CalculatePlaythrough(progressionPlacer.ProgressionLocations, true);
        }
        catch (Exception e)
        {
            Generator.Logger.LogDebug($"Failed to build playthrough");
            string msg = "Unable to generate valid playthrough for verification - probably safe to ignore but who knows!";
            Generator.Logger.LogError(msg);
            MessageBox.Show(msg);
        }
    }

    protected abstract HashSet<string> GetReorderItemCategories();

    protected abstract Dictionary<string, I> GetReorderItems();

    protected virtual void ReorderItems()
    {
        var itemReorderer = new ItemReorderer<L, I>(Generator, GetReorderItemCategories(), GetReorderItems());
        itemReorderer.ReorderItems(PossibleLocations, SphereCalculator);
    }

    protected virtual void PostPlacement()
    {

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

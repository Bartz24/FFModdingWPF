using Bartz24.Data;
using Bartz24.RandoWPF;
using FF12Rando;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF12Rando;

public class FF12AssumedItemPlacementAlgorithm : AssumedItemPlacementAlgorithm<ItemLocation>
{
    private readonly TreasureRando treasureRando;
    private readonly Dictionary<string, int> AreaDepths = new();

    private List<string> ParentsRemoved { get; set; } = new();

    public FF12AssumedItemPlacementAlgorithm(Dictionary<string, ItemLocation> itemLocations, List<string> hintsByLocations, SeedGenerator generator, int maxFail) : base(itemLocations, hintsByLocations, generator, maxFail)
    {
        treasureRando = generator.Get<TreasureRando>();
    }

    protected override bool TryImportantPlacement(int attempt, List<string> locations, List<string> important, List<string> accessibleAreas)
    {
        ParentsRemoved.Clear();
        return base.TryImportantPlacement(attempt, locations, important, accessibleAreas);
    }

    public override void RemoveItems(List<string> locations, Dictionary<string, int> items, (string, int)? nextItem, string rep)
    {
        List<string> possible, newPossible = null;
        List<string> newAccessibleAreas = Logic.GetNewAreasAvailable(items, new List<string>());
        possible = locations.Where(t => !Placement.ContainsKey(t) && Logic.IsValid(t, rep, items, newAccessibleAreas)).Shuffle();

        base.RemoveItems(locations, items, nextItem, rep);

        if (ItemLocations[rep] is TreasureRando.RewardData reward && reward.IsFakeOnly)
        {
            RemoveFakeItems(items, reward);
        }

        while (newPossible == null || newPossible.Count < possible.Count)
        {
            newAccessibleAreas = Logic.GetNewAreasAvailable(items, new List<string>());
            if (newPossible != null)
            {
                possible = new List<string>(newPossible);
            }

            newPossible = locations.Where(t => !Placement.ContainsKey(t) && Logic.IsValid(t, rep, items, newAccessibleAreas)).Shuffle();

            // Do not remove fake items from rep as it was already removed
            List<string> removed = possible.Where(s => !newPossible.Contains(s)).ToList();
            removed.Where(s => ItemLocations[s] is TreasureRando.RewardData).Select(s=> ((TreasureRando.RewardData)ItemLocations[s]).Parent).Distinct().ForEach(parent =>
            {
                RemoveFakeItems(items, parent);
            });
        }
    }

    private void RemoveFakeItems(Dictionary<string, int> items, TreasureRando.RewardData reward)
    {
        if (ParentsRemoved.Contains(reward.Parent.ID))
        {
            return;
        }

        ParentsRemoved.Add(reward.Parent.ID);

        PartyRando partyRando = treasureRando.Generator.Get<PartyRando>();
        reward.Parent.FakeItems.ForEach(item =>
        {
            if (partyRando.CharacterMapping.Contains(item))
            {
                string newChar = partyRando.CharacterMapping[partyRando.Characters[partyRando.CharacterMapping.ToList().IndexOf(item)]];
                items[newChar] -= 1;
                if (items[newChar] <= 0)
                {
                    items.Remove(newChar);
                    Generator.Logger.LogDebug($"Removed fake item {newChar} from items");
                }
            }
            else
            {
                items[item] -= 1;
                if (items[item] <= 0)
                {
                    items.Remove(item);
                    Generator.Logger.LogDebug($"Removed fake item {item} from items");
                }
            }
        });
    }
}

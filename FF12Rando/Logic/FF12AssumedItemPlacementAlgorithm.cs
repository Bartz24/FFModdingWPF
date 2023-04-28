using Bartz24.Data;
using Bartz24.RandoWPF;
using FF12Rando;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF12Rando;

public class FF12AssumedItemPlacementAlgorithm : AssumedItemPlacementAlgorithm<ItemLocation>
{
    private readonly TreasureRando treasureRando;
    private readonly Dictionary<string, int> AreaDepths = new();

    public FF12AssumedItemPlacementAlgorithm(Dictionary<string, ItemLocation> itemLocations, List<string> hintsByLocations, RandomizerManager randomizers, int maxFail) : base(itemLocations, hintsByLocations, maxFail)
    {
        treasureRando = randomizers.Get<TreasureRando>();
    }

    public override void RemoveItems(List<string> locations, Dictionary<string, int> items, (string, int)? nextItem, string rep)
    {
        List<string> possible, newPossible = null;
        List<string> newAccessibleAreas = Logic.GetNewAreasAvailable(items, new List<string>());
        possible = locations.Where(t => !Placement.ContainsKey(t) && Logic.IsValid(t, rep, items, newAccessibleAreas)).Shuffle();

        base.RemoveItems(locations, items, nextItem, rep);

        PartyRando partyRando = treasureRando.Randomizers.Get<PartyRando>();
        while (newPossible == null || newPossible.Count < possible.Count)
        {
            newAccessibleAreas = Logic.GetNewAreasAvailable(items, new List<string>());
            if (newPossible != null)
            {
                possible = new List<string>(newPossible);
            }

            newPossible = locations.Where(t => !Placement.ContainsKey(t) && Logic.IsValid(t, rep, items, newAccessibleAreas)).Shuffle();

            List<string> removed = possible.Where(s => !newPossible.Contains(s)).ToList();
            removed.Where(s => ItemLocations[s] is TreasureRando.RewardData).ForEach(s =>
            {
                ((TreasureRando.RewardData)ItemLocations[s]).Parent.FakeItems.ForEach(item =>
                {
                    if (partyRando.CharacterMapping.Contains(item))
                    {
                        string newChar = partyRando.CharacterMapping[partyRando.Characters[partyRando.CharacterMapping.ToList().IndexOf(item)]];
                        items[newChar] -= 1;
                    }
                    else
                    {
                        items[item] -= 1;
                    }
                });
            });
        }
    }
}

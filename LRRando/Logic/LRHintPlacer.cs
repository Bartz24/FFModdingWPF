using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando;
public class LRHintPlacer : HintPlacer<string, ItemLocation, LRItemPlacer>
{
    public LRHintPlacer(SeedGenerator generator, LRItemPlacer itemPlacer, HashSet<string> hintLocations) : base(generator, itemPlacer, hintLocations)
    {
    }

    public override string GetHintText(ItemLocation location)
    {
        int index = LRFlags.Other.HintsSpecific.Values.IndexOf(LRFlags.Other.HintsSpecific.SelectedValue);
        if (index == LRFlags.Other.HintsSpecific.Values.Count - 1)
        {
            LRFlags.Other.HintsMain.SetRand();
            index = RandomNum.RandInt(0, LRFlags.Other.HintsSpecific.Values.Count - 2);
            RandomNum.ClearRand();
        }

        if (location.GetItem(false) == null)
        {
            throw new Exception("Location " + location.ID + " has no item. Cannot generate hint for missing item.");
        }

        EquipRando equipRando = Generator.Get<EquipRando>();

        switch (index)
        {
            case 0:
            default:
                {
                    return $"{location.Name} has {equipRando.GetItemName(location.GetItem(false).Value.Item)}";
                }
            case 1:
                {
                    string type = "Other";
                    if (location.IsKeyItem())
                    {
                        type = "a Key Item";
                    }

                    if (location.IsEPAbility())
                    {
                        type = "an EP Ability";
                    }

                    return $"{location.Name} has {type}";
                }
            case 2:
                {
                    return $"{location.Areas[0]} has {equipRando.GetItemName(location.GetItem(false).Value.Item)}";
                }
            case 3:
                {
                    return $"{location.Name} has ?????";
                }
        }
    }

    protected override bool IsHintable(ItemLocation location)
    {
        return location.IsEPAbility() || location.IsKeyItem();
    }

    protected override IEnumerable<string> GetPossibleLocations(ItemLocation location)
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        return base.GetPossibleLocations(location).Where(hint =>
        {
            // Ignore the 0-1 hint in the main pool
            // Will default to 0-1 if none other are valid
            if (hint == "fl_mnyu_001e")
            {
                return false;
            }

            // If the location is missable, allow in any hint
            if (!ItemPlacer.SphereCalculator.Spheres.ContainsKey(location))
            {
                return true;
            }

            var hintFake = treasureRando.ItemLocations[treasureRando.hintData[hint].FakeLocationLink];
            return ItemPlacer.SphereCalculator.Spheres[hintFake] < ItemPlacer.SphereCalculator.Spheres[location];
        }).DefaultIfEmpty("fl_mnyu_001e");
    }
}

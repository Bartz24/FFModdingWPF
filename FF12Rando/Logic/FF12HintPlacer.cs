using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF12Rando;
public class FF12HintPlacer : HintPlacer<int, ItemLocation, FF12ItemPlacer>
{
    public FF12HintPlacer(SeedGenerator generator, FF12ItemPlacer itemPlacer, HashSet<int> hintLocations) : base(generator, itemPlacer, hintLocations)
    {
    }

    public override string GetHintText(ItemLocation location)
    {
        string val;
        int index = FF12Flags.Other.HintsSpecific.Values.IndexOf(FF12Flags.Other.HintsSpecific.SelectedValue);
        if (index == FF12Flags.Other.HintsSpecific.Values.Count - 1)
        {
            FF12Flags.Other.HintsMain.SetRand();
            index = RandomNum.RandInt(0, FF12Flags.Other.HintsSpecific.Values.Count - 2);
            RandomNum.ClearRand();
        }

        if (location.GetItem(false) == null)
        {
            throw new Exception("Location " + location.ID + " has no item. Cannot generate hint for missing item.");
        }

        EquipRando equipRando = Generator.Get<EquipRando>();
        if (!equipRando.itemData.ContainsKey(location.GetItem(false).Value.Item))
        {
            throw new Exception("Location " + location.ID + " has item " + location.GetItem(false).Value.Item + " which is not in the item data. Cannot generate hint for missing item.");
        }

        ItemData itemData = equipRando.itemData[location.GetItem(false).Value.Item];

        switch (index)
        {
            case 0:
            default:
                val = $"{location.Name} has {itemData.Name}";
                break;
            case 1:
                string type = "Other";

                if (itemData.IntID == 0x8070)
                {
                    type = "a Writ of Transit";
                }
                else if (itemData.IntID == 0x2112 || itemData.IntID == 0x2113)
                {
                    type = "a Chop";
                }
                else if (itemData.IntID == 0x2116)
                {
                    type = "a Black Orb";
                }
                else if (itemData.ID.StartsWith("30") || itemData.ID.StartsWith("40"))
                {
                    type = "an Ability";
                }
                else if (itemData.IntID is >= 0x80B9 and <= 0x80D6)
                {
                    type = "a Useless Trophy";
                }
                else if (itemData.Category == "Key" && itemData.IntID >= 0x8000)
                {
                    type = "an Important Key Item";
                }

                val = $"{location.Name} has {type}";
                break;
            case 2:
                val = $"{location.Areas[0]} has {itemData.Name}";
                break;
            case 3:
                val = $"{location.Name} has ????";
                break;
        }

        return val;
    }

    protected override bool IsHintable(ItemLocation location)
    {
        var item = location.GetItem(false);
        if (item == null || location.Traits.Contains("Missable"))
        {
            return false;
        }

        EquipRando equipRando = Generator.Get<EquipRando>();
        if (equipRando.itemData.ContainsKey(item.Value.Item) && equipRando.itemData[item.Value.Item].Category == "Key")
        {
            return true;
        }

        if (equipRando.itemData.ContainsKey(item.Value.Item) && equipRando.itemData[item.Value.Item].Category == "Ability" && FF12Flags.Other.HintAbilities.FlagEnabled)
        {
            return true;
        }

        return false;
    }
}

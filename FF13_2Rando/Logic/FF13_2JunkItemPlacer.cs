using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando.Logic;

public class FF13_2JunkItemPlacer : JunkItemPlacer<FF13_2ItemLocation>
{
    private HashSet<string> usedItems = new();
    public FF13_2JunkItemPlacer(SeedGenerator generator) : base(generator)
    {
    }

    public override (string Item, int Amount) GetNewItem((string Item, int Amount) orig, FF13_2ItemLocation location)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        string repItem = null;
        int amount = orig.Amount;

        if (!equipRando.itemData.ContainsKey(orig.Item))
        {
            repItem = orig.Item;
        }
        else
        {
            do
            {
                string category = equipRando.itemData[orig.Item1].Category;
                // Always replace junk adornments with materials
                if (equipRando.itemData[orig.Item].Category == "Adornment")
                {
                    category = "Material";
                }

                IEnumerable<ItemData> possible = equipRando.itemData.Values.Where(i =>
                    i.Category == category).Where(i =>
                    {
                        // Don't allow dlc items if rando DLC wasn't enabled
                        return i.Traits.Contains("DLC") ? FF13_2Flags.Other.RandoDLC.Enabled : true;
                    });

                repItem = RandomNum.SelectRandomOrDefault(possible)?.ID;
            } while (repItem == null);
            // Add to used items if an weapon, adornment, monster crystal or accessory
            if (equipRando.itemData[repItem].Category == "Adornment" ||
                equipRando.itemData[repItem].Category == "Weapon" ||
                equipRando.itemData[repItem].Category == "Accessory" ||
                equipRando.itemData[repItem].Category == "MonsterCrystal")
            {
                usedItems.Add(repItem);
            }
        }

        return ModifyAmount((repItem, amount));
    }
}

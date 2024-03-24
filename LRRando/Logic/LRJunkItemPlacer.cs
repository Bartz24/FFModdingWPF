using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando;
public class LRJunkItemPlacer : JunkItemPlacer<ItemLocation>
{
    private HashSet<string> usedItems = new ();
    public LRJunkItemPlacer(SeedGenerator generator) : base(generator)
    {
    }

    public override void PlaceItems()
    {
        usedItems.Clear();
        base.PlaceItems();
    }

    public override (string Item, int Amount) GetNewItem((string Item, int Amount) orig)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        string repItem = null;
        int amount = orig.Amount;

        if (!equipRando.itemData.ContainsKey(orig.Item) || 
            equipRando.itemData[orig.Item].Category == "Adornment" && equipRando.itemData[orig.Item].Traits.Contains("Always"))
        {
            repItem = orig.Item;
        }
        else
        {
            do
            {
                string category = equipRando.itemData[orig.Item1].Category;
                if (equipRando.itemData[orig.Item].Category == "Adornment" && equipRando.itemData[orig.Item].Traits.Contains(item: "Remove"))
                {
                    category = "Material";
                }
                else if (LRFlags.Items.ReplaceAny.Enabled)
                {
                    category = equipRando.itemData.Values.Select(i => i.Category).Distinct().Shuffle().First();
                }

                int rankRange = LRFlags.Items.ReplaceRank.Value;
                IEnumerable<ItemData> possible = equipRando.itemData.Values.Where(i =>
                    i.Category == category &&
                    i.Rank >= equipRando.itemData[orig.Item1].Rank - rankRange &&
                    i.Rank <= equipRando.itemData[orig.Item1].Rank + rankRange &&
                    !i.Traits.Contains("Ignore"));
                // Remove adornments with the "Always" or "Remove" traits or used items
                possible = possible.Where(i => 
                    !(i.Category == "Adornment" && i.Traits.Contains("Always")) && 
                    !(i.Category == "Adornment" && i.Traits.Contains("Remove")) &&
                    !usedItems.Contains(i.ID)
                    );
                if (!LRFlags.Items.IncludeDLCItems.Enabled)
                {
                    possible = possible.Where(i => !i.Traits.Contains("DLC"));
                }

                repItem = RandomNum.SelectRandomOrDefault(possible)?.ID;
            } while (repItem == null);

            // Add to used items if an adornment, weapon, shield, garb, or accessory
            if (equipRando.itemData[repItem].Category == "Adornment" ||
                equipRando.itemData[repItem].Category == "Weapon" ||
                equipRando.itemData[repItem].Category == "Shield" ||
                equipRando.itemData[repItem].Category == "Garb" ||
                equipRando.itemData[repItem].Category == "Accessory")
            {
                usedItems.Add(repItem);
            }
        }

        return ModifyAmount((repItem, amount));
    }
}

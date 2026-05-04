using Bartz24.RandoWPF;
using System.Collections.Generic;
using System.Linq;

namespace LRRando;
public class LRJunkItemPlacer : JunkItemPlacer<ItemLocation>
{
    private HashSet<string> usedItems = new();
    public LRJunkItemPlacer(SeedGenerator generator) : base(generator)
    {
    }

    public override void PlaceItems()
    {
        usedItems.Clear();
        base.PlaceItems();
    }

    public override (string Item, int Amount) GetNewItem((string Item, int Amount) orig, ItemLocation location)
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
                else if (LRFlags.Items.ReplaceAny.Enabled && !location.Traits.Contains("Same"))
                {
                    category = equipRando.itemData.Values.Select(i => i.Category).Distinct()
                        .Where(c => c != "Key" && c != "Adornment" && c != "EP Ability").Shuffle().First();
                }

                int rankRange = LRFlags.Items.ReplaceRank.Value;
                IEnumerable<ItemData> possible = equipRando.itemData.Values.Where(i =>
                    i.Category == category &&
                    i.Rank >= equipRando.itemData[orig.Item1].Rank - rankRange &&
                    i.Rank <= equipRando.itemData[orig.Item1].Rank + rankRange &&
                    !i.Traits.Contains("Ignore") &&
                    !i.Traits.Contains("Key"));
                if (!LRFlags.Items.IsIncludeDLCItems())
                {
                    possible = possible.Where(i => !i.Traits.Contains("DLC"));
                }

                repItem = RandomNum.SelectRandomOrDefault(possible)?.ID;
            } while (repItem == null);

            // Add to used items if an weapon, shield, garb, or accessory
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

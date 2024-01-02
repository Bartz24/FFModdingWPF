using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF12Rando;
public class FF12UsefulItemPlacer : UsefulItemPlacer<ItemLocation>
{
    public HashSet<string> UsedAbilities = new();
    public FF12UsefulItemPlacer(SeedGenerator generator, bool logWarnings) : base(generator, logWarnings)
    {
    }

    public override void PlaceItems()
    {
        UsedAbilities.Clear();
        base.PlaceItems();
    }

    public override void ApplyToGameData()
    {
        foreach (var loc in FinalPlacement.Keys)
        {
            var rep = FinalPlacement[loc];
            var newItem = GetNewItem(rep.GetItem(true));
            loc.SetItem(newItem.Value.Item, newItem.Value.Amount);
        }
    }

    private (string Item, int Amount)? GetNewItem((string Item, int Amount)? item)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        if (item == null || !equipRando.itemData.ContainsKey(item.Value.Item1) || equipRando.itemData[item.Value.Item1].Category != "Ability")
        {
            return item;
        }   

        string rep = RandomNum.SelectRandom(equipRando.itemData.Values.Where(i => i.Category == "Ability" && !UsedAbilities.Contains(i.ID))).ID;
        UsedAbilities.Add(rep);
        return (rep, item.Value.Item2);
    }
}

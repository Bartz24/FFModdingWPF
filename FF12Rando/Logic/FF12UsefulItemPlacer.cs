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

    public override (string Item, int Amount) GetNewItem((string Item, int Amount) orig)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        if (!equipRando.itemData.ContainsKey(orig.Item) || equipRando.itemData[orig.Item].Category != "Ability")
        {
            return orig;
        }   

        string rep = RandomNum.SelectRandom(equipRando.itemData.Values.Where(i => i.Category == "Ability" && !UsedAbilities.Contains(i.ID))).ID;
        UsedAbilities.Add(rep);
        return (rep, orig.Amount);
    }
}

using Bartz24.FF12;
using Bartz24.RandoWPF;
using System.Collections.Generic;
using System.Linq;

namespace FF12Rando;
public class APShopRando : ShopRando
{
    public APShopRando(SeedGenerator randomizers) : base(randomizers)
    {
    }

    protected override HashSet<string> GetUsedAbilities()
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        return RandoFlags.GetArchipelagoData<FF12ArchipelagoData>().UsedItems.Select(s => equipRando.itemData.Values.FirstOrDefault(i => i.Name == s)).Where(i => i != null && i.Category == "Ability").Select(i => i.ID).ToHashSet();
    }

    protected override int? GetShopSphere(ShopData shop)
    {
        return RandoFlags.GetArchipelagoData<FF12ArchipelagoData>().Spheres
            .Where(data => data.ID == shop.Name + " Shop")
            .Select(data => (int?)data.Sphere)
            .FirstOrDefault();
    }
}

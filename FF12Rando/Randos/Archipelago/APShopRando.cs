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

    protected override int GetSphere(DataStoreShop shop)
    {
        return RandoFlags.GetArchipelagoData<FF12ArchipelagoData>().Spheres
            .Where(data => data.ID == shopData[shop.ID].Name + " Shop")
            .Select(data => data.Sphere)
            .FirstOrDefault(0);
    }
}

using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando;
public class APEquipRando : EquipRando
{
    public APEquipRando(SeedGenerator randomizers) : base(randomizers)
    {
    }

    public override void Load()
    {
        base.Load();

        // Create unique AP key items based on LRArchipelagoData item placements
        var apData = RandoFlags.GetArchipelagoData<LRArchipelagoData>();
        for (int i = 0; i < apData.ItemPlacements.Count; i++)
        {
            string idx = (i + 1).ToString("D4");
            string itemId = $"key_r_ap_{idx}";

            if (!items.Keys.Contains(itemId))
            {
                var apUnique = items.Copy("key_b_20", itemId);
                apUnique.sItemNameStringId = $"$zzz_r_ap_{idx}"; // Name will be populated in TextRando
                apUnique.sHelpStringId = $"$zzz_r_aph_{idx}";
                apUnique.u16SortAllByKCategory = 101;
                apUnique.u16SortCategoryByCategory = 151;
            }
        }
    }
}

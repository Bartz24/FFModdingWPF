using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando;
public class APEquipRando : EquipRando
{
    private HashSet<string> usedItems = new();

    public APEquipRando(SeedGenerator randomizers) : base(randomizers)
    {
    }

    public override void Load()
    {
        base.Load();

        // Create unique AP key items based on LRArchipelagoData item placements
        var apData = RandoFlags.GetArchipelagoData<LRArchipelagoData>();
        foreach (var placement in apData.ItemPlacements)
        {
            string idx = placement.Address.ToString("D4");
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

        // Create key_r_multi_# for tracking number of AP items collected
        // Treated as being base-50 for each "digit"
        for (int i = 0; i < 3; i++)
        {
            string itemId = $"key_r_multi_{i}";
            if (!items.Keys.Contains(itemId))
            {
                var apMulti = items.Copy("key_b_20", itemId);
                apMulti.sItemNameStringId = $"$zzz_r_multi_{i}"; // Name will be populated in TextRando
                apMulti.sHelpStringId = $"$zzz_r_multih_{i}";
                apMulti.u16SortAllByKCategory = 101;
                apMulti.u16SortCategoryByCategory = 150;
            }
        }

        // Add key_r_added to indicate the game successfully added AP items to inventory
        if (!items.Keys.Contains("key_r_added"))
        {
            var apAdded = items.Copy("key_b_20", "key_r_added");
            apAdded.sItemNameStringId = "$zzz_r_added"; // Name will be populated in TextRando
            apAdded.sHelpStringId = "$zzz_r_addedh";
            apAdded.u16SortAllByKCategory = 101;
            apAdded.u16SortCategoryByCategory = 152;
        }

        usedItems = apData.UsedItems;
    }

    public override void PostLoad()
    {
        base.PostLoad();

        var usedIds = usedItems.Select(dispName =>
        {
            // Find the item ID by display name (ignoring case and whitespace)
            var match = itemData.Values.FirstOrDefault(item => dispName == GetItemName(item.ID));
            if (match != null)
            {
                return match.ID;
            }

            return null;
        }).Where(id => id != null).ToList();

        RemainingEquip = RemainingEquip.Where(id => !usedIds.Contains(id)).ToList();

        FilterOutDLCItems();
    }
}

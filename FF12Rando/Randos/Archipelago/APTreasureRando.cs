using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FF12Rando;
class APTreasureRando : TreasureRando
{
    public APTreasureRando(SeedGenerator randomizers) : base(randomizers)
    {
    }

    public override void Randomize()
    {
        ItemLocations.Values.ForEach(l => l.SetItem("80E6", 1));
        ItemLocations.Values.Where(l=>l is RewardLocation r && r.Index != 1).ForEach(l => l.SetItem(null, 0));
        ItemLocations.Values.Where(l => l is StartingInvLocation s && s.Index > 0).ForEach(l => l.SetItem(null, 0));

        // Set filler items from AP data
        var fillerItems = RandoFlags.GetArchipelagoData<FF12ArchipelagoData>().FillerItemPlacements;

        EquipRando equipRando = Generator.Get<EquipRando>();

        fillerItems.ForEach(data =>
        {
            int intID = -1;
            try
            {
                intID = Convert.ToInt32(data.ID, 16);
            } catch (Exception)
            {
                // Ignore as some are strings
            }

            var itemID = equipRando.itemData.Values.FirstOrDefault(i => i.Name == data.Item)?.ID;

            if (itemID == null)
            {
                // If the name matches the pattern <Number> Gil, then it's a gil item
                if (Regex.IsMatch(data.Item, @"^\d+ Gil$"))                
                {
                    itemID = "Gil";
                }
                else
                {
                    throw new Exception("Filler item not found: " + data.Item);
                }
            }

            var l = ItemLocations.Values.FirstOrDefault(l => l is StartingInvLocation s && s.IntID == intID && s.Index == data.Index);
            if (l != null)
            {
                l.SetItem(itemID, data.Amount);
                return;
            }

            l = ItemLocations.Values.FirstOrDefault(l => l is RewardLocation r && r.IntID == intID && r.Index == data.Index);
            if (l != null)
            {
                l.SetItem(itemID, data.Amount);
                return;
            }

            l = ItemLocations.Values.FirstOrDefault(l => l is TreasureLocation t && t.MapID == data.ID && t.Index == data.Index);
            if (l != null)
            {
                l.SetItem(itemID, data.Amount);
                return;
            }

            throw new Exception("Filler item placement not found: " + data.ID + ":" + data.Index);
        });

        var treasures = RandoFlags.GetArchipelagoData<FF12ArchipelagoData>().Treasures.Select(data => ItemLocations.Values.First(l => l is TreasureLocation t && t.MapID == data.MapName && t.Index == data.Index)).Select(l => (TreasureLocation)l).ToList();

        SetTreasureRespawns(treasures);

        if (!FF12Flags.Items.Shops.FlagEnabled)
        {
            List<ItemLocation> initialAbilities = ItemLocations.Values.Where(l =>
            l is StartingInvLocation && l.GetItem(true) != null &&
            (l.GetItem(true).Value.Item.StartsWith("30") || l.GetItem(true).Value.Item.StartsWith("40"))).ToList();

            initialAbilities.ForEach(l =>
            {
                // Find the next empty spot for that character and add there
                var nextEmpty = ItemLocations.Values.Where(other => other is StartingInvLocation s && s.IntID == ((StartingInvLocation)l).IntID && s.GetItem(false) == null).First();
                nextEmpty.SetItem(l.GetItem(true).Value.Item, l.GetItem(true).Value.Amount);
            });
        }

        // Remove 80E6 from rewards that have other items or gil
        foreach (var l in ItemLocations.Values.Where(l => l is RewardLocation r && r.Index == 1 && r.GetItem(false) != null && r.GetItem(false).Value.Item1 == "80E6"))
        {
            // Get the other locations with the same ID
            var otherLocations = ItemLocations.Values.Where(other => other is RewardLocation r && r.IntID == ((RewardLocation)l).IntID && other != l).ToList();

            if (otherLocations.Any(other => other.GetItem(false) != null))
            {
                l.SetItem(null, 0);
            }
        }
    }
}

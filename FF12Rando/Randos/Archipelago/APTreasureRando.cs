using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FF12Rando;
class APTreasureRando : TreasureRando
{
    public APTreasureRando(SeedGenerator randomizers) : base(randomizers)
    {
    }

    /// <summary>
    /// The key item every Archipelago check physically holds. What actually gets sent is decided by
    /// the server, so this is only a placeholder in the game's own data.
    /// </summary>
    public const string ArchipelagoItemID = "80E6";

    public override void Randomize()
    {
        ItemLocations.Values.ForEach(l => l.SetItem(ArchipelagoItemID, 1));
        ItemLocations.Values.Where(l => l is RewardLocation r && r.Index != 1).ForEach(l => l.SetItem(null, 0));
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
            }
            catch (Exception)
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

        // Remove 80E6 from rewards that have other items or gil. The check that a sibling slot is
        // filled is what keeps a reward from being emptied entirely: it only ever clears a slot that
        // is not the last one holding something.
        foreach (var l in ItemLocations.Values.Where(l => l is RewardLocation r && r.Index == 1 && r.GetItem(false) != null && r.GetItem(false).Value.Item1 == ArchipelagoItemID))
        {
            // Get the other locations with the same ID
            var otherLocations = ItemLocations.Values.Where(other => other is RewardLocation r && r.IntID == ((RewardLocation)l).IntID && other != l).ToList();

            if (otherLocations.Any(other => other.GetItem(false) != null))
            {
                l.SetItem(null, 0);
            }
        }

        VerifyRewardsNotEmpty();

        // Runs last so the menu reflects the final contents of every outfitter slot. The base
        // Randomize is not called here, so without this the menu would keep its vanilla text.
        UpdateOutfittersText();
    }

    private void VerifyRewardsNotEmpty()
    {
        List<int> empty = ItemLocations.Values.OfType<RewardLocation>()
            .GroupBy(r => r.IntID)
            .Where(group => group.All(r => r.GetItem(false) == null))
            .Select(group => group.Key)
            .ToList();

        if (empty.Count > 0)
        {
            throw new Exception($"Rewards were left with no items at all: {string.Join(", ", empty.Select(id => id.ToString("X4")))}. Something is wrong with the Archipelago item placement.");
        }
    }

    protected override string GetRewardItemDisplay(RewardLocation location, string itemID, int amount)
    {
        if (itemID != ArchipelagoItemID)
        {
            return base.GetRewardItemDisplay(location, itemID, amount);
        }

        string apItemName = RandoFlags.GetArchipelagoData<FF12ArchipelagoData>().Spheres
            .Where(data => data.Index == location.Index && ParseRewardID(data.ID) == location.IntID)
            .Select(data => data.ItemDisplay)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(apItemName) ? base.GetRewardItemDisplay(location, itemID, amount) : apItemName;
    }

    private static int ParseRewardID(string id)
    {
        try
        {
            return Convert.ToInt32(id, 16);
        }
        catch (Exception)
        {
            return -1;
        }
    }
}

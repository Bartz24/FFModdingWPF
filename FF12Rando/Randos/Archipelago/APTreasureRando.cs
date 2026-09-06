using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        // Runs last so it sees the final contents. Base Randomize is replaced rather than extended
        // here, so without this call the outfitters text, the price options and the Seitengrat pass
        // would all be skipped for Archipelago seeds.
        ApplyPostPlacement();
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

    protected override string GetOutfitterDisplay(DataStoreReward reward, List<RewardLocation> locations)
    {
        if (locations.Count == 0)
        {
            return base.GetOutfitterDisplay(reward, locations);
        }

        int rewardID = locations[0].IntID;
        SortedDictionary<int, string> byIndex = new();

        // Checks the server owns. Only these carry a display name for another player's item.
        foreach (RewardLocation location in locations)
        {
            if (ArchipelagoRewardDisplays.TryGetValue((rewardID, location.Index), out string apItemName))
            {
                byIndex[location.Index] = apItemName;
            }
        }

        // Slots the world excluded from the pool hold a real local item instead of a check.
        foreach (RewardLocation location in locations.Where(l => !byIndex.ContainsKey(l.Index)))
        {
            var content = location.GetItem(false);
            if (content == null || content.Value.Item1 == ArchipelagoItemID)
            {
                // Empty, or the placeholder standing in for a slot that is not a check at all.
                continue;
            }

            byIndex[location.Index] = location.Index == 0
                ? $"{reward.Gil} Gil"
                : GetRewardItemDisplay(location, content.Value.Item1, content.Value.Item2);
        }

        // Never leave the row blank; the placeholder name beats nothing at all.
        return byIndex.Count == 0 ? base.GetOutfitterDisplay(reward, locations) : string.Join(", ", byIndex.Values);
    }

    private Dictionary<(int RewardID, int Index), string> archipelagoRewardDisplays;

    private Dictionary<(int RewardID, int Index), string> ArchipelagoRewardDisplays
    {
        get
        {
            if (archipelagoRewardDisplays != null)
            {
                return archipelagoRewardDisplays;
            }

            archipelagoRewardDisplays = new();
            foreach (var data in RandoFlags.GetArchipelagoData<FF12ArchipelagoData>().Spheres)
            {
                // Sphere ids are hex reward ids for rewards, but map or shop names for everything
                // else, so most entries are expected to fail this and are skipped.
                if (string.IsNullOrWhiteSpace(data.ItemDisplay) ||
                    !int.TryParse(data.ID, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rewardID) ||
                    rewardID < 0x9000)
                {
                    continue;
                }

                archipelagoRewardDisplays[(rewardID, data.Index)] = data.ItemDisplay;
            }

            return archipelagoRewardDisplays;
        }
    }
}

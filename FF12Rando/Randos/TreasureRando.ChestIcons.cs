using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FF12Rando;

/// <summary>
/// A chest's or urn's line in treasureIcons.txt is its icon number; RandoTreasureBank.lua answers
/// condition ChestConditionBase | number. The map screen reads its own copy of each map's navi
/// data, so treasureNavi.bin replaces it.
/// </summary>
public partial class TreasureRando
{
    public const ushort ChestNaviSprite = 63;
    public const ushort UrnNaviSprite = 61;
    public const ushort ChestConditionBase = 0x8000;
    private const int MaxChestIcons = 0x4000;

    private Dictionary<string, MapNaviOrigin> mapNaviOrigins = new();
    private List<UrnData> urns = new();

    private readonly record struct MapNaviOrigin(float X, float Z, float Width, float Height);

    /// <summary>
    /// Save is a chest's respawn id or an urn's treasure flag. Positions are world units x 10.
    /// </summary>
    private readonly record struct ChestIcon(string MapID, int Save, short PosX, short PosZ, bool IsUrn = false);

    /// <summary>Urns are daiji_box_bind script actors that record their reward with settreasureflag.</summary>
    private readonly record struct UrnData(string RewardID, string MapID, int TreasureFlag, float X, float Z);

    private void LoadMapNaviOrigins()
    {
        mapNaviOrigins = File.ReadAllLines("data\\mapNaviOrigins.csv")
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Split(','))
            .ToDictionary(
                p => p[0].ToUpperInvariant(),
                p => new MapNaviOrigin(
                    float.Parse(p[1], CultureInfo.InvariantCulture),
                    float.Parse(p[2], CultureInfo.InvariantCulture),
                    float.Parse(p[3], CultureInfo.InvariantCulture),
                    float.Parse(p[4], CultureInfo.InvariantCulture)));
    }

    private void LoadUrns()
    {
        urns.Clear();
        FileHelpers.ReadCSVFile(@"data\urns.csv", row => urns.Add(new UrnData(
            row[0].ToUpperInvariant(),
            row[1],
            int.Parse(row[2]),
            float.Parse(row[3], CultureInfo.InvariantCulture),
            float.Parse(row[4], CultureInfo.InvariantCulture))), FileHelpers.CSVFileHeader.HasHeader);
    }

    /// <summary>Urns whose reward holds anything this seed.</summary>
    private IEnumerable<UrnData> GetFilledUrns()
    {
        return urns.Where(u => Enumerable.Range(0, 3).Any(i =>
            ItemLocations.TryGetValue($"{u.RewardID}:{i}", out ItemLocation l) && l.GetItem(false) != null));
    }

    /// <summary>The chests that spawn this seed, then the filled urns, in icon number order.</summary>
    private List<ChestIcon> GetChestIcons()
    {
        List<ChestIcon> icons = ItemLocations.Values
            .Where(l => l is TreasureLocation && l.GetItem(false) != null)
            .Select(l => (TreasureLocation)l)
            .Select(l => (Location: l, Data: ebpAreas[l.MapID].TreasureList[l.Index]))
            .Where(t => t.Data.SpawnChance > 0 && t.Data.Respawn < 255)
            .OrderBy(t => t.Location.MapID, StringComparer.Ordinal)
            .ThenBy(t => t.Location.Index)
            .Select(t => new ChestIcon(t.Location.MapID, t.Data.Respawn, t.Data.PosX, t.Data.PosZ))
            .ToList();

        icons.AddRange(GetFilledUrns()
            .OrderBy(u => u.MapID, StringComparer.Ordinal)
            .ThenBy(u => u.TreasureFlag)
            .Select(u => new ChestIcon(u.MapID, u.TreasureFlag, (short)MathF.Round(u.X * 10), (short)MathF.Round(u.Z * 10), true)));

        if (icons.Count > MaxChestIcons)
        {
            throw new Exception($"{icons.Count} chests and urns is more than the {MaxChestIcons} the icon conditions can address.");
        }

        return icons;
    }

    /// <summary>Invariant culture: some locales write a U+2212 minus, which the Lua reader rejects.</summary>
    private void SaveChestIconList(List<ChestIcon> icons)
    {
        File.WriteAllLines($"{Generator.OutFolder}\\treasureIcons.txt",
            icons.Select(i => FormattableString.Invariant($"{areaMapping[i.MapID]},{i.Save},{i.PosX},{i.PosZ}") + (i.IsUrn ? ",u" : "")));
    }

    private void SaveChestNaviIcons(List<ChestIcon> icons)
    {
        using FileStream file = File.Create($"{Generator.OutFolder}\\treasureNavi.bin");
        using BinaryWriter writer = new(file);

        foreach (IGrouping<string, (ChestIcon Icon, int Number)> map in icons
            .Select((icon, number) => (Icon: icon, Number: number))
            .GroupBy(i => i.Icon.MapID))
        {
            string mapHex = areaMapping[map.Key].ToUpperInvariant();
            if (!mapNaviOrigins.TryGetValue(mapHex, out MapNaviOrigin origin))
            {
                // Trial Mode floors and a few others have no map screen. The minimap marker
                // still works there since it comes from the chest or urn actor.
                continue;
            }

            List<EbpNaviIcons.Entry> entries = new();
            foreach ((ChestIcon icon, int number) in map)
            {
                float x = icon.PosX / 10f - origin.X;
                float z = icon.PosZ / 10f - origin.Z;
                if (x < 0 || z < 0 || x > origin.Width || z > origin.Height)
                {
                    // Off the map image (bhm_a03 spans several images); drawing it would put
                    // the icon somewhere misleading.
                    continue;
                }

                // The map screen skips icons at a coordinate of 0.
                short sx = (short)Math.Max(1, MathF.Round(x));
                short sz = (short)Math.Max(1, MathF.Round(z));
                ushort sprite = icon.IsUrn ? UrnNaviSprite : ChestNaviSprite;
                entries.Add(new EbpNaviIcons.Entry(sx, sz, sx, sz, sprite, (ushort)(ChestConditionBase | number)));
            }

            if (entries.Count == 0)
            {
                continue;
            }

            byte[] blob = EbpNaviIcons.BuildWithEntries(GetMapEbpData(map.Key), EbpNaviIcons.MapScreenSection, entries);
            writer.Write((ushort)int.Parse(mapHex, NumberStyles.HexNumber));
            writer.Write((ushort)0);
            writer.Write(blob.Length);
            writer.Write(blob);
            writer.Write(new byte[(4 - blob.Length % 4) % 4]);
        }
    }

    /// <summary>The map's ebp; maps with urns but no chests are not loaded as treasure areas.</summary>
    private byte[] GetMapEbpData(string mapID)
    {
        if (ebpAreas.TryGetValue(mapID, out DataStoreEBP ebp))
        {
            return ebp.Data;
        }

        string area = mapID.Substring(0, mapID.Length - 2);
        return File.ReadAllBytes($"data\\ps2data\\plan_master\\us\\plan_map\\{area}\\{mapID}\\global\\{mapID}.ebp");
    }
}

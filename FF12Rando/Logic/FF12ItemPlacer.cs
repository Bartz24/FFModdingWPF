using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace FF12Rando;
public class FF12ItemPlacer : ItemPlacer<ItemLocation>
{
    public ProgressionItemPlacer<ItemLocation> ProgressionItemPlacer { get; set; }
    public FF12UsefulItemPlacer UsefulItemPlacer { get; set; }
    public FF12JunkItemPlacer JunkItemPlacer { get; set; }

    public SphereCalculator<ItemLocation> SphereCalculator { get; set; }

    public override Dictionary<ItemLocation, ItemLocation> FinalPlacement
    {
        get
        {
            Dictionary<ItemLocation, ItemLocation> final = new();
            if (ProgressionItemPlacer != null)
            {
                final = final.Concat(ProgressionItemPlacer.FinalPlacement).ToDictionary(k => k.Key, v => v.Value);
            }

            if (UsefulItemPlacer != null)
            {
                final = final.Concat(UsefulItemPlacer.FinalPlacement).ToDictionary(k => k.Key, v => v.Value);
            }

            if (JunkItemPlacer != null)
            {
                final = final.Concat(JunkItemPlacer.FinalPlacement).ToDictionary(k => k.Key, v => v.Value);
            }

            return final;
        }
    }

    public FF12ItemPlacer(SeedGenerator generator) : base(generator)
    {
        SphereCalculator = new(Generator);
    }

    private HashSet<ItemLocation> GetProgressionLocations(HashSet<ItemLocation> fixedItems)
    {
        return PossibleLocations.Where(l =>
        {
            if (fixedItems.Contains(l))
            {
                return false;
            }

            if (l.Traits.Contains("Missable"))
            {
                return false;
            }

            foreach (string item in FF12Flags.Items.KeyItems.DictValues.Keys)
            {
                if (FF12Flags.Items.KeyItems.SelectedKeys.Contains(item) && l.GetItem(true)?.Item == item)
                {
                    return true;
                }
            }

            if (l.Traits.Any(s => s.StartsWith("Chop")) && IsRandomizedChop(l))
            {
                return true;
            }

            if (l.Traits.Any(s => s.StartsWith("BlackOrb")) && IsRandomizedOrb(l))
            {
                return true;
            }

            List<string> placeTraits = new() { "Hunt", "ClanRank", "ClanBoss", "ClanEsper", "Grindy", "Hidden" };

            if (FF12Flags.Items.KeyPlaceHunt.Enabled && l.Traits.Contains("Hunt"))
            {
                return true;
            }

            if (FF12Flags.Items.KeyPlaceClanRank.Enabled && l.Traits.Contains("ClanRank"))
            {
                return true;
            }

            if (FF12Flags.Items.KeyPlaceClanBoss.Enabled && l.Traits.Contains("ClanBoss"))
            {
                return true;
            }

            if (FF12Flags.Items.KeyPlaceClanEsper.Enabled && l.Traits.Contains("ClanEsper"))
            {
                return true;
            }

            if (FF12Flags.Items.KeyPlaceGrindy.Enabled && l.Traits.Contains("Grindy"))
            {
                return true;
            }

            if (FF12Flags.Items.KeyPlaceHidden.Enabled && l.Traits.Contains("Hidden"))
            {
                return true;
            }

            if (FF12Flags.Items.KeyPlaceTreasure.Enabled && !l.Traits.Intersect(placeTraits).Any())
            {
                return true;
            }

            return false;
        }).ToHashSet();
    }

    private HashSet<ItemLocation> GetProgressionItems(HashSet<ItemLocation> fixedItems)
    {
        return LocationsToPlace.Where(l =>
        {
            if (fixedItems.Contains(l))
            {
                return false;
            }

            foreach (string item in FF12Flags.Items.KeyItems.DictValues.Keys)
            {
                if (FF12Flags.Items.KeyItems.SelectedKeys.Contains(item) && l.GetItem(true)?.Item == item)
                {
                    return true;
                }
            }

            if (l.Traits.Any(s => s.StartsWith("Chop")) && IsRandomizedChop(l))
            {
                return true;
            }

            if (l.Traits.Any(s => s.StartsWith("BlackOrb")) && IsRandomizedOrb(l))
            {
                return true;
            }

            return false;
        }).ToHashSet();
    }

    private HashSet<ItemLocation> GetFixedItems()
    {
        return LocationsToPlace.Where(l =>
        {
            if (FF12Flags.Items.WritGoals.SelectedValues.Contains(FF12Flags.Items.WritGoalCid2) && l.Traits.Contains("WritCid2"))
            {
                return true;
            }

            if (l is FakeLocation)
            {
                return true;
            }

            foreach (string item in FF12Flags.Items.KeyItems.DictValues.Keys)
            {
                if (!FF12Flags.Items.KeyItems.SelectedKeys.Contains(item) && l.GetItem(true)?.Item == item)
                {
                    return true;
                }
            }

            if (l.Traits.Any(s => s.StartsWith("Chop") && !IsRandomizedChop(l)))
            {
                return true;
            }

            if (l.Traits.Any(s => s.StartsWith("BlackOrb") && !IsRandomizedOrb(l)))
            {
                return true;
            }

            return false;
        }).ToHashSet();
    }

    public override void PlaceItems()
    {
        Dictionary<string, double> areaMults = PossibleLocations.SelectMany(t => t.Areas).Distinct().ToDictionary(s => s, _ => RandomNum.RandInt(10, 200) * 0.01d);

        ProgressionItemPlacer = new(Generator, GetDifficulty(), areaMults);
        UsefulItemPlacer = new(Generator, false);
        JunkItemPlacer = new(Generator, this);

        // Place progression items first
        var fixedItems = GetFixedItems();
        var progressionItems = GetProgressionItems(fixedItems);
        var progressionLocations = GetProgressionLocations(fixedItems);

        ProgressionItemPlacer.LocationsToPlace = progressionItems;
        ProgressionItemPlacer.PossibleLocations = progressionLocations;
        ProgressionItemPlacer.FixedLocations = fixedItems;
        ProgressionItemPlacer.PlaceItems();

        // Place useful items next
        // Useful items are abilities
        var usefulItems = LocationsToPlace.Except(fixedItems).Where(l => (l.GetItem(false).Value.Item.StartsWith("30") || l.GetItem(false).Value.Item.StartsWith("40")) && !progressionItems.Contains(l)).ToHashSet();
        var usefulLocations = PossibleLocations.Except(fixedItems).Except(progressionLocations).Concat(ProgressionItemPlacer.GetUnusedLocations()).ToHashSet();
        // Remove missable locations
        usefulLocations = usefulLocations.Where(l => !l.Traits.Contains("Missable")).ToHashSet();

        UsefulItemPlacer.LocationsToPlace = usefulItems;
        UsefulItemPlacer.PossibleLocations = usefulLocations;
        UsefulItemPlacer.PlaceItems();

        // Place junk items last
        // Junk items are everything else
        var junkItems = LocationsToPlace.Except(fixedItems).Except(progressionItems).Except(usefulItems).ToHashSet();
        var junkLocations = PossibleLocations.Except(fixedItems).Except(progressionLocations).Except(usefulLocations).Concat(UsefulItemPlacer.GetUnusedLocations()).ToHashSet();
        
        JunkItemPlacer.LocationsToPlace = junkItems;
        JunkItemPlacer.PossibleLocations = junkLocations;
        JunkItemPlacer.PlaceItems();
    }

    public override void ApplyToGameData()
    {
        ProgressionItemPlacer.ApplyToGameData();
        UsefulItemPlacer.ApplyToGameData();
        JunkItemPlacer.ApplyToGameData();

        ClearUnsetLocations();

        // Calculate spheres
        SphereCalculator = new SphereCalculator<ItemLocation>(Generator);
        SphereCalculator.CalculateSpheres(PossibleLocations);

        // Reorder items
        EquipRando equipRando = Generator.Get<EquipRando>();
        var categories = new HashSet<string>() { "Item", "Weapon", "Armor", "Accessory" };
        var itemReorderer = new ItemReorderer<ItemLocation, ItemData>(Generator, categories, equipRando.itemData);
        itemReorderer.ReorderItems(PossibleLocations, SphereCalculator);

        // Place a Writ of Transit in a location in the max sphere that is non-missable
        if (FF12Flags.Items.WritGoals.SelectedValues.Contains(FF12Flags.Items.WritGoalMaxSphere))
        {
            int sphere = SphereCalculator.Spheres.Values.Max();
            bool placed = false;
            while (!placed)
            {
                HashSet<ItemLocation> maxSphere = PossibleLocations.Where(l =>
                {
                    return SphereCalculator.Spheres.GetValueOrDefault(l, 0) == sphere
                            && l is not FakeLocation
                            && !l.Traits.Contains("Missable")
                            && (l.GetItem(false) == null ||
                                equipRando.itemData.ContainsKey(l.GetItem(false)?.Item)
                                && categories.Contains(equipRando.itemData[l.GetItem(false)?.Item].Category))
                            && l.GetItem(false)?.Item != "8070";
                }).ToHashSet();

                if (maxSphere.Count > 0)
                {
                    ItemLocation l = RandomNum.SelectRandom(maxSphere);
                    l.SetItem("8070", 1);
                    placed = true;
                }
                else
                {
                    sphere--;
                }
            }
        }
    }

    public int GetDifficulty()
    {
        switch (FF12Flags.Items.KeyDepth.SelectedIndex)
        {
            case 0:
            default:
                return 10;
            case 1:
                return 7;
            case 2:
                return 5;
            case 3:
                return 3;
            case 4:
                return 1;
        }
    }

    public bool IsRandomizedChop(ItemLocation location)
    {
        string chopStr = location.Traits.FirstOrDefault(s => s.StartsWith("Chop"));
        if (chopStr == null)
        {
            return false;
        }

        int chop = int.Parse(chopStr.Substring(4));

        return chop <= FF12Flags.Items.KeyChops.Value;
    }

    public bool IsRandomizedOrb(ItemLocation location)
    {
        string orbStr = location.Traits.FirstOrDefault(s => s.StartsWith("BlackOrb"));
        if (orbStr == null)
        {
            return false;
        }

        int orb = int.Parse(orbStr.Substring(8));

        return orb <= FF12Flags.Items.KeyBlackOrbs.Value;
    }
}

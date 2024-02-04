using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace FF12Rando;
public class FF12ItemPlacer : CombinedItemPlacer<ItemLocation, ItemData>
{
    public ProgressionItemPlacer<ItemLocation> ProgressionItemPlacer { get; set; }
    public FF12UsefulItemPlacer UsefulItemPlacer { get; set; }
    public FF12JunkItemPlacer JunkItemPlacer { get; set; }

    public FF12ItemPlacer(SeedGenerator generator) : base(generator) 
    { 

    }

    protected override HashSet<ItemLocation> GetLocationsForPlacer(HashSet<ItemLocation> usedLocations, ItemPlacer<ItemLocation> placer)
    {
        var possible = PossibleLocations.Except(usedLocations).ToHashSet();

        if (placer == ProgressionItemPlacer)
        {
            return GetProgressionLocations(possible);
        }
        else if (placer == UsefulItemPlacer)
        {
            return possible.Where(l => !l.Traits.Contains("Missable")).ToHashSet();
        }
        else if (placer == JunkItemPlacer)
        {
            return possible;
        }
        else
        {
            throw new Exception("Unknown placer");
        }
    }

    protected HashSet<ItemLocation> GetProgressionLocations(HashSet<ItemLocation> possible)
    {
        return possible.Where(l =>
        {
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

    protected override HashSet<ItemLocation> GetReplacementsForPlacer(HashSet<ItemLocation> usedReplacements, ItemPlacer<ItemLocation> placer)
    {
        var remaining = Replacements.Except(usedReplacements).ToHashSet();
        if (placer == ProgressionItemPlacer)
        {
            return remaining.Where(l =>
            {
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
        else if (placer == UsefulItemPlacer)
        {
            return remaining
                .Where(l => 
                    l.GetItem(false).Value.Item.StartsWith("30") ||
                    l.GetItem(false).Value.Item.StartsWith("40"))
                .ToHashSet();
        }
        else if (placer == JunkItemPlacer)
        {
            return remaining;
        }
        else
        {
            throw new Exception("Unknown placer");
        }
    }

    protected override HashSet<ItemLocation> GetFixedLocations()
    {
        return Replacements.Where(l =>
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

    protected override void RebuildPlacers()
    {
        Dictionary<string, double> areaMults = PossibleLocations.SelectMany(t => t.Areas).Distinct().ToDictionary(s => s, _ => RandomNum.RandInt(10, 200) * 0.01d);

        ProgressionItemPlacer = new(Generator, GetDifficulty(), areaMults);
        ProgressionItemPlacer.FixedLocations = GetFixedLocations();
        UsefulItemPlacer = new(Generator, false);
        JunkItemPlacer = new(Generator, this);

        Placers = new() { ProgressionItemPlacer, UsefulItemPlacer, JunkItemPlacer };
    }

    public override void ApplyToGameData()
    {
        base.ApplyToGameData();

        EquipRando equipRando = Generator.Get<EquipRando>();
        var categories = GetReorderItemCategories();

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

    protected override HashSet<string> GetReorderItemCategories()
    {
        return new() { "Item", "Weapon", "Armor", "Accessory" };
    }

    protected override Dictionary<string, ItemData> GetReorderItems()
    {
        return Generator.Get<EquipRando>().itemData;
    }

    protected override int GetDifficultyIndex()
    {
        return FF12Flags.Items.KeyDepth.SelectedIndex;
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

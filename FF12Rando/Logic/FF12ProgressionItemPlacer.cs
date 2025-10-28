using Bartz24.FF12;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data.Areas;
using Bartz24.RandoWPF.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FF12Rando.TreasureRando;

namespace FF12Rando;
public class FF12ProgressionItemPlacer : ProgressionItemPlacer<ItemLocation>
{
    public FF12ProgressionItemPlacer(SeedGenerator generator, AreaGraph areaGraph, int depthDiff, Dictionary<string, double> areaMults) : base(generator, areaGraph, depthDiff, areaMults)
    {
    }

    protected override string GetSimilarItemType(ItemLocation location)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        var item = location.GetItem(false);
        if (item != null && equipRando.itemData[item?.Item].IntID is >= 0x80B9 and <= 0x80D6)
        {
            return "Trophy";
        }
        else if (item != null && equipRando.itemData[item?.Item].Category == "Esper")
        {
            return "Esper";
        }
        else if (item != null && equipRando.itemData[item?.Item].IntID is >= 0x80E1 and <= 0x80E5)
        {
            return "Aeropass";
        }
        else if (item != null && equipRando.itemData[item?.Item].IntID is
            0x8089 or
            0x808B or
            0x808C or
            0x8078 or
            0x80AC)
        {
            return "Cid2Unlock";
        }

        return base.GetSimilarItemType(location);
    }

    protected override (int min, int max)? GetCustomItemTypeRange(string itemTypeName)
    {
        // Trophies should not be limited
        if (itemTypeName == "Trophy")
        {
            return (0, 100);
        }
        // Black orbs should appear in later half at minimum most of the time
        else if (itemTypeName == "2116" && RandomNum.RandInt(0, 99) < 60)
        {
            int min = RandomNum.RandInt(50, 90);
            int max = Math.Min(min + RandomNum.RandInt(10, 30), 100);
            return (min, max);
        }
        else if (itemTypeName == "Cid2Unlock")
        {
            int min = RandomNum.RandInt(5, 60);
            int max = Math.Min(min + RandomNum.RandInt(10, 50), 100);
            return (min, max);
        }
        else if (itemTypeName == "Esper")
        {
            int min = RandomNum.RandInt(20, 90);
            int max = Math.Min(min + RandomNum.RandInt(10, 60), 100);
            return (min, max);
        }
        // Cactus Flower
        else if (itemTypeName == "8073")
        {
            int min = RandomNum.RandInt(50, 70);
            int max = Math.Min(min + RandomNum.RandInt(20, 30), 100);
            return (min, max);
        }
        // Clan Primer
        else if (itemTypeName == "8071")
        {
            int min = RandomNum.RandInt(10, 60);
            int max = Math.Min(min + RandomNum.RandInt(50, 80), 100);
            return (min, max);
        }
        else if (itemTypeName == "Aeropass")
        {
            int min = RandomNum.RandInt(30, 90);
            int max = Math.Min(min + RandomNum.RandInt(40, 60), 100);
            return (min, max);
        }

        return base.GetCustomItemTypeRange(itemTypeName);
    }

    protected override void PlaceFixed()
    {
        // Edge case for victory
        if (RemainingFixed.Count == 1 && RemainingFixed.First().Name == "Final Boss Victory")
        {
            PlaceItem(RemainingFixed.First(), RemainingFixed.First());
            RemainingFixed.Remove(RemainingFixed.First());
        }
        else
        {
            base.PlaceFixed();
        }
    }
}

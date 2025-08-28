using Bartz24.FF12;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data.Areas;
using Bartz24.RandoWPF.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    protected override int GetLocationOffset(ItemLocation location, string itemType)
    {
        int offset = base.GetLocationOffset(location, itemType);
        if (itemType == "Cid2Unlock")
        {
            offset += RandomNum.RandInt(5, 50);
        }
        if (itemType == "Esper")
        {
            offset += RandomNum.RandInt(10, 30);
        }

        return offset;
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

using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data.Areas;
using Bartz24.RandoWPF.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando.Logic;
public class FF13_2ProgressionItemPlacer : ProgressionItemPlacer<FF13_2ItemLocation>
{
    public FF13_2ProgressionItemPlacer(SeedGenerator generator, AreaGraph areaGraph, int depthDiff, Dictionary<string, double> areaMults) : base(generator, areaGraph, depthDiff, areaMults)
    {
    }

    protected override (int min, int max)? GetCustomItemTypeRange(string itemTypeName)
    {
        // TODO: validate this does sensible things at all difficulty levels...
        // May need further refining - probably not quite this simple?
        if (itemTypeName.StartsWith("frg_cmn_gvtn") & FF13_2Flags.Other.ForceAcadVoidEndgame.Enabled)
        {
            // Force graviton cores to be adjusted based on difficulty if fixed endgame is enabled
            var diffLevel = FF13_2Flags.Items.KeyDepth.SelectedIndex;
            return (20 * diffLevel, 50 + 10 * diffLevel);
        }
        return null;
    }
}


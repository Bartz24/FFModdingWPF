using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF13Rando;

public class ManualTreasureRando : TreasureRando
{
    public ManualTreasureRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Randomize()
    {
        CrystariumRando crystariumRando = Generator.Get<CrystariumRando>();

        // Get the first roles of each character, these will be skipped
        List<string> ignoreList = new();
        foreach (string charName in crystariumRando.chars)
        {
            string firstRole = crystariumRando.GetFirstRole(charName);
            string itemName = $"z_ran_{charName.Substring(0, 3).ToLower()}_{firstRole.Substring(0, 3).ToLower()}";
            ignoreList.Add(itemName);

            string weapName = $"z_ini_{charName.Substring(0, 3)}_wea";
            ignoreList.Add(weapName);
        }

        foreach (FF13ItemLocation loc in ItemLocations.Values)
        {
            if (ignoreList.Contains(loc.ID))
            {
                continue;
            }

            loc.SetItem("ap_item", 1);
        }
    }
}

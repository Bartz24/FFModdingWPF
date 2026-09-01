using Bartz24.FF13Series;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando;

public class FF13_2ArchipelagoSeedGenerator: FF13_2SeedGenerator
{
    public FF13_2ArchipelagoSeedGenerator(): base()
    {

    }

    protected override void SetRandomizers()
    {
        // Replace treasure equip and text with AP equivalents (and crux?)
        Randomizers = new()
        {
            // local only (for now?)
            new CrystariumRando(this),
            new APEquipRando(this),
            // exposed for future work but not currently supported to shuffle worlds
            new APHistoriaCruxRando(this),
            new APTreasureRando(this),
            // local only
            new BattleRando(this),
            // local only
            new EnemyRando(this),
            // local only
            new MusicRando(this),
            new APTextRando(this)
        };
    }

    protected override void VerifyDLCOptions()
    {
        var apData = RandoFlags.GetArchipelagoData<FF13_2ArchipelagoData>();

        var dlcEnabled = apData.AllowDLCItems;

        var nodeMaybe = new FF13_2AreaNode();
        var hasDlcNode = apData.AreaGraph.TryGetValue("h_zz_NA0970", out nodeMaybe);
        if (hasDlcNode)
        {
            if (nodeMaybe.loc_x != -1 || nodeMaybe.loc_y != -1)
            {
                dlcEnabled = true;
            }
        }

        if (dlcEnabled && !Nova.IsModInstalled(SetupData.Paths["Nova"], "DLC Restoration - Console Content", "13-2"))
        {
            throw new RandoException("The 'Allow DLC Locations' or 'Allow DLC Items' flags are turned on and require the following mod that is detected to be missing:\n" +
                "'DLC Restoration - Console Content'\n\n" +
                "Download and install the mod from the Core Mods download in the Nova discord server.\n" +
                "Once this mod is installed, you will be able to generate the rando modpack.", "Additional mods required");
        }
    }


}

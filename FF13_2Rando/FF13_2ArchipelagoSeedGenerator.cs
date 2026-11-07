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

    private bool DoesGraphIndicateDlcEnabled(Dictionary<string, FF13_2AreaNode> graph)
    {
        // If 970 is placed at (1,5) then its potentially vanilla
        var dlcRootNode = graph["h_zz_NA0970"];
        if (dlcRootNode.loc_x != 1 || dlcRootNode.loc_y != 5)
        {
            return true;
        }
        // If any of the links are not the expected dlc ones then we're probably also enabled
        string[] expectedLinks = ["h_va_NA0001", "h_cs_NA0001", "h_cl_NA0001"];
        if (dlcRootNode.links.Where(l => !expectedLinks.Contains(l.target_area)).Any())
        {
            return true;
        }
        var valDlcNode = graph["h_va_NA0001"];
        if (valDlcNode.loc_x != 1 || valDlcNode.loc_y != 6)
        {
            return true;
        }
        var csDlcNode = graph["h_cs_NA0001"];
        if (csDlcNode.loc_x != 0 || csDlcNode.loc_y != 4)
        {
            return true;
        }
        var clDlcNode = graph["h_cl_NA0001"];
        if (clDlcNode.loc_x != 0 || clDlcNode.loc_y != 5)
        {
            return true;
        }

        return false;
    }

    protected override void VerifyDLCOptions()
    {
        var apData = RandoFlags.GetArchipelagoData<FF13_2ArchipelagoData>();

        var dlcEnabled = apData.AllowDLCItems;

        var nodeMaybe = new FF13_2AreaNode();
        var version = apData.Version;
        if (version == "0.3.1")
        {
            dlcEnabled |= apData.AllowDLCLocations;
        }
        else
        {
            dlcEnabled |= DoesGraphIndicateDlcEnabled(apData.AreaGraph);
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

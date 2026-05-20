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
}

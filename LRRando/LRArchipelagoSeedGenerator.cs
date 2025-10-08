using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando;
public class LRArchipelagoSeedGenerator : LRSeedGenerator
{
    public LRArchipelagoSeedGenerator() : base()
    {
    }

    protected override void SetRandomizers()
    {
        Randomizers = new()
        {
            new QuestRando(this),
            new APTreasureRando(this),
            new APEquipRando(this),
            new ShopRando(this),
            new AbilityRando(this),
            new EnemyRando(this),
            new BattleRando(this),
            new MusicRando(this),
            new APTextRando(this)
        };
    }
}

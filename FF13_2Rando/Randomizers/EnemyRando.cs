using Bartz24.Data;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF13_2Rando;

public class EnemyRando : Randomizer
{
    public Dictionary<string, DataStoreDB3<DataStoreBtCharaSpec>> enemies = new();
    public Dictionary<string, DataStoreDB3<DataStoreBtCharaSpec>> enemiesOrig = new();
    private readonly string[] x000 =
    {
        "bt_chsp_x000_2",
        "bt_chsp_x000_3",
        "bt_chsp_x000_5",
        //"bt_chsp_x000_6",
        "bt_chsp_x000_7",
        "bt_chsp_x000_8",
        "bt_chsp_x000_9",
        "bt_chsp_x000_11",
        "bt_chsp_x000_107",
        "bt_chsp_x000_108"
    };

    public EnemyRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Enemy Data...");
        x000.ForEach(s =>
        {
            DataStoreDB3<DataStoreBtCharaSpec> db3 = new();
            db3.LoadDB3(Generator, "13-2", @"\btscene\pack\wdb\_x000.bin\" + s + ".wdb", false);
            enemies.Add(s, db3);
        });
        x000.ForEach(s =>
        {
            DataStoreDB3<DataStoreBtCharaSpec> db3 = new();
            db3.LoadDB3(Generator, "13-2", @"\btscene\pack\wdb\_x000.bin\" + s + ".wdb", false);
            enemiesOrig.Add(s, db3);
        });

        GetEnemies(e => e.u14DropProb2 is > 0 and < 7500).ForEach(e => e.u14DropProb2 = Math.Min((int)(e.u14DropProb2 * 2.5), 7500));
    }

    public DataStoreBtCharaSpec GetEnemy(string id, bool orig = false)
    {
        Dictionary<string, DataStoreDB3<DataStoreBtCharaSpec>> dbs = orig ? enemiesOrig : enemies;
        return dbs.Values.SelectMany(db3 => db3.Values.Where(e => e.name == id)).First();
    }
    public bool HasEnemy(string id)
    {
        return enemies.Values.SelectMany(db3 => db3.Values.Where(e => e.name == id)).Count() > 0;
    }
    public IEnumerable<DataStoreBtCharaSpec> GetEnemies()
    {
        return GetEnemies(_ => true);
    }
    public IEnumerable<DataStoreBtCharaSpec> GetEnemies(Func<DataStoreBtCharaSpec, bool> predicate)
    {
        return enemies.Values.SelectMany(db3 => db3.Values.Where(e => predicate(e)));
    }

    public override void Randomize()
    {
        if (FF13_2Flags.Debug.HighStats.FlagEnabled)
        {
            GetEnemy("pc008").u24MaxHp = 99999;
            GetEnemy("pc010").u24MaxHp = 99999;
            GetEnemy("pc008").u16StatusStr = 9999;
            GetEnemy("pc010").u16StatusStr = 9999;
            GetEnemy("pc008").u16StatusMgk = 9999;
            GetEnemy("pc010").u16StatusMgk = 9999;
        }
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Enemy Data...");
        x000.ForEach(s =>
        {
            enemies[s].SaveDB3(Generator, @"\btscene\pack\wdb\_x000.bin\" + s + ".wdb");
            SetupData.WPDTracking[Generator.DataOutFolder + @"\btscene\pack\wdb\x000.bin"].Add(s + ".wdb");
        });
    }
}

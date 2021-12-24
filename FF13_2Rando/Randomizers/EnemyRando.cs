using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bartz24.FF13_2_LR.Enums;

namespace FF13_2Rando
{
    public class EnemyRando : Randomizer
    {
        public Dictionary<string, DataStoreDB3<Bartz24.FF13_2.DataStoreBtCharaSpec>> enemies = new Dictionary<string, DataStoreDB3<Bartz24.FF13_2.DataStoreBtCharaSpec>>();

        string[] x000 = new string[] {
            "bt_chsp_x000_2",
            "bt_chsp_x000_3",
            "bt_chsp_x000_5",
            //"bt_chsp_x000_6",
            "bt_chsp_x000_7",
            "bt_chsp_x000_8",
            "bt_chsp_x000_9",
            "bt_chsp_x000_11"
        };

        public EnemyRando(RandomizerManager randomizers) : base(randomizers) {  }

        public override string GetProgressMessage()
        {
            return "Randomizing Enemies...";
        }
        public override string GetID()
        {
            return "Enemies";
        }

        public override void Load()
        {
            x000.ForEach(s => {
                DataStoreDB3<Bartz24.FF13_2.DataStoreBtCharaSpec> db3 = new DataStoreDB3<Bartz24.FF13_2.DataStoreBtCharaSpec>();
                db3.LoadDB3("13-2", @"\btscene\pack\wdb\_x000.bin\" + s + ".wdb", false);
                enemies.Add(s, db3);
            });
            
        }
        public override void Randomize(Action<int> progressSetter)
        {
            enemies.Values.ForEach(db3 => db3.Values.Where(e => e.name.StartsWith("m")).ForEach(e => e.u24MaxHp = 10));
            enemies.Values.ForEach(db3 => db3.Values.Where(e => e.name.StartsWith("m760b")).ForEach(e => e.u24MaxHp = 10000));
        }

        public override void Save()
        {
            x000.ForEach(s => {
                enemies[s].SaveDB3(@"\btscene\pack\wdb\_x000.bin\" + s + ".wdb");
                SetupData.WPDTracking[SetupData.OutputFolder + @"\btscene\pack\wdb\x000.bin"].Add(s + ".wdb");
            });      
        }
    }
}

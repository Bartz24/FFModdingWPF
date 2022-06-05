using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13Rando
{
    public class BattleRando : Randomizer
    {
        public DataStoreWDB<DataStoreBtScene> btscene = new DataStoreWDB<DataStoreBtScene>();
        public DataStoreWDB<DataStoreBtScene> btsceneOrig = new DataStoreWDB<DataStoreBtScene>();

        public BattleRando(RandomizerManager randomizers) : base(randomizers) {  }

        public override string GetProgressMessage()
        {
            return "Randomizing Battles...";
        }
        public override string GetID()
        {
            return "Battles";
        }

        public override void Load()
        {
            btscene.LoadWDB("13", @"\db\resident\bt_scene.wdb");
            btsceneOrig.LoadWDB("13", @"\db\resident\bt_scene.wdb");

            List<DataStoreBtScene> test = btscene.Values.Where(b => b.sDrop100Id_string != "").ToList();
        }
        public override void Randomize(Action<int> progressSetter)
        {
            TextRando textRando = Randomizers.Get<TextRando>("Text");
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>("Treasures");
        }

        public override void Save()
        {
            btscene.SaveWDB(@"\db\resident\bt_scene.wdb");

        }
    }
}

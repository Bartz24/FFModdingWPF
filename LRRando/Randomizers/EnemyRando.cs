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

namespace LRRando
{
    public class EnemyRando : Randomizer
    {
        public DataStoreDB3<DataStoreBtCharaSpec> enemies = new DataStoreDB3<DataStoreBtCharaSpec>();

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
            string path = Nova.GetNovaFile("LR", @"db\resident\bt_chara_spec.wdb", SetupData.Paths["Nova"], SetupData.Paths["LR"]);
            string outPath = SetupData.OutputFolder + @"\db\resident\bt_chara_spec.wdb";
            FileExtensions.CopyFile(path, outPath);

            enemies.Load("LR", outPath, SetupData.Paths["Nova"]);
        }
        public override void Randomize(Action<int> progressSetter)
        {
        }

        public override void Save()
        {
            //string outPath = SetupData.OutputFolder + @"\db\resident\bt_chara_spec.wdb";
            //enemies.Save(outPath, SetupData.Paths["Nova"]);
            enemies.DeleteDB3(@"\db\resident\bt_chara_spec.db3");
        }
    }
}

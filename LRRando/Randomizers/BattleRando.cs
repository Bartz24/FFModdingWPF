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
    public class BattleRando : Randomizer
    {
        DataStoreDB3<DataStoreBtScene> btScenes = new DataStoreDB3<DataStoreBtScene>();
        Dictionary<string, EnemyData> enemyData = new Dictionary<string, EnemyData>();

        public BattleRando(RandomizerManager randomizers) : base(randomizers) { }

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
            string path = Nova.GetNovaFile("LR", @"db\resident\bt_scene.wdb", SetupData.Paths["Nova"], SetupData.Paths["LR"]);
            string outPath = SetupData.OutputFolder + @"\db\resident\bt_scene.wdb";
            FileExtensions.CopyFile(path, outPath);

            btScenes.Load("LR", outPath, SetupData.Paths["Nova"]);

            enemyData = File.ReadAllLines(@"data\enemies.csv").Select(s => new EnemyData(s.Split(","))).ToDictionary(e => e.ID, e => e);
        }
        public override void Randomize(Action<int> progressSetter)
        {
            LRFlags.Other.Enemies.SetRand();
            btScenes.Values.Where(b => b.name != "btsc05600").ForEach(b =>
            {
                List<EnemyData> oldEnemies = b.GetCharSpecs().Where(s => enemyData.Keys.Contains(s)).Select(s => enemyData[s]).ToList();
                if (oldEnemies.Count > 0)
                {
                    List<EnemyData> newEnemies = new List<EnemyData>();
                    foreach (EnemyData oldEnemy in oldEnemies)
                    {
                        EnemyData next = enemyData.Values.Where(e => e.Class == oldEnemy.Class).ToList().Shuffle().First();
                        newEnemies.Add(next);
                    }
                    List<string> charSpecs = newEnemies.Select(e => e.ID).ToList();
                    newEnemies.Where(e => e.Parts.Count > 0).ForEach(e => charSpecs.AddRange(e.Parts));
                    if (charSpecs.Count > newEnemies.Count)
                        b.u4BtChInitSetNum = newEnemies.Count;
                    else
                        b.u4BtChInitSetNum = 0;
                    b.SetCharSpecs(charSpecs);
                }
            });
            RandomNum.ClearRand();
        }

        public override void Save()
        {
            string outPath = SetupData.OutputFolder + @"\db\resident\bt_scene.wdb";
            btScenes.Save(outPath, SetupData.Paths["Nova"]);
        }

        public class EnemyData
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Class { get; set; }
            public List<string> Parts { get; set; }
            public EnemyData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Class = row[2];
                Parts = row.Skip(3).ToList();
            }
        }
    }
}

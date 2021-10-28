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
            if (LRFlags.Other.Enemies.FlagEnabled)
            {
                LRFlags.Other.Enemies.SetRand();
                btScenes.Values.Where(b => !IgnoredBtScenes.Contains(b.name)).ForEach(b =>
                {
                    List<EnemyData> oldEnemies = b.GetCharSpecs().Where(s => enemyData.Keys.Contains(s)).Select(s => enemyData[s]).ToList();
                    int count = oldEnemies.Count;
                    if (LRFlags.Other.EncounterSize.FlagEnabled && count > 0)
                    {
                        count = RandomNum.RandInt(Math.Max(1, count - 2), Math.Min(10, count + 2));
                        if (count > oldEnemies.Count)
                        {
                            for (int i = oldEnemies.Count; i < count; i++)
                            {
                                oldEnemies.Add(oldEnemies[RandomNum.RandInt(0, oldEnemies.Count - 1)]);
                            }
                        }
                        if (count < oldEnemies.Count)
                        {
                            for (int i = oldEnemies.Count; i > count; i--)
                            {
                                oldEnemies.RemoveAt(RandomNum.RandInt(0, oldEnemies.Count - 1));
                            }
                        }
                    }
                    if (count > 0)
                    {
                        List<EnemyData> newEnemies = new List<EnemyData>();
                        List<string> charSpecs = new List<string>();
                        while (charSpecs.Count == 0 || charSpecs.Count > 10)
                        {
                            newEnemies.Clear();
                            foreach (EnemyData oldEnemy in oldEnemies)
                            {
                                EnemyData next = enemyData.Values.Where(e => LRFlags.Other.EnemiesSize.FlagEnabled && e.Class != "Omega" || e.Class == oldEnemy.Class).ToList().Shuffle().First();
                                newEnemies.Add(next);
                            }
                            charSpecs = newEnemies.Select(e => e.ID).ToList();
                            newEnemies.Where(e => e.Parts.Count > 0).ForEach(e => charSpecs.AddRange(e.Parts));
                        }
                        if (charSpecs.Count > newEnemies.Count)
                            b.u4BtChInitSetNum = newEnemies.Count;
                        else
                            b.u4BtChInitSetNum = 0;
                        b.SetCharSpecs(charSpecs);
                    }
                });
                RandomNum.ClearRand();
            }
        }

        private List<string> IgnoredBtScenes
        {
            get
            {
                List<string> list = new List<string>();

                list.Add("btsc05600");
                list.Add("btsc04901");
                list.Add("btsc01960");
                list.Add("btsc08600");
                list.Add("btsc01040");

                return list;
            }
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
                Parts = row.Skip(3).Where(s => !String.IsNullOrEmpty(s)).ToList();
            }
        }
    }
}

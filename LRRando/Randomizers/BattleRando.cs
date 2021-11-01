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
        DataStoreDB3<DataStoreRCharaSet> charaSets = new DataStoreDB3<DataStoreRCharaSet>();
        Dictionary<string, EnemyData> enemyData = new Dictionary<string, EnemyData>();
        List<List<string>> bossData = new List<List<string>>();

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
            charaSets.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_charaset.wdb", false);

            enemyData = File.ReadAllLines(@"data\enemies.csv").Select(s => new EnemyData(s.Split(","))).ToDictionary(e => e.ID, e => e);
            bossData = File.ReadAllLines(@"data\bosses.csv").Select(s => s.Split(",").Where(s2 => !string.IsNullOrEmpty(s2)).ToList()).ToList();
        }
        public override void Randomize(Action<int> progressSetter)
        {
            EnemyRando enemyRando = randomizers.Get<EnemyRando>("Enemies");
            if (LRFlags.Other.Enemies.FlagEnabled)
            {
                LRFlags.Other.Enemies.SetRand();
                Dictionary<int, int> shuffledBosses = new Dictionary<int, int>();
                if (LRFlags.Other.Bosses.FlagEnabled)
                {
                    List<int> list = Enumerable.Range(0, bossData.Count).Where(i => bossData[i][0] != "m370" || LRFlags.Other.Ereshkigal.FlagEnabled).ToList();
                    List<int> shuffled = list.Shuffle().ToList();
                    shuffledBosses = Enumerable.Range(0, bossData.Count).ToDictionary(i => i, i => list.Contains(i) ? shuffled[list.IndexOf(i)] : i);
                }
                btScenes.Values.Where(b => !IgnoredBtScenes.Contains(b.name)).ForEach(b =>
                {
                    List<EnemyData> oldEnemies = b.GetCharSpecs().Where(s => enemyData.Keys.Contains(s)).Where(s =>
                            !s.EndsWith("tuto") || LRFlags.Other.Prologue.FlagEnabled).Select(s => enemyData[s]).ToList();
                    int count = oldEnemies.Count;
                    if (LRFlags.Other.EncounterSize.FlagEnabled && count > 0 && oldEnemies[0].Class != "Boss")
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
                        if (oldEnemies[0].Class != "Boss" || LRFlags.Other.Bosses.FlagEnabled && (oldEnemies[0].ID != "m370" || LRFlags.Other.Ereshkigal.FlagEnabled))
                        {
                            List<EnemyData> newEnemies = new List<EnemyData>();
                            List<string> charSpecs = new List<string>();
                            GetEnemiesList(oldEnemies, newEnemies, charSpecs, shuffledBosses);
                            if (charSpecs.Count > newEnemies.Count)
                                b.u4BtChInitSetNum = newEnemies.Count;
                            else
                                b.u4BtChInitSetNum = 0;
                            b.SetCharSpecs(charSpecs);
                            if (CharaSetMapping.ContainsKey(b.name))
                            {
                                CharaSetMapping[b.name].ForEach(m =>
                                {
                                    List<string> specs = charaSets[m].GetCharaSpecs();
                                    specs.AddRange(charSpecs.Where(s => !specs.Contains(s)).Select(s => enemyRando.enemies[s].sCharaSpec_string));
                                    charaSets[m].SetCharaSpecs(specs);
                                });
                            }
                        }
                    }
                });
                RandomNum.ClearRand();
            }
        }

        private void GetEnemiesList(List<EnemyData> oldEnemies, List<EnemyData> newEnemies, List<string> charSpecs, Dictionary<int, int> shuffledBosses)
        {
            newEnemies.Clear();
            charSpecs.Clear();
            if (oldEnemies[0].Class == "Boss")
            {
                int typeIndex = Enumerable.Range(0, bossData.Count).Where(i => bossData[i].Contains(oldEnemies[0].ID)).First();
                int tierIndex = bossData[typeIndex].IndexOf(oldEnemies[0].ID);

                int newTypeIndex = shuffledBosses[typeIndex];
                if (tierIndex == bossData[typeIndex].Count - 1)
                {
                    newEnemies.Add(enemyData[bossData[newTypeIndex][bossData[newTypeIndex].Count - 1]]);
                }
                else if (tierIndex == 0)
                {
                    newEnemies.Add(enemyData[bossData[newTypeIndex][0]]);
                }
                else
                {
                    int fromTop = bossData[typeIndex].Count - tierIndex - 1;
                    newEnemies.Add(enemyData[bossData[newTypeIndex][Math.Max(Math.Min(bossData[newTypeIndex].Count - 1, 1), bossData[newTypeIndex].Count - 1 - fromTop)]]);
                }
                charSpecs.AddRange(newEnemies.Select(e => e.ID));
            }
            else
            {
                while (charSpecs.Count == 0 || charSpecs.Count > 10)
                {
                    charSpecs.Clear();
                    newEnemies.Clear();
                    foreach (EnemyData oldEnemy in oldEnemies)
                    {
                        EnemyData next = enemyData.Values.Where(e => LRFlags.Other.EnemiesSize.FlagEnabled
                            && oldEnemy.Class != "Omega"
                            && oldEnemy.Class != "Boss"
                            && e.Class != "Omega"
                            && e.Class != "Boss"
                            || e.Class == oldEnemy.Class).Where(e =>
                            !e.ID.EndsWith("tuto") || LRFlags.Other.Prologue.FlagEnabled).ToList().Shuffle().First();
                        newEnemies.Add(next);
                    }
                    charSpecs.AddRange(newEnemies.Select(e => e.ID));
                }
            }
            newEnemies.Where(e => e.Parts.Count > 0).ForEach(e => charSpecs.AddRange(e.Parts));
        }

        private List<string> IgnoredBtScenes
        {
            get
            {
                List<string> list = new List<string>();

                //list.Add("btsc05600");
                //list.Add("btsc04901");
                //list.Add("btsc01960");
                //list.Add("btsc08600");
                //list.Add("btsc08601");
                list.Add("btsc02953");
                //list.Add("btsc01040");

                return list;
            }
        }

        private Dictionary<string, string[]> CharaSetMapping
        {
            get
            {
                Dictionary<string, string[]> dict = new Dictionary<string, string[]>();

                dict.Add("btsc05600", new string[] { "chset_yuaa_010" });
                dict.Add("btsc05610", new string[] { "chset_yuaa_020" });
                dict.Add("btsc01040", new string[] { "chset_lxaa_125" });
                dict.Add("btsc01800", new string[] { "chset_lxaa_161" });
                dict.Add("btsc01801", new string[] { "chset_lxaa_161" });
                dict.Add("btsc04901", new string[] { "chset_ddaa_060" });
                dict.Add("btsc07800", new string[] { "chset_ddaa_240" });
                dict.Add("btsc07801", new string[] { "chset_ddaa_240" });
                dict.Add("btsc02904", new string[] { "chset_yuaa_151" });
                dict.Add("btsc05900", new string[] { "chset_yuaa_370" });
                dict.Add("btsc05901", new string[] { "chset_yuaa_370" });
                dict.Add("btsc05902", new string[] { "chset_yuaa_370" });
                dict.Add("btsc06800", new string[] { "chset_wlaa_180" });
                dict.Add("btsc01960", new string[] { "chest_edlv_01" });
                dict.Add("btsc08600", new string[] { "chest_edlv_01" });
                dict.Add("btsc08601", new string[] { "chset_edaa_040", "chset_edaa_041", "chset_edaa_050" });

                dict.Add("btsc02810", new string[] { "chest_yutg_001" });
                dict.Add("btsc02811", new string[] { "chest_yutg_002" });
                dict.Add("btsc02812", new string[] { "chest_yutg_003" });
                dict.Add("btsc02813", new string[] { "chest_yutg_004" });
                dict.Add("btsc02814", new string[] { "chest_yutg_005" });
                dict.Add("btsc02815", new string[] { "chest_yutg_006" });
                dict.Add("btsc02816", new string[] { "chest_yutg_007" });
                dict.Add("btsc02817", new string[] { "chest_yutg_008" });
                dict.Add("btsc02818", new string[] { "chest_yutg_009" });
                dict.Add("btsc02819", new string[] { "chest_yutg_010" });
                dict.Add("btsc02953", new string[] { "chset_yutg_011" });

                return dict;
            }
        }

        public override void Save()
        {
            string outPath = SetupData.OutputFolder + @"\db\resident\bt_scene.wdb";
            btScenes.Save(outPath, SetupData.Paths["Nova"]);
            charaSets.SaveDB3(@"\db\resident\_wdbpack.bin\r_charaset.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_charaset.wdb");
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

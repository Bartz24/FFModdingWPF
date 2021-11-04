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
        Dictionary<string, Dictionary<int, BossData>> bossData = new Dictionary<string, Dictionary<int, BossData>>();

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
            bossData = File.ReadAllLines(@"data\bosses.csv").Select(s => new BossData(s.Split(","))).GroupBy(b => b.Group).ToDictionary(p => p.Key, p => p.ToDictionary(b => b.Tier, b => b));
        }
        public override void Randomize(Action<int> progressSetter)
        {
            EnemyRando enemyRando = randomizers.Get<EnemyRando>("Enemies");
            if (LRFlags.Other.Enemies.FlagEnabled)
            {
                LRFlags.Other.Enemies.SetRand();
                Dictionary<string, string> shuffledBosses = new Dictionary<string, string>();
                if (LRFlags.Other.Bosses.FlagEnabled)
                {
                    List<string> list = bossData.Keys.Where(k => k != "Ereshkigal" || LRFlags.Other.Ereshkigal.FlagEnabled).ToList();
                    List<string> shuffled = list.Shuffle().ToList();
                    shuffledBosses = Enumerable.Range(0, list.Count).ToDictionary(i => list[i], i => shuffled[i]);
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
                            if (b.name == "btsc02902")
                                UpdateEnemyLists(oldEnemies, newEnemies, charSpecs, shuffledBosses, enemyData.Values.Where(e => e.Parts.Count == 0 && e.Class == "Small").ToList());
                            else
                                UpdateEnemyLists(oldEnemies, newEnemies, charSpecs, shuffledBosses, enemyData.Values.ToList());

                            if (newEnemies.Count > 0)
                            {
                                if (charSpecs.Count > newEnemies.Sum(e => e.Size))
                                    b.u4BtChInitSetNum = newEnemies.Sum(e => e.Size);
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
                    }
                });
                RandomNum.ClearRand();
            }
        }

        private void UpdateEnemyLists(List<EnemyData> oldEnemies, List<EnemyData> newEnemies, List<string> charSpecs, Dictionary<string, string> shuffledBosses, List<EnemyData> allowed)
        {
            TextRando textRando = randomizers.Get<TextRando>("Text");
            newEnemies.Clear();
            charSpecs.Clear();
            if (oldEnemies[0].Class == "Boss")
            {
                BossData oldBoss = bossData.Values.SelectMany(d => d.Values).First(b => b.ID == oldEnemies[0].ID && b.ScoreID != "");
                string group = oldBoss.Group;
                if (!shuffledBosses.ContainsKey(group))
                    return;

                int tierIndex = oldBoss.Tier;
                string newGroup = shuffledBosses[group];

                BossData newBoss;
                if (tierIndex >= 3)
                {
                    newBoss = bossData[newGroup][tierIndex];
                }
                else if (tierIndex == 1)
                {
                    newBoss = bossData[newGroup][bossData[newGroup].Keys.Min()];
                }
                else
                {
                    List<int> tiers = bossData[newGroup].Keys.Where(i => i <= 3).ToList();
                    int fromTop = tiers.Max() - tiers.IndexOf(tierIndex) - 1;
                    int newTier = tiers[Math.Max(Math.Min(tiers.Count - fromTop, 1), tiers.Count - 1)];
                    newBoss = bossData[newGroup][newTier];
                }
                textRando.sysUS[oldBoss.ScoreID] = newBoss.Name;
                textRando.sysUS[newBoss.NameID] = newBoss.Name;
                newEnemies.Add(enemyData[newBoss.ID]);
                charSpecs.AddRange(newEnemies.Select(e => e.ID));
                // No Score ID means new stats
                if (newBoss.ScoreID == "")
                {
                    charSpecs.ForEach(s =>
                    {
                        Directory.GetFiles(@"data\bosses").Where(f => Path.GetFileName(f).StartsWith(s)).ForEach(f =>
                        {
                            FileExtensions.CopyFile(f, SetupData.OutputFolder + @"\db\resident\_bt_chara_spec.wdb\" + Path.GetFileName(f), true);
                        });
                    });
                }
            }
            else
            {
                while (charSpecs.Count == 0 || charSpecs.Count > 10)
                {
                    charSpecs.Clear();
                    newEnemies.Clear();
                    foreach (EnemyData oldEnemy in oldEnemies)
                    {
                        EnemyData next = allowed.Where(e => LRFlags.Other.EnemiesSize.FlagEnabled
                            && oldEnemy.Class != "Omega"
                            && oldEnemy.Class != "Boss"
                            && e.Class != "Omega"
                            && e.Class != "Boss"
                            || e.Class == oldEnemy.Class)
                            .Where(e => !e.ID.EndsWith("tuto") || LRFlags.Other.Prologue.FlagEnabled)
                            .ToList().Shuffle().First();
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
                list.Add("btsc02902");
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
            public int Size { get; set; }
            public List<string> Parts { get; set; }
            public EnemyData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Class = row[2];
                Size = int.Parse(row[3]);
                Parts = row.Skip(4).Where(s => !String.IsNullOrEmpty(s)).ToList();
            }
        }
        public class BossData
        {
            public string Group { get; set; }
            public int Tier { get; set; }
            public string ID { get; set; }
            public string NameID { get; set; }
            public string ScoreID { get; set; }
            public string Name { get; set; }
            public BossData(string[] row)
            {
                Group = row[0];
                Tier = int.Parse(row[1]);
                ID = row[2];
                NameID = row[3];
                ScoreID = row[4];
                Name = row[5];
            }
        }
    }
}

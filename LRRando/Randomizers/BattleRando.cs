using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
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
        public DataStoreDB3<DataStoreBtScene> btScenes = new DataStoreDB3<DataStoreBtScene>();
        DataStoreDB3<DataStoreRCharaSet> charaSets = new DataStoreDB3<DataStoreRCharaSet>();
        Dictionary<string, EnemyData> enemyData = new Dictionary<string, EnemyData>();
        Dictionary<string, Dictionary<int, BossData>> bossData = new Dictionary<string, Dictionary<int, BossData>>();
        Dictionary<string, Dictionary<int, BossStatsData>> bossStatsData = new Dictionary<string, Dictionary<int, BossStatsData>>();

        Dictionary<string, string> shuffledBosses = new Dictionary<string, string>();

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
            bossStatsData = File.ReadAllLines(@"data\bossesStats.csv").Select(s => new BossStatsData(s.Split(","))).GroupBy(b => b.ID).ToDictionary(p => p.Key, p => p.ToDictionary(b => b.Tier, b => b));
            shuffledBosses.Clear();
        }
        public override void Randomize(Action<int> progressSetter)
        {
            EnemyRando enemyRando = randomizers.Get<EnemyRando>("Enemies");
            if (LRFlags.Enemies.EnemyLocations.FlagEnabled)
            {
                LRFlags.Enemies.EnemyLocations.SetRand();
                if (LRFlags.Enemies.Bosses.Enabled)
                {
                    List<string> list = bossData.Keys.Where(k => (k != "Ereshkigal" || LRFlags.Enemies.Ereshkigal.Enabled) && (k != "Zaltys" || LRFlags.Enemies.Zaltys.Enabled)).ToList();
                    List<string> shuffled = list.Shuffle().ToList();
                    shuffledBosses = Enumerable.Range(0, list.Count).ToDictionary(i => list[i], i => shuffled[i]);
                }
                btScenes.Values.Where(b => !IgnoredBtScenes.Contains(b.name)).ForEach(b =>
                {
                    List<EnemyData> oldEnemies = b.GetCharSpecs().Where(s => enemyData.Keys.Contains(s)).Where(s =>
                            !s.EndsWith("tuto") || s == "m352_tuto" || s == "m390_tuto" || LRFlags.Enemies.Prologue.Enabled).Select(s => enemyData[s]).ToList();
                    int count = oldEnemies.Count;
                    if (LRFlags.Enemies.EncounterSize.Enabled && count > 0 && oldEnemies[0].Class != "Boss" && !oldEnemies[0].ID.EndsWith("tuto") && b.name != "btsc08601")
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
                        if (oldEnemies[0].Class != "Boss" || LRFlags.Enemies.Bosses.Enabled && (oldEnemies[0].ID != "m370" || LRFlags.Enemies.Ereshkigal.Enabled) && (oldEnemies[0].ID != "m352_tuto" || LRFlags.Enemies.Zaltys.Enabled))
                        {
                            List<EnemyData> newEnemies = new List<EnemyData>();
                            List<string> charSpecs = new List<string>();

                            List<EnemyData> validEnemies = enemyData.Values.ToList();
                            int variety = -1;
                            if (CharaSetMapping.ContainsKey(b.name) || oldEnemies.Count > 7)
                            {
                                // Limit forced fights to a max variety of 3 enemies with no parts to avoid memory? issues
                                validEnemies = validEnemies.Where(e => e.Parts.Count == 0).ToList();
                                variety = 3;
                            }
                            if (oldEnemies[0].ID.EndsWith("tuto") && oldEnemies[0].Class != "Boss")
                            {
                                validEnemies = validEnemies.Where(e => e.Traits.Contains("Tuto")).ToList();
                                variety = 2;
                            }

                            UpdateEnemyLists(oldEnemies, newEnemies, charSpecs, shuffledBosses, validEnemies, variety);

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
                                        specs.AddRange(charSpecs.Where(s => !specs.Contains(s)).Select(s => enemyRando.enemies[s].sCharaSpec_string).Distinct());
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

        private void UpdateEnemyLists(List<EnemyData> oldEnemies, List<EnemyData> newEnemies, List<string> charSpecs, Dictionary<string, string> shuffledBosses, List<EnemyData> allowed, int maxVariety)
        {
            TextRando textRando = randomizers.Get<TextRando>("Text");
            EnemyRando enemyRando = randomizers.Get<EnemyRando>("Enemies");
            newEnemies.Clear();
            charSpecs.Clear();
            if (oldEnemies[0].Class == "Boss")
            {
                BossData oldBoss = bossData.Values.SelectMany(d => d.Values).First(b => b.ID == oldEnemies[0].ID && b.ScoreID != "");
                string group = oldBoss.Group;
                if (!shuffledBosses.ContainsKey(group) || shuffledBosses[group] == group)
                    return;

                int oldTierIndex = oldBoss.Tier;
                string newGroup = shuffledBosses[group];

                int newTierIndex = -1;
                if (oldTierIndex >= 3 || oldTierIndex == 0)
                {
                    newTierIndex = oldTierIndex;
                }
                else if (oldTierIndex == 1)
                {
                    newTierIndex = bossData[newGroup].Keys.Where(i => i > 0).Min();
                }
                else
                {
                    List<int> tiers = bossData[newGroup].Keys.Where(i => i <= 3).ToList();
                    int fromTop = tiers.Count - tiers.IndexOf(oldTierIndex) - 1;
                    newTierIndex = tiers[Math.Min(Math.Max(tiers.Count - fromTop - 1, 1), tiers.Count - 1)];
                }
                BossData newBoss = bossData[newGroup][newTierIndex];

                textRando.mainSysUS[oldBoss.ScoreID] = newBoss.Name;
                if (!string.IsNullOrEmpty(newBoss.NameID))
                    textRando.mainSysUS[newBoss.NameID] = newBoss.Name;

                newEnemies.Add(enemyData[newBoss.ID]);
                charSpecs.AddRange(newEnemies.Select(e => e.ID));
                if (newGroup != "Caius" || group == "Caius")
                    newEnemies.Where(e => e.Parts.Count > 0).ForEach(e => charSpecs.AddRange(e.Parts));
                else if (newGroup == "Caius")
                {
                    textRando.mainSysUS["$m_740b_ac200"] = "Megaflare???";
                    textRando.mainSysUS["$m_740opb_ac200"] = "Megaflare???";
                    textRando.mainSysUS["$m_745w_ac900"] = "Megaflare???";
                }
                // No Score ID means new stats
                if (newBoss.ScoreID == "")
                {
                    charSpecs.ForEach(s =>
                    {
                        bossStatsData.Keys.Where(k => k.StartsWith(s)).ForEach(k =>
                        {
                            enemyRando.enemies[k].u24MaxHp = bossStatsData[k][newTierIndex].HP;
                            enemyRando.enemies[k].u16StatusStr = bossStatsData[k][newTierIndex].Strength;
                            enemyRando.enemies[k].u16StatusMgk = bossStatsData[k][newTierIndex].Magic;
                            enemyRando.enemies[k].u12KeepVal = bossStatsData[k][newTierIndex].Keep;
                            enemyRando.enemies[k].i10ElemDefExVal0 = bossStatsData[k][newTierIndex].PhysicalRes;
                            enemyRando.enemies[k].i10ElemDefExVal1 = bossStatsData[k][newTierIndex].MagicRes;
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
                        if (maxVariety > 0 && newEnemies.Distinct().Count() >= maxVariety)
                        {
                            newEnemies.Add(newEnemies[RandomNum.RandInt(0, newEnemies.Count - 1)]);
                        }
                        else
                        {
                            EnemyData next = allowed.Where(e => LRFlags.Enemies.EnemiesSize.Enabled
                            && oldEnemy.Class != "Omega"
                            && oldEnemy.Class != "Boss"
                            && e.Class != "Omega"
                            && e.Class != "Boss"
                            || e.Class == oldEnemy.Class)
                            .Where(e => !e.ID.EndsWith("tuto") || LRFlags.Enemies.Prologue.Enabled)
                            .ToList().Shuffle().First();
                            newEnemies.Add(next);
                        }

                    }
                    charSpecs.AddRange(newEnemies.Select(e => e.ID));
                }
            }
            newEnemies.Where(e => e.Parts.Count > 0).Distinct().ForEach(e => charSpecs.AddRange(e.Parts));
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
        public override HTMLPage GetDocumentation()
        {
            HTMLPage page = new HTMLPage("Bosses", "template/documentation.html");

            page.HTMLElements.Add(new Table("", (new string[] { "Original Boss", "New Boss", }).ToList(), (new int[] { 50, 50 }).ToList(), shuffledBosses.Select(p =>
            {
                return new string[] { p.Key, p.Value }.ToList();
            }).ToList()));

            page.HTMLElements.Add(new Table("Encounters", (new string[] { "ID (Actual Location TBD)", "New Enemies" }).ToList(), (new int[] { 60, 40 }).ToList(), btScenes.Values.Where(b => int.Parse(b.name.Substring(4)) >= 1100).Select(b =>
            {
                List<string> names = b.GetCharSpecs().Select(e => enemyData.ContainsKey(e) ? enemyData[e].Name : (e + " (???)")).GroupBy(e => e).Select(g => $"{g.Key} x {g.Count()}").ToList();
                return new string[] { b.name, string.Join(",", names) }.ToList();
            }).ToList()));

            return page;
        }

        public override void Save()
        {
            // Apply rando drops
            TransferBattleDrops();

            string outPath = SetupData.OutputFolder + @"\db\resident\bt_scene.wdb";
            btScenes.Save(outPath, SetupData.Paths["Nova"]);
            charaSets.SaveDB3(@"\db\resident\_wdbpack.bin\r_charaset.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_charaset.wdb");
        }

        private void TransferBattleDrops()
        {
            TreasureRando treasureRando = randomizers.Get<TreasureRando>("Treasures");
            treasureRando.BattleDrops.Keys.ForEach(btscName =>
            {
                btScenes[btscName].sDropItem0_string = treasureRando.BattleDrops[btscName];
                btScenes[btscName].u16DropProb0 = 10000;
                btScenes[btscName].u8NumDrop0 = 1;

            });
        }

        public class EnemyData
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public List<string> Traits { get; set; }
            public string Class { get; set; }
            public int Size { get; set; }
            public List<string> Parts { get; set; }
            public EnemyData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Traits = row[2].Split("|").ToList();
                Class = row[3];
                Size = int.Parse(row[4]);
                Parts = row.Skip(5).Where(s => !String.IsNullOrEmpty(s)).ToList();
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
        public class BossStatsData
        {
            public string ID { get; set; }
            public int Tier { get; set; }
            public int HP { get; set; }
            public int Strength { get; set; }
            public int Magic { get; set; }
            public int Keep { get; set; }
            public int PhysicalRes { get; set; }
            public int MagicRes { get; set; }
            public BossStatsData(string[] row)
            {
                ID = row[0];
                Tier = int.Parse(row[1]);
                HP = int.Parse(row[2]);
                Strength = int.Parse(row[3]);
                Magic = int.Parse(row[4]);
                Keep = int.Parse(row[5]);
                PhysicalRes = int.Parse(row[6]);
                MagicRes = int.Parse(row[7]);
            }
        }
    }
}

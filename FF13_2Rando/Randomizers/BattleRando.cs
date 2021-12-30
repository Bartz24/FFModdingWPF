using Bartz24.Data;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando
{
    public class BattleRando : Randomizer
    {
        public DataStoreDB3<DataStoreBtScene> btScenes = new DataStoreDB3<DataStoreBtScene>();
        DataStoreDB3<DataStoreRCharaSet> charaSets = new DataStoreDB3<DataStoreRCharaSet>();
        Dictionary<string, EnemyData> enemyData = new Dictionary<string, EnemyData>();

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
            btScenes.LoadDB3("13-2", @"\db\resident\bt_scene.wdb");
            enemyData = File.ReadAllLines(@"data\enemies.csv").Select(s => new EnemyData(s.Split(","))).ToDictionary(e => e.ID, e => e);

            charaSets.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_charaset.wdb", false);

        }
        public override void Randomize(Action<int> progressSetter)
        {
            EnemyRando enemyRando = randomizers.Get<EnemyRando>("Enemies");
            if (FF13_2Flags.Enemies.EnemyLocations.FlagEnabled)
            {
                FF13_2Flags.Enemies.EnemyLocations.SetRand();
                btScenes.Values.Where(b => !IgnoredBtScenes.Contains(b.name)).ForEach(b =>
                {
                    List<EnemyData> oldEnemies = b.GetCharSpecs().Where(s => enemyData.Keys.Contains(s)).Select(s => enemyData[s]).ToList();
                    int count = oldEnemies.Count;
                    if (count > 0)
                    {
                        if (!oldEnemies[0].Traits.Contains("Boss"))
                        {
                            List<EnemyData> newEnemies = new List<EnemyData>();
                            List<string> charSpecs = new List<string>();

                            List<EnemyData> validEnemies = enemyData.Values.ToList();
                            int variety = -1;
                            if (CharaSetMapping.ContainsKey(b.name))
                            {
                                // Limit forced fights to a max variety of 3 enemies with no parts to avoid memory? issues
                                validEnemies = validEnemies.Where(e => e.Parts.Count == 0).ToList();
                                variety = 3;
                                CharaSetMapping[b.name].ForEach(m =>
                                {
                                    List<string> specs = charaSets[m].GetCharaSpecs();
                                    if (specs.Count == 58)
                                        validEnemies = validEnemies.Where(e => specs.Contains(enemyRando.HasEnemy(e.ID) ? enemyRando.GetEnemy(e.ID).sCharaSpec_string : e.ID)).ToList();
                                    if (specs.Count == 57)
                                        variety = 1;
                                    if (specs.Count == 56)
                                        variety = 2;
                                });
                            }

                            UpdateEnemyLists(oldEnemies, newEnemies, charSpecs, validEnemies, variety);

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
                                        specs.AddRange(charSpecs.Where(s => !specs.Contains(s)).Select(s => enemyRando.HasEnemy(s) ? enemyRando.GetEnemy(s).sCharaSpec_string : s));
                                        specs = specs.Distinct().ToList();
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

        private void UpdateEnemyLists(List<EnemyData> oldEnemies, List<EnemyData> newEnemies, List<string> charSpecs, List<EnemyData> allowed, int maxVariety)
        {
            EnemyRando enemyRando = randomizers.Get<EnemyRando>("Enemies");
            newEnemies.Clear();
            charSpecs.Clear();
            {
                int attempts = 0;
                while (charSpecs.Count == 0 || charSpecs.Count > 10)
                {
                    charSpecs.Clear();
                    newEnemies.Clear();
                    foreach (EnemyData oldEnemy in oldEnemies)
                    {
                        if (maxVariety > 0 && newEnemies.Distinct().Count() >= maxVariety)
                        {
                            if (attempts > 20)
                            {
                                newEnemies.Add(newEnemies.ToList().Shuffle().First());
                            }
                            else if (newEnemies.Where(e => e.Rank >= oldEnemy.Rank - 2 && e.Rank <= oldEnemy.Rank + 2 && !e.Traits.Contains("Boss")).Count() > 0)
                            {
                                newEnemies.Add(newEnemies.Where(e => e.Rank >= oldEnemy.Rank - 2 && e.Rank <= oldEnemy.Rank + 2 && !e.Traits.Contains("Boss"))
                            .ToList().Shuffle().First());
                            }
                            else
                            {
                                newEnemies.Clear();
                                attempts++;
                                break;
                            }
                        }
                        else
                        {
                            EnemyData next = allowed.Where(e => e.Rank >= oldEnemy.Rank - 2 && e.Rank <= oldEnemy.Rank + 2 && !e.Traits.Contains("Boss"))
                            .ToList().Shuffle().First();
                            newEnemies.Add(next);
                        }

                    }
                    charSpecs.AddRange(newEnemies.Select(e => e.ID));
                }
                newEnemies.Where(e => e.Parts.Count > 0).ForEach(e => charSpecs.AddRange(e.Parts));
            }
        }

        private List<string> IgnoredBtScenes
        {
            get
            {
                List<string> list = new List<string>();

                return list;
            }
        }

        private Dictionary<string, string[]> CharaSetMapping
        {
            get
            {
                Dictionary<string, string[]> dict = new Dictionary<string, string[]>();

                dict.Add("btsc01110", new string[] { "chset_hmaa_001" });
                dict.Add("btsc01120", new string[] { "chset_hmaa_001" });
                dict.Add("btsc01130", new string[] { "chset_hmaa_001" });
                dict.Add("btsc01140", new string[] { "chset_hmaa_001" });

                dict.Add("btsc02050", new string[] { "chset_bjaa_tuto" });
                dict.Add("btsc02001", new string[] { "chset_bjaa_tuto" });

                dict.Add("btsc05950", new string[] { "chset_snda_002", "chset_snda_e050", "chset_snda_e140" });
                dict.Add("btsc05960", new string[] { "chset_snda_002", "chset_snda_e050", "chset_snda_e140" });

                dict.Add("btsc05800", new string[] { "chset_snea_def", "chset_snea_load" });

                dict.Add("btsc08000", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08001", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08002", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08003", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08010", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08011", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08012", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08013", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08014", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08015", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08020", new string[] { "chset_acea_001" });
                dict.Add("btsc08021", new string[] { "chset_acea_001" });
                dict.Add("btsc08022", new string[] { "chset_acea_001" });
                dict.Add("btsc08023", new string[] { "chset_acea_001" });
                dict.Add("btsc08030", new string[] { "chset_acea_001" });
                dict.Add("btsc08031", new string[] { "chset_acea_001" });
                dict.Add("btsc08032", new string[] { "chset_acea_001" });
                dict.Add("btsc08033", new string[] { "chset_acea_001" });
                dict.Add("btsc08040", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08041", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08042", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08043", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08050", new string[] { "chset_acea_002" });
                dict.Add("btsc08510", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08511", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08512", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08513", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08520", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08521", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08522", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08700", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08710", new string[] { "chset_acea_e060" });
                dict.Add("btsc08720", new string[] { "chset_acea_e060" });

                dict.Add("btsc09830", new string[] { "chset_gtca_e058" });
                dict.Add("btsc09900", new string[] { "chset_gtca_e058" });
                dict.Add("btsc09910", new string[] { "chset_gtca_e058" });
                dict.Add("btsc09920", new string[] { "chset_gtca_e058" });

                return dict;
            }
        }

        public override void Save()
        {
            btScenes.SaveDB3(@"\db\resident\bt_scene.wdb");

            charaSets.SaveDB3(@"\db\resident\_wdbpack.bin\r_charaset.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_charaset.wdb");
        }
        public class EnemyData
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public List<string> Traits { get; set; }
            public int Rank { get; set; }
            public int Size { get; set; }
            public List<string> Parts { get; set; }
            public EnemyData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Traits = row[2].Split("|").ToList();
                Rank = int.Parse(row[3]);
                Size = int.Parse(row[4]);
                Parts = row.Skip(5).Where(s => !String.IsNullOrEmpty(s)).ToList();
            }
        }
    }
}

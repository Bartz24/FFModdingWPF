using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
        Dictionary<string, Dictionary<string, BossData>> bossData = new Dictionary<string, Dictionary<string, BossData>>();

        Dictionary<string, Dictionary<string, string>> charaSetEnemyMappings = new Dictionary<string, Dictionary<string, string>>();

        Dictionary<string, string> shuffledBosses = new Dictionary<string, string>();

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

            bossData.Clear();
            using (CsvParser csv = new CsvParser(new StreamReader(@"data\bosses.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
            {
                while (csv.Read())
                {
                    if (csv.Row > 1)
                    {
                        BossData b = new BossData(csv.Record);
                        if (!bossData.ContainsKey(b.Group))
                            bossData.Add(b.Group, new Dictionary<string, BossData>());

                        bossData[b.Group].Add(b.ID, b);
                    }
                }
            }

        }
        public override void Randomize(Action<int> progressSetter)
        {
            EnemyRando enemyRando = Randomizers.Get<EnemyRando>("Enemies");
            charaSetEnemyMappings = CharaSetMapping.Values.SelectMany(a => a).Distinct().ToDictionary(s => s, _ => new Dictionary<string, string>());
            if (FF13_2Flags.Enemies.EnemyLocations.FlagEnabled)
            {
                FF13_2Flags.Enemies.EnemyLocations.SetRand();
                if (FF13_2Flags.Enemies.Bosses.Enabled)
                {
                    List<string> list = bossData.Keys.Where(g => !bossData[g].Values.First(b => b.Traits.Contains("Main")).Traits.Contains("NoShuffle")).ToList();
                    // DLC fights require entry
                    if (!FF13_2Flags.Enemies.DLCBosses.Enabled)
                        list = list.Where(g => !bossData[g].Values.First(b => b.Traits.Contains("Main")).Traits.Contains("ForceEntry")).ToList();
                    List<string> shuffled = list.Shuffle().ToList();
                    shuffledBosses = Enumerable.Range(0, list.Count).ToDictionary(i => list[i], i => shuffled[i]);
                }
                btScenes.Values.Where(b => !IgnoredBtScenes.Contains(b.name)).ForEach(b =>
                {
                    List<EnemyData> oldEnemies = b.GetCharSpecs().Where(s => enemyData.Keys.Contains(s)).Select(s => enemyData[s]).ToList();
                    int count = oldEnemies.Count;
                    if (!FF13_2Flags.Enemies.LargeEnc.Enabled)
                        count = Math.Min(4, count);
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
                    if (count > 0)
                    {
                        if (!oldEnemies[0].Traits.Contains("Boss") || FF13_2Flags.Enemies.Bosses.Enabled)
                        {
                            List<EnemyData> newEnemies = new List<EnemyData>();
                            List<string> charSpecs = new List<string>();

                            List<EnemyData> validEnemies = enemyData.Values.ToList();
                            if (CharaSetMapping.ContainsKey(b.name))
                            {
                                validEnemies = validEnemies.Where(e => e.Parts.Count() == 0).ToList();
                            }

                            UpdateEnemyLists(oldEnemies, newEnemies, charSpecs, validEnemies, b.name, b.name.StartsWith("btsc011"));

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

        private void UpdateBossStats(BossData newBoss, BossData origBoss)
        {
            EnemyRando enemyRando = Randomizers.Get<EnemyRando>("Enemies");
            DataStoreBtCharaSpec newEnemy = enemyRando.GetEnemy(newBoss.ID);
            DataStoreBtCharaSpec origEnemy = enemyRando.GetEnemy(origBoss.ID, true);

            if (newBoss.HPMult != -1)
                newEnemy.u24MaxHp = (int)(origEnemy.u24MaxHp / origBoss.HPMult * newBoss.HPMult);

            if (newBoss.STRMult != -1)
                newEnemy.u16StatusStr = (int)(origEnemy.u16StatusStr / origBoss.STRMult * newBoss.STRMult);

            if (newBoss.MAGMult != -1)
                newEnemy.u16StatusMgk = (int)(origEnemy.u16StatusMgk / origBoss.MAGMult * newBoss.MAGMult);

            if (newBoss.HPMult != -1)
                newEnemy.u24MaxHp = (int)(origEnemy.u24MaxHp / origBoss.HPMult * newBoss.HPMult);

            if (newBoss.StaggerPointMult != -1 && newEnemy.u12BrChainBonus != 1000 && origEnemy.u12BrChainBonus != 1000)
                newEnemy.u12BrChainBonus = Math.Min(999, (int)((origEnemy.u12BrChainBonus - 100) / origBoss.StaggerPointMult * newBoss.StaggerPointMult) + 100);

            if (newBoss.ChainResMult != -1)
                newEnemy.u12MaxBp = Math.Min(100, (int)(origEnemy.u12MaxBp / origBoss.ChainResMult * newBoss.ChainResMult));

            if (newBoss.CPGilMult != -1)
            {
                newEnemy.u24AbilityPoint = (int)(origEnemy.u24AbilityPoint / origBoss.CPGilMult * newBoss.CPGilMult);
                newEnemy.u16DropGil = (int)(origEnemy.u16DropGil / origBoss.CPGilMult * newBoss.CPGilMult);
            }

            newEnemy.u12KeepVal = origEnemy.u12KeepVal;
            newEnemy.s10DropItem0_string = origEnemy.s10DropItem0_string;
            newEnemy.s10DropItem1_string = origEnemy.s10DropItem1_string;
            newEnemy.s10DropItem2_string = origEnemy.s10DropItem2_string;
            newEnemy.u8NumDrop0 = origEnemy.u8NumDrop0;
            newEnemy.u8NumDrop1 = origEnemy.u8NumDrop1;
            newEnemy.u8NumDrop2 = origEnemy.u8NumDrop2;
            newEnemy.u14DropProb0 = origEnemy.u14DropProb0;
            newEnemy.u14DropProb1 = origEnemy.u14DropProb1;
            newEnemy.u14DropProb2 = origEnemy.u14DropProb2;
        }

        private void UpdateEnemyLists(List<EnemyData> oldEnemies, List<EnemyData> newEnemies, List<string> charSpecs, List<EnemyData> allowed, string btsceneName, bool sameRank)
        {
            int maxVariety = 3;
            int attempts = 0;

            EnemyRando enemyRando = Randomizers.Get<EnemyRando>("Enemies");
            newEnemies.Clear();
            charSpecs.Clear();
            if (oldEnemies[0].Traits.Contains("Boss"))
            {
                bool noEntry = true;
                foreach (EnemyData e in oldEnemies)
                {
                    if (bossData.Values.SelectMany(d => d.Values).Where(b => b.ID == e.ID).Count() == 0)
                        continue;
                    BossData oldBoss = bossData.Values.SelectMany(d => d.Values).First(b => b.ID == e.ID && b.Traits.Contains("Main"));
                    if (!shuffledBosses.ContainsKey(oldBoss.Group))
                        return;
                    string newGroup = shuffledBosses[oldBoss.Group];
                    if (oldBoss.Group != newGroup)
                    {
                        newEnemies.Add(enemyData[bossData[newGroup].Values.First(b => b.Traits.Contains("Main")).ID]);
                        bossData[newGroup].Values.ForEach(b => UpdateBossStats(b, oldBoss));
                        if (!bossData[newGroup].Values.First(b => b.Traits.Contains("Main")).Traits.Contains("NoEntry"))
                            noEntry = false;
                        if (bossData[oldBoss.Group].Values.First(b => b.Traits.Contains("Main")).Traits.Contains("ForceEntry"))
                            noEntry = false;
                    }
                }
                if (newEnemies.Count > 0)
                {
                    if (noEntry)
                    {
                        btScenes[btsceneName].s10BtChEntryId_string = "";
                        btScenes[btsceneName].s10PartyEntryId_string = "";
                    }
                    else
                    {
                        btScenes[btsceneName].s10BtChEntryId_string = "btsc_def_e00";
                        btScenes[btsceneName].s10PartyEntryId_string = "btsc_def_p00";
                    }
                    charSpecs.AddRange(newEnemies.Select(e => e.ID));
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
                        Func<EnemyData, bool> pred = e => e.Rank >= oldEnemy.Rank - 2 && e.Rank <= oldEnemy.Rank + 2 && !e.Traits.Contains("Boss");
                        Func<EnemyData, bool> predSame = e => e.Rank == oldEnemy.Rank && !e.Traits.Contains("Boss");

                        if (CharaSetMapping.ContainsKey(btsceneName))
                        {
                            string newMapping = CharaSetMapping[btsceneName].Where(c => charaSetEnemyMappings[c].ContainsKey(oldEnemy.ID)).Select(c => charaSetEnemyMappings[c][oldEnemy.ID]).FirstOrDefault();
                            if (newMapping == null)
                            {
                                newMapping = allowed.Where(sameRank ? predSame : pred)
                                .ToList().Shuffle().First().ID;
                            }

                            newEnemies.Add(enemyData[newMapping]);
                            foreach (string charaset in CharaSetMapping[btsceneName])
                            {
                                if (!charaSetEnemyMappings[charaset].ContainsKey(oldEnemy.ID))
                                    charaSetEnemyMappings[charaset].Add(oldEnemy.ID, newMapping);
                            }
                        }
                        else if (maxVariety > 0 && newEnemies.Distinct().Count() >= maxVariety)
                        {
                            if (attempts > 20)
                            {
                                newEnemies.Add(newEnemies.ToList().Shuffle().First());
                            }
                            else if (newEnemies.Where(sameRank ? predSame : pred).Count() > 0)
                            {
                                newEnemies.Add(newEnemies.Where(sameRank ? predSame : pred)
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
                            EnemyData next = allowed.Where(sameRank ? predSame : pred)
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
                list.Add("btsc08000");
                list.Add("btsc08001");
                list.Add("btsc08002");
                list.Add("btsc08003");
                list.Add("btsc08010");
                list.Add("btsc08011");
                list.Add("btsc08012");
                list.Add("btsc08013");
                list.Add("btsc08014");
                list.Add("btsc08020");
                list.Add("btsc08021");
                list.Add("btsc08022");
                list.Add("btsc08023");
                list.Add("btsc08510");
                list.Add("btsc08511");
                list.Add("btsc08512");
                list.Add("btsc08513");
                list.Add("btsc08520");
                list.Add("btsc08521");
                list.Add("btsc08522");
                list.Add("btsc08700");

                return list;
            }
        }

        private Dictionary<string, string[]> CharaSetMapping
        {
            get
            {
                Dictionary<string, string[]> dict = new Dictionary<string, string[]>();

                dict.Add("btsc01110", new string[] { "chset_hmaa_001", "chset_hmaa_e025" });
                dict.Add("btsc01120", new string[] { "chset_hmaa_001" });
                dict.Add("btsc01130", new string[] { "chset_hmaa_001" });
                dict.Add("btsc01140", new string[] { "chset_hmaa_001" });

                dict.Add("btsc01900", new string[] { "chset_hmaa_e050" });
                dict.Add("btsc01910", new string[] { "chset_hmaa_e130" });

                dict.Add("btsc02050", new string[] { "chset_bjaa_tuto" });
                dict.Add("btsc02001", new string[] { "chset_bjaa_tuto" });

                dict.Add("btsc02810", new string[] { "chset_bjba_def" });

                dict.Add("btsc03900", new string[] { "chset_gyaa_020" });

                dict.Add("btsc04900", new string[] { "chset_gwca_def" });
                dict.Add("btsc04909", new string[] { "chset_gwca_def" });

                dict.Add("btsc05920", new string[] { "chset_snda_002" });
                dict.Add("btsc05950", new string[] { "chset_snda_npc" });
                dict.Add("btsc05960", new string[] { "chset_snda_npc" });

                dict.Add("btsc05800", new string[] { "chset_snea_def", "chset_snea_load" });

                dict.Add("btsc06100", new string[] { "chset_clzb_lig" });
                dict.Add("btsc06110", new string[] { "chset_clzb_lig" });

                dict.Add("btsc06500", new string[] { "chset_clzb_gil" });
                dict.Add("btsc06510", new string[] { "chset_clzb_gil" });

                dict.Add("btsc06600", new string[] { "chset_clzb_snw" });

                dict.Add("btsc07800", new string[] { "chset_gdza_p2h" });
                dict.Add("btsc07810", new string[] { "chset_gdza_p2s" });
                dict.Add("btsc07820", new string[] { "chset_gdza_p3r" });
                dict.Add("btsc07830", new string[] { "chset_gdza_p3t" });

                dict.Add("btsc08000", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08001", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08002", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08003", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08510", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08511", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08512", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08020", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08021", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08022", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08023", new string[] { "chset_acea_001" });
                dict.Add("btsc08032", new string[] { "chset_acea_001" });
                dict.Add("btsc08040", new string[] { "chset_acea_001", "chset_acea_002" });

                dict.Add("btsc08513", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08520", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08521", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08522", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08014", new string[] { "chset_acea_001", "chset_acea_002" });
                dict.Add("btsc08050", new string[] { "chset_acea_002" });

                dict.Add("btsc08700", new string[] { "chset_acea_e030" });
                dict.Add("btsc08710", new string[] { "chset_acea_e060" });
                dict.Add("btsc08720", new string[] { "chset_acea_e060" });

                dict.Add("btsc09830", new string[] { "chset_gtca_def" });
                dict.Add("btsc09900", new string[] { "chset_gtca_def" });
                dict.Add("btsc09910", new string[] { "chset_gtca_def" });
                dict.Add("btsc09920", new string[] { "chset_gtca_def" });

                dict.Add("btsc11900", new string[] { "chset_spza_def" });
                dict.Add("btsc11909", new string[] { "chset_spza_def" });

                dict.Add("btsc11910", new string[] { "chset_acfa_e012" });

                dict.Add("btsc13900", new string[] { "chset_ddha_e030" });
                dict.Add("btsc13909", new string[] { "chset_ddha_e030" });
                dict.Add("btsc13950", new string[] { "chset_ddha_e040" });

                dict.Add("btsc15900", new string[] { "chset_lsza_d01a" });
                dict.Add("btsc15910", new string[] { "chset_lsza_d01a" });
                dict.Add("btsc15950", new string[] { "chset_lsza_d01a" });
                dict.Add("btsc15990", new string[] { "chset_lsza_d03b" });

                return dict;
            }
        }

        public override HTMLPage GetDocumentation()
        {
            HTMLPage page = new HTMLPage("Encounters", "template/documentation.html");

            page.HTMLElements.Add(new Table("Bosses", (new string[] { "Original Boss", "New Boss", }).ToList(), (new int[] { 50, 50 }).ToList(), shuffledBosses.Select(p =>
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
        public class BossData
        {
            public string Group { get; set; }
            public int Rank { get; set; }
            public string ID { get; set; }
            public float HPMult { get; set; }
            public float STRMult { get; set; }
            public float MAGMult { get; set; }
            public float StaggerPointMult { get; set; }
            public float ChainResMult { get; set; }
            public float CPGilMult { get; set; }
            public List<string> Traits { get; set; }
            public BossData(string[] row)
            {
                Group = row[0];
                Rank = int.Parse(row[1]);
                ID = row[2];
                HPMult = int.Parse(row[3]) == -1 ? -1 : int.Parse(row[3]) / 100f;
                STRMult = int.Parse(row[4]) == -1 ? -1 : int.Parse(row[4]) / 100f;
                MAGMult = int.Parse(row[5]) == -1 ? -1 : int.Parse(row[5]) / 100f;
                StaggerPointMult = int.Parse(row[6]) == -1 ? -1 : int.Parse(row[6]) / 100f;
                ChainResMult = int.Parse(row[7]) == -1 ? -1 : int.Parse(row[7]) / 100f;
                CPGilMult = int.Parse(row[8]) == -1 ? -1 : int.Parse(row[8]) / 100f;
                Traits = row[9].Split("|").ToList();
            }
        }
    }
}

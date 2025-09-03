using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
using LRRando;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace LRRando;

public partial class BattleRando : Randomizer
{
    public DataStoreDB3<DataStoreBtScene> btScenes = new();
    public DataStoreDB3<DataStoreBtScene> btScenesOrig = new();
    private readonly DataStoreDB3<DataStoreRCharaSet> charaSets = new();
    private readonly Dictionary<string, EnemyData> enemyData = new();
    private readonly Dictionary<string, Dictionary<int, BossData>> bossData = new();
    private readonly Dictionary<string, Dictionary<int, BossStatsData>> bossStatsData = new();
    private Dictionary<string, string> shuffledBosses = new();

    public BattleRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Battle Data...");
        btScenesOrig.LoadDB3(Generator, "LR", @"\db\resident\bt_scene.wdb");
        RandoUI.SetUIProgressDeterminate("Loading Battle Data...", 20, 100);
        btScenes.LoadDB3(Generator, "LR", @"\db\resident\bt_scene.wdb");
        RandoUI.SetUIProgressDeterminate("Loading Battle Data...", 50, 100);
        charaSets.LoadDB3(Generator, "LR", @"\db\resident\_wdbpack.bin\r_charaset.wdb", false);

        RandoUI.SetUIProgressDeterminate("Loading Battle Data...", 80, 100);
        FileHelpers.ReadCSVFile(@"data\enemies.csv", row =>
        {
            EnemyData e = new(row);
            enemyData.Add(e.ID, e);
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\bosses.csv", row =>
        {
            BossData b = new(row);
            if (!bossData.ContainsKey(b.Group))
            {
                bossData.Add(b.Group, new Dictionary<int, BossData>());
            }

            bossData[b.Group].Add(b.Tier, b);
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\bossesStats.csv", row =>
        {
            BossStatsData b = new(row);
            if (!bossStatsData.ContainsKey(b.ID))
            {
                bossStatsData.Add(b.ID, new Dictionary<int, BossStatsData>());
            }

            bossStatsData[b.ID].Add(b.Tier, b);
        }, FileHelpers.CSVFileHeader.HasHeader);

        shuffledBosses.Clear();
    }
    public override void Randomize()
    {
        EnemyRando enemyRando = Generator.Get<EnemyRando>();
        RandoUI.SetUIProgressIndeterminate("Randomizing Battle Data...");
        if (LRFlags.Enemies.EnemyLocations.FlagEnabled)
        {
            LRFlags.Enemies.EnemyLocations.SetRand();
            if (LRFlags.Enemies.Bosses.SelectedKeys.Count > 0)
            {
                List<string> list = bossData.Keys.Where(k => LRFlags.Enemies.Bosses.SelectedKeys.Contains(k)).ToList();
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
                    if (oldEnemies[0].Class != "Boss" || LRFlags.Enemies.Bosses.SelectedKeys.SelectMany(name => bossData[name].Values.Select(b => b.ID)).Contains(oldEnemies[0].ID))
                    {
                        List<EnemyData> newEnemies = new();
                        List<string> charSpecs = new();

                        List<EnemyData> validEnemies = enemyData.Values.ToList();
                        int variety = -1;
                        if (CharaSetMapping.ContainsKey(b.name) || count > 7)
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

                        UpdateEnemyLists(b, oldEnemies, newEnemies, charSpecs, shuffledBosses, validEnemies, variety);

                        if (newEnemies.Count > 0)
                        {
                            b.u4BtChInitSetNum = charSpecs.Count > newEnemies.Sum(e => e.Size) ? newEnemies.Sum(e => e.Size) : 0;
                            b.SetCharSpecs(charSpecs);
                            // Update normal encounter entry IDs to ones supporting 3+ enemies
                            if (b.s8BtChEntryId_string is "btsc_def_e00" or "btsc_def_m03")
                            {
                                b.s8BtChEntryId_string = "btsc_def_e00";
                            }

                            if (CharaSetMapping.ContainsKey(b.name))
                            {
                                CharaSetMapping[b.name].ForEach(m =>
                                {
                                    List<string> specs = charaSets[m].CharaSpecs;
                                    specs.AddRange(charSpecs.Where(s => !specs.Contains(s)).Select(s => enemyRando.enemies[s].sCharaSpec_string).Distinct());
                                    charaSets[m].CharaSpecs = specs;
                                });
                            }
                        }
                    }
                }
            });
            RandomNum.ClearRand();
        }
    }

    private void UpdateEnemyLists(DataStoreBtScene btScene, List<EnemyData> oldEnemies, List<EnemyData> newEnemies, List<string> charSpecs, Dictionary<string, string> shuffledBosses, List<EnemyData> allowed, int maxVariety)
    {
        TextRando textRando = Generator.Get<TextRando>();
        EnemyRando enemyRando = Generator.Get<EnemyRando>();
        newEnemies.Clear();
        charSpecs.Clear();
        if (oldEnemies[0].Class == "Boss")
        {
            BossData oldBoss = bossData.Values.SelectMany(d => d.Values).First(b => b.ID == oldEnemies[0].ID && b.ScoreID != "");
            string group = oldBoss.Group;
            if (!shuffledBosses.ContainsKey(group) || shuffledBosses[group] == group)
            {
                return;
            }

            int oldTierIndex = oldBoss.Tier;
            string newGroup = shuffledBosses[group];

            int newTierIndex = -1;
            if (oldTierIndex is >= 3 or 0)
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
            {
                textRando.mainSysUS[newBoss.NameID] = newBoss.Name;
            }

            newEnemies.Add(enemyData[newBoss.ID]);
            charSpecs.AddRange(newEnemies.Select(e => e.ID));
            if (newGroup == "Caius" && group != "Caius")
            {
                btScene.s8PartyEntryId_string = "btsc_caius";
                btScene.s8BtChEntryId_string = "btsc_caius";
                btScene.u1EvenNoShift = 1;
            }

            if (newGroup == "Aeronite" && group != "Aeronite")
            {
                btScene.s8PartyEntryId_string = "btsc00375";
                btScene.s8BtChEntryId_string = "btsc00375";
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
                        enemyRando.enemies[k].u12BrkLimitVal = bossStatsData[k][newTierIndex].BreakPoint;
                    });
                });
            }
        }
        else
        {
            while (charSpecs.Count is 0 or > 10)
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
                        EnemyData next = allowed.Where(e => (LRFlags.Enemies.EnemiesSize.Enabled
                        && oldEnemy.Class != "Omega"
                        && oldEnemy.Class != "Boss"
                        && e.Class != "Omega"
                        && e.Class != "Boss")
                        || e.Class == oldEnemy.Class)
                        .Where(e => !e.ID.EndsWith("tuto") || LRFlags.Enemies.Prologue.Enabled)
                        .Shuffle().First();
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
            List<string> list = new()
            {
                //list.Add("btsc05600");
                //list.Add("btsc04901");
                //list.Add("btsc01960");
                //list.Add("btsc08600");
                "btsc08601",
                "btsc02953",
                "btsc02902"
            };
            //list.Add("btsc01040");

            return list;
        }
    }

    private Dictionary<string, string[]> CharaSetMapping
    {
        get
        {
            Dictionary<string, string[]> dict = new()
            {
                { "btsc05600", new string[] { "chset_yuaa_010" } },
                { "btsc05610", new string[] { "chset_yuaa_020" } },
                { "btsc01040", new string[] { "chset_lxaa_125" } },
                { "btsc01800", new string[] { "chset_lxaa_161" } },
                { "btsc01801", new string[] { "chset_lxaa_161" } },
                { "btsc04901", new string[] { "chset_ddaa_060" } },
                { "btsc07800", new string[] { "chset_ddaa_240" } },
                { "btsc07801", new string[] { "chset_ddaa_240" } },
                { "btsc02904", new string[] { "chset_yuaa_151" } },
                { "btsc05900", new string[] { "chset_yuaa_370" } },
                { "btsc05901", new string[] { "chset_yuaa_370" } },
                { "btsc05902", new string[] { "chset_yuaa_370" } },
                { "btsc06800", new string[] { "chset_wlaa_180" } },
                { "btsc01960", new string[] { "chest_edlv_01" } },
                { "btsc08600", new string[] { "chest_edlv_01" } },
                { "btsc08601", new string[] { "chset_edaa_040", "chset_edaa_041", "chset_edaa_050" } },

                { "btsc02810", new string[] { "chest_yutg_001" } },
                { "btsc02811", new string[] { "chest_yutg_002" } },
                { "btsc02812", new string[] { "chest_yutg_003" } },
                { "btsc02813", new string[] { "chest_yutg_004" } },
                { "btsc02814", new string[] { "chest_yutg_005" } },
                { "btsc02815", new string[] { "chest_yutg_006" } },
                { "btsc02816", new string[] { "chest_yutg_007" } },
                { "btsc02817", new string[] { "chest_yutg_008" } },
                { "btsc02818", new string[] { "chest_yutg_009" } },
                { "btsc02819", new string[] { "chest_yutg_010" } },
                { "btsc02953", new string[] { "chset_yutg_011" } }
            };

            return dict;
        }
    }
    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        HTMLPage page = new("Bosses & Encounters", "template/documentation.html");

        page.HTMLElements.Add(new Table("Bosses", (new string[] { "Original Boss", "New Boss" }).ToList(), (new int[] { 50, 50 }).ToList(), shuffledBosses.Select(p =>
        {
            return new string[] { p.Key, p.Value }.ToList();
        }).ToList()));

        page.HTMLElements.Add(new Table("Encounters", (new string[] { "ID (Actual Location TBD)", "New Enemies" }).ToList(), (new int[] { 60, 40 }).ToList(), btScenes.Values.Where(b => int.Parse(b.name.Substring(4)) >= 1000).Select(b =>
        {
            List<string> names = b.GetCharSpecs().Take(b.u4BtChInitSetNum > 0 ? b.u4BtChInitSetNum : int.MaxValue).Select(e => enemyData.ContainsKey(e) ? enemyData[e].Name : e + " (???)").GroupBy(e => e).Select(g => $"{g.Key} x {g.Count()}").ToList();
            return new string[] { b.name, string.Join(",", names) }.ToList();
        }).ToList()));
        pages.Add("encounters", page);
        return pages;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Battle Data...");
        // Apply rando drops
        TransferBattleDrops();

        string outPath = Generator.DataOutFolder + @"\db\resident\bt_scene.wdb";
        btScenes.Save(outPath, SetupData.Paths["Nova"]);
        charaSets.SaveDB3(Generator, @"\db\resident\_wdbpack.bin\r_charaset.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_charaset.wdb");
    }

    private void TransferBattleDrops()
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        treasureRando.BattleDrops.Keys.ForEach(btscName =>
        {
            btScenes[btscName].sDropItem0_string = treasureRando.BattleDrops[btscName];
            btScenes[btscName].u16DropProb0 = 10000;
            btScenes[btscName].u8NumDrop0 = 1;

        });
    }

    public Dictionary<string, string> RandomisedBosses()
    {
        return new Dictionary<string, string>(shuffledBosses);
    }
}

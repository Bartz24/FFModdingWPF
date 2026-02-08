using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using FF12Rando;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando;

public enum EnemyStatType
{
    HP,
    MP,
    STR,
    MAG,
    VIT,
    SPD,
    EVA,
    DEF,
    MRES,
    ATK,
    LP,
    EXP,
    SIZE
}

public partial class EnemyRando : Randomizer
{
    private readonly Dictionary<string, DataStoreARD> ards = new();
    private readonly Dictionary<string, EnemyData> enemyData = new();

    public EnemyRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Enemy Data...");
        Directory.GetFiles("data\\ps2data\\plan_master\\in", "*.ard", SearchOption.AllDirectories).ForEach(s =>
        {
            string fileName = Path.GetFileName(s);
            string name = fileName.Substring(0, fileName.LastIndexOf("."));
            DataStoreARD ard = new();
            ard.LoadData(File.ReadAllBytes(s));
            ards.Add(name, ard);
        });

        FileHelpers.ReadCSVFile(@"data\enemies.csv", row =>
        {
            EnemyData e = new(row);
            enemyData.Add(e.ID, e);
        }, FileHelpers.CSVFileHeader.HasHeader);
    }

    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Enemy Data...");
        ApplyEXPLPScaling();

        if (FF12Flags.Stats.EnemyStats.FlagEnabled)
        {
            FF12Flags.Stats.EnemyStats.SetRand();

            RandomizeStats();

            RandomNum.ClearRand();
        }
    }

    private void ApplyEXPLPScaling()
    {
        ards.ForEach(pair =>
        {
            List<DataStoreARDStats> bossStats = new();
            enemyData.Values.Where(e => e.Area == pair.Key).ForEach(e =>
            {
                List<DataStoreARDStats> defaults;
                List<DataStoreARDStats> levels;
                if (e.Index == -1)
                {
                    defaults = pair.Value.BasicInfo.Where(b => b.NameID == e.IntID).Select(b => pair.Value.DefaultStats[b.DefaultStatsIndex]).Where(s => s.LP > 0).Distinct().ToList();
                    levels = pair.Value.BasicInfo.Where(b => b.NameID == e.IntID).Select(b => pair.Value.LevelStats[b.LevelStatsIndex]).Where(s => s.LP > 0).Distinct().ToList();
                }
                else
                {
                    defaults = new DataStoreARDStats[] { pair.Value.DefaultStats[pair.Value.BasicInfo[e.Index].DefaultStatsIndex] }.ToList();
                    levels = new DataStoreARDStats[] { pair.Value.LevelStats[pair.Value.BasicInfo[e.Index].LevelStatsIndex] }.ToList();
                }

                if (e.Traits.Contains("Boss"))
                {
                    defaults.ForEach(s => s.Experience = (uint)(BossScaling.EXPTable[Math.Min(BossScaling.EXPTable.Length, e.Rank)] * e.EXPLPScale / 100));
                    defaults.ForEach(s => s.LP = (byte)(BossScaling.LPTable[Math.Min(BossScaling.LPTable.Length, e.Rank)] * e.EXPLPScale / 100));
                    bossStats.AddRange(defaults);
                    bossStats.AddRange(levels);
                }
            });
            pair.Value.DefaultStats.ForEach(s => ApplyEXPMult(s, bossStats.Contains(s)));
            pair.Value.LevelStats.ForEach(s => ApplyEXPMult(s, bossStats.Contains(s)));

            pair.Value.DefaultStats.ForEach(s => ApplyLPMult(s, bossStats.Contains(s)));
            pair.Value.LevelStats.ForEach(s => ApplyLPMult(s, bossStats.Contains(s)));
        });
    }

    private void ApplyEXPMult(DataStoreARDStats stats, bool isBoss)
    {
        float expMult = FF12Flags.Other.EXPMult.FlagEnabled ? (isBoss ? FF12Flags.Other.EXPMultBossAmt.Value : FF12Flags.Other.EXPMultAmt.Value) / 100f : 1;
        stats.Experience = (uint)(stats.Experience * expMult);
    }

    private void ApplyLPMult(DataStoreARDStats stats, bool isBoss)
    {
        float lpMult = FF12Flags.Other.LPMult.FlagEnabled ? (isBoss ? FF12Flags.Other.LPMultBossAmt.Value : FF12Flags.Other.LPMultAmt.Value) / 100f : 1;
        stats.LP = (byte)Math.Min(stats.LP * lpMult, 255);
    }

    private void RandomizeStats()
    {
        // Group by enemy name ID to ensure all variants of the same enemy get the same stat modifiers
        Dictionary<int, HashSet<DataStoreARDStats>> enemyStatsGroups = new();
        foreach (var ard in ards.Values)
        {
            foreach (var basicInfo in ard.BasicInfo)
            {
                if (!enemyStatsGroups.ContainsKey(basicInfo.NameID))
                {
                    enemyStatsGroups[basicInfo.NameID] = new HashSet<DataStoreARDStats>();
                }

                enemyStatsGroups[basicInfo.NameID].Add(ard.DefaultStats[basicInfo.DefaultStatsIndex]);
                enemyStatsGroups[basicInfo.NameID].Add(ard.LevelStats[basicInfo.LevelStatsIndex]);
            }
        }

        // Randomize stats for each enemy group
        foreach (var group in enemyStatsGroups)
        {
            StatDef<EnemyStatType> size = new()
            {
                Type = EnemyStatType.SIZE,
                MinValue = 1,
                MaxValue = GetMaxSize(group.Key),
                MinMultiplier = 1.0 / FF12Flags.Stats.EnemySize.Value,
                MaxMultiplier = FF12Flags.Stats.EnemySize.Value
            };
            StatDef<EnemyStatType> hp = new()
            {
                Type = EnemyStatType.HP,
                MinValue = 10,
                MaxValue = 999999999,
                MinMultiplier = 1.0 / (FF12Flags.Stats.EnemyHPMP.Value / 100.0),
                MaxMultiplier = FF12Flags.Stats.EnemyHPMP.Value / 100.0
            };
            StatDef<EnemyStatType> mp = new()
            {
                Type = EnemyStatType.MP,
                MinValue = 0,
                MaxValue = 9999,
                MinMultiplier = 1.0 / (FF12Flags.Stats.EnemyHPMP.Value / 100.0),
                MaxMultiplier = FF12Flags.Stats.EnemyHPMP.Value / 100.0
            };
            StatDef<EnemyStatType> str = new()
            {
                Type = EnemyStatType.STR,
                MinValue = 1,
                MaxValue = 255,
                MinMultiplier = 1.0 / (FF12Flags.Stats.EnemyBaseStats.Value / 100.0),
                MaxMultiplier = FF12Flags.Stats.EnemyBaseStats.Value / 100.0
            };
            StatDef<EnemyStatType> mag = new()
            {
                Type = EnemyStatType.MAG,
                MinValue = 1,
                MaxValue = 255,
                MinMultiplier = 1.0 / (FF12Flags.Stats.EnemyBaseStats.Value / 100.0),
                MaxMultiplier = FF12Flags.Stats.EnemyBaseStats.Value / 100.0
            };
            StatDef<EnemyStatType> vit = new()
            {
                Type = EnemyStatType.VIT,
                MinValue = 1,
                MaxValue = 255,
                MinMultiplier = 1.0 / (FF12Flags.Stats.EnemyBaseStats.Value / 100.0),
                MaxMultiplier = FF12Flags.Stats.EnemyBaseStats.Value / 100.0
            };
            StatDef<EnemyStatType> spd = new()
            {
                Type = EnemyStatType.SPD,
                MinValue = 1,
                MaxValue = 255,
                MinMultiplier = 1.0 / (FF12Flags.Stats.EnemyBaseStats.Value / 100.0),
                MaxMultiplier = FF12Flags.Stats.EnemyBaseStats.Value / 100.0
            };
            StatDef<EnemyStatType> eva = new()
            {
                Type = EnemyStatType.EVA,
                MinValue = 1,
                MaxValue = 255,
                MinMultiplier = 1.0 / (FF12Flags.Stats.EnemyBaseStats.Value / 100.0),
                MaxMultiplier = FF12Flags.Stats.EnemyBaseStats.Value / 100.0
            };
            StatDef<EnemyStatType> def = new()
            {
                Type = EnemyStatType.DEF,
                MinValue = 1,
                MaxValue = 255,
                MinMultiplier = 1.0 / (FF12Flags.Stats.EnemyBaseStats.Value / 100.0),
                MaxMultiplier = FF12Flags.Stats.EnemyBaseStats.Value / 100.0
            };
            StatDef<EnemyStatType> mres = new()
            {
                Type = EnemyStatType.MRES,
                MinValue = 1,
                MaxValue = 255,
                MinMultiplier = 1.0 / (FF12Flags.Stats.EnemyBaseStats.Value / 100.0),
                MaxMultiplier = FF12Flags.Stats.EnemyBaseStats.Value / 100.0
            };
            StatDef<EnemyStatType> atk = new()
            {
                Type = EnemyStatType.ATK,
                MinValue = 1,
                MaxValue = 255,
                MinMultiplier = 1.0 / (FF12Flags.Stats.EnemyBaseStats.Value / 100.0),
                MaxMultiplier = FF12Flags.Stats.EnemyBaseStats.Value / 100.0
            };
            StatDef<EnemyStatType> lp = new()
            {
                Type = EnemyStatType.LP,
                MinValue = 1,
                MaxValue = 255,
                MinMultiplier = 1.0 / (FF12Flags.Stats.EnemyEXPLP.Value / 100.0),
                MaxMultiplier = FF12Flags.Stats.EnemyEXPLP.Value / 100.0
            };
            StatDef<EnemyStatType> exp = new()
            {
                Type = EnemyStatType.EXP,
                MinValue = 0,
                MaxValue = 999999,
                MinMultiplier = 1.0 / (FF12Flags.Stats.EnemyEXPLP.Value / 100.0),
                MaxMultiplier = FF12Flags.Stats.EnemyEXPLP.Value / 100.0
            };

            size.RandomizeFunc = () =>
            {
                double newMult = RandomNum.RandMultiplier(FF12Flags.Stats.EnemySize.Value);

                double substatMult = Math.Sqrt(newMult);
                if (FF12Flags.Stats.EnemyHPMP.Value > 100)
                {
                    hp.Multiplier *= substatMult;
                    mp.Multiplier *= substatMult;
                }

                if (FF12Flags.Stats.EnemyBaseStats.Value > 100)
                {
                    str.Multiplier *= substatMult;
                    mag.Multiplier *= substatMult;
                    vit.Multiplier *= substatMult;
                    spd.Multiplier *= substatMult;
                    eva.Multiplier *= substatMult;
                    def.Multiplier *= substatMult;
                    mres.Multiplier *= substatMult;
                    atk.Multiplier *= substatMult;
                }

                if (FF12Flags.Stats.EnemySize.Value > 100)
                {
                    size.Multiplier *= newMult;
                }
            };

            Func<StatDef<EnemyStatType>, int, Action> statRandomizeFuncBuilder = (statType, flagValue) =>
            {
                return () =>
                {
                    double newMult = RandomNum.RandMultiplier(flagValue);

                    if (FF12Flags.Stats.EnemyEXPLP.Value > 100)
                    {
                        exp.Multiplier *= newMult;
                        lp.Multiplier *= newMult;
                    }

                    if (flagValue > 100)
                    {
                        statType.Multiplier *= newMult;
                    }
                };
            };
            hp.RandomizeFunc = statRandomizeFuncBuilder(hp, FF12Flags.Stats.EnemyHPMP.Value);
            mp.RandomizeFunc = statRandomizeFuncBuilder(mp, FF12Flags.Stats.EnemyHPMP.Value);
            str.RandomizeFunc = statRandomizeFuncBuilder(str, FF12Flags.Stats.EnemyBaseStats.Value);
            mag.RandomizeFunc = statRandomizeFuncBuilder(mag, FF12Flags.Stats.EnemyBaseStats.Value);
            vit.RandomizeFunc = statRandomizeFuncBuilder(vit, FF12Flags.Stats.EnemyBaseStats.Value);
            spd.RandomizeFunc = statRandomizeFuncBuilder(spd, FF12Flags.Stats.EnemyBaseStats.Value);
            eva.RandomizeFunc = statRandomizeFuncBuilder(eva, FF12Flags.Stats.EnemyBaseStats.Value);
            def.RandomizeFunc = statRandomizeFuncBuilder(def, FF12Flags.Stats.EnemyBaseStats.Value);
            mres.RandomizeFunc = statRandomizeFuncBuilder(mres, FF12Flags.Stats.EnemyBaseStats.Value);
            atk.RandomizeFunc = statRandomizeFuncBuilder(atk, FF12Flags.Stats.EnemyBaseStats.Value);

            exp.RandomizeFunc = () =>
            {
                if (FF12Flags.Stats.EnemyEXPLP.Value > 100)
                {
                    double newMult = RandomNum.RandMultiplier(FF12Flags.Stats.EnemyEXPLP.Value);
                    exp.Multiplier *= newMult;
                }
            };
            lp.RandomizeFunc = () =>
            {
                if (FF12Flags.Stats.EnemyEXPLP.Value > 100)
                {
                    double newMult = RandomNum.RandMultiplier(FF12Flags.Stats.EnemyEXPLP.Value);
                    lp.Multiplier *= newMult;
                }
            };

            StatRandomizer<EnemyStatType> statRandomizer = new();
            statRandomizer[EnemyStatType.HP] = hp;
            statRandomizer[EnemyStatType.MP] = mp;
            statRandomizer[EnemyStatType.STR] = str;
            statRandomizer[EnemyStatType.MAG] = mag;
            statRandomizer[EnemyStatType.VIT] = vit;
            statRandomizer[EnemyStatType.SPD] = spd;
            statRandomizer[EnemyStatType.EVA] = eva;
            statRandomizer[EnemyStatType.DEF] = def;
            statRandomizer[EnemyStatType.MRES] = mres;
            statRandomizer[EnemyStatType.ATK] = atk;
            statRandomizer[EnemyStatType.LP] = lp;
            statRandomizer[EnemyStatType.EXP] = exp;
            statRandomizer[EnemyStatType.SIZE] = size;

            statRandomizer.Randomize();

            foreach (var stats in group.Value)
            {
                if (FF12Flags.Stats.EnemyHPMP.Value > 100)
                {
                    stats.HP = (uint)statRandomizer[EnemyStatType.HP].ApplyMult((int)stats.HP);
                    stats.MP = (ushort)statRandomizer[EnemyStatType.MP].ApplyMult((int)stats.MP);
                }

                if (FF12Flags.Stats.EnemyBaseStats.Value > 100)
                {
                    stats.Strength = (byte)statRandomizer[EnemyStatType.STR].ApplyMult((int)stats.Strength);
                    stats.MagickPower = (byte)statRandomizer[EnemyStatType.MAG].ApplyMult((int)stats.MagickPower);
                    stats.Vitality = (byte)statRandomizer[EnemyStatType.VIT].ApplyMult((int)stats.Vitality);
                    stats.Speed = (byte)statRandomizer[EnemyStatType.SPD].ApplyMult((int)stats.Speed);
                    stats.Evade = (byte)statRandomizer[EnemyStatType.EVA].ApplyMult((int)stats.Evade);
                    stats.Defense = (byte)statRandomizer[EnemyStatType.DEF].ApplyMult((int)stats.Defense);
                    stats.MagickResist = (byte)statRandomizer[EnemyStatType.MRES].ApplyMult((int)stats.MagickResist);
                    stats.AttackPower = (byte)statRandomizer[EnemyStatType.ATK].ApplyMult((int)stats.AttackPower);
                }

                if (FF12Flags.Stats.EnemyEXPLP.Value > 100)
                {
                    stats.LP = (byte)statRandomizer[EnemyStatType.LP].ApplyMultControlled((int)stats.LP);
                    stats.Experience = (uint)statRandomizer[EnemyStatType.EXP].ApplyMultControlled((int)stats.Experience);
                }
            }

            // Apply size updates
            // Find any basic info entries with the same name ID as the current group
            if (FF12Flags.Stats.EnemySize.Value > 100)
            {
                var basicInfoEntries = ards.Values.SelectMany(ard => ard.BasicInfo).Where(b => b.NameID == group.Key);
                foreach (var basicInfo in basicInfoEntries)
                {
                    basicInfo.SizeX = (ushort)statRandomizer[EnemyStatType.SIZE].ApplyMult((int)basicInfo.SizeX);
                    basicInfo.SizeY = (ushort)statRandomizer[EnemyStatType.SIZE].ApplyMult((int)basicInfo.SizeY);
                    basicInfo.SizeZ = (ushort)statRandomizer[EnemyStatType.SIZE].ApplyMult((int)basicInfo.SizeZ);
                }
            }

            foreach (var basicInfo in ards.Values.SelectMany(ard => ard.BasicInfo).Where(b => b.NameID == group.Key))
            {
                // DEBUG
                if (FF12Flags.Debug.MaxEnemySize.FlagEnabled)
                {
                    basicInfo.SizeX = (ushort)GetMaxSize(basicInfo.NameID);
                    basicInfo.SizeY = (ushort)GetMaxSize(basicInfo.NameID);
                    basicInfo.SizeZ = (ushort)GetMaxSize(basicInfo.NameID);
                }

                DataStoreARDExtendedInfo extended = basicInfo.ParentARD.ExtendedInfo[basicInfo.ExtendedInfoIndex];
                // If bat enemies get big enough, you can't target them, so give them flying info to allow targeting as a flying enemy.
                if (extended.Genus == 20 && extended.IsFlying && !extended.HasFlyingInfo && basicInfo.SizeX > 150)
                {
                    extended.HasFlyingInfo = true;
                }
            }
        }
    }

    private static int GetMaxSize(int nameID)
    {
        // Ixtab
        if (nameID == 16489)
        {
            return 200;
        }
        // Tyranorox
        if (nameID == 16770)
        {
            return 125;
        }
        // Marilith
        if (nameID == 16486)
        {
            return 200;
        }
        // Demon Wall
        if (nameID == 16875)
        {
            return 100;
        }

        return 300;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Enemy Data...");
        ards.ForEach(p =>
        {
            File.WriteAllBytes($"{Generator.DataOutFolder}\\plan_master\\in\\plan_map\\{p.Key}\\area\\{p.Key}.ard", p.Value.Data);
        });
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        var pages = base.GetDocumentation();

        HTMLPage page = new("Enemies", "template/documentation.html");

        // For each ARD area, add a table of enemies present with their stats
        foreach (var areaPair in ards.OrderBy(p => p.Key))
        {
            string areaName = areaPair.Key;
            var ard = areaPair.Value;

            // Group basic infos by NameID (enemy kind)
            var groups = ard.BasicInfo.GroupBy(b => b.NameID);

            List<List<object>> rows = new();

            foreach (var g in groups)
            {
                int nameId = g.Key;

                // Pick a display name from enemies.csv if available, else fallback to ID
                string enemyName = enemyData.Values.FirstOrDefault(e => e.IntID == nameId)?.Name ?? $"{nameId}";

                // Use the first entry for size display (sizes may vary slightly per placement)
                var bfSample = g.First();
                string sizeDisp = $"{bfSample.SizeX}";

                // Collect distinct default/level stats indices referenced by this enemy in this ARD
                var defaultIdxs = g.Select(b => (int)b.DefaultStatsIndex).Distinct().ToList();
                var levelIdxs = g.Select(b => (int)b.LevelStatsIndex).Distinct().ToList();

                // Default stats rows
                foreach (int idx in defaultIdxs)
                {
                    var s = ard.DefaultStats[idx];

                    rows.Add(new List<object>
                    {
                        enemyName,
                        s.HP,
                        s.MP,
                        s.Strength,
                        s.MagickPower,
                        s.Vitality,
                        s.Speed,
                        s.Evade,
                        s.Defense,
                        s.MagickResist,
                        s.AttackPower,
                        s.LP,
                        s.Experience,
                        sizeDisp
                    });
                }
            }

            // Sort rows by enemy name then by stats type (Default before Level)
            rows = rows
                .OrderBy(r => r[0].ToString())
                .ThenBy(r => r[1].ToString())
                .ToList();

            if (rows.Count == 0)
            {
                // Nothing to show for this area (rare), skip adding empty table
                continue;
            }

            // Build table for this area
            var columns = new List<string>
            {
                "Name",
                "HP","MP","STR","MAG","VIT","SPD","EVA","DEF","MRES","ATK","LP","EXP",
                "Size"
            };
            var widths = new List<int> { 14, 8, 8, 6, 6, 6, 6, 6, 6, 6, 6, 6, 8, 8 };

            page.HTMLElements.Add(new Table(areaName, columns, widths, rows, id: $"enemies_{areaName.Replace(" ", "_").ToLower()}"));
        }

        pages.Add("enemies", page);
        return pages;
    }
}

using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using FF12Rando;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando;

public class EnemyRando : Randomizer
{
    private readonly Dictionary<string, DataStoreARD> ards = new();
    private readonly Dictionary<string, EnemyData> enemyData = new();

    public EnemyRando(RandomizerManager randomizers) : base(randomizers) { }

    public override void Load()
    {
        Randomizers.SetUIProgress("Loading Enemy Data...", 0, -1);
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
        Randomizers.SetUIProgress("Randomizing Enemy Data...", 0, -1);
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

    public override void Save()
    {
        Randomizers.SetUIProgress("Saving Enemy Data...", 0, -1);
        ards.ForEach(p =>
        {
            File.WriteAllBytes($"outdata\\ps2data\\plan_master\\in\\plan_map\\{p.Key}\\area\\{p.Key}.ard", p.Value.Data);
        });
    }
    public class EnemyData : CSVDataRow
    {
        [RowIndex(0)]
        public string Name { get; set; }
        [RowIndex(1), FieldTypeOverride(FieldType.HexInt)]
        public int IntID { get; set; }
        public string ID { get; set; }
        [RowIndex(2)]
        public int Rank { get; set; }
        [RowIndex(3)]
        public string Area { get; set; }
        [RowIndex(4)]
        public int Index { get; set; }
        [RowIndex(5)]
        public int EXPLPScale { get; set; }
        [RowIndex(6)]
        public List<string> Traits { get; set; }
        public EnemyData(string[] row) : base(row)
        {
            ID = row[3] + ":" + row[1] + ":" + row[4];
        }
    }
}

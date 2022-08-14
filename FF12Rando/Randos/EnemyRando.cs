using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando
{
    public class EnemyRando : Randomizer
    {
        Dictionary<string, DataStoreARD> ards = new Dictionary<string, DataStoreARD>();
        Dictionary<string, EnemyData> enemyData = new Dictionary<string, EnemyData>();

        public EnemyRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Directory.GetFiles("data\\ps2data\\plan_master\\in", "*.ard", SearchOption.AllDirectories).ForEach(s =>
            {
                string fileName = Path.GetFileName(s);
                string name = fileName.Substring(0, fileName.LastIndexOf("."));
                DataStoreARD ard = new DataStoreARD();
                ard.LoadData(File.ReadAllBytes(s));
                ards.Add(name, ard);
            });

            FileHelpers.ReadCSVFile(@"data\enemies.csv", row =>
            {
                EnemyData e = new EnemyData(row);
                enemyData.Add(e.ID, e);
            }, FileHelpers.CSVFileHeader.HasHeader);
        }
        public override void Randomize(Action<int> progressSetter)
        {
            ards.ForEach(pair =>
            {
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
                    }
                });
                float expMult = FF12Flags.Other.ExpMult.FlagEnabled ? FF12Flags.Other.EXPMultAmt.Value / 100f : 1;
                pair.Value.DefaultStats.ForEach(s => s.Experience = (uint)(s.Experience * expMult));
                pair.Value.LevelStats.ForEach(s => s.Experience = (uint)(s.Experience * expMult));

                float lpMult = FF12Flags.Other.LPMult.FlagEnabled ? FF12Flags.Other.LPMultAmt.Value / 100f : 1;
                pair.Value.DefaultStats.ForEach(s => s.LP = (byte)Math.Min(s.LP * lpMult, 255));
                pair.Value.LevelStats.ForEach(s => s.LP = (byte)Math.Min(s.LP * lpMult, 255));
            });
        }

        public override void Save()
        {
            ards.ForEach(p =>
            {
                File.WriteAllBytes($"outdata\\ps2data\\plan_master\\in\\plan_map\\{p.Key}\\area\\{p.Key}.ard", p.Value.Data);
            });
        }
        public class EnemyData
        {
            public string Name { get; set; }
            public int IntID { get; set; }
            public string ID { get; set; }
            public int Rank { get; set; }
            public string Area { get; set; }
            public int Index { get; set; }
            public int EXPLPScale { get; set; }
            public List<string> Traits { get; set; }
            public EnemyData(string[] row)
            {
                Name = row[0];
                IntID = Convert.ToInt32(row[1], 16);
                ID = row[3] + ":" + row[1] + ":" + row[4];
                Rank = int.Parse(row[2]);
                Area = row[3];
                Index = int.Parse(row[4]);
                EXPLPScale = int.Parse(row[5]);
                Traits = row[6].Split("|").Where(s => !String.IsNullOrEmpty(s)).ToList();
            }
        }
    }
}

using Bartz24.Data;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FF13_2Rando
{
    public class CrystariumRando : Randomizer
    {
        public DataStoreDB3<DataStoreRGrowPc> crystSerah = new DataStoreDB3<DataStoreRGrowPc>();
        public DataStoreDB3<DataStoreRGrowPc> crystNoel = new DataStoreDB3<DataStoreRGrowPc>();
        public DataStoreDB3<DataStoreRGrowSt> crystMonster = new DataStoreDB3<DataStoreRGrowSt>();

        public Dictionary<string, AbilityData> abilityData = new Dictionary<string, AbilityData>();

        public CrystariumRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetUIProgress("Loading Crystarium Data...", 0, -1);
            crystSerah.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_grow_pc008.wdb", false);
            crystNoel.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_grow_pc010.wdb", false);
            crystMonster.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_grow_st.wdb", false);

            abilityData.Clear();
            using (CsvParser csv = new CsvParser(new StreamReader(@"data\abilities.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
            {
                while (csv.Read())
                {
                    AbilityData a = new AbilityData(csv.Record);
                    abilityData.Add(a.ID, a);
                }
            }
        }
        public override void Randomize()
        {
            Randomizers.SetUIProgress("Randomizing Crystarium Data...", 0, -1);
            if (FF13_2Flags.Stats.RandCrystAbi.FlagEnabled)
            {
                FF13_2Flags.Stats.RandCrystAbi.SetRand();
                {
                    List<string> learned = new List<string>();
                    crystSerah.Values.Where(c => c.sAbilityId_string != "").ForEach(c =>
                    {
                        string next = GetPossiblePc(learned, GetPcRole(c.u4Role), c.u7Lv == 1).Shuffle().First();
                        learned.Add(next);
                        c.sAbilityId_string = next;
                    });
                }
                {
                    List<string> learned = new List<string>();
                    crystNoel.Values.Where(c => c.sAbilityId_string != "").ForEach(c =>
                    {
                        string next = GetPossiblePc(learned, GetPcRole(c.u4Role), c.u7Lv == 1).Shuffle().First();
                        learned.Add(next);
                        c.sAbilityId_string = next;
                    });
                }
                crystMonster.Values.Where(m => !m.name.StartsWith("pc") && m.u5RoleStyle % 5 == 1).ForEach(m =>
                  {
                      List<string> learned = new List<string>();
                      List<string> orig = m.GetAbilities();
                      for (int i = 0; i < orig.Count; i++)
                      {
                          if (abilityData.ContainsKey(orig[i]))
                          {
                              string next = GetPossibleMon(learned, GetMonsterRole(m.u5RoleStyle)).Shuffle().First();
                              learned.Add(next);
                          }
                          else
                              learned.Add(orig[i]);
                      }
                      m.SetAbilities(learned);
                  });
                RandomNum.ClearRand();
            }
        }

        private List<string> GetPossiblePc(List<string> learned, string role, bool level1)
        {
            List<AbilityData> list = abilityData.Values.Where(a => a.Role == role && !a.Traits.Contains("Mon") && !learned.Contains(a.ID)).ToList();

            list = list.Where(a => a.Requirements.IsValid(learned.ToDictionary(a => a, _ => 1))).ToList();

            if (level1)
                list = list.Where(a => !a.Traits.Contains("Auto")).ToList();
            return list.Select(a => a.ID).ToList();
        }

        private List<string> GetPossibleMon(List<string> learned, string role)
        {
            List<AbilityData> list = new List<AbilityData>();
            while (list.Count == 0)
            {
                list = abilityData.Values.Where(a => (a.Role == role || a.Role == "") && !learned.Contains(a.ID)).ToList();

                list = list.Where(a => a.Requirements.IsValid(learned.ToDictionary(a => a, _ => 1))).ToList();

                if (learned.Count == 0)
                    list = list.Where(a => !a.Traits.Contains("Auto")).ToList();
                if (learned.Count == 0 || RandomNum.RandInt(0, 99) < 50)
                    list = list.Where(a => a.Role != "").ToList();
            }
            return list.Select(a => a.ID).ToList();
        }

        private string GetPcRole(int role)
        {
            switch (role)
            {
                case 1:
                    return "SEN";
                case 2:
                    return "COM";
                case 3:
                    return "RAV";
                case 4:
                    return "SYN";
                case 5:
                    return "SAB";
                case 6:
                    return "MED";
                default:
                    throw new Exception("Invalid role ID: " + role);
            }
        }

        private string GetMonsterRole(int roleStyle)
        {
            switch (roleStyle)
            {
                case 1:
                    return "SEN";
                case 6:
                    return "COM";
                case 11:
                    return "RAV";
                case 16:
                    return "SYN";
                case 21:
                    return "SAB";
                case 26:
                    return "MED";
                default:
                    throw new Exception("Invalid role ID: " + roleStyle);
            }
        }

        public override void Save()
        {
            Randomizers.SetUIProgress("Saving Crystarium Data...", 0, -1);
            crystSerah.SaveDB3(@"\db\resident\_wdbpack.bin\r_grow_pc008.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_grow_pc008.wdb");
            crystNoel.SaveDB3(@"\db\resident\_wdbpack.bin\r_grow_pc010.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_grow_pc010.wdb");
            crystMonster.SaveDB3(@"\db\resident\_wdbpack.bin\r_grow_st.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_grow_st.wdb");
        }
        public class AbilityData
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
            public List<string> Traits { get; set; }
            public ItemReq Requirements { get; set; }
            public AbilityData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Role = row[2];
                Traits = row[3].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                Requirements = ItemReq.Parse(row[4]);
            }
        }
    }
}

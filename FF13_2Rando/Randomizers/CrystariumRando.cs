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

namespace FF13_2Rando;

public partial class CrystariumRando : Randomizer
{
    public DataStoreDB3<DataStoreRGrowPc> crystSerah = new();
    public DataStoreDB3<DataStoreRGrowPc> crystNoel = new();
    public DataStoreDB3<DataStoreRGrowSt> crystMonster = new();

    public Dictionary<string, AbilityData> abilityData = new();

    public CrystariumRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Crystarium Data...");
        crystSerah.LoadDB3(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_grow_pc008.wdb", false);
        crystNoel.LoadDB3(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_grow_pc010.wdb", false);
        crystMonster.LoadDB3(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_grow_st.wdb", false);

        abilityData.Clear();
        using (CsvParser csv = new(new StreamReader(@"data\abilities.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
        {
            while (csv.Read())
            {
                AbilityData a = new(csv.Record);
                abilityData.Add(a.ID, a);
            }
        }
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Crystarium Data...");
        if (FF13_2Flags.Stats.RandCrystAbi.FlagEnabled)
        {
            FF13_2Flags.Stats.RandCrystAbi.SetRand();
            {
                List<string> learned = new();
                crystSerah.Values.Where(c => c.sAbilityId_string != "").ForEach(c =>
                {
                    string next = GetPossiblePc(learned, GetPcRole(c.u4Role), c.u7Lv == 1).Shuffle().First();
                    learned.Add(next);
                    c.sAbilityId_string = next;
                });
            }

            {
                List<string> learned = new();
                crystNoel.Values.Where(c => c.sAbilityId_string != "").ForEach(c =>
                {
                    string next = GetPossiblePc(learned, GetPcRole(c.u4Role), c.u7Lv == 1).Shuffle().First();
                    learned.Add(next);
                    c.sAbilityId_string = next;
                });
            }

            crystMonster.Values.Where(m => !m.name.StartsWith("pc") && m.u5RoleStyle % 5 == 1).ForEach(m =>
              {
                  List<string> learned = new();
                  List<string> orig = m.GetAbilities();
                  for (int i = 0; i < orig.Count; i++)
                  {
                      if (abilityData.ContainsKey(orig[i]))
                      {
                          string next = GetPossibleMon(learned, GetMonsterRole(m.u5RoleStyle)).Shuffle().First();
                          learned.Add(next);
                      }
                      else
                      {
                          learned.Add(orig[i]);
                      }
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
        {
            list = list.Where(a => !a.Traits.Contains("Auto")).ToList();
        }

        return list.Select(a => a.ID).ToList();
    }

    private List<string> GetPossibleMon(List<string> learned, string role)
    {
        List<AbilityData> list = new();
        while (list.Count == 0)
        {
            list = abilityData.Values.Where(a => (a.Role == role || a.Role == "") && !learned.Contains(a.ID)).ToList();

            list = list.Where(a => a.Requirements.IsValid(learned.ToDictionary(a => a, _ => 1))).ToList();

            if (learned.Count == 0)
            {
                list = list.Where(a => !a.Traits.Contains("Auto")).ToList();
            }

            if (learned.Count == 0 || RandomNum.RandInt(0, 99) < 50)
            {
                list = list.Where(a => a.Role != "").ToList();
            }
        }

        return list.Select(a => a.ID).ToList();
    }

    private string GetPcRole(int role)
    {
        return role switch
        {
            1 => "SEN",
            2 => "COM",
            3 => "RAV",
            4 => "SYN",
            5 => "SAB",
            6 => "MED",
            _ => throw new Exception("Invalid role ID: " + role),
        };
    }

    private string GetMonsterRole(int roleStyle)
    {
        return roleStyle switch
        {
            1 => "SEN",
            6 => "COM",
            11 => "RAV",
            16 => "SYN",
            21 => "SAB",
            26 => "MED",
            _ => throw new Exception("Invalid role ID: " + roleStyle),
        };
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Crystarium Data...");
        crystSerah.SaveDB3(Generator, @"\db\resident\_wdbpack.bin\r_grow_pc008.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_grow_pc008.wdb");
        crystNoel.SaveDB3(Generator, @"\db\resident\_wdbpack.bin\r_grow_pc010.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_grow_pc010.wdb");
        crystMonster.SaveDB3(Generator, @"\db\resident\_wdbpack.bin\r_grow_st.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_grow_st.wdb");
    }
}

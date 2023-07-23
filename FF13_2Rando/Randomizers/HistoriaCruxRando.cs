using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FF13_2Rando;

public class HistoriaCruxRando : Randomizer
{
    public DataStoreDB3<DataStoreRGateTable> gateTable = new();
    public DataStoreDB3<DataStoreRGateTable> gateTableOrig = new();

    public Dictionary<string, GateData> gateData = new();
    public Dictionary<string, AreaData> areaData = new();

    public Dictionary<string, string> placement = new();

    public HistoriaCruxRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        Randomizers.SetUIProgress("Loading Historia Crux Data...", 0, -1);
        gateTable.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_gatetab.wdb", false);
        gateTableOrig.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_gatetab.wdb", false);

        gateData.Clear();
        using (CsvParser csv = new(new StreamReader(@"data\historia.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
        {
            while (csv.Read())
            {
                GateData t = new(csv.Record);
                gateData.Add(t.ID, t);
            }
        }

        FileHelpers.ReadCSVFile(@"data\areas.csv", row =>
        {
            AreaData a = new(row);
            areaData.Add(a.ID, a);
        }, FileHelpers.CSVFileHeader.HasHeader);
    }
    public override void Randomize()
    {
        Randomizers.SetUIProgress("Randomizing Historia Crux Data...", 0, -1);
        if (FF13_2Flags.Other.HistoriaCrux.FlagEnabled)
        {
            FF13_2Flags.Other.HistoriaCrux.SetRand();

            List<string> openings = gateData.Keys
                .Where(id => !gateData[id].Traits.Contains("Paradox"))
                .Where(id => FF13_2Flags.Other.RandoDLC.Enabled || !gateData[id].Traits.Contains("DLC"))
                .Select(id => gateTable[id].sOpenHistoria1_string)
                .Select(s => s.Substring(0, s.Length - 2))
                .Distinct().ToList();

            if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 0)
            {
                openings = openings.Where(o => o != "h_hm_AD0003").ToList();
            }

            if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 1)
            {
                openings = openings.Where(o => o != "h_bj_AD0005").ToList();
            }

            placement = GetPlacement(new Dictionary<string, string>(), openings).Item2;

            placement.Keys.ToList().ForEach(open =>
            {
                gateTable.Keys.Where(id => gateTableOrig[id].sOpenHistoria1_string.StartsWith(open)).ToList().ForEach(id => gateTable[id].sOpenHistoria1_string = placement[open] + "_a");
            });

            if (placement.ContainsKey("h_hm_AD0003"))
            {
                gateTable["hs_hmaa10_zz"].sArea_string = placement["h_hm_AD0003"];
            }

            RandomNum.ClearRand();
        }
    }

    private (bool, Dictionary<string, string>) GetPlacement(Dictionary<string, string> soFar, List<string> openings)
    {
        List<string> available = GetAvailableLocations(soFar);
        List<string> remaining = openings.Where(t => !soFar.ContainsValue(t)).Shuffle();

        foreach (string rep in remaining)
        {
            List<string> possible = openings.Where(o => !soFar.ContainsKey(o) && IsAllowed(o, soFar, available)).Shuffle();
            if (possible.Count == 0)
            {
                return (false, soFar);
            }
        }

        foreach (string rep in remaining)
        {
            List<string> possible = openings.Where(o => !soFar.ContainsKey(o) && IsAllowed(o, soFar, available)).ToList();
            while (possible.Count > 0)
            {
                string next = SelectNext(possible);
                soFar.Add(next, rep);
                if (soFar.Count == openings.Count)
                {
                    return (true, soFar);
                }

                (bool, Dictionary<string, string>) result = GetPlacement(soFar, openings);
                if (result.Item1)
                {
                    return result;
                }
                else
                {
                    possible.Remove(next);
                    soFar.Remove(next);
                }
            }
        }

        return (false, soFar);
    }

    private string SelectNext(List<string> possible)
    {
        return possible[RandomNum.RandInt(0, possible.Count - 1)];
    }

    public List<string> GetIDsForOpening(string open, bool orig = true)
    {
        return gateData.Keys.Where(id => (orig ? gateTableOrig[id] : gateTable[id]).sOpenHistoria1_string.StartsWith(open)).ToList();
    }

    private bool IsAllowed(string open, Dictionary<string, string> soFar, List<string> available)
    {
        foreach (string id in GetIDsForOpening(open))
        {
            if (!available.Contains(gateData[id].Location))
            {
                return false;
            }

            if (available.Intersect(gateData[id].Requirements).Count() != gateData[id].Requirements.Count)
            {
                return false;
            }

            if (gateData[id].Traits.Contains("Graviton") && !HasGravitonLocations(available))
            {
                return false;
            }

            if (gateData[id].Traits.Contains("Wild") && !HasWildArtefacts(soFar, available))
            {
                return false;
            }

            if (gateData[id].MinMogLevel > GetMogLevel(available))
            {
                return false;
            }
            // Hard code for Bresha 5 wild artefact if key items aren't rando
            if (!FF13_2Flags.Items.Treasures.FlagEnabled || !FF13_2Flags.Items.KeySide.Enabled || TooSmallOfPool())
            {
                if (gateData[id].ItemRequirements.GetPossibleRequirements().Contains("key_lockjail") && 2 > GetMogLevel(available))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public int GetMogLevel(List<string> available)
    {
        return available.Contains("h_dd_AD0700") ? 3 : available.Contains("h_sn_AD0300") ? 2 : available.Contains("h_bj_AD0005") ? 1 : 0;
    }

    private bool HasGravitonLocations(List<string> available)
    {
        if (!FF13_2Flags.Items.Treasures.FlagEnabled || !FF13_2Flags.Items.KeyGraviton.Enabled || TooSmallOfPool())
        {
            // If graviton cores aren't rando, use normal logic
            List<string> gravitons = new();
            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_hm_AD0003"); // Bodhum 3. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_bj_AD0005"); // Bresha 5. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_gw_AD0200"); // Oerba 200. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_ac_AD0400"); // Academia 400. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_gy_AD0100"); // Yaschas 100. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_gw_AD0400"); // Oerba 400. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                gravitons.Add("h_sn_AD0400"); // Sunleth 400. requires moogle hunt
            }

            if (available.Intersect(gravitons).Count() < 5)
            {
                return false;
            }
        }

        return true;
    }

    private bool HasWildArtefacts(Dictionary<string, string> soFar, List<string> available)
    {
        if (!FF13_2Flags.Items.Treasures.FlagEnabled || !FF13_2Flags.Items.KeyWild.Enabled || TooSmallOfPool())
        {
            // If wild artefacts aren't rando, use normal logic
            List<string> wilds = new();
            if (GetMogLevel(available) >= 1)
            {
                wilds.Add("h_bj_AD0005"); // Bresha 5. requires moogle hunt
            }

            if (GetMogLevel(available) >= 1)
            {
                wilds.Add("h_bj_AD0300"); // Bresha 300. requires moogle hunt
            }

            if (GetMogLevel(available) >= 2)
            {
                wilds.Add("h_gw_AD0200"); // Oerba 200. requires moogle throw
            }

            if (GetMogLevel(available) >= 2)
            {
                wilds.Add("h_sn_AD0300"); // Sunleth 300. requires moogle throw
            }

            if (GetMogLevel(available) >= 2)
            {
                wilds.Add("h_gd_NA0000"); // Archylte. requires moogle throw
            }

            wilds.Add("h_gt_AD0200"); // Augusta 200
            if (GetMogLevel(available) >= 1)
            {
                wilds.Add("h_aa_AD0400"); // Academia 4XX. requires moogle hunt
            }

            if (GetMogLevel(available) >= 2)
            {
                wilds.Add("h_gy_AD0100"); // Yaschas 100. requires moogle hunt and throw
            }

            if (GetMogLevel(available) >= 1)
            {
                wilds.Add("h_dd_AD0700"); // Dying World 700. requires moogle hunt
            }

            if (available.Contains("h_sn_AD0300") && available.Contains("h_gd_NA0000") && available.Contains("h_gh_AD0010") && available.Contains("h_cl_NA0000"))
            {
                wilds.Add("h_cs_NA0000"); // Serendipity. requires completing Yaschas 1X and Sunleth 300
            }

            int wildsNeeded = GetWildsNeeded(available);

            if (available.Intersect(wilds).Count() < wildsNeeded)
            {
                return false;
            }
        }

        return true;
    }

    public int GetWildsNeeded(List<string> available)
    {
        return available.SelectMany(l =>
        gateData.Values.Where(g =>
          g.Location == l &&
          g.Traits.Contains("Wild") &&
          g.Requirements.Intersect(available).Count() == g.Requirements.Count &&
          GetMogLevel(available) >= g.MinMogLevel)
        ).Count();
    }

    private List<string> GetAvailableLocations(Dictionary<string, string> soFar)
    {
        List<string> list = new()
        {
            "start"
        };
        if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 0)
        {
            list.Add("h_hm_AD0003");
        }

        if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 1)
        {
            list.Add("h_bj_AD0005");
        }

        list.AddRange(soFar.Values);

        // Unlock Void after Ch 2
        if (list.Contains("h_gh_AD0010") && list.Contains("h_sn_AD0300") && list.Contains("h_gd_NA0000"))
        {
            list.Add("h_sp_NA0001");
        }

        // Unlock Serendipity after Yaschas 1X and Sunleth 300
        if (list.Contains("h_sn_AD0300") && list.Contains("h_gd_NA0000") && list.Contains("h_gh_AD0010") && list.Contains("h_cl_NA0000"))
        {
            list.Add("h_cs_NA0000");
        }

        // Unlock Dying World/Bodhum 700 after Academia 4XX and Graviton and Mog Level >= 1
        if (list.Contains("h_aa_AD0400") && HasGravitonLocations(list) && GetMogLevel(list) > 0)
        {
            list.Add("h_dd_AD0700");
            list.Add("h_hm_AD0700");
            list.Add("h_zz_NA0950");
        }

        return list.Distinct().ToList();
    }

    private bool TooSmallOfPool()
    {
        if (FF13_2Flags.Items.KeyPlaceTreasure.Enabled)
        {
            return false; // There's many treasures
        }

        int size = 0;
        if (FF13_2Flags.Items.KeyWild.Enabled)
        {
            size++;
        }

        if (FF13_2Flags.Items.KeyGraviton.Enabled)
        {
            size++;
        }

        if (FF13_2Flags.Items.KeySide.Enabled)
        {
            size++;
        }

        if (FF13_2Flags.Items.KeyGateSeal.Enabled)
        {
            size++;
        }
        // Academia 4XX can softlock without Brain Blast
        if (FF13_2Flags.Items.KeyPlaceBrainBlast.Enabled)
        {
            size++;
        }

        return size < 5;
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        HTMLPage page = new("Historia Crux", "template/documentation.html");

        BattleRando battleRando = Randomizers.Get<BattleRando>();

        Dictionary<string, int> diffs = battleRando.GetAreaDifficulties();

        page.HTMLElements.Add(new Table("", (new string[] { "Original Gate", "New Location", "Estimated Battle Difficulty of New Location" }).ToList(), (new int[] { 40, 40, 20 }).ToList(),
            gateData.Values.Where(g => !g.Traits.Contains("Paradox")).Select(g =>
          {
              string id = gateTable[g.ID].sOpenHistoria1_string;
              string shortID = id.Substring(0, id.Length - 2);
              return (new string[] { g.GateOriginal, areaData[shortID].Name, diffs.ContainsKey(shortID) ? diffs[shortID].ToString() : "-" }).ToList();
          }).ToList()));
        pages.Add("historia_crux", page);
        return pages;
    }

    public override void Save()
    {
        Randomizers.SetUIProgress("Saving Historia Crux Data...", 0, -1);
        gateTable.SaveDB3(@"\db\resident\_wdbpack.bin\r_gatetab.wdb");
        SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_gatetab.wdb");
    }
    public class GateData : CSVDataRow
    {
        [RowIndex(0)]
        public string Location { get; set; }
        [RowIndex(1)]
        public string ID { get; set; }
        [RowIndex(2)]
        public List<string> Traits { get; set; }
        [RowIndex(3)]
        public List<string> Requirements { get; set; }
        [RowIndex(4)]
        public int MinMogLevel { get; set; }
        [RowIndex(5)]
        public string GateOriginal { get; set; }
        [RowIndex(6)]
        public ItemReq ItemRequirements { get; set; }
        public GateData(string[] row) : base(row)
        {
        }
    }
    public class AreaData : CSVDataRow
    {
        [RowIndex(0)]
        public string ID { get; set; }
        [RowIndex(1)]
        public string Name { get; set; }
        [RowIndex(2)]
        public string BattleTableID { get; set; }
        public AreaData(string[] row) : base(row)
        {
        }
    }
}

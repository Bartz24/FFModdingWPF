using Bartz24.Docs;
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
    public class HistoriaCruxRando : Randomizer
    {
        public DataStoreDB3<DataStoreRGateTable> gateTable = new DataStoreDB3<DataStoreRGateTable>();
        public DataStoreDB3<DataStoreRGateTable> gateTableOrig = new DataStoreDB3<DataStoreRGateTable>();

        public Dictionary<string, GateData> gateData = new Dictionary<string, GateData>();
        public Dictionary<string, AreaData> areaData = new Dictionary<string, AreaData>();

        public Dictionary<string, string> placement = new Dictionary<string, string>();

        public HistoriaCruxRando(RandomizerManager randomizers) : base(randomizers) {  }

        public override string GetProgressMessage()
        {
            return "Randomizing Historia Crux...";
        }
        public override string GetID()
        {
            return "Historia Crux";
        }

        public override void Load()
        {
            gateTable.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_gatetab.wdb", false);
            gateTableOrig.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_gatetab.wdb", false);

            gateData.Clear();
            using (CsvParser csv = new CsvParser(new StreamReader(@"data\historia.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
            {
                while (csv.Read())
                {
                    GateData t = new GateData(csv.Record);
                    gateData.Add(t.ID, t);
                }
            }
            areaData.Clear();
            using (CsvParser csv = new CsvParser(new StreamReader(@"data\areas.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
            {
                while (csv.Read())
                {
                    AreaData a = new AreaData(csv.Record);
                    areaData.Add(a.ID, a);
                }
            }
        }
        public override void Randomize(Action<int> progressSetter)
        {
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
                    openings = openings.Where(o => o != "h_hm_AD0003").ToList();
                if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 1)
                    openings = openings.Where(o => o != "h_bj_AD0005").ToList();

                placement = GetPlacement(new Dictionary<string, string>(), openings).Item2;

                placement.Keys.ToList().ForEach(open =>
                {
                    gateTable.Keys.Where(id => gateTableOrig[id].sOpenHistoria1_string.StartsWith(open)).ToList().ForEach(id => gateTable[id].sOpenHistoria1_string = placement[open] + "_a");
                });

                if (placement.ContainsKey("h_hm_AD0003"))
                    gateTable["hs_hmaa10_zz"].sArea_string = placement["h_hm_AD0003"];

                RandomNum.ClearRand();
            }
        }

        private Tuple<bool, Dictionary<string, string>> GetPlacement(Dictionary<string, string> soFar, List<string> openings)
        {
            List<string> available = GetAvailableLocations(soFar);
            List<string> remaining = openings.Where(t => !soFar.ContainsValue(t)).ToList().Shuffle().ToList();

            foreach (string rep in remaining)
            {
                List<string> possible = openings.Where(o => !soFar.ContainsKey(o) && IsAllowed(o, soFar, available)).ToList().Shuffle().ToList();
                if (possible.Count == 0)
                    return new Tuple<bool, Dictionary<string, string>>(false, soFar);
            }

            foreach (string rep in remaining)
            {
                List<string> possible = openings.Where(o => !soFar.ContainsKey(o) && IsAllowed(o, soFar, available)).ToList();
                while (possible.Count > 0)
                {
                    string next = SelectNext(possible);
                    soFar.Add(next, rep);
                    if (soFar.Count == openings.Count)
                        return new Tuple<bool, Dictionary<string, string>>(true, soFar);
                    Tuple<bool, Dictionary<string, string>> result = GetPlacement(soFar, openings);
                    if (result.Item1)
                        return result;
                    else
                    {
                        possible.Remove(next);
                        soFar.Remove(next);
                    }
                }
            }
            return new Tuple<bool, Dictionary<string, string>>(false, soFar);
        }

        private string SelectNext(List<string> possible)
        {
            return possible[RandomNum.RandInt(0, possible.Count - 1)];
        }

        public List<string> GetIDsForOpening(string open)
        {
            return gateData.Keys.Where(id => gateTableOrig[id].sOpenHistoria1_string.StartsWith(open)).ToList();
        }

        private bool IsAllowed(string open, Dictionary<string, string> soFar, List<string> available)
        {
            foreach (string id in GetIDsForOpening(open))
            {
                if (!available.Contains(gateData[id].Location))
                    return false;
                if (available.Intersect(gateData[id].Requirements).Count() != gateData[id].Requirements.Count)
                    return false;
                if (gateData[id].Traits.Contains("Graviton") && !HasGravitonLocations(available))
                    return false;
                if (gateData[id].Traits.Contains("Wild") && !HasWildArtefacts(soFar, available))
                    return false;
                if (gateData[id].MinMogLevel > GetMogLevel(available))
                    return false;
                // Hard code for Bresha 5 wild artefact if key items aren't rando
                if (!FF13_2Flags.Items.Treasures.FlagEnabled || !FF13_2Flags.Items.KeySide.Enabled || TooSmallOfPool())
                {
                    if (gateData[id].ItemRequirements.GetPossibleRequirements().Contains("key_lockjail") && 2 > GetMogLevel(available))
                        return false;
                }
            }
            return true;
        }

        public int GetMogLevel(List<string> available)
        {
            if (available.Contains("h_dd_AD0700"))
                return 3;
            if (available.Contains("h_sn_AD0300"))
                return 2;
            if (available.Contains("h_bj_AD0005"))
                return 1;
            return 0;
        }

        private bool HasGravitonLocations(List<string> available)
        {
            if (!FF13_2Flags.Items.Treasures.FlagEnabled || !FF13_2Flags.Items.KeyGraviton.Enabled || TooSmallOfPool())
            {
                // If graviton cores aren't rando, use normal logic
                List<string> gravitons = new List<string>();
                if (GetMogLevel(available) >= 1)
                    gravitons.Add("h_hm_AD0003"); // Bodhum 3. requires moogle hunt
                if (GetMogLevel(available) >= 1)
                    gravitons.Add("h_bj_AD0005"); // Bresha 5. requires moogle hunt
                if (GetMogLevel(available) >= 1)
                    gravitons.Add("h_gw_AD0200"); // Oerba 200. requires moogle hunt
                if (GetMogLevel(available) >= 1)
                    gravitons.Add("h_ac_AD0400"); // Academia 400. requires moogle hunt
                if (GetMogLevel(available) >= 1)
                    gravitons.Add("h_gy_AD0100"); // Yaschas 100. requires moogle hunt
                if (GetMogLevel(available) >= 1)
                    gravitons.Add("h_gw_AD0400"); // Oerba 400. requires moogle hunt
                if (GetMogLevel(available) >= 1)
                    gravitons.Add("h_sn_AD0400"); // Sunleth 400. requires moogle hunt
                if (available.Intersect(gravitons).Count() < 5)
                    return false;
            }
            return true;
        }

        private bool HasWildArtefacts(Dictionary<string, string> soFar, List<string> available)
        {
            if (!FF13_2Flags.Items.Treasures.FlagEnabled || !FF13_2Flags.Items.KeyWild.Enabled || TooSmallOfPool())
            {
                // If wild artefacts aren't rando, use normal logic
                List<string> wilds = new List<string>();
                if (GetMogLevel(available) >= 1)
                    wilds.Add("h_bj_AD0005"); // Bresha 5. requires moogle hunt
                if (GetMogLevel(available) >= 1)
                    wilds.Add("h_bj_AD0300"); // Bresha 300. requires moogle hunt
                if (GetMogLevel(available) >= 2)
                    wilds.Add("h_gw_AD0200"); // Oerba 200. requires moogle throw
                if (GetMogLevel(available) >= 2)
                    wilds.Add("h_sn_AD0300"); // Sunleth 300. requires moogle throw
                if (GetMogLevel(available) >= 2)
                    wilds.Add("h_gd_NA0000"); // Archylte. requires moogle throw
                wilds.Add("h_gt_AD0200"); // Augusta 200
                if (GetMogLevel(available) >= 1)
                    wilds.Add("h_aa_AD0400"); // Academia 4XX. requires moogle hunt
                if (GetMogLevel(available) >= 2)
                    wilds.Add("h_gy_AD0100"); // Yaschas 100. requires moogle hunt and throw
                if (GetMogLevel(available) >= 1)
                    wilds.Add("h_dd_AD0700"); // Dying World 700. requires moogle hunt
                if (available.Contains("h_sn_AD0300") && available.Contains("h_gd_NA0000") && available.Contains("h_gh_AD0010") && available.Contains("h_cl_NA0000"))
                    wilds.Add("h_cs_NA0000"); // Serendipity. requires completing Yaschas 1X and Sunleth 300
                int wildsNeeded = GetWildsNeeded(available);

                if (available.Intersect(wilds).Count() < wildsNeeded)
                    return false;
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
            List<string> list = new List<string>();
            list.Add("start");
            if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 0)
                list.Add("h_hm_AD0003");
            if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 1)
                list.Add("h_bj_AD0005");

            list.AddRange(soFar.Values);

            // Unlock Void after Ch 2
            if (list.Contains("h_gh_AD0010") && list.Contains("h_sn_AD0300") && list.Contains("h_gd_NA0000"))
                list.Add("h_sp_NA0001");

            // Unlock Serendipity after Yaschas 1X and Sunleth 300
            if (list.Contains("h_sn_AD0300") && list.Contains("h_gd_NA0000") && list.Contains("h_gh_AD0010") && list.Contains("h_cl_NA0000"))
                list.Add("h_cs_NA0000");

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
                return false; // There's many treasures
            int size = 0;
            if (FF13_2Flags.Items.KeyWild.Enabled)
                size++;
            if (FF13_2Flags.Items.KeyGraviton.Enabled)
                size++;
            if (FF13_2Flags.Items.KeySide.Enabled)
                size++;
            if (FF13_2Flags.Items.KeyGateSeal.Enabled)
                size++;
            // Academia 4XX can softlock without Brain Blast
            if (FF13_2Flags.Items.KeyPlaceBrainBlast.Enabled)
                size++;

            return size < 5;
        }
        
        public override HTMLPage GetDocumentation()
        {
            HTMLPage page = new HTMLPage("Historia Crux", "template/documentation.html");

            page.HTMLElements.Add(new Table("", (new string[] { "Original Gate", "New Location" }).ToList(), (new int[] { 60, 40 }).ToList(),
                gateData.Values.Where(g => !g.Traits.Contains("Paradox")).Select(g =>
              {
                  string id = gateTable[g.ID].sOpenHistoria1_string;
                  return new string[] { g.GateOriginal, areaData[id.Substring(0, id.Length - 2)].Name }.ToList();
              }).ToList()));
            return page;
        }

        public override void Save()
        {
            gateTable.SaveDB3(@"\db\resident\_wdbpack.bin\r_gatetab.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_gatetab.wdb");
        }
        public class GateData
        {
            public string Location { get; set; }
            public string ID { get; set; }
            public List<string> Traits { get; set; }
            public List<string> Requirements { get; set; }
            public int MinMogLevel { get; set; }
            public string GateOriginal { get; set; }
            public ItemReq ItemRequirements { get; set; }
            public GateData(string[] row)
            {
                Location = row[0];
                ID = row[1];
                Traits = row[2].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                Requirements = row[3].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                MinMogLevel = int.Parse(row[4]);
                GateOriginal = row[5];
                ItemRequirements = ItemReq.Parse(row[6]);
            }
        }
        public class AreaData
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public AreaData(string[] row)
            {
                ID = row[0];
                Name = row[1];
            }
        }
    }
}

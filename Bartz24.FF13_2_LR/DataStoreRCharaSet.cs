using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.FF13_2_LR;

public class DataStoreRCharaSet : DataStoreWDBEntry
{
    public int iMemorySizeLimit { get; set; }
    public int iVideoMemorySizeLimit { get; set; }
    public string sCharaSpecId0 { get; set; }
    public string sCharaSpecId1 { get; set; }
    public string sCharaSpecId2 { get; set; }
    public string sCharaSpecId3 { get; set; }
    public string sCharaSpecId4 { get; set; }
    public string sCharaSpecId5 { get; set; }
    public string sCharaSpecId6 { get; set; }
    public string sCharaSpecId7 { get; set; }
    public string sCharaSpecId8 { get; set; }
    public string sCharaSpecId9 { get; set; }
    public string sCharaSpecId10 { get; set; }
    public string sCharaSpecId11 { get; set; }
    public string sCharaSpecId12 { get; set; }
    public string sCharaSpecId13 { get; set; }
    public string sCharaSpecId14 { get; set; }
    public string sCharaSpecId15 { get; set; }
    public string sCharaSpecId16 { get; set; }
    public string sCharaSpecId17 { get; set; }
    public string sCharaSpecId18 { get; set; }
    public string sCharaSpecId19 { get; set; }
    public string sCharaSpecId20 { get; set; }
    public string sCharaSpecId21 { get; set; }
    public string sCharaSpecId22 { get; set; }
    public string sCharaSpecId23 { get; set; }
    public string sCharaSpecId24 { get; set; }
    public string sCharaSpecId25 { get; set; }
    public string sCharaSpecId26 { get; set; }
    public string sCharaSpecId27 { get; set; }
    public string sCharaSpecId28 { get; set; }
    public string sCharaSpecId29 { get; set; }
    public string sCharaSpecId30 { get; set; }
    public string sCharaSpecId31 { get; set; }
    public string sCharaSpecId32 { get; set; }
    public string sCharaSpecId33 { get; set; }
    public string sCharaSpecId34 { get; set; }
    public string sCharaSpecId35 { get; set; }
    public string sCharaSpecId36 { get; set; }
    public string sCharaSpecId37 { get; set; }
    public string sCharaSpecId38 { get; set; }
    public string sCharaSpecId39 { get; set; }
    public string sCharaSpecId40 { get; set; }
    public string sCharaSpecId41 { get; set; }
    public string sCharaSpecId42 { get; set; }
    public string sCharaSpecId43 { get; set; }
    public string sCharaSpecId44 { get; set; }
    public string sCharaSpecId45 { get; set; }
    public string sCharaSpecId46 { get; set; }
    public string sCharaSpecId47 { get; set; }
    public string sCharaSpecId48 { get; set; }
    public string sCharaSpecId49 { get; set; }
    public string sCharaSpecId50 { get; set; }
    public string sCharaSpecId51 { get; set; }
    public string sCharaSpecId52 { get; set; }
    public string sCharaSpecId53 { get; set; }
    public string sCharaSpecId54 { get; set; }
    public string sCharaSpecId55 { get; set; }
    public string sCharaSpecId56 { get; set; }
    public string sCharaSpecId57 { get; set; }
        public int u1PartyLoadRequestIndex0 { get; set; }
    public int u1PartyLoadRequestIndex1 { get; set; }
    public int u1PartyLoadRequestIndex2 { get; set; }
    public int u1PartyLoadRequestIndex3 { get; set; }
    public int u1PartyLoadRequestIndex4 { get; set; }
    public int u1PartyLoadRequestIndex5 { get; set; }

    public List<string> CharaSpecs
    {
        get
        {
            List<string> list = new();
            for (int i = 0; i < 58; i++)
            {
                list.Add(this.GetPropValue<string>($"sCharaSpecId{i}"));
            }

            return list.Where(s => s != "").ToList();
        }
        set
        {
            if (value.Count > 58)
            {
                throw new Exception("Too many Chara Specs being added to " + record);
            }

            for (int i = 0; i < 58; i++)
            {
                if (i < value.Count)
                {
                    this.SetPropValue($"sCharaSpecId{i}", value[i]);
                }
                else
                {
                    this.SetPropValue($"sCharaSpecId{i}", "");
                }
            }
        }
    }
}

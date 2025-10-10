using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.FF13;

public class DataStoreCharaSet : DataStoreWDBEntry
{
    public uint iMemorySizeLimit
    {
        get => Data.ReadUInt(0x0);
        set => Data.SetUInt(0x0, value);
    }
    public uint iVideoMemorySizeLimit
    {
        get => Data.ReadUInt(0x4);
        set => Data.SetUInt(0x4, value);
    }
    public uint sCharaSpecId0_pointer
    {
        get => Data.ReadUInt(0x8 + (0 * 4));
        set => Data.SetUInt(0x8 + (0 * 4), value);
    }
    public string sCharaSpecId0 { get; set; }
    public uint sCharaSpecId1_pointer
    {
        get => Data.ReadUInt(0x8 + (1 * 4));
        set => Data.SetUInt(0x8 + (1 * 4), value);
    }
    public string sCharaSpecId1 { get; set; }
    public uint sCharaSpecId2_pointer
    {
        get => Data.ReadUInt(0x8 + (2 * 4));
        set => Data.SetUInt(0x8 + (2 * 4), value);
    }
    public string sCharaSpecId2 { get; set; }
    public uint sCharaSpecId3_pointer
    {
        get => Data.ReadUInt(0x8 + (3 * 4));
        set => Data.SetUInt(0x8 + (3 * 4), value);
    }
    public string sCharaSpecId3 { get; set; }
    public uint sCharaSpecId4_pointer
    {
        get => Data.ReadUInt(0x8 + (4 * 4));
        set => Data.SetUInt(0x8 + (4 * 4), value);
    }
    public string sCharaSpecId4 { get; set; }
    public uint sCharaSpecId5_pointer
    {
        get => Data.ReadUInt(0x8 + (5 * 4));
        set => Data.SetUInt(0x8 + (5 * 4), value);
    }
    public string sCharaSpecId5 { get; set; }
    public uint sCharaSpecId6_pointer
    {
        get => Data.ReadUInt(0x8 + (6 * 4));
        set => Data.SetUInt(0x8 + (6 * 4), value);
    }
    public string sCharaSpecId6 { get; set; }
    public uint sCharaSpecId7_pointer
    {
        get => Data.ReadUInt(0x8 + (7 * 4));
        set => Data.SetUInt(0x8 + (7 * 4), value);
    }
    public string sCharaSpecId7 { get; set; }
    public uint sCharaSpecId8_pointer
    {
        get => Data.ReadUInt(0x8 + (8 * 4));
        set => Data.SetUInt(0x8 + (8 * 4), value);
    }
    public string sCharaSpecId8 { get; set; }
    public uint sCharaSpecId9_pointer
    {
        get => Data.ReadUInt(0x8 + (9 * 4));
        set => Data.SetUInt(0x8 + (9 * 4), value);
    }
    public string sCharaSpecId9 { get; set; }
    public uint sCharaSpecId10_pointer
    {
        get => Data.ReadUInt(0x8 + (10 * 4));
        set => Data.SetUInt(0x8 + (10 * 4), value);
    }
    public string sCharaSpecId10 { get; set; }
    public uint sCharaSpecId11_pointer
    {
        get => Data.ReadUInt(0x8 + (11 * 4));
        set => Data.SetUInt(0x8 + (11 * 4), value);
    }
    public string sCharaSpecId11 { get; set; }
    public uint sCharaSpecId12_pointer
    {
        get => Data.ReadUInt(0x8 + (12 * 4));
        set => Data.SetUInt(0x8 + (12 * 4), value);
    }
    public string sCharaSpecId12 { get; set; }
    public uint sCharaSpecId13_pointer
    {
        get => Data.ReadUInt(0x8 + (13 * 4));
        set => Data.SetUInt(0x8 + (13 * 4), value);
    }
    public string sCharaSpecId13 { get; set; }
    public uint sCharaSpecId14_pointer
    {
        get => Data.ReadUInt(0x8 + (14 * 4));
        set => Data.SetUInt(0x8 + (14 * 4), value);
    }
    public string sCharaSpecId14 { get; set; }
    public uint sCharaSpecId15_pointer
    {
        get => Data.ReadUInt(0x8 + (15 * 4));
        set => Data.SetUInt(0x8 + (15 * 4), value);
    }
    public string sCharaSpecId15 { get; set; }
    public uint sCharaSpecId16_pointer
    {
        get => Data.ReadUInt(0x8 + (16 * 4));
        set => Data.SetUInt(0x8 + (16 * 4), value);
    }
    public string sCharaSpecId16 { get; set; }
    public uint sCharaSpecId17_pointer
    {
        get => Data.ReadUInt(0x8 + (17 * 4));
        set => Data.SetUInt(0x8 + (17 * 4), value);
    }
    public string sCharaSpecId17 { get; set; }
    public uint sCharaSpecId18_pointer
    {
        get => Data.ReadUInt(0x8 + (18 * 4));
        set => Data.SetUInt(0x8 + (18 * 4), value);
    }
    public string sCharaSpecId18 { get; set; }
    public uint sCharaSpecId19_pointer
    {
        get => Data.ReadUInt(0x8 + (19 * 4));
        set => Data.SetUInt(0x8 + (19 * 4), value);
    }
    public string sCharaSpecId19 { get; set; }
    public uint sCharaSpecId20_pointer
    {
        get => Data.ReadUInt(0x8 + (20 * 4));
        set => Data.SetUInt(0x8 + (20 * 4), value);
    }
    public string sCharaSpecId20 { get; set; }
    public uint sCharaSpecId21_pointer
    {
        get => Data.ReadUInt(0x8 + (21 * 4));
        set => Data.SetUInt(0x8 + (21 * 4), value);
    }
    public string sCharaSpecId21 { get; set; }
    public uint sCharaSpecId22_pointer
    {
        get => Data.ReadUInt(0x8 + (22 * 4));
        set => Data.SetUInt(0x8 + (22 * 4), value);
    }
    public string sCharaSpecId22 { get; set; }
    public uint sCharaSpecId23_pointer
    {
        get => Data.ReadUInt(0x8 + (23 * 4));
        set => Data.SetUInt(0x8 + (23 * 4), value);
    }
    public string sCharaSpecId23 { get; set; }
    public uint sCharaSpecId24_pointer
    {
        get => Data.ReadUInt(0x8 + (24 * 4));
        set => Data.SetUInt(0x8 + (24 * 4), value);
    }
    public string sCharaSpecId24 { get; set; }
    public uint sCharaSpecId25_pointer
    {
        get => Data.ReadUInt(0x8 + (25 * 4));
        set => Data.SetUInt(0x8 + (25 * 4), value);
    }
    public string sCharaSpecId25 { get; set; }
    public uint sCharaSpecId26_pointer
    {
        get => Data.ReadUInt(0x8 + (26 * 4));
        set => Data.SetUInt(0x8 + (26 * 4), value);
    }
    public string sCharaSpecId26 { get; set; }
    public uint sCharaSpecId27_pointer
    {
        get => Data.ReadUInt(0x8 + (27 * 4));
        set => Data.SetUInt(0x8 + (27 * 4), value);
    }
    public string sCharaSpecId27 { get; set; }
    public uint sCharaSpecId28_pointer
    {
        get => Data.ReadUInt(0x8 + (28 * 4));
        set => Data.SetUInt(0x8 + (28 * 4), value);
    }
    public string sCharaSpecId28 { get; set; }
    public uint sCharaSpecId29_pointer
    {
        get => Data.ReadUInt(0x8 + (29 * 4));
        set => Data.SetUInt(0x8 + (29 * 4), value);
    }
    public string sCharaSpecId29 { get; set; }
    public uint sCharaSpecId30_pointer
    {
        get => Data.ReadUInt(0x8 + (30 * 4));
        set => Data.SetUInt(0x8 + (30 * 4), value);
    }
    public string sCharaSpecId30 { get; set; }
    public uint sCharaSpecId31_pointer
    {
        get => Data.ReadUInt(0x8 + (31 * 4));
        set => Data.SetUInt(0x8 + (31 * 4), value);
    }
    public string sCharaSpecId31 { get; set; }
    public uint sCharaSpecId32_pointer
    {
        get => Data.ReadUInt(0x8 + (32 * 4));
        set => Data.SetUInt(0x8 + (32 * 4), value);
    }
    public string sCharaSpecId32 { get; set; }
    public uint sCharaSpecId33_pointer
    {
        get => Data.ReadUInt(0x8 + (33 * 4));
        set => Data.SetUInt(0x8 + (33 * 4), value);
    }
    public string sCharaSpecId33 { get; set; }
    public uint sCharaSpecId34_pointer
    {
        get => Data.ReadUInt(0x8 + (34 * 4));
        set => Data.SetUInt(0x8 + (34 * 4), value);
    }
    public string sCharaSpecId34 { get; set; }
    public uint sCharaSpecId35_pointer
    {
        get => Data.ReadUInt(0x8 + (35 * 4));
        set => Data.SetUInt(0x8 + (35 * 4), value);
    }
    public string sCharaSpecId35 { get; set; }
    public uint sCharaSpecId36_pointer
    {
        get => Data.ReadUInt(0x8 + (36 * 4));
        set => Data.SetUInt(0x8 + (36 * 4), value);
    }
    public string sCharaSpecId36 { get; set; }
    public uint sCharaSpecId37_pointer
    {
        get => Data.ReadUInt(0x8 + (37 * 4));
        set => Data.SetUInt(0x8 + (37 * 4), value);
    }
    public string sCharaSpecId37 { get; set; }
    public uint sCharaSpecId38_pointer
    {
        get => Data.ReadUInt(0x8 + (38 * 4));
        set => Data.SetUInt(0x8 + (38 * 4), value);
    }
    public string sCharaSpecId38 { get; set; }
    public uint sCharaSpecId39_pointer
    {
        get => Data.ReadUInt(0x8 + (39 * 4));
        set => Data.SetUInt(0x8 + (39 * 4), value);
    }
    public string sCharaSpecId39 { get; set; }
    public uint sCharaSpecId40_pointer
    {
        get => Data.ReadUInt(0x8 + (40 * 4));
        set => Data.SetUInt(0x8 + (40 * 4), value);
    }
    public string sCharaSpecId40 { get; set; }
    public uint sCharaSpecId41_pointer
    {
        get => Data.ReadUInt(0x8 + (41 * 4));
        set => Data.SetUInt(0x8 + (41 * 4), value);
    }
    public string sCharaSpecId41 { get; set; }
    public uint sCharaSpecId42_pointer
    {
        get => Data.ReadUInt(0x8 + (42 * 4));
        set => Data.SetUInt(0x8 + (42 * 4), value);
    }
    public string sCharaSpecId42 { get; set; }
    public uint sCharaSpecId43_pointer
    {
        get => Data.ReadUInt(0x8 + (43 * 4));
        set => Data.SetUInt(0x8 + (43 * 4), value);
    }
    public string sCharaSpecId43 { get; set; }
    public uint sCharaSpecId44_pointer
    {
        get => Data.ReadUInt(0x8 + (44 * 4));
        set => Data.SetUInt(0x8 + (44 * 4), value);
    }
    public string sCharaSpecId44 { get; set; }
    public uint sCharaSpecId45_pointer
    {
        get => Data.ReadUInt(0x8 + (45 * 4));
        set => Data.SetUInt(0x8 + (45 * 4), value);
    }
    public string sCharaSpecId45 { get; set; }
    public uint sCharaSpecId46_pointer
    {
        get => Data.ReadUInt(0x8 + (46 * 4));
        set => Data.SetUInt(0x8 + (46 * 4), value);
    }
    public string sCharaSpecId46 { get; set; }
    public uint sCharaSpecId47_pointer
    {
        get => Data.ReadUInt(0x8 + (47 * 4));
        set => Data.SetUInt(0x8 + (47 * 4), value);
    }
    public string sCharaSpecId47 { get; set; }
    public uint sCharaSpecId48_pointer
    {
        get => Data.ReadUInt(0x8 + (48 * 4));
        set => Data.SetUInt(0x8 + (48 * 4), value);
    }
    public string sCharaSpecId48 { get; set; }
    public uint sCharaSpecId49_pointer
    {
        get => Data.ReadUInt(0x8 + (49 * 4));
        set => Data.SetUInt(0x8 + (49 * 4), value);
    }
    public string sCharaSpecId49 { get; set; }
    public uint sCharaSpecId50_pointer
    {
        get => Data.ReadUInt(0x8 + (50 * 4));
        set => Data.SetUInt(0x8 + (50 * 4), value);
    }
    public string sCharaSpecId50 { get; set; }
    public uint sCharaSpecId51_pointer
    {
        get => Data.ReadUInt(0x8 + (51 * 4));
        set => Data.SetUInt(0x8 + (51 * 4), value);
    }
    public string sCharaSpecId51 { get; set; }
    public uint sCharaSpecId52_pointer
    {
        get => Data.ReadUInt(0x8 + (52 * 4));
        set => Data.SetUInt(0x8 + (52 * 4), value);
    }
    public string sCharaSpecId52 { get; set; }
    public uint sCharaSpecId53_pointer
    {
        get => Data.ReadUInt(0x8 + (53 * 4));
        set => Data.SetUInt(0x8 + (53 * 4), value);
    }
    public string sCharaSpecId53 { get; set; }
    public uint sCharaSpecId54_pointer
    {
        get => Data.ReadUInt(0x8 + (54 * 4));
        set => Data.SetUInt(0x8 + (54 * 4), value);
    }
    public string sCharaSpecId54 { get; set; }
    public uint sCharaSpecId55_pointer
    {
        get => Data.ReadUInt(0x8 + (55 * 4));
        set => Data.SetUInt(0x8 + (55 * 4), value);
    }
    public string sCharaSpecId55 { get; set; }
    public uint sCharaSpecId56_pointer
    {
        get => Data.ReadUInt(0x8 + (56 * 4));
        set => Data.SetUInt(0x8 + (56 * 4), value);
    }
    public string sCharaSpecId56 { get; set; }
    public uint sCharaSpecId57_pointer
    {
        get => Data.ReadUInt(0x8 + (57 * 4));
        set => Data.SetUInt(0x8 + (57 * 4), value);
    }
    public string sCharaSpecId57 { get; set; }
    public uint sCharaSpecId58_pointer
    {
        get => Data.ReadUInt(0x8 + (58 * 4));
        set => Data.SetUInt(0x8 + (58 * 4), value);
    }
    public string sCharaSpecId58 { get; set; }
    public uint sCharaSpecId59_pointer
    {
        get => Data.ReadUInt(0x8 + (59 * 4));
        set => Data.SetUInt(0x8 + (59 * 4), value);
    }
    public string sCharaSpecId59 { get; set; }
    public uint sCharaSpecId60_pointer
    {
        get => Data.ReadUInt(0x8 + (60 * 4));
        set => Data.SetUInt(0x8 + (60 * 4), value);
    }
    public string sCharaSpecId60 { get; set; }
    public uint sCharaSpecId61_pointer
    {
        get => Data.ReadUInt(0x8 + (61 * 4));
        set => Data.SetUInt(0x8 + (61 * 4), value);
    }
    public string sCharaSpecId61 { get; set; }
    public uint sCharaSpecId62_pointer
    {
        get => Data.ReadUInt(0x8 + (62 * 4));
        set => Data.SetUInt(0x8 + (62 * 4), value);
    }
    public string sCharaSpecId62 { get; set; }
    public uint sCharaSpecId63_pointer
    {
        get => Data.ReadUInt(0x8 + (63 * 4));
        set => Data.SetUInt(0x8 + (63 * 4), value);
    }
    public string sCharaSpecId63 { get; set; }

    public override int GetDefaultLength()
    {
        return 0x10C;
    }

    public void SetCharaSpecs(List<string> list)
    {
        if (list.Count > 64)
        {
            throw new Exception("Too many Chara Specs being added");
        }

        for (int i = 0; i < 64; i++)
        {
            if (i < list.Count)
            {
                this.SetPropValue($"sCharaSpecId{i}", list[i]);
            }
            else
            {
                this.SetPropValue($"sCharaSpecId{i}", "");
            }
        }
    }

    public List<string> GetCharaSpecs()
    {
        List<string> list = new();
        for (int i = 0; i < 64; i++)
        {
            list.Add(this.GetPropValue<string>($"sCharaSpecId{i}"));
        }

        return list.Where(s => s != "").ToList();
    }
}
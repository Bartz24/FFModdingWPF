using Bartz24.Data;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.FF13;

public class DataStoreShop : DataStoreWDBEntry
{
    public uint sItemLabel1_pointer
    {
        get => Data.ReadUInt(0x18 + (0 * 4));
        set => Data.SetUInt(0x18 + (0 * 4), value);
    }
    public string sItemLabel1_string { get; set; }
    public uint sItemLabel2_pointer
    {
        get => Data.ReadUInt(0x18 + (1 * 4));
        set => Data.SetUInt(0x18 + (1 * 4), value);
    }
    public string sItemLabel2_string { get; set; }
    public uint sItemLabel3_pointer
    {
        get => Data.ReadUInt(0x18 + (2 * 4));
        set => Data.SetUInt(0x18 + (2 * 4), value);
    }
    public string sItemLabel3_string { get; set; }
    public uint sItemLabel4_pointer
    {
        get => Data.ReadUInt(0x18 + (3 * 4));
        set => Data.SetUInt(0x18 + (3 * 4), value);
    }
    public string sItemLabel4_string { get; set; }
    public uint sItemLabel5_pointer
    {
        get => Data.ReadUInt(0x18 + (4 * 4));
        set => Data.SetUInt(0x18 + (4 * 4), value);
    }
    public string sItemLabel5_string { get; set; }
    public uint sItemLabel6_pointer
    {
        get => Data.ReadUInt(0x18 + (5 * 4));
        set => Data.SetUInt(0x18 + (5 * 4), value);
    }
    public string sItemLabel6_string { get; set; }
    public uint sItemLabel7_pointer
    {
        get => Data.ReadUInt(0x18 + (6 * 4));
        set => Data.SetUInt(0x18 + (6 * 4), value);
    }
    public string sItemLabel7_string { get; set; }
    public uint sItemLabel8_pointer
    {
        get => Data.ReadUInt(0x18 + (7 * 4));
        set => Data.SetUInt(0x18 + (7 * 4), value);
    }
    public string sItemLabel8_string { get; set; }
    public uint sItemLabel9_pointer
    {
        get => Data.ReadUInt(0x18 + (8 * 4));
        set => Data.SetUInt(0x18 + (8 * 4), value);
    }
    public string sItemLabel9_string { get; set; }
    public uint sItemLabel10_pointer
    {
        get => Data.ReadUInt(0x18 + (9 * 4));
        set => Data.SetUInt(0x18 + (9 * 4), value);
    }
    public string sItemLabel10_string { get; set; }
    public uint sItemLabel11_pointer
    {
        get => Data.ReadUInt(0x18 + (10 * 4));
        set => Data.SetUInt(0x18 + (10 * 4), value);
    }
    public string sItemLabel11_string { get; set; }
    public uint sItemLabel12_pointer
    {
        get => Data.ReadUInt(0x18 + (11 * 4));
        set => Data.SetUInt(0x18 + (11 * 4), value);
    }
    public string sItemLabel12_string { get; set; }
    public uint sItemLabel13_pointer
    {
        get => Data.ReadUInt(0x18 + (12 * 4));
        set => Data.SetUInt(0x18 + (12 * 4), value);
    }
    public string sItemLabel13_string { get; set; }
    public uint sItemLabel14_pointer
    {
        get => Data.ReadUInt(0x18 + (13 * 4));
        set => Data.SetUInt(0x18 + (13 * 4), value);
    }
    public string sItemLabel14_string { get; set; }
    public uint sItemLabel15_pointer
    {
        get => Data.ReadUInt(0x18 + (14 * 4));
        set => Data.SetUInt(0x18 + (14 * 4), value);
    }
    public string sItemLabel15_string { get; set; }
    public uint sItemLabel16_pointer
    {
        get => Data.ReadUInt(0x18 + (15 * 4));
        set => Data.SetUInt(0x18 + (15 * 4), value);
    }
    public string sItemLabel16_string { get; set; }
    public uint sItemLabel17_pointer
    {
        get => Data.ReadUInt(0x18 + (16 * 4));
        set => Data.SetUInt(0x18 + (16 * 4), value);
    }
    public string sItemLabel17_string { get; set; }
    public uint sItemLabel18_pointer
    {
        get => Data.ReadUInt(0x18 + (17 * 4));
        set => Data.SetUInt(0x18 + (17 * 4), value);
    }
    public string sItemLabel18_string { get; set; }
    public uint sItemLabel19_pointer
    {
        get => Data.ReadUInt(0x18 + (18 * 4));
        set => Data.SetUInt(0x18 + (18 * 4), value);
    }
    public string sItemLabel19_string { get; set; }
    public uint sItemLabel20_pointer
    {
        get => Data.ReadUInt(0x18 + (19 * 4));
        set => Data.SetUInt(0x18 + (19 * 4), value);
    }
    public string sItemLabel20_string { get; set; }
    public uint sItemLabel21_pointer
    {
        get => Data.ReadUInt(0x18 + (20 * 4));
        set => Data.SetUInt(0x18 + (20 * 4), value);
    }
    public string sItemLabel21_string { get; set; }
    public uint sItemLabel22_pointer
    {
        get => Data.ReadUInt(0x18 + (21 * 4));
        set => Data.SetUInt(0x18 + (21 * 4), value);
    }
    public string sItemLabel22_string { get; set; }
    public uint sItemLabel23_pointer
    {
        get => Data.ReadUInt(0x18 + (22 * 4));
        set => Data.SetUInt(0x18 + (22 * 4), value);
    }
    public string sItemLabel23_string { get; set; }
    public uint sItemLabel24_pointer
    {
        get => Data.ReadUInt(0x18 + (23 * 4));
        set => Data.SetUInt(0x18 + (23 * 4), value);
    }
    public string sItemLabel24_string { get; set; }
    public uint sItemLabel25_pointer
    {
        get => Data.ReadUInt(0x18 + (24 * 4));
        set => Data.SetUInt(0x18 + (24 * 4), value);
    }
    public string sItemLabel25_string { get; set; }
    public uint sItemLabel26_pointer
    {
        get => Data.ReadUInt(0x18 + (25 * 4));
        set => Data.SetUInt(0x18 + (25 * 4), value);
    }
    public string sItemLabel26_string { get; set; }
    public uint sItemLabel27_pointer
    {
        get => Data.ReadUInt(0x18 + (26 * 4));
        set => Data.SetUInt(0x18 + (26 * 4), value);
    }
    public string sItemLabel27_string { get; set; }
    public uint sItemLabel28_pointer
    {
        get => Data.ReadUInt(0x18 + (27 * 4));
        set => Data.SetUInt(0x18 + (27 * 4), value);
    }
    public string sItemLabel28_string { get; set; }
    public uint sItemLabel29_pointer
    {
        get => Data.ReadUInt(0x18 + (28 * 4));
        set => Data.SetUInt(0x18 + (28 * 4), value);
    }
    public string sItemLabel29_string { get; set; }
    public uint sItemLabel30_pointer
    {
        get => Data.ReadUInt(0x18 + (29 * 4));
        set => Data.SetUInt(0x18 + (29 * 4), value);
    }
    public string sItemLabel30_string { get; set; }
    public uint sItemLabel31_pointer
    {
        get => Data.ReadUInt(0x18 + (30 * 4));
        set => Data.SetUInt(0x18 + (30 * 4), value);
    }
    public string sItemLabel31_string { get; set; }
    public uint sItemLabel32_pointer
    {
        get => Data.ReadUInt(0x18 + (31 * 4));
        set => Data.SetUInt(0x18 + (31 * 4), value);
    }
    public string sItemLabel32_string { get; set; }

    public List<string> GetItems()
    {
        List<string> list = new();
        for (int i = 1; i <= 32; i++)
        {
            list.Add(this.GetPropValue<string>($"sItemLabel{i}_string"));
        }

        return list.Where(s => s != "").ToList();
    }
    public void SetItems(List<string> list)
    {
        for (int i = 1; i <= 32; i++)
        {
            this.SetPropValue($"sItemLabel{i}_string", i > list.Count ? "" : list[i - 1]);
        }
    }
    public override int GetDefaultLength()
    {
        return 0x9C;
    }
}

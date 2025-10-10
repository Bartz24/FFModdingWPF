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
    public string sItemLabel1 { get; set; }
    public uint sItemLabel2_pointer
    {
        get => Data.ReadUInt(0x18 + (1 * 4));
        set => Data.SetUInt(0x18 + (1 * 4), value);
    }
    public string sItemLabel2 { get; set; }
    public uint sItemLabel3_pointer
    {
        get => Data.ReadUInt(0x18 + (2 * 4));
        set => Data.SetUInt(0x18 + (2 * 4), value);
    }
    public string sItemLabel3 { get; set; }
    public uint sItemLabel4_pointer
    {
        get => Data.ReadUInt(0x18 + (3 * 4));
        set => Data.SetUInt(0x18 + (3 * 4), value);
    }
    public string sItemLabel4 { get; set; }
    public uint sItemLabel5_pointer
    {
        get => Data.ReadUInt(0x18 + (4 * 4));
        set => Data.SetUInt(0x18 + (4 * 4), value);
    }
    public string sItemLabel5 { get; set; }
    public uint sItemLabel6_pointer
    {
        get => Data.ReadUInt(0x18 + (5 * 4));
        set => Data.SetUInt(0x18 + (5 * 4), value);
    }
    public string sItemLabel6 { get; set; }
    public uint sItemLabel7_pointer
    {
        get => Data.ReadUInt(0x18 + (6 * 4));
        set => Data.SetUInt(0x18 + (6 * 4), value);
    }
    public string sItemLabel7 { get; set; }
    public uint sItemLabel8_pointer
    {
        get => Data.ReadUInt(0x18 + (7 * 4));
        set => Data.SetUInt(0x18 + (7 * 4), value);
    }
    public string sItemLabel8 { get; set; }
    public uint sItemLabel9_pointer
    {
        get => Data.ReadUInt(0x18 + (8 * 4));
        set => Data.SetUInt(0x18 + (8 * 4), value);
    }
    public string sItemLabel9 { get; set; }
    public uint sItemLabel10_pointer
    {
        get => Data.ReadUInt(0x18 + (9 * 4));
        set => Data.SetUInt(0x18 + (9 * 4), value);
    }
    public string sItemLabel10 { get; set; }
    public uint sItemLabel11_pointer
    {
        get => Data.ReadUInt(0x18 + (10 * 4));
        set => Data.SetUInt(0x18 + (10 * 4), value);
    }
    public string sItemLabel11 { get; set; }
    public uint sItemLabel12_pointer
    {
        get => Data.ReadUInt(0x18 + (11 * 4));
        set => Data.SetUInt(0x18 + (11 * 4), value);
    }
    public string sItemLabel12 { get; set; }
    public uint sItemLabel13_pointer
    {
        get => Data.ReadUInt(0x18 + (12 * 4));
        set => Data.SetUInt(0x18 + (12 * 4), value);
    }
    public string sItemLabel13 { get; set; }
    public uint sItemLabel14_pointer
    {
        get => Data.ReadUInt(0x18 + (13 * 4));
        set => Data.SetUInt(0x18 + (13 * 4), value);
    }
    public string sItemLabel14 { get; set; }
    public uint sItemLabel15_pointer
    {
        get => Data.ReadUInt(0x18 + (14 * 4));
        set => Data.SetUInt(0x18 + (14 * 4), value);
    }
    public string sItemLabel15 { get; set; }
    public uint sItemLabel16_pointer
    {
        get => Data.ReadUInt(0x18 + (15 * 4));
        set => Data.SetUInt(0x18 + (15 * 4), value);
    }
    public string sItemLabel16 { get; set; }
    public uint sItemLabel17_pointer
    {
        get => Data.ReadUInt(0x18 + (16 * 4));
        set => Data.SetUInt(0x18 + (16 * 4), value);
    }
    public string sItemLabel17 { get; set; }
    public uint sItemLabel18_pointer
    {
        get => Data.ReadUInt(0x18 + (17 * 4));
        set => Data.SetUInt(0x18 + (17 * 4), value);
    }
    public string sItemLabel18 { get; set; }
    public uint sItemLabel19_pointer
    {
        get => Data.ReadUInt(0x18 + (18 * 4));
        set => Data.SetUInt(0x18 + (18 * 4), value);
    }
    public string sItemLabel19 { get; set; }
    public uint sItemLabel20_pointer
    {
        get => Data.ReadUInt(0x18 + (19 * 4));
        set => Data.SetUInt(0x18 + (19 * 4), value);
    }
    public string sItemLabel20 { get; set; }
    public uint sItemLabel21_pointer
    {
        get => Data.ReadUInt(0x18 + (20 * 4));
        set => Data.SetUInt(0x18 + (20 * 4), value);
    }
    public string sItemLabel21 { get; set; }
    public uint sItemLabel22_pointer
    {
        get => Data.ReadUInt(0x18 + (21 * 4));
        set => Data.SetUInt(0x18 + (21 * 4), value);
    }
    public string sItemLabel22 { get; set; }
    public uint sItemLabel23_pointer
    {
        get => Data.ReadUInt(0x18 + (22 * 4));
        set => Data.SetUInt(0x18 + (22 * 4), value);
    }
    public string sItemLabel23 { get; set; }
    public uint sItemLabel24_pointer
    {
        get => Data.ReadUInt(0x18 + (23 * 4));
        set => Data.SetUInt(0x18 + (23 * 4), value);
    }
    public string sItemLabel24 { get; set; }
    public uint sItemLabel25_pointer
    {
        get => Data.ReadUInt(0x18 + (24 * 4));
        set => Data.SetUInt(0x18 + (24 * 4), value);
    }
    public string sItemLabel25 { get; set; }
    public uint sItemLabel26_pointer
    {
        get => Data.ReadUInt(0x18 + (25 * 4));
        set => Data.SetUInt(0x18 + (25 * 4), value);
    }
    public string sItemLabel26 { get; set; }
    public uint sItemLabel27_pointer
    {
        get => Data.ReadUInt(0x18 + (26 * 4));
        set => Data.SetUInt(0x18 + (26 * 4), value);
    }
    public string sItemLabel27 { get; set; }
    public uint sItemLabel28_pointer
    {
        get => Data.ReadUInt(0x18 + (27 * 4));
        set => Data.SetUInt(0x18 + (27 * 4), value);
    }
    public string sItemLabel28 { get; set; }
    public uint sItemLabel29_pointer
    {
        get => Data.ReadUInt(0x18 + (28 * 4));
        set => Data.SetUInt(0x18 + (28 * 4), value);
    }
    public string sItemLabel29 { get; set; }
    public uint sItemLabel30_pointer
    {
        get => Data.ReadUInt(0x18 + (29 * 4));
        set => Data.SetUInt(0x18 + (29 * 4), value);
    }
    public string sItemLabel30 { get; set; }
    public uint sItemLabel31_pointer
    {
        get => Data.ReadUInt(0x18 + (30 * 4));
        set => Data.SetUInt(0x18 + (30 * 4), value);
    }
    public string sItemLabel31 { get; set; }
    public uint sItemLabel32_pointer
    {
        get => Data.ReadUInt(0x18 + (31 * 4));
        set => Data.SetUInt(0x18 + (31 * 4), value);
    }
    public string sItemLabel32 { get; set; }

    public List<string> GetItems()
    {
        List<string> list = new();
        for (int i = 1; i <= 32; i++)
        {
            list.Add(this.GetPropValue<string>($"sItemLabel{i}"));
        }

        return list.Where(s => s != "").ToList();
    }
    public void SetItems(List<string> list)
    {
        for (int i = 1; i <= 32; i++)
        {
            this.SetPropValue($"sItemLabel{i}", i > list.Count ? "" : list[i - 1]);
        }
    }
    public override int GetDefaultLength()
    {
        return 0x9C;
    }
}

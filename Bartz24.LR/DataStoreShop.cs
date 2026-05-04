using Bartz24.Data;
using Bartz24.FF13_2_LR;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.LR;

public class DataStoreShop : DataStoreWDBEntry
{
    public string sShopNameLabel { get; set; }
    public string sBaseId { get; set; }
    public string sItemLabel1 { get; set; }
    public string sItemLabel2 { get; set; }
    public string sItemLabel3 { get; set; }
    public string sItemLabel4 { get; set; }
    public string sItemLabel5 { get; set; }
    public string sItemLabel6 { get; set; }
    public string sItemLabel7 { get; set; }
    public string sItemLabel8 { get; set; }
    public string sItemLabel9 { get; set; }
    public string sItemLabel10 { get; set; }
    public string sItemLabel11 { get; set; }
    public string sItemLabel12 { get; set; }
    public string sItemLabel13 { get; set; }
    public string sItemLabel14 { get; set; }
    public string sItemLabel15 { get; set; }
    public string sItemLabel16 { get; set; }
    public string sItemLabel17 { get; set; }
    public string sItemLabel18 { get; set; }
    public string sItemLabel19 { get; set; }
    public string sItemLabel20 { get; set; }
    public string sItemLabel21 { get; set; }
    public string sItemLabel22 { get; set; }
    public string sItemLabel23 { get; set; }
    public string sItemLabel24 { get; set; }
    public string sItemLabel25 { get; set; }
    public string sItemLabel26 { get; set; }
    public string sItemLabel27 { get; set; }
    public string sItemLabel28 { get; set; }
    public string sItemLabel29 { get; set; }
    public string sItemLabel30 { get; set; }
    public string sItemLabel31 { get; set; }
    public string sItemLabel32 { get; set; }
    public int u3Category { get; set; }
    public int u4Day { get; set; }
    public int u8SaveIndex { get; set; }
    public int u7PowRate { get; set; }
    public int u7AtbRate { get; set; }
    public int u2ItemFlag1 { get; set; }
    public int u7ChainRate { get; set; }
    public int u7BalanceRate { get; set; }
    public int u7RandRate { get; set; }
    public int u2ItemFlag2 { get; set; }
    public int u2ItemFlag3 { get; set; }
    public int u2ItemFlag4 { get; set; }
    public int u2ItemFlag5 { get; set; }
    public int u2ItemFlag6 { get; set; }
    public int u16CreateHqCoef { get; set; }
    public int u16DisHqCoef { get; set; }
    public int u2ItemFlag7 { get; set; }
    public int u2ItemFlag8 { get; set; }
    public int u2ItemFlag9 { get; set; }
    public int u2ItemFlag10 { get; set; }
    public int u2ItemFlag11 { get; set; }
    public int u2ItemFlag12 { get; set; }
    public int u2ItemFlag13 { get; set; }
    public int u2ItemFlag14 { get; set; }
    public int u2ItemFlag15 { get; set; }
    public int u2ItemFlag16 { get; set; }
    public int u2ItemFlag17 { get; set; }
    public int u2ItemFlag18 { get; set; }
    public int u2ItemFlag19 { get; set; }
    public int u2ItemFlag20 { get; set; }
    public int u2ItemFlag21 { get; set; }
    public int u2ItemFlag22 { get; set; }
    public int u2ItemFlag23 { get; set; }
    public int u2ItemFlag24 { get; set; }
    public int u2ItemFlag25 { get; set; }
    public int u2ItemFlag26 { get; set; }
    public int u2ItemFlag27 { get; set; }
    public int u2ItemFlag28 { get; set; }
    public int u2ItemFlag29 { get; set; }
    public int u2ItemFlag30 { get; set; }
    public int u2ItemFlag31 { get; set; }
    public int u2ItemFlag32 { get; set; }

    public List<string> GetItems()
    {
        List<string> list = new();
        for (int i = 1; i <= 32; i++)
        {
            list.Add(this.GetPropValue<string>($"sItemLabel{i}"));
        }

        return list.Where(s => s != "").ToList();
    }
    public List<int> GetItemFlags()
    {
        List<int> list = new();
        for (int i = 1; i <= 32; i++)
        {
            list.Add(this.GetPropValue<int>($"u2ItemFlag{i}"));
        }

        return list;
    }
    public void SetItems(List<string> list)
    {
        for (int i = 1; i <= 32; i++)
        {
            this.SetPropValue($"sItemLabel{i}", i > list.Count ? "" : list[i - 1]);
        }
    }
    public void SetItemFlags(List<int> list)
    {
        for (int i = 1; i <= 32; i++)
        {
            this.SetPropValue($"u2ItemFlag{i}", i > list.Count ? 0 : list[i - 1]);
        }
    }
}

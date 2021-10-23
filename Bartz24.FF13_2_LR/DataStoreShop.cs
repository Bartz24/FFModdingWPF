using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bartz24.Data;

namespace Bartz24.FF13_2_LR
{
	public class DataStoreShop : DataStoreDB3SubEntry
	{
		public int sShopNameLabel_pointer { get; set; }
		public string sShopNameLabel_string { get; set; }
		public int sBaseId_pointer { get; set; }
		public string sBaseId_string { get; set; }
		public int sItemLabel1_pointer { get; set; }
		public string sItemLabel1_string { get; set; }
		public int sItemLabel2_pointer { get; set; }
		public string sItemLabel2_string { get; set; }
		public int sItemLabel3_pointer { get; set; }
		public string sItemLabel3_string { get; set; }
		public int sItemLabel4_pointer { get; set; }
		public string sItemLabel4_string { get; set; }
		public int sItemLabel5_pointer { get; set; }
		public string sItemLabel5_string { get; set; }
		public int sItemLabel6_pointer { get; set; }
		public string sItemLabel6_string { get; set; }
		public int sItemLabel7_pointer { get; set; }
		public string sItemLabel7_string { get; set; }
		public int sItemLabel8_pointer { get; set; }
		public string sItemLabel8_string { get; set; }
		public int sItemLabel9_pointer { get; set; }
		public string sItemLabel9_string { get; set; }
		public int sItemLabel10_pointer { get; set; }
		public string sItemLabel10_string { get; set; }
		public int sItemLabel11_pointer { get; set; }
		public string sItemLabel11_string { get; set; }
		public int sItemLabel12_pointer { get; set; }
		public string sItemLabel12_string { get; set; }
		public int sItemLabel13_pointer { get; set; }
		public string sItemLabel13_string { get; set; }
		public int sItemLabel14_pointer { get; set; }
		public string sItemLabel14_string { get; set; }
		public int sItemLabel15_pointer { get; set; }
		public string sItemLabel15_string { get; set; }
		public int sItemLabel16_pointer { get; set; }
		public string sItemLabel16_string { get; set; }
		public int sItemLabel17_pointer { get; set; }
		public string sItemLabel17_string { get; set; }
		public int sItemLabel18_pointer { get; set; }
		public string sItemLabel18_string { get; set; }
		public int sItemLabel19_pointer { get; set; }
		public string sItemLabel19_string { get; set; }
		public int sItemLabel20_pointer { get; set; }
		public string sItemLabel20_string { get; set; }
		public int sItemLabel21_pointer { get; set; }
		public string sItemLabel21_string { get; set; }
		public int sItemLabel22_pointer { get; set; }
		public string sItemLabel22_string { get; set; }
		public int sItemLabel23_pointer { get; set; }
		public string sItemLabel23_string { get; set; }
		public int sItemLabel24_pointer { get; set; }
		public string sItemLabel24_string { get; set; }
		public int sItemLabel25_pointer { get; set; }
		public string sItemLabel25_string { get; set; }
		public int sItemLabel26_pointer { get; set; }
		public string sItemLabel26_string { get; set; }
		public int sItemLabel27_pointer { get; set; }
		public string sItemLabel27_string { get; set; }
		public int sItemLabel28_pointer { get; set; }
		public string sItemLabel28_string { get; set; }
		public int sItemLabel29_pointer { get; set; }
		public string sItemLabel29_string { get; set; }
		public int sItemLabel30_pointer { get; set; }
		public string sItemLabel30_string { get; set; }
		public int sItemLabel31_pointer { get; set; }
		public string sItemLabel31_string { get; set; }
		public int sItemLabel32_pointer { get; set; }
		public string sItemLabel32_string { get; set; }
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
			List<string> list = new List<string>();
			for (int i = 1; i <= 32; i++)
			{
				list.Add(this.GetPropValue<string>($"sItemLabel{i}_string"));
			}
			return list.Where(s => s != "").ToList();
		}
		public List<int> GetItemFlags()
		{
			List<int> list = new List<int>();
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
				this.SetPropValue($"sItemLabel{i}_string", i > list.Count ? "" : list[i - 1]);
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bartz24.Data;
using Bartz24.FF13_2_LR;

namespace Bartz24.FF13_2
{
	public class DataStoreRGrowSt : DataStoreDB3SubEntry
	{
		public int sBtCharaSpecId_pointer { get; set; }
		public string sBtCharaSpecId_string { get; set; }
		public int iHpMin { get; set; }
		public int iHpMax { get; set; }
		public int iPhyMin { get; set; }
		public int iPhyMax { get; set; }
		public int iMgkMin { get; set; }
		public int iMgkMax { get; set; }
		public int sAbi0_pointer { get; set; }
		public string sAbi0_string { get; set; }
		public int sAbi1_pointer { get; set; }
		public string sAbi1_string { get; set; }
		public int sAbi2_pointer { get; set; }
		public string sAbi2_string { get; set; }
		public int sAbi3_pointer { get; set; }
		public string sAbi3_string { get; set; }
		public int sAbi4_pointer { get; set; }
		public string sAbi4_string { get; set; }
		public int sAbi5_pointer { get; set; }
		public string sAbi5_string { get; set; }
		public int sAbi6_pointer { get; set; }
		public string sAbi6_string { get; set; }
		public int sAbi7_pointer { get; set; }
		public string sAbi7_string { get; set; }
		public int sAbi8_pointer { get; set; }
		public string sAbi8_string { get; set; }
		public int sAbi9_pointer { get; set; }
		public string sAbi9_string { get; set; }
		public int sAbi10_pointer { get; set; }
		public string sAbi10_string { get; set; }
		public int sAbi11_pointer { get; set; }
		public string sAbi11_string { get; set; }
		public int sAbi12_pointer { get; set; }
		public string sAbi12_string { get; set; }
		public int sAbi13_pointer { get; set; }
		public string sAbi13_string { get; set; }
		public int sAbi14_pointer { get; set; }
		public string sAbi14_string { get; set; }
		public int sAbi15_pointer { get; set; }
		public string sAbi15_string { get; set; }
		public int sAbi16_pointer { get; set; }
		public string sAbi16_string { get; set; }
		public int sAbi17_pointer { get; set; }
		public string sAbi17_string { get; set; }
		public int sAbi18_pointer { get; set; }
		public string sAbi18_string { get; set; }
		public int sAbi19_pointer { get; set; }
		public string sAbi19_string { get; set; }
		public int sAbi20_pointer { get; set; }
		public string sAbi20_string { get; set; }
		public int sAbi21_pointer { get; set; }
		public string sAbi21_string { get; set; }
		public int sAbi22_pointer { get; set; }
		public string sAbi22_string { get; set; }
		public int sAbi23_pointer { get; set; }
		public string sAbi23_string { get; set; }
		public int sAbi24_pointer { get; set; }
		public string sAbi24_string { get; set; }
		public int sAbi25_pointer { get; set; }
		public string sAbi25_string { get; set; }
		public int sAbi26_pointer { get; set; }
		public string sAbi26_string { get; set; }
		public int sAbi27_pointer { get; set; }
		public string sAbi27_string { get; set; }
		public int sAbi28_pointer { get; set; }
		public string sAbi28_string { get; set; }
		public int sAbi29_pointer { get; set; }
		public string sAbi29_string { get; set; }
		public int sAbi30_pointer { get; set; }
		public string sAbi30_string { get; set; }
		public int sAbi31_pointer { get; set; }
		public string sAbi31_string { get; set; }
		public int sRebirth0_pointer { get; set; }
		public string sRebirth0_string { get; set; }
		public int sRebirth1_pointer { get; set; }
		public string sRebirth1_string { get; set; }
		public int sRebirth2_pointer { get; set; }
		public string sRebirth2_string { get; set; }
		public int sRebirth3_pointer { get; set; }
		public string sRebirth3_string { get; set; }
		public int sRebirth4_pointer { get; set; }
		public string sRebirth4_string { get; set; }
		public int sRebirth5_pointer { get; set; }
		public string sRebirth5_string { get; set; }
		public int sRebirth6_pointer { get; set; }
		public string sRebirth6_string { get; set; }
		public int sRebirth7_pointer { get; set; }
		public string sRebirth7_string { get; set; }
		public int sConstellationId_pointer { get; set; }
		public string sConstellationId_string { get; set; }
		public int sComboName_pointer { get; set; }
		public string sComboName_string { get; set; }
		public int sComboButtonHelp_pointer { get; set; }
		public string sComboButtonHelp_string { get; set; }
		public int sFlavor0_pointer { get; set; }
		public string sFlavor0_string { get; set; }
		public int sFlavor1_pointer { get; set; }
		public string sFlavor1_string { get; set; }
		public int sFlavor2_pointer { get; set; }
		public string sFlavor2_string { get; set; }
		public int sFlavor3_pointer { get; set; }
		public string sFlavor3_string { get; set; }
		public int u5RoleStyle { get; set; }
		public int u7InitLv { get; set; }
		public int u7MaxLv { get; set; }
		public int u4CpType { get; set; }
		public int u5HpType { get; set; }
		public int u2Factor0 { get; set; }
		public int u2Factor1 { get; set; }
		public int u5PhyType { get; set; }
		public int u5MgkType { get; set; }
		public int u7Lv0 { get; set; }
		public int u7Lv1 { get; set; }
		public int u7Lv2 { get; set; }
		public int u2Factor2 { get; set; }
		public int u7Lv3 { get; set; }
		public int u2Factor3 { get; set; }
		public int u7Lv4 { get; set; }
		public int u2Factor4 { get; set; }
		public int u7Lv5 { get; set; }
		public int u2Factor5 { get; set; }
		public int u2Factor6 { get; set; }
		public int u7Lv6 { get; set; }
		public int u7Lv7 { get; set; }
		public int u2Factor7 { get; set; }
		public int u7Lv8 { get; set; }
		public int u2Factor8 { get; set; }
		public int u7Lv9 { get; set; }
		public int u2Factor9 { get; set; }
		public int u7Lv10 { get; set; }
		public int u2Factor10 { get; set; }
		public int u7Lv11 { get; set; }
		public int u2Factor11 { get; set; }
		public int u7Lv12 { get; set; }
		public int u2Factor12 { get; set; }
		public int u2Factor13 { get; set; }
		public int u7Lv13 { get; set; }
		public int u7Lv14 { get; set; }
		public int u2Factor14 { get; set; }
		public int u7Lv15 { get; set; }
		public int u2Factor15 { get; set; }
		public int u7Lv16 { get; set; }
		public int u2Factor16 { get; set; }
		public int u7Lv17 { get; set; }
		public int u2Factor17 { get; set; }
		public int u7Lv18 { get; set; }
		public int u2Factor18 { get; set; }
		public int u7Lv19 { get; set; }
		public int u2Factor19 { get; set; }
		public int u2Factor20 { get; set; }
		public int u7Lv20 { get; set; }
		public int u7Lv21 { get; set; }
		public int u2Factor21 { get; set; }
		public int u7Lv22 { get; set; }
		public int u2Factor22 { get; set; }
		public int u7Lv23 { get; set; }
		public int u2Factor23 { get; set; }
		public int u7Lv24 { get; set; }
		public int u2Factor24 { get; set; }
		public int u7Lv25 { get; set; }
		public int u2Factor25 { get; set; }
		public int u7Lv26 { get; set; }
		public int u2Factor26 { get; set; }
		public int u2Factor27 { get; set; }
		public int u7Lv27 { get; set; }
		public int u7Lv28 { get; set; }
		public int u2Factor28 { get; set; }
		public int u7Lv29 { get; set; }
		public int u2Factor29 { get; set; }
		public int u7Lv30 { get; set; }
		public int u2Factor30 { get; set; }
		public int u7Lv31 { get; set; }
		public int u2Factor31 { get; set; }
		public int u10LimitLv0 { get; set; }
		public int u10LimitLv1 { get; set; }
		public int u10LimitLv2 { get; set; }
		public int u10LimitLv3 { get; set; }
		public int u10LimitLv4 { get; set; }
		public int u2PhyMaterial { get; set; }
		public int u10LimitLv5 { get; set; }
		public int u10LimitLv6 { get; set; }
		public int u10LimitLv7 { get; set; }
		public int u2MgkMaterial { get; set; }
		public int u2HpMaterial { get; set; }
		public int u2AnyMaterial { get; set; }
		public int u9FaceIcon { get; set; }
		public int i8IconX { get; set; }

		public List<string> GetAbilities()
		{
			List<string> list = new List<string>();
			for (int i = 1; i < 32; i++)
			{
				list.Add(this.GetPropValue<string>($"sAbi{i}_string"));
			}
			return list.Where(s => s != "").ToList();
		}
		public void SetAbilities(List<string> list)
		{
			for (int i = 1; i < 32; i++)
			{
				this.SetPropValue($"sAbi{i}_string", i > list.Count ? "" : list[i - 1]);
			}
		}
	}
}

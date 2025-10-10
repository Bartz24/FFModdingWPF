using Bartz24.Data;
using Bartz24.FF13_2_LR;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.FF13_2;

public class DataStoreRGrowSt : DataStoreWDBEntry
{
    public string sBtCharaSpecId { get; set; }
    public int iHpMin { get; set; }
    public int iHpMax { get; set; }
    public int iPhyMin { get; set; }
    public int iPhyMax { get; set; }
    public int iMgkMin { get; set; }
    public int iMgkMax { get; set; }
    public string sAbi0 { get; set; }
    public string sAbi1 { get; set; }
    public string sAbi2 { get; set; }
    public string sAbi3 { get; set; }
    public string sAbi4 { get; set; }
    public string sAbi5 { get; set; }
    public string sAbi6 { get; set; }
    public string sAbi7 { get; set; }
    public string sAbi8 { get; set; }
    public string sAbi9 { get; set; }
    public string sAbi10 { get; set; }
    public string sAbi11 { get; set; }
    public string sAbi12 { get; set; }
    public string sAbi13 { get; set; }
    public string sAbi14 { get; set; }
    public string sAbi15 { get; set; }
    public string sAbi16 { get; set; }
    public string sAbi17 { get; set; }
    public string sAbi18 { get; set; }
    public string sAbi19 { get; set; }
    public string sAbi20 { get; set; }
    public string sAbi21 { get; set; }
    public string sAbi22 { get; set; }
    public string sAbi23 { get; set; }
    public string sAbi24 { get; set; }
    public string sAbi25 { get; set; }
    public string sAbi26 { get; set; }
    public string sAbi27 { get; set; }
    public string sAbi28 { get; set; }
    public string sAbi29 { get; set; }
    public string sAbi30 { get; set; }
    public string sAbi31 { get; set; }
    public string sRebirth0 { get; set; }
    public string sRebirth1 { get; set; }
    public string sRebirth2 { get; set; }
    public string sRebirth3 { get; set; }
    public string sRebirth4 { get; set; }
    public string sRebirth5 { get; set; }
    public string sRebirth6 { get; set; }
    public string sRebirth7 { get; set; }
    public string sConstellationId { get; set; }
    public string sComboName { get; set; }
    public string sComboButtonHelp { get; set; }
    public string sFlavor0 { get; set; }
    public string sFlavor1 { get; set; }
    public string sFlavor2 { get; set; }
    public string sFlavor3 { get; set; }
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
        List<string> list = new();
        for (int i = 1; i < 32; i++)
        {
            list.Add(this.GetPropValue<string>($"sAbi{i}"));
        }

        return list.Where(s => s != "").ToList();
    }
    public void SetAbilities(List<string> list)
    {
        for (int i = 1; i < 32; i++)
        {
            this.SetPropValue($"sAbi{i}", i > list.Count ? "" : list[i - 1]);
        }
    }
}

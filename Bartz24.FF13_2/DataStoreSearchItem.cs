using Bartz24.Data;
using Bartz24.FF13_2_LR;

namespace Bartz24.FF13_2;

public class DataStoreSearchItem : DataStoreWDBEntry
{
    public string sItemName0 { get; set; }
    public string sItemName1 { get; set; }
    public string sItemName2 { get; set; }
    public string sItemName3 { get; set; }
    public string sItemName4 { get; set; }
    public string sItemName5 { get; set; }
    public string sItemName6 { get; set; }
    public string sItemName7 { get; set; }
        public int u8Count0 { get; set; }
    public int u8Random0 { get; set; }
    public int u8Max0 { get; set; }
    public int u8Count1 { get; set; }
    public int u8Random1 { get; set; }
    public int u8Max1 { get; set; }
    public int u8Count2 { get; set; }
    public int u8Random2 { get; set; }
    public int u8Max2 { get; set; }
    public int u8Count3 { get; set; }
    public int u8Random3 { get; set; }
    public int u8Max3 { get; set; }
    public int u8Count4 { get; set; }
    public int u8Random4 { get; set; }
    public int u8Max4 { get; set; }
    public int u8Count5 { get; set; }
    public int u8Random5 { get; set; }
    public int u8Max5 { get; set; }
    public int u8Count6 { get; set; }
    public int u8Random6 { get; set; }
    public int u8Max6 { get; set; }
    public int u8Count7 { get; set; }
    public int u8Random7 { get; set; }
    public int u8Max7 { get; set; }

    public string GetItem(int index)
    {
        return this.GetPropValue<string>($"sItemName{index}");
    }
    public void SetItem(int index, string item)
    {
        this.SetPropValue($"sItemName{index}", item);
    }
    public int GetMax(int index)
    {
        return this.GetPropValue<int>($"u8Max{index}");
    }
    public void SetMax(int index, int max)
    {
        this.SetPropValue($"u8Max{index}", max);
    }
    public int GetCount(int index)
    {
        return this.GetPropValue<int>($"u8Count{index}");
    }
    public void SetCount(int index, int max)
    {
        this.SetPropValue($"u8Count{index}", max);
    }
    public int GetRandom(int index)
    {
        return this.GetPropValue<int>($"u8Random{index}");
    }
    public void SetRandom(int index, int max)
    {
        this.SetPropValue($"u8Random{index}", max);
    }
}

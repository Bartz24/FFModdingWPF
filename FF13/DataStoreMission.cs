using Bartz24.Data;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Bartz24.FF13;

public class DataStoreMission : DataStoreWDBEntry
{
    public uint sCharaSpecId0_pointer
    {
        get => Data.ReadUInt(0x34);
        set => Data.SetUInt(0x34, value);
    }
    public string sCharaSpecId0 { get; set; }
    public uint sCharaSpecId1_pointer
    {
        get => Data.ReadUInt(0x38);
        set => Data.SetUInt(0x38, value);
    }
    public string sCharaSpecId1 { get; set; }
    public uint sCharaSpecId2_pointer
    {
        get => Data.ReadUInt(0x3C);
        set => Data.SetUInt(0x3C, value);
    }
    public string sCharaSpecId2 { get; set; }
    public uint sCharaSpecId3_pointer
    {
        get => Data.ReadUInt(0x40);
        set => Data.SetUInt(0x40, value);
    }
    public string sCharaSpecId3 { get; set; }

    public override int GetDefaultLength()
    {
        return 0x54;
    }

    public void SetCharaSpecs(List<string> list)
    {
        if (list.Count > 4)
        {
            throw new Exception("Too many Chara Specs being added");
        }

        for (int i = 0; i < 4; i++)
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
        for (int i = 0; i < 4; i++)
        {
            list.Add(this.GetPropValue<string>($"sCharaSpecId{i}"));
        }

        return list.Where(s => s != "").ToList();
    }
}

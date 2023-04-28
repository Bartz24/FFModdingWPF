using Bartz24.Data;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.FF12;

public class DataStorePartyMember : DataStore
{
    public enum RecalculateLevelLPType : byte
    {
        Never = 0,
        Once = 1,
        Always = 2,
        Never2 = 3
    }
    public RecalculateLevelLPType RecalculateLevelLP
    {
        get => (RecalculateLevelLPType)Data.ReadByte(0x2C, 3, 2);
        set => Data.SetByte(0x2C, (byte)value, 3, 2);
    }
    public byte Level
    {
        get => Data.ReadByte(0x2E);
        set => Data.SetByte(0x2E, value);
    }
    public ushort LP
    {
        get => Data.ReadUShort(0x44);
        set => Data.SetUShort(0x44, value);
    }
    public List<byte> ItemAmounts
    {
        get => Enumerable.Range(0, 10).Where(i => Data.ReadUShort(0x58 + (i * 2)) != 0xFFFF).Select(i => Data.ReadByte(0x34 + i)).ToList();
        set => Enumerable.Range(0, 10).ForEach(i => Data.SetByte(0x34 + i, i >= value.Count ? (byte)0 : value[i]));
    }
    public List<ushort> ItemIDs
    {
        get => Enumerable.Range(0, 10).Select(i => Data.ReadUShort(0x58 + (i * 2))).Where(a => a != 0xFFFF).ToList();
        set => Enumerable.Range(0, 10).ForEach(i => Data.SetUShort(0x58 + (i * 2), i >= value.Count ? (ushort)0xFFFF : value[i]));
    }
    public override int GetDefaultLength()
    {
        return 0x80;
    }
}

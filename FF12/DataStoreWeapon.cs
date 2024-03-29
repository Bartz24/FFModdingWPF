﻿using Bartz24.Data;

namespace Bartz24.FF12;

public class DataStoreWeapon : DataStoreEquip
{
    public byte Range
    {
        get => Data.ReadByte(0x18);
        set => Data.SetByte(0x18, value);
    }
    public byte AttackPower
    {
        get => Data.ReadByte(0x1A);
        set => Data.SetByte(0x1A, value);
    }
    public byte KnockbackChance
    {
        get => Data.ReadByte(0x1B);
        set => Data.SetByte(0x1B, value);
    }
    public byte ComboChance
    {
        get => Data.ReadByte(0x1C);
        set => Data.SetByte(0x1C, value);
    }
    public byte Evade
    {
        get => Data.ReadByte(0x1D);
        set => Data.SetByte(0x1D, value);
    }
    public Element Elements
    {
        get => (Element)Data.ReadByte(0x1E);
        set => Data.SetByte(0x1E, (byte)value);
    }
    public byte StatusChance
    {
        get => Data.ReadByte(0x1F);
        set => Data.SetByte(0x1F, value);
    }
    public Status StatusEffects
    {
        get => (Status)Data.ReadUInt(0x20);
        set => Data.SetUInt(0x20, (uint)value);
    }
    public byte ChargeTime
    {
        get => Data.ReadByte(0x27);
        set => Data.SetByte(0x27, value);
    }
}

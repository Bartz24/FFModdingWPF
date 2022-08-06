using Bartz24.Data;

namespace Bartz24.FF12
{
    public class DataStoreARDStats : DataStore
    {
        public ushort CP
        {
            get { return Data.ReadUShort(0x0); }
            set { Data.SetUShort(0x0, value); }
        }
        public uint HP
        {
            get { return Data.ReadUInt(0x20); }
            set { Data.SetUInt(0x20, value); }
        }
        public ushort MP
        {
            get { return Data.ReadUShort(0x24); }
            set { Data.SetUShort(0x24, value); }
        }
        public byte Strength
        {
            get { return Data.ReadByte(0x26); }
            set { Data.SetByte(0x26, value); }
        }
        public byte MagickPower
        {
            get { return Data.ReadByte(0x27); }
            set { Data.SetByte(0x27, value); }
        }
        public byte Vitality
        {
            get { return Data.ReadByte(0x28); }
            set { Data.SetByte(0x28, value); }
        }
        public byte Speed
        {
            get { return Data.ReadByte(0x29); }
            set { Data.SetByte(0x29, value); }
        }
        public byte Evade
        {
            get { return Data.ReadByte(0x2A); }
            set { Data.SetByte(0x2A, value); }
        }
        public byte Defense
        {
            get { return Data.ReadByte(0x2B); }
            set { Data.SetByte(0x2B, value); }
        }
        public byte MagickResist
        {
            get { return Data.ReadByte(0x2C); }
            set { Data.SetByte(0x2C, value); }
        }
        public byte AttackPower
        {
            get { return Data.ReadByte(0x2D); }
            set { Data.SetByte(0x2D, value); }
        }
        public byte LP
        {
            get { return Data.ReadByte(0x2F); }
            set { Data.SetByte(0x2F, value); }
        }
        public uint Gil
        {
            get { return Data.ReadUInt(0x30); }
            set { Data.SetUInt(0x30, value); }
        }
        public uint Experience
        {
            get { return Data.ReadUInt(0x34); }
            set { Data.SetUInt(0x34, value); }
        }
        public override int GetDefaultLength()
        {
            return 0x38;
        }
    }
}

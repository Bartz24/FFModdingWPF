using Bartz24.Data;

namespace Bartz24.FF12
{
    public class DataStoreAttribute : DataStore
    {
        public ushort HP
        {
            get { return Data.ReadUShort(0x0); }
            set { Data.SetUShort(0x0, value); }
        }
        public ushort MP
        {
            get { return Data.ReadUShort(0x2); }
            set { Data.SetUShort(0x2, value); }
        }
        public byte Strength
        {
            get { return Data.ReadByte(0x4); }
            set { Data.SetByte(0x4, value); }
        }
        public byte MagickPower
        {
            get { return Data.ReadByte(0x5); }
            set { Data.SetByte(0x5, value); }
        }
        public byte Vitality
        {
            get { return Data.ReadByte(0x6); }
            set { Data.SetByte(0x6, value); }
        }
        public byte Speed
        {
            get { return Data.ReadByte(0x7); }
            set { Data.SetByte(0x7, value); }
        }

        public Status StatusEffectsOnEquip
        {
            get { return (Status)Data.ReadUInt(0x8); }
            set { Data.SetUInt(0x8, (uint)value); }
        }
        public Status StatusEffectsImmune
        {
            get { return (Status)Data.ReadUInt(0xC); }
            set { Data.SetUInt(0xC, (uint)value); }
        }
        public Element ElementsAbsorb
        {
            get { return (Element)Data.ReadByte(0x10); }
            set { Data.SetByte(0x10, (byte)value); }
        }
        public Element ElementsImmune
        {
            get { return (Element)Data.ReadByte(0x11); }
            set { Data.SetByte(0x11, (byte)value); }
        }
        public Element ElementsHalf
        {
            get { return (Element)Data.ReadByte(0x12); }
            set { Data.SetByte(0x12, (byte)value); }
        }
        public Element ElementsWeak
        {
            get { return (Element)Data.ReadByte(0x13); }
            set { Data.SetByte(0x13, (byte)value); }
        }
        public Element ElementsBoost
        {
            get { return (Element)Data.ReadByte(0x14); }
            set { Data.SetByte(0x14, (byte)value); }
        }
        public override int GetDefaultLength()
        {
            return 0x18;
        }
    }
}

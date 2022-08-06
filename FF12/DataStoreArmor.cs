using Bartz24.Data;

namespace Bartz24.FF12
{
    public class DataStoreArmor : DataStoreEquip
    {
        public byte Defense
        {
            get { return Data.ReadByte(0x18); }
            set { Data.SetByte(0x18, value); }
        }
        public byte MagickResist
        {
            get { return Data.ReadByte(0x19); }
            set { Data.SetByte(0x19, value); }
        }
        public byte AugmentOffset
        {
            get { return Data.ReadByte(0x1A); }
            set { Data.SetByte(0x1A, value); }
        }
    }
}

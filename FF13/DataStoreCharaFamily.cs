using Bartz24.Data;

namespace Bartz24.FF13
{
    public class DataStoreCharaFamily : DataStoreWDBEntry
    {
        public byte u8RunSpeed
        {
            get { return Data.ReadByte(0x17); }
            set { Data.SetByte(0x17, value); }
        }

        public override int GetDefaultLength()
        {
            return 0x58;
        }
    }
}

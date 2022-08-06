using Bartz24.Data;

namespace Bartz24.FF12
{
    public class DataStoreARDBasicInfo : DataStore
    {
        public byte ExtendedInfoIndex
        {
            get { return Data.ReadByte(0x0); }
            set { Data.SetByte(0x0, value); }
        }
        public ushort NameID
        {
            get { return Data.ReadUShort(0x08); }
            set { Data.SetUShort(0x08, value); }
        }
        public byte DefaultStatsIndex
        {
            get { return Data.ReadByte(0x22); }
            set { Data.SetByte(0x22, value); }
        }
        public byte LevelStatsIndex
        {
            get { return Data.ReadByte(0x24); }
            set { Data.SetByte(0x24, value); }
        }
        public override int GetDefaultLength()
        {
            return 0x58;
        }
    }
}

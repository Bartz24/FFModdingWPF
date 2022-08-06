using Bartz24.Data;

namespace Bartz24.FF13
{
    public class DataStoreTreasurebox : DataStoreWDBEntry
    {
        public uint sItemResourceId_pointer
        {
            get { return Data.ReadUInt(0x0); }
            set { Data.SetUInt(0x0, value); }
        }
        public string sItemResourceId_string { get; set; }
        public uint iItemCount
        {
            get { return Data.ReadUInt(0x4); }
            set { Data.SetUInt(0x4, value); }
        }
        public uint sItemResourceId_pointer_end
        {
            get { return Data.ReadUInt(0x8); }
            set { Data.SetUInt(0x8, value); }
        }

        public override int GetDefaultLength()
        {
            return 0xC;
        }
    }
}

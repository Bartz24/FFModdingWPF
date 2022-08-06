using Bartz24.Data;

namespace Bartz24.FF13
{
    public class DataStoreWDBEntry : DataStore
    {
        public string ID { get; set; }

        public override int GetDefaultLength()
        {
            return -1;
        }
    }
}

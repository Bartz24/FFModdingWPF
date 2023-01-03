using System.Text;

namespace Bartz24.Data
{
    public class DataStoreString : DataStore
    {
        public string Value { get; set; }

        public override byte[] Data
        {
            get
            {
                return Encoding.UTF8.GetBytes(Value).Concat(new byte[] { 0 });
            }
            set
            {
                if (value.Length == 1 && value[0] == 0)
                    this.Value = "";
                else
                    this.Value = Encoding.UTF8.GetString(value.SubArray(0, value.Length - 1));
            }
        }

        public override void LoadData(byte[] data, int offset = 0)
        {
            Value = data.ReadString(offset);
        }

        public override int GetDefaultLength()
        {
            return -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is DataStoreString)
                return this.Value == ((DataStoreString)obj).Value;
            return base.Equals(obj);
        }

        public static bool operator ==(DataStoreString a, DataStoreString b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DataStoreString a, DataStoreString b)
        {
            return !a.Equals(b);
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public DataStoreString Substring(int index)
        {
            DataStoreString sub = new DataStoreString();
            sub.LoadData(Data, index);
            return sub;
        }
    }
}

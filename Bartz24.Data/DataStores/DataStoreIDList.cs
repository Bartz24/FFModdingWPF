using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.Data
{
    public class DataStoreIDList<T> : DataStoreList<T> where T : DataStoreID, new()
    {
        public T this[string s]
        {
            get => list.Find(id => id.ID == s);
        }

        public void UpdateOffsets()
        {
            for(int i = 1; i < Count; i++)
            {
                this[i].Offset = this[i - 1].Offset + this[i - 1].DataSize;
            }
        }

        public int IndexOf(string obj)
        {
            for(int i = 0; i < Count; i++)
            {
                if (this[i].ID == obj)
                    return i;
            }
            return -1;
        }
    }
}

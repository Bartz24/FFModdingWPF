using System.Linq;

namespace Bartz24.Data;

public class DataStoreStringPointerList : DataStorePointerList<DataStoreString>
{
    public DataStoreStringPointerList(DataStoreString nullVal) : base(nullVal)
    {
    }
    public new DataStoreString this[int i]
    {
        get
        {
            if (!pointerToIndexDictionary.ContainsKey(i))
            {
                int newIndex = pointerToIndexDictionary.Keys.Where(p => p < i).Max();
                return list[pointerToIndexDictionary[newIndex]].Substring(i - newIndex);
            }

            return list[pointerToIndexDictionary[i]];
        }
        set
        {
            list[pointerToIndexDictionary[i]] = value;
            UpdatePointers();
        }
    }
}

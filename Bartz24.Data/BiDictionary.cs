using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.Data;

public class BiDictionary<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
{
    private readonly Dictionary<T1, T2> forward = new();
    private readonly Dictionary<T2, T1> reverse = new();

    public BiDictionary()
    {
        Forward = new Indexer<T1, T2>(forward);
        Reverse = new Indexer<T2, T1>(reverse);
    }

    public Indexer<T1, T2> Forward { get; private set; }
    public Indexer<T2, T1> Reverse { get; private set; }

    public IEnumerable<T1> Keys => Forward.Keys;
    public IEnumerable<T2> Values => Forward.Values;

    public void Add(T1 t1, T2 t2)
    {
        forward.Add(t1, t2);
        reverse.Add(t2, t1);
    }

    public void Remove(T1 t1)
    {
        T2 revKey = Forward[t1];
        forward.Remove(t1);
        reverse.Remove(revKey);
    }

    public void Remove(T2 t2)
    {
        T1 forwardKey = Reverse[t2];
        reverse.Remove(t2);
        forward.Remove(forwardKey);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
    {
        return forward.GetEnumerator();
    }

    public class Indexer<T3, T4>
    {
        private readonly Dictionary<T3, T4> dict;

        public IEnumerable<T3> Keys => dict.Keys;
        public IEnumerable<T4> Values => dict.Values;

        public Indexer(Dictionary<T3, T4> dictionary)
        {
            dict = dictionary;
        }

        public T4 this[T3 index]
        {
            get => dict[index];
            set => dict[index] = value;
        }

        public bool Contains(T3 key)
        {
            return dict.ContainsKey(key);
        }
    }
}

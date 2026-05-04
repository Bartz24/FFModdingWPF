using System;
using System.Collections;
using System.Collections.Generic;

namespace Bartz24.Data;
public class OrderedSet<T> : ICollection<T>
{
    private readonly IDictionary<T, LinkedListNode<T>> m_Dictionary;
    private readonly LinkedList<T> m_LinkedList;

    public OrderedSet()
        : this(EqualityComparer<T>.Default)
    {
    }

    public OrderedSet(IEqualityComparer<T> comparer)
    {
        m_Dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
        m_LinkedList = new LinkedList<T>();
    }

    public OrderedSet(IEnumerable<T> source)
        : this()
    {
        foreach (T obj in source)
        {
            Add(obj);
        }
    }

    public int Count
    {
        get { return m_Dictionary.Count; }
    }

    public virtual bool IsReadOnly
    {
        get { return m_Dictionary.IsReadOnly; }
    }

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    public bool Add(T item)
    {
        if (m_Dictionary.ContainsKey(item)) return false;
        LinkedListNode<T> node = m_LinkedList.AddLast(item);
        m_Dictionary.Add(item, node);
        return true;
    }

    public void Clear()
    {
        m_LinkedList.Clear();
        m_Dictionary.Clear();
    }

    public bool Remove(T item)
    {
        LinkedListNode<T> node;
        bool found = m_Dictionary.TryGetValue(item, out node);
        if (!found) return false;
        m_Dictionary.Remove(item);
        m_LinkedList.Remove(node);
        return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return m_LinkedList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Contains(T item)
    {
        return m_Dictionary.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        m_LinkedList.CopyTo(array, arrayIndex);
    }

    public void UnionWith(IEnumerable<T> value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        if (ReferenceEquals(this, value)) return;

        // Fast path: just try to add each element from the other set.
        // This preserves existing order and appends new ones in 'value' order.
        foreach (var item in value)
            Add(item);
    }

    public void RemoveWhere(Func<object, bool> predicate)
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        var node = m_LinkedList.First;
        while (node != null)
        {
            var next = node.Next; // capture before removal

            if (predicate(node.Value))
            {
                // Remove from dictionary first (by key), then from list (by node)
                m_Dictionary.Remove(node.Value);
                m_LinkedList.Remove(node);
            }

            node = next;
        }
    }
}

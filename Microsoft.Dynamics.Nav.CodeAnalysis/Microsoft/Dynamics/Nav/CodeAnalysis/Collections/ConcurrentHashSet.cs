using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Collections;

[DebuggerDisplay("Count = {Count}")]
public class ConcurrentHashSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	private readonly ConcurrentDictionary<T, byte> concurrentDictionary;

	public int Count => concurrentDictionary.Keys.Count;

	public bool IsReadOnly => false;

	public ConcurrentHashSet()
	{
		concurrentDictionary = new ConcurrentDictionary<T, byte>();
	}

	public ConcurrentHashSet(IEnumerable<T> collection)
	{
		concurrentDictionary = new ConcurrentDictionary<T, byte>();
		foreach (T item in collection)
		{
			concurrentDictionary.TryAdd(item, 0);
		}
	}

	public bool Add(T item)
	{
		return concurrentDictionary.TryAdd(item, 0);
	}

	void ICollection<T>.Add(T item)
	{
		Add(item);
	}

	public void Clear()
	{
		concurrentDictionary.Clear();
	}

	public bool Remove(T item)
	{
		byte value;
		return concurrentDictionary.TryRemove(item, out value);
	}

	public bool Contains(T item)
	{
		return concurrentDictionary.ContainsKey(item);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return concurrentDictionary.Keys.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		concurrentDictionary.Keys.CopyTo(array, arrayIndex);
	}
}

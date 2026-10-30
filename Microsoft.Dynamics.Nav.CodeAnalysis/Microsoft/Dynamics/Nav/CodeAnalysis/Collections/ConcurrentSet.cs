using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Collections;

[DebuggerDisplay("Count = {Count}")]
internal sealed class ConcurrentSet<T> : IEnumerable<T>, IEnumerable
{
	public struct KeyEnumerator
	{
		private readonly IEnumerator<KeyValuePair<T, byte>> keyValuePairEnumerator;

		public T Current => keyValuePairEnumerator.Current.Key;

		internal KeyEnumerator(IEnumerable<KeyValuePair<T, byte>> data)
		{
			keyValuePairEnumerator = data.GetEnumerator();
		}

		public bool MoveNext()
		{
			return keyValuePairEnumerator.MoveNext();
		}

		public void Reset()
		{
			keyValuePairEnumerator.Reset();
		}
	}

	private const int DefaultConcurrencyLevel = 2;

	private const int DefaultCapacity = 31;

	private readonly ConcurrentDictionary<T, byte> dictionaryProxy;

	public int Count => dictionaryProxy.Count;

	public bool IsEmpty => dictionaryProxy.IsEmpty;

	public ConcurrentSet()
	{
		dictionaryProxy = new ConcurrentDictionary<T, byte>(2, 31);
	}

	public ConcurrentSet(IEqualityComparer<T> equalityComparer)
	{
		dictionaryProxy = new ConcurrentDictionary<T, byte>(2, 31, equalityComparer);
	}

	public bool Contains(T value)
	{
		return dictionaryProxy.ContainsKey(value);
	}

	public bool Add(T value)
	{
		return dictionaryProxy.TryAdd(value, 0);
	}

	public bool Remove(T value)
	{
		byte value2;
		return dictionaryProxy.TryRemove(value, out value2);
	}

	public void Clear()
	{
		dictionaryProxy.Clear();
	}

	public KeyEnumerator GetEnumerator()
	{
		return new KeyEnumerator(dictionaryProxy);
	}

	private IEnumerator<T> GetEnumeratorImpl()
	{
		foreach (KeyValuePair<T, byte> item in dictionaryProxy)
		{
			yield return item.Key;
		}
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumeratorImpl();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumeratorImpl();
	}
}

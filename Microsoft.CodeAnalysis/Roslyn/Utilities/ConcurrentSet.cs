using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Roslyn.Utilities;

[DebuggerDisplay("Count = {Count}")]
internal sealed class ConcurrentSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	public struct KeyEnumerator
	{
		private readonly IEnumerator<KeyValuePair<T, byte>> _kvpEnumerator;

		public T Current => _kvpEnumerator.Current.Key;

		internal KeyEnumerator(IEnumerable<KeyValuePair<T, byte>> data)
		{
			_kvpEnumerator = data.GetEnumerator();
		}

		public bool MoveNext()
		{
			return _kvpEnumerator.MoveNext();
		}

		public void Reset()
		{
			_kvpEnumerator.Reset();
		}
	}

	private const int DefaultConcurrencyLevel = 2;

	private const int DefaultCapacity = 31;

	private readonly ConcurrentDictionary<T, byte> _dictionary;

	public int Count => _dictionary.Count;

	public bool IsEmpty => _dictionary.IsEmpty;

	public bool IsReadOnly => false;

	public ConcurrentSet()
	{
		_dictionary = new ConcurrentDictionary<T, byte>(2, 31);
	}

	public ConcurrentSet(IEqualityComparer<T> equalityComparer)
	{
		_dictionary = new ConcurrentDictionary<T, byte>(2, 31, equalityComparer);
	}

	public bool Contains(T value)
	{
		return _dictionary.ContainsKey(value);
	}

	public bool Add(T value)
	{
		return _dictionary.TryAdd(value, 0);
	}

	public void AddRange(IEnumerable<T> values)
	{
		if (values == null)
		{
			return;
		}
		foreach (T value in values)
		{
			Add(value);
		}
	}

	public bool Remove(T value)
	{
		byte value2;
		return _dictionary.TryRemove(value, out value2);
	}

	public void Clear()
	{
		_dictionary.Clear();
	}

	public KeyEnumerator GetEnumerator()
	{
		return new KeyEnumerator(_dictionary);
	}

	private IEnumerator<T> GetEnumeratorImpl()
	{
		foreach (KeyValuePair<T, byte> item in _dictionary)
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

	void ICollection<T>.Add(T item)
	{
		Add(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		throw new NotImplementedException();
	}
}

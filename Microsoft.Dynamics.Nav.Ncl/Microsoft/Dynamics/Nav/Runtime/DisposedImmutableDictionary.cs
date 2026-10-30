using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DisposedImmutableDictionary<TKey, TValue> : IImmutableDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	public string OwnerName { get; set; }

	public TValue this[TKey key]
	{
		get
		{
			throw new ObjectDisposedException(OwnerName);
		}
	}

	public IEnumerable<TKey> Keys
	{
		get
		{
			throw new ObjectDisposedException(OwnerName);
		}
	}

	public IEnumerable<TValue> Values
	{
		get
		{
			throw new ObjectDisposedException(OwnerName);
		}
	}

	public int Count
	{
		get
		{
			throw new ObjectDisposedException(OwnerName);
		}
	}

	public DisposedImmutableDictionary(string ownerName)
	{
		OwnerName = ownerName;
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		throw new ObjectDisposedException(OwnerName);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool ContainsKey(TKey key)
	{
		throw new ObjectDisposedException(OwnerName);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		throw new ObjectDisposedException(OwnerName);
	}

	public IImmutableDictionary<TKey, TValue> Clear()
	{
		throw new ObjectDisposedException(OwnerName);
	}

	public IImmutableDictionary<TKey, TValue> Add(TKey key, TValue value)
	{
		throw new ObjectDisposedException(OwnerName);
	}

	public IImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
	{
		throw new ObjectDisposedException(OwnerName);
	}

	public IImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value)
	{
		throw new ObjectDisposedException(OwnerName);
	}

	public IImmutableDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
	{
		throw new ObjectDisposedException(OwnerName);
	}

	public IImmutableDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
	{
		throw new ObjectDisposedException(OwnerName);
	}

	public IImmutableDictionary<TKey, TValue> Remove(TKey key)
	{
		throw new ObjectDisposedException(OwnerName);
	}

	public bool Contains(KeyValuePair<TKey, TValue> pair)
	{
		throw new ObjectDisposedException(OwnerName);
	}

	public bool TryGetKey(TKey equalKey, out TKey actualKey)
	{
		throw new ObjectDisposedException(OwnerName);
	}
}

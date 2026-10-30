using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class LockFreeDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	private IImmutableDictionary<TKey, TValue> internalDictionary;

	public virtual int Count => internalDictionary.Count;

	public bool IsDisposed { get; private set; }

	public virtual IEnumerable<TValue> Values => internalDictionary.Values;

	public LockFreeDictionary()
		: this((IEqualityComparer<TKey>)EqualityComparer<TKey>.Default, (IEqualityComparer<TValue>)EqualityComparer<TValue>.Default)
	{
	}

	public LockFreeDictionary(IEqualityComparer<TKey> keyComparer)
		: this(keyComparer, (IEqualityComparer<TValue>)EqualityComparer<TValue>.Default)
	{
	}

	public LockFreeDictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
	{
		internalDictionary = ImmutableDictionary.Create(keyComparer, valueComparer);
	}

	public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return internalDictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public virtual void AddOrUpdate(TKey key, TValue value)
	{
		AddOrUpdate(ref internalDictionary, key, value, (TKey oldKey, TValue oldValue) => value);
	}

	public virtual bool TryAdd(TKey key, TValue value)
	{
		return TryAdd(ref internalDictionary, key, value);
	}

	public virtual IEnumerable<KeyValuePair<TKey, TValue>> Clear()
	{
		IImmutableDictionary<TKey, TValue> value = internalDictionary.Clear();
		return Interlocked.Exchange(ref internalDictionary, value);
	}

	public virtual IEnumerable<KeyValuePair<TKey, TValue>> ClearAndDestroy()
	{
		IImmutableDictionary<TKey, TValue> value = new DisposedImmutableDictionary<TKey, TValue>("internalDictionary");
		IImmutableDictionary<TKey, TValue> result = Interlocked.Exchange(ref internalDictionary, value);
		IsDisposed = true;
		return result;
	}

	public virtual TValue Remove(TKey key)
	{
		TryRemove(ref internalDictionary, key, out var value);
		return value;
	}

	public virtual bool TryGetValue(TKey key, out TValue value)
	{
		return internalDictionary.TryGetValue(key, out value);
	}

	public virtual bool ContainsKey(TKey key)
	{
		return internalDictionary.ContainsKey(key);
	}

	private static TValue AddOrUpdate(ref IImmutableDictionary<TKey, TValue> location, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
	{
		IImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		TValue val;
		bool flag;
		do
		{
			val = ((!immutableDictionary.TryGetValue(key, out var value)) ? addValue : updateValueFactory(key, value));
			IImmutableDictionary<TKey, TValue> value2 = immutableDictionary.SetItem(key, val);
			IImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return val;
	}

	private static bool TryAdd(ref IImmutableDictionary<TKey, TValue> location, TKey key, TValue value)
	{
		IImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		bool flag;
		do
		{
			if (immutableDictionary.ContainsKey(key))
			{
				return false;
			}
			IImmutableDictionary<TKey, TValue> value2 = immutableDictionary.Add(key, value);
			IImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return true;
	}

	private static bool TryRemove(ref IImmutableDictionary<TKey, TValue> location, TKey key, out TValue value)
	{
		IImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		bool flag;
		do
		{
			if (!immutableDictionary.TryGetValue(key, out value))
			{
				return false;
			}
			IImmutableDictionary<TKey, TValue> value2 = immutableDictionary.Remove(key);
			IImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return true;
	}
}

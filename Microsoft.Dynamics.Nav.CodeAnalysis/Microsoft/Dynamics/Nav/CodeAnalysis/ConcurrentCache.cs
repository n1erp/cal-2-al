using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class ConcurrentCache<TKey, TValue> : CachingBase<ConcurrentCache<TKey, TValue>.Entry> where TKey : IEquatable<TKey>
{
	internal class Entry
	{
		internal int Hash { get; }

		internal TKey Key { get; }

		internal TValue Value { get; }

		internal Entry(int hash, TKey key, TValue value)
		{
			Hash = hash;
			Key = key;
			Value = value;
		}
	}

	public ConcurrentCache(int size)
		: base(size)
	{
	}

	public bool TryAdd(TKey key, TValue value)
	{
		int hashCode = key.GetHashCode();
		int num = hashCode & Mask;
		Entry entry = Entries[num];
		if (entry != null && entry.Hash == hashCode && entry.Key.Equals(key))
		{
			return false;
		}
		Entries[num] = new Entry(hashCode, key, value);
		return true;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		int hashCode = key.GetHashCode();
		int num = hashCode & Mask;
		Entry entry = Entries[num];
		if (entry != null && entry.Hash == hashCode && entry.Key.Equals(key))
		{
			value = entry.Value;
			return true;
		}
		value = default;
		return false;
	}
}

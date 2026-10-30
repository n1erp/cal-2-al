using System;

namespace Microsoft.CodeAnalysis;

internal class ConcurrentCache<TKey, TValue> : CachingBase<ConcurrentCache<TKey, TValue>.Entry> where TKey : IEquatable<TKey>
{
	internal class Entry
	{
		internal readonly int hash;

		internal readonly TKey key;

		internal readonly TValue value;

		internal Entry(int hash, TKey key, TValue value)
		{
			this.hash = hash;
			this.key = key;
			this.value = value;
		}
	}

	public ConcurrentCache(int size)
		: base(size)
	{
	}

	public bool TryAdd(TKey key, TValue value)
	{
		int hashCode = key.GetHashCode();
		int num = hashCode & mask;
		Entry entry = entries[num];
		if (entry != null && entry.hash == hashCode && entry.key.Equals(key))
		{
			return false;
		}
		entries[num] = new Entry(hashCode, key, value);
		return true;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		int hashCode = key.GetHashCode();
		int num = hashCode & mask;
		Entry entry = entries[num];
		if (entry != null && entry.hash == hashCode && entry.key.Equals(key))
		{
			value = entry.value;
			return true;
		}
		value = default;
		return false;
	}
}

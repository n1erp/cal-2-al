using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class CachingFactory<TKey, TValue> : CachingBase<CachingFactory<TKey, TValue>.Entry>
{
	internal struct Entry
	{
		internal int Hash { get; set; }

		internal TValue Value { get; set; }
	}

	private readonly int size;

	private readonly Func<TKey, TValue> valueFactory;

	private readonly Func<TKey, int> keyHash;

	private readonly Func<TKey, TValue, bool> keyValueEquality;

	public CachingFactory(int size, Func<TKey, TValue> valueFactory, Func<TKey, int> keyHash, Func<TKey, TValue, bool> keyValueEquality)
		: base(size)
	{
		this.size = size;
		this.valueFactory = valueFactory;
		this.keyHash = keyHash;
		this.keyValueEquality = keyValueEquality;
	}

	public void Add(TKey key, TValue value)
	{
		int num = GetKeyHash(key);
		int num2 = num & Mask;
		Entries[num2].Hash = num;
		Entries[num2].Value = value;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		int num = GetKeyHash(key);
		int num2 = num & Mask;
		Entry[] entries = Entries;
		if (entries[num2].Hash == num)
		{
			TValue value2 = entries[num2].Value;
			if (keyValueEquality(key, value2))
			{
				value = value2;
				return true;
			}
		}
		value = default;
		return false;
	}

	public TValue GetOrMakeValue(TKey key)
	{
		int num = GetKeyHash(key);
		int num2 = num & Mask;
		Entry[] entries = Entries;
		if (entries[num2].Hash == num)
		{
			TValue value = entries[num2].Value;
			if (keyValueEquality(key, value))
			{
				return value;
			}
		}
		TValue val = valueFactory(key);
		entries[num2].Hash = num;
		entries[num2].Value = val;
		return val;
	}

	private int GetKeyHash(TKey key)
	{
		int num = keyHash(key) | size;
		DebugAssertHelper.Assert(num != 0);
		return num;
	}
}

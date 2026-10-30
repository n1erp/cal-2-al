using System;

namespace Microsoft.CodeAnalysis;

internal class CachingFactory<TKey, TValue> : CachingBase<CachingFactory<TKey, TValue>.Entry>
{
	internal struct Entry
	{
		internal int hash;

		internal TValue value;
	}

	private readonly int _size;

	private readonly Func<TKey, TValue> _valueFactory;

	private readonly Func<TKey, int> _keyHash;

	private readonly Func<TKey, TValue, bool> _keyValueEquality;

	public CachingFactory(int size, Func<TKey, TValue> valueFactory, Func<TKey, int> keyHash, Func<TKey, TValue, bool> keyValueEquality)
		: base(size)
	{
		_size = size;
		_valueFactory = valueFactory;
		_keyHash = keyHash;
		_keyValueEquality = keyValueEquality;
	}

	public void Add(TKey key, TValue value)
	{
		int keyHash = GetKeyHash(key);
		int num = keyHash & mask;
		entries[num].hash = keyHash;
		entries[num].value = value;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		int keyHash = GetKeyHash(key);
		int num = keyHash & mask;
		Entry[] array = entries;
		if (array[num].hash == keyHash)
		{
			TValue value2 = array[num].value;
			if (_keyValueEquality(key, value2))
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
		int keyHash = GetKeyHash(key);
		int num = keyHash & mask;
		Entry[] array = entries;
		if (array[num].hash == keyHash)
		{
			TValue value = array[num].value;
			if (_keyValueEquality(key, value))
			{
				return value;
			}
		}
		TValue val = _valueFactory(key);
		array[num].hash = keyHash;
		array[num].value = val;
		return val;
	}

	private int GetKeyHash(TKey key)
	{
		return _keyHash(key) | _size;
	}
}

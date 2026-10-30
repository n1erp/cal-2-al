using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLLookupArray<TKey, TValue> where TKey : IEquatable<TKey>
{
	private KeyValuePair<TKey, TValue>[] valuePairs;

	public int Count => valuePairs.Length;

	public NCLLookupArray(KeyValuePair<TKey, TValue>[] valuePairs)
	{
		if (valuePairs == null)
		{
			throw new ArgumentNullException("valuePairs");
		}
		this.valuePairs = valuePairs;
	}

	public bool TryGetIndexFromKey(TKey key, out int index)
	{
		for (int i = 0; i < valuePairs.Length; i++)
		{
			if (valuePairs[i].Key.Equals(key))
			{
				index = i;
				return true;
			}
		}
		index = -1;
		return false;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		if (TryGetIndexFromKey(key, out var index))
		{
			value = valuePairs[index].Value;
			return true;
		}
		value = default;
		return false;
	}

	public KeyValuePair<TKey, TValue> GetKeyValuePair(int index)
	{
		return valuePairs[index];
	}
}

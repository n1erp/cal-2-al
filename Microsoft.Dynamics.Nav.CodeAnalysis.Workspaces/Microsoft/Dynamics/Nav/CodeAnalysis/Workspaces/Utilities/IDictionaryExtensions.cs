using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class IDictionaryExtensions
{
	public static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K key, Func<K, V> function)
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			value = function(key);
			dictionary.Add(key, value);
		}
		return value;
	}

	public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
	{
		if (dictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		return default;
	}

	public static bool DictionaryEquals<K, V>(this IDictionary<K, V> left, IDictionary<K, V> right, IEqualityComparer<KeyValuePair<K, V>> comparer = null)
	{
		comparer = comparer ?? EqualityComparer<KeyValuePair<K, V>>.Default;
		if (left.Count != right.Count)
		{
			return false;
		}
		return left.All((KeyValuePair<K, V> pair) => comparer.Equals(pair));
	}

	public static void MultiAdd<TKey, TValue, TCollection>(this IDictionary<TKey, TCollection> dictionary, TKey key, TValue value) where TCollection : ICollection<TValue>, new()
	{
		if (!dictionary.TryGetValue(key, out var value2))
		{
			value2 = new TCollection();
			dictionary.Add(key, value2);
		}
		value2.Add(value);
	}

	public static void MultiRemove<TKey, TValue, TCollection>(this IDictionary<TKey, TCollection> dictionary, TKey key, TValue value) where TCollection : ICollection<TValue>
	{
		if (dictionary.TryGetValue(key, out var value2))
		{
			value2.Remove(value);
			if (value2.Count == 0)
			{
				dictionary.Remove(key);
			}
		}
	}

	public static void MultiAddRange<TKey, TValue, TCollection>(this IDictionary<TKey, TCollection> dictionary, TKey key, IEnumerable<TValue> values) where TCollection : ICollection<TValue>, new()
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			value = new TCollection();
			dictionary.Add(key, value);
		}
		value.AddRange(values);
	}
}

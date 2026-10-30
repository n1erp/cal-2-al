using System;
using System.Collections.Concurrent;

namespace Roslyn.Utilities;

internal static class ConcurrentDictionaryExtensions
{
	public static void Add<K, V>(this ConcurrentDictionary<K, V> dict, K key, V value)
	{
		if (!dict.TryAdd(key, value))
		{
			throw new ArgumentException("adding a duplicate");
		}
	}
}

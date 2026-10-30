using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem.Helpers;

public static class ExtensibilityHelper
{
	public static IEnumerable<KeyValuePair<TValue, TKey>> SwapPairs<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> pairs)
	{
		return pairs.Select((KeyValuePair<TKey, TValue> kvp) => new KeyValuePair<TValue, TKey>(kvp.Value, kvp.Key));
	}

	internal static void ForEach<T>(this IEnumerable<T> elements, Action<T> action)
	{
		foreach (T element in elements)
		{
			action(element);
		}
	}
}

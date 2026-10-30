using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class KeyValuePair
{
	public static KeyValuePair<K, V> Create<K, V>(K key, V value)
	{
		return new KeyValuePair<K, V>(key, value);
	}
}

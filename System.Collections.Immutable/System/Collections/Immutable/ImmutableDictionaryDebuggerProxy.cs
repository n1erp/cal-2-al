using System.Collections.Generic;

namespace System.Collections.Immutable;

internal class ImmutableDictionaryDebuggerProxy<TKey, TValue> : ImmutableEnumerableDebuggerProxy<KeyValuePair<TKey, TValue>>
{
	public ImmutableDictionaryDebuggerProxy(IImmutableDictionary<TKey, TValue> dictionary)
		: base((IEnumerable<KeyValuePair<TKey, TValue>>)dictionary)
	{
	}
}

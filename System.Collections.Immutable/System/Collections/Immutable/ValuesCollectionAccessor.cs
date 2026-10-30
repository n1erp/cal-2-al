namespace System.Collections.Immutable;

internal class ValuesCollectionAccessor<TKey, TValue> : KeysOrValuesCollectionAccessor<TKey, TValue, TValue>
{
	internal ValuesCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionary)
		: base(dictionary, dictionary.Values)
	{
	}

	public override bool Contains(TValue item)
	{
		if (Dictionary is ImmutableSortedDictionary<TKey, TValue> immutableSortedDictionary)
		{
			return immutableSortedDictionary.ContainsValue(item);
		}
		if (Dictionary is IImmutableDictionaryInternal<TKey, TValue> immutableDictionaryInternal)
		{
			return immutableDictionaryInternal.ContainsValue(item);
		}
		throw new NotSupportedException();
	}
}

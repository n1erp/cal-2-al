namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IDictionaryCacheRemoval<TKey>
{
	NavDatabase OriginDatabase { get; }

	bool Remove(DictionaryCacheItem<TKey> item);
}

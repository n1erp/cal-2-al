namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class DictionaryCacheBase<TKey>
{
	internal abstract NavDatabase OriginDatabase { get; }

	internal abstract bool Remove(DictionaryCacheItem<TKey> item);
}

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NCLObjectMetadataLRUCacheItem<T> : DictionaryCacheItem<NCLObjectMetadataCacheKey>
{
	internal override NCLObjectMetadataCacheKey Key { get; }

	public T Value { get; }

	public NCLObjectMetadataLRUCacheItem(NCLObjectMetadataCacheKey key, T value, int size)
		: base(size)
	{
		Key = key;
		Value = value;
	}
}

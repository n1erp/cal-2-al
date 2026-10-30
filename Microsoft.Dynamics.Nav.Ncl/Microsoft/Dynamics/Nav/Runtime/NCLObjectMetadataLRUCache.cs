using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NCLObjectMetadataLRUCache<T> : DictionaryCache<NCLObjectMetadataCacheKey, NCLObjectMetadataLRUCacheItem<T>>
{
	public NCLObjectMetadataLRUCache(LRUCache lruCache, IEqualityComparer<NCLObjectMetadataCacheKey> comparer)
		: base(lruCache, comparer)
	{
	}
}

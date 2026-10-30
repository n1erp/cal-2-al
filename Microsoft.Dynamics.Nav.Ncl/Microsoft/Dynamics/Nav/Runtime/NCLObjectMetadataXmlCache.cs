using System;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NCLObjectMetadataXmlCache : INCLObjectMetadataXmlCache
{
	private readonly DictionaryCache<NCLObjectMetadataCacheKey, NCLObjectMetadataLRUCacheItem<XmlDocument>> wrappedCache;

	private readonly object metadataXmlLockObject = new object();

	public NCLObjectMetadataXmlCache(DictionaryCache<NCLObjectMetadataCacheKey, NCLObjectMetadataLRUCacheItem<XmlDocument>> lruDocumentCache)
	{
		wrappedCache = lruDocumentCache;
	}

	public bool TryGetValue(NCLObjectMetadataCacheKey key, out XmlDocument document)
	{
		document = null;
		lock (metadataXmlLockObject)
		{
			if (wrappedCache.TryGetValue(key, DateTime.MinValue, out var result))
			{
				document = result.Value;
			}
		}
		return document != null;
	}

	public void Add(NCLObjectMetadataCacheKey key, XmlDocument document)
	{
		lock (metadataXmlLockObject)
		{
			if (!wrappedCache.TryGetValueWhileKeepingLRUPosition(key, out var _))
			{
				wrappedCache.Add(new NCLObjectMetadataLRUCacheItem<XmlDocument>(key, document, 1));
			}
		}
	}
}

using System.Xml;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NCLObjectMetadataCache : INCLObjectMetadataCache
{
	private readonly INCLObjectMetadataXmlCache documentCache;

	private readonly INCLObjectMetadataBlobCache blobCache;

	private readonly object metadataXmlLockObject = new object();

	public NCLObjectMetadataCache(INCLObjectMetadataXmlCache documentCache, INCLObjectMetadataBlobCache blobCache)
	{
		this.documentCache = documentCache;
		this.blobCache = blobCache;
	}

	public XmlDocument GetOrAdd(NCLObjectMetadataCacheKey key, XmlGenerator xmlGenerator, BlobGenerator blobGenerator)
	{
		XmlDocument document;
		lock (metadataXmlLockObject)
		{
			if (!documentCache.TryGetValue(key, out document))
			{
				NavBLOB orAddBlob = blobCache.GetOrAddBlob(key, blobGenerator);
				document = xmlGenerator(orAddBlob);
				documentCache.Add(key, document);
			}
		}
		return document;
	}
}

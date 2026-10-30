using System.Xml;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NullNCLObjectMetadataCache : INCLObjectMetadataCache
{
	public XmlDocument GetOrAdd(NCLObjectMetadataCacheKey key, XmlGenerator xmlGenerator, BlobGenerator blobGenerator)
	{
		return xmlGenerator(blobGenerator(key));
	}
}

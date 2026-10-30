using System.Xml;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface INCLObjectMetadataCache
{
	XmlDocument GetOrAdd(NCLObjectMetadataCacheKey key, XmlGenerator xmlGenerator, BlobGenerator blobGenerator);
}

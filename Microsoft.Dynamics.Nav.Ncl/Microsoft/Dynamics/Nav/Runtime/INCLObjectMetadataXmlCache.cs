using System.Xml;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface INCLObjectMetadataXmlCache
{
	bool TryGetValue(NCLObjectMetadataCacheKey key, out XmlDocument document);

	void Add(NCLObjectMetadataCacheKey key, XmlDocument document);
}

using System.Xml;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLObjectXmlMetadata
{
	public XmlDocument Document { get; }

	public long Timestamp { get; }

	public NavText MetadataHash { get; }

	public int SchemaHash { get; }

	public NCLObjectXmlMetadata(XmlDocument document, long timestamp, NavText metadataHash, int schemaHash = 0)
	{
		Document = document;
		Timestamp = timestamp;
		MetadataHash = metadataHash;
		SchemaHash = schemaHash;
	}
}

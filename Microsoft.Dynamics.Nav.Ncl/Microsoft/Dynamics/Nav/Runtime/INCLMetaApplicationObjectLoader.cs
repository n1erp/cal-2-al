using Microsoft.Dynamics.Nav.Runtime.Apps;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INCLMetaApplicationObjectLoader
{
	INCLObjectXmlMetadataLoader XmlMetadataLoader { get; }

	INCLCodeLoader CodeLoader { get; }

	NCLMetadata MetadataCache { get; }

	INavAppClrTypeRetriever AppClrTypeRetriever { get; }
}

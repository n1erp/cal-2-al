using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IUserPageMetadataRetrieverV1
{
	NavXmlDelta Get(int pageId, string personalizationId);

	void Remove(int pageId);

	void Remove(int pageId, string personalizationId);

	void RemoveAll();
}

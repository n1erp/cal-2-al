using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IProfilePageMetadataRetrieverV1
{
	NavXmlDelta Get(int pageId, string personalizationId);

	NavXmlDelta Get(NavProfileKey profileKey, int pageId, string personalizationId);

	void Remove(NavProfileKey profileKey);

	void Remove(NavProfileKey profileKey, int pageId);

	void Remove(NavProfileKey profileKey, int pageId, string personalizationId);

	void RemoveAll();
}

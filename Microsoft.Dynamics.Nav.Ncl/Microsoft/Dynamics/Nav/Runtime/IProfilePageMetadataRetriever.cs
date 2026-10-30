using System;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IProfilePageMetadataRetriever
{
	NavAppObjectMetadataRuntimeDeltas Get(NavProfileKey profileKey, int pageId);

	NavAppObjectMetadataRuntimeDeltas Get(NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner owner);

	void Remove(NavProfileKey profileKey);

	void Remove(NavProfileKey profileKey, ProfilePageMetadataOwner owner);

	void Remove(NavProfileKey profileKey, int pageId);

	void Remove(NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner owner);

	void Remove(string tenantId, Guid appId);

	void RemoveAll();
}

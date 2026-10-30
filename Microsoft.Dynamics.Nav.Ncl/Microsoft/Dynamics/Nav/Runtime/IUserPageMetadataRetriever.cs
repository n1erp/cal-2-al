using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IUserPageMetadataRetriever
{
	NavAppObjectMetadataRuntimeDeltas Get(int pageId);

	void Remove(int pageId);

	void RemoveAll();
}

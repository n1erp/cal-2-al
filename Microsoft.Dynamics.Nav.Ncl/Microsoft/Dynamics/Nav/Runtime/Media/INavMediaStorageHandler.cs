using System;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

public interface INavMediaStorageHandler
{
	void CleanupExpiredFiles(NavTenant tenant);

	Uri GetDocumentUrl(Guid documentId);

	Guid ImportWithUrlAccess(NavStream stream, string fileName, int minutesToExpire);
}

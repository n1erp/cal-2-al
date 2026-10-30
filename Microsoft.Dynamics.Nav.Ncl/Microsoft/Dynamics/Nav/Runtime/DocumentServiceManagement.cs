using System.Net;
using System.Security;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class DocumentServiceManagement
{
	public static void Reset(NavSession session)
	{
		if (session.DocumentStorageService == null)
		{
			session.DocumentStorageService = new NavDocumentStorageService(session);
		}
		session.DocumentStorageService.Reset();
	}

	public static void SetDocumentServiceType(NavSession session, string documentServiceType)
	{
		session.DocumentStorageService.SetDocumentServiceType(documentServiceType);
	}

	public static ICredentials ProvideCredentials(NavSession session, out string errorMessage)
	{
		return session.DocumentStorageService.ProvideCredentials(null, out errorMessage);
	}

	public static ICredentials AuthenticateCredentials(NavSession session, string userName, SecureString securePassword, out string errorMessage)
	{
		if (!session.DocumentStorageService.AuthenticateCredentials(userName, securePassword, out var credentials, out errorMessage))
		{
			return null;
		}
		return credentials;
	}
}

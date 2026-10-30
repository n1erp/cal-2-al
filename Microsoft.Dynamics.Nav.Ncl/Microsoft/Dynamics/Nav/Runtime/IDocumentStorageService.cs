using System.IO;
using System.Net;
using System.Security;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface IDocumentStorageService
{
	void Reset();

	void SetDocumentServiceType(string documentServiceType);

	bool Enabled();

	ICredentials ProvideCredentials(ICredentials suggestedCredentials, out string errorMessage);

	bool AuthenticateCredentials(string userName, SecureString securePassword, out ICredentials credentials, out string errorMessage);

	string Save(Stream source, string targetName, bool overwrite);
}

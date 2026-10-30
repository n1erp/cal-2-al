using System.Net;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IGraphTableConnection
{
	string EntityEndpoint { get; }

	string EntityListEndpoint { get; }

	string Proxy { get; }

	string RequestHeaders { get; }

	NavDiagnostics Diagnostics { get; }

	int MaxPageSize { get; set; }

	bool DisableIfMatch { get; }

	void SetAuthorizationHeader(HttpWebRequest request);

	void ClearCredentialCache();

	string ReplaceSharedMailboxToken(string input);
}

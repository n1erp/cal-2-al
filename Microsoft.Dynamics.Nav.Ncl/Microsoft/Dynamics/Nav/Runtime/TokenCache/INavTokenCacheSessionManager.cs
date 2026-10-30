using System;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime.TokenCache;

internal interface INavTokenCacheSessionManager
{
	NavTokenCacheBase NavTokenCache { get; set; }

	Guid NavUserSecurityId { get; }

	NavDiagnostics GetMostSpecificDiagnosticInstance();

	void RunTenantActionInSystemSession(Action<NavTenant, NavSession> action);

	void RunTenantActionInSystemSessionWithoutTransaction(Action<NavTenant, NavSession> action);
}

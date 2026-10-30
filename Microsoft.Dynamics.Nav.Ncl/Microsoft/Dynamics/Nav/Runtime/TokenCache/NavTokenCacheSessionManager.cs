using System;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.TokenCache;

internal class NavTokenCacheSessionManager : INavTokenCacheSessionManager
{
	private readonly NavSession session;

	public NavTokenCacheBase NavTokenCache
	{
		get
		{
			return session.NavTokenCache;
		}
		set
		{
			session.NavTokenCache = value;
		}
	}

	public Guid NavUserSecurityId => session.User.Id;

	public NavTokenCacheSessionManager(NavSession session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		this.session = session;
	}

	public NavDiagnostics GetMostSpecificDiagnosticInstance()
	{
		return DiagnosticsResolver.GetMostSpecificInstance(session);
	}

	public void RunTenantActionInSystemSession(Action<NavTenant, NavSession> action)
	{
		((ITenantSessionHandler)session.Tenant).RunTenantActionInSystemSession(action, true, false, false, (LanguageSetting)null);
	}

	public void RunTenantActionInSystemSessionWithoutTransaction(Action<NavTenant, NavSession> action)
	{
		((ITenantSessionHandler)session.Tenant).RunTenantActionInSystemSessionWithoutTransaction(action, true, false, (LanguageSetting)null);
	}
}

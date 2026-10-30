using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface ITenantSessionHandler
{
	string Id { get; }

	IEnumerable<NavSession> ActiveSessions { get; }

	NavSession CreateSystemSession();

	NavSession CreateTaskSchedulerSession(Guid userGuid, string userName, TimeZoneInfo timeZone, string navAppId);

	NavSession GetSessionById(int sessionId);

	void RunTenantActionInSystemSession(Action<NavTenant, NavSession> action, bool throwExceptions = false, bool useCurrentCulture = false, bool allowAppsDisabledMode = false, LanguageSetting language = null);

	void RunTenantActionInSystemSessionWithExclusiveAccess(Action<NavTenant, NavSession, ExclusiveTenantActionScope> action);

	void RunTenantActionInSystemSessionWithoutTransaction(Action<NavTenant, NavSession> action, bool throwExceptions = false, bool useCurrentCulture = false, LanguageSetting language = null);

	void StopAllSessions();

	void StopAllSessionsExcept(NavSession sessionThatShouldNotBeStopped);
}

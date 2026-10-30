using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavTaskSchedulerHelpers
{
	internal static T RunCodeAsSystemSessionInTenant<T>(NavTenant tenant, Func<NavSession, NavRecord, T> code, bool allowAppsDisabledMode)
	{
		NavSession session = NavCurrentThread.Session;
		try
		{
			using NavSession navSession = ((ITenantSessionHandler)tenant).CreateSystemSession();
			navSession.Open(useUserPersonalization: false, null, allowAppsDisabledMode);
			using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000175 }))
			{
				using NavRecord arg = new NavRecord(navSession, 2000000175, SecurityFiltering.Ignored);
				try
				{
					navSession.BeginTransaction();
					T result = code(navSession, arg);
					navSession.EndTransaction(commit: true);
					return result;
				}
				catch (Exception)
				{
					navSession.EndTransaction(commit: false);
					throw;
				}
			}
		}
		finally
		{
			NavCurrentThread.Session = session;
		}
	}

	internal static void RunAsSystemSession(NavTenant tenant, Action<NavSession> action)
	{
		using NavSession navSession = ((ITenantSessionHandler)tenant).CreateSystemSession();
		navSession.Open(useUserPersonalization: false);
		action(navSession);
	}
}

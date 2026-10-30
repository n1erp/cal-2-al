using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Authentication;

internal static class AuthenticationLockoutPolicy
{
	internal static void RecordAuthenticationFailed(NavUser user, NavTenant tenant)
	{
		if (user != null && tenant != null && ServerUserSettings.Instance.LockoutPolicyFailedAuthenticationCount.Value >= 1)
		{
			string cacheKey = "FailedSignInCount_" + user.Id.ToString();
			List<DateTime> list = tenant.GetFromCache<List<DateTime>>(cacheKey) ?? new List<DateTime>();
			int detectionWindow = ServerUserSettings.Instance.LockoutPolicyFailedAuthenticationWindow.Value;
			if (detectionWindow < 1)
			{
				detectionWindow = 86400;
			}
			list.RemoveAll((DateTime t) => t < DateTime.UtcNow.AddSeconds(detectionWindow * -1));
			if (list.Count() + 1 >= ServerUserSettings.Instance.LockoutPolicyFailedAuthenticationCount.Value)
			{
				UserManagement.SetUserStateDisabled(user, tenant);
				tenant.Diagnostics.SendTraceTag("00007E7", Category.Security, Verbosity.Normal, Lang.LockoutPolicyHasDisabledUser, DiagnosticParameter.Euii(user.Id), DiagnosticParameter.SystemMetadata(list.Count()));
				EventLogWriter.Write(string.Format(CultureInfo.CurrentCulture, Lang.LockoutPolicyHasDisabledUser, user.Name, list.Count()), EventLogEntryType.FailureAudit, null, 0, null, 0);
				tenant.RemoveFromCache(cacheKey);
			}
			else
			{
				tenant.RemoveFromCache(cacheKey);
				list.Add(DateTime.UtcNow);
				tenant.AddToCache(cacheKey, list, list.Last().AddSeconds(detectionWindow), TimeSpan.FromSeconds(detectionWindow));
			}
		}
	}

	internal static void RecordAuthenticationSucceeded(NavUser user, NavTenant tenant)
	{
		if (user != null && tenant != null && ServerUserSettings.Instance.LockoutPolicyFailedAuthenticationCount.Value >= 1)
		{
			string cacheKey = "FailedSignInCount_" + user.Id.ToString();
			tenant.RemoveFromCache(cacheKey);
		}
	}
}

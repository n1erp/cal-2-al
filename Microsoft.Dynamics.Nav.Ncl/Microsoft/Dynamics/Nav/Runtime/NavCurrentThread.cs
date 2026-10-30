using System;
using System.Globalization;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavCurrentThread
{
	public static NavSession Session
	{
		get
		{
			return NavThreadLocalStorage.Current.Session?.Target;
		}
		set
		{
			NavThreadLocalStorage.Current.Session = value?.GetReference();
			NavThreadLocalStorage.Current.SqlDebuggingStatics = value?.SqlDebuggingStatistics;
		}
	}

	public static Guid? ClientActivityId
	{
		get
		{
			return NavThreadLocalStorage.Current.ClientActivityId ?? (NavThreadLocalStorage.Current.ClientActivityId = ServiceCallCorrelationProvider.GetCorrelationId(CorrelationIdType.ClientActivityId));
		}
		set
		{
			NavThreadLocalStorage.Current.ClientActivityId = value;
		}
	}

	public static Guid? ClientSessionId
	{
		get
		{
			return NavThreadLocalStorage.Current.ClientSessionId ?? (NavThreadLocalStorage.Current.ClientSessionId = ServiceCallCorrelationProvider.GetCorrelationId(CorrelationIdType.ClientSessionId));
		}
		set
		{
			NavThreadLocalStorage.Current.ClientSessionId = value;
		}
	}

	public static OperationContext OperationContext
	{
		get
		{
			return NavThreadLocalStorage.Current.OperationContext ?? OperationContext.Current;
		}
		set
		{
			NavThreadLocalStorage.Current.OperationContext = value;
		}
	}

	public static int SessionId
	{
		get
		{
			int result = -1;
			if (Session != null)
			{
				result = Session.Id;
			}
			return result;
		}
	}

	public static string UserName
	{
		get
		{
			string result = string.Empty;
			if (Session != null)
			{
				result = Session.UserDisplayName;
			}
			return result;
		}
	}

	public static bool InitializingAppGroup
	{
		get
		{
			return NavThreadLocalStorage.Current.InitializingAppGroup;
		}
		set
		{
			NavThreadLocalStorage.Current.InitializingAppGroup = value;
		}
	}

	public static NavAppInstallationContext AppInstallationContext => Session?.AppInstallationContext;

	public static AppSyncContext AppSyncContext => Session?.AppSyncContext;

	public static NavAppUpgradeContext AppUpgradeContext => Session?.AppUpgradeContext;

	public static string FormPersonalizationId
	{
		get
		{
			return NavThreadLocalStorage.Current.FormPersonalizationId;
		}
		set
		{
			NavThreadLocalStorage.Current.FormPersonalizationId = value;
		}
	}

	public static string DrillDownPersonalizationId
	{
		get
		{
			return NavThreadLocalStorage.Current.DrillDownPersonalizationId;
		}
		set
		{
			NavThreadLocalStorage.Current.DrillDownPersonalizationId = value;
		}
	}

	public static NavAppGroup ResolveAppGroup(NavSession preferredSession = null)
	{
		NavAppGroup navAppGroup = TryResolveAppGroup(preferredSession);
		if (navAppGroup == null)
		{
			return NavAppGroup.BaseGroup;
		}
		return navAppGroup;
	}

	public static NavAppGroup TryResolveAppGroup(NavSession preferredSession = null)
	{
		return NavThreadLocalStorage.Current.OverriddenAppGroup ?? preferredSession?.NavAppGroup ?? Session?.NavAppGroup;
	}

	public static void SetOverriddenAppGroups(NavAppGroup appGroup)
	{
		if (NavThreadLocalStorage.Current.OverriddenAppGroup != null)
		{
			string original = string.Format(CultureInfo.InvariantCulture, "Extension group override already set -- ExistingAllGroup:{0}; NewAllGroup:{1}", NavThreadLocalStorage.Current.OverriddenAppGroup.GroupId, appGroup.GroupId);
			original = TraceStringHelper.AppendStackTrace(original);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000OO", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, original);
			throw new InvalidOperationException("The extension group override could not be set because an override is already in effect.");
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000H0", Category.Extensions, Verbosity.Normal, "Setting extension group override -- AllGroup:{0}", DiagnosticParameter.SystemMetadata(appGroup.GroupId));
		NavThreadLocalStorage.Current.OverriddenAppGroup = appGroup;
	}

	public static void ClearOverriddenAppGroup()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000H2", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing extension group override");
		NavThreadLocalStorage.Current.OverriddenAppGroup = null;
	}
}

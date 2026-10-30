using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public static class NavUpgradeEventConfiguration
{
	private static readonly ApplicationObjectId SystemCodeunit = new ApplicationObjectId(ObjectType.CodeUnit, 2000000008);

	private static readonly List<NavEventSubscriptionSettingsEntry> settingsEntries = new List<NavEventSubscriptionSettingsEntry>
	{
		new NavEventSubscriptionSettingsEntry(SystemCodeunit, "OnAfterUpgradeCommitPerDatabase", NavEventSubscriptionSettings.IsolatedMode),
		new NavEventSubscriptionSettingsEntry(SystemCodeunit, "OnAfterUpgradeCommitPerCompany", NavEventSubscriptionSettings.IsolatedMode)
	};

	private static readonly List<NavEventExclusionFilter> exclusionFilters = new List<NavEventExclusionFilter>
	{
		new NavEventExclusionFilter(SystemCodeunit, "OnCheckPreconditionsPerDatabase", ExcludeFromInvokeBecauseNotPartOfUpgradeContext),
		new NavEventExclusionFilter(SystemCodeunit, "OnCheckPreconditionsPerCompany", ExcludeFromInvokeBecauseNotPartOfUpgradeContext),
		new NavEventExclusionFilter(SystemCodeunit, "OnUpgradePerDatabase", ExcludeFromInvokeBecauseNotPartOfUpgradeContext),
		new NavEventExclusionFilter(SystemCodeunit, "OnUpgradePerCompany", ExcludeFromInvokeBecauseNotPartOfUpgradeContext),
		new NavEventExclusionFilter(SystemCodeunit, "OnValidateUpgradePerDatabase", ExcludeFromInvokeBecauseNotPartOfUpgradeContext),
		new NavEventExclusionFilter(SystemCodeunit, "OnValidateUpgradePerCompany", ExcludeFromInvokeBecauseNotPartOfUpgradeContext),
		new NavEventExclusionFilter(SystemCodeunit, "OnAfterUpgradeCommitPerDatabase", ExcludeFromInvokeBecauseNotPartOfUpgradeContext),
		new NavEventExclusionFilter(SystemCodeunit, "OnAfterUpgradeCommitPerCompany", ExcludeFromInvokeBecauseNotPartOfUpgradeContext)
	};

	private static readonly List<NavEventErrorReporter> errorReporters = new List<NavEventErrorReporter>
	{
		new NavEventErrorReporter(SystemCodeunit, "OnAfterUpgradeCommitPerDatabase", ReportIsolatedEventError),
		new NavEventErrorReporter(SystemCodeunit, "OnAfterUpgradeCommitPerCompany", ReportIsolatedEventError)
	};

	public static IEnumerable<NavEventSubscriptionSettingsEntry> SettingsEntries => settingsEntries;

	public static IEnumerable<NavEventExclusionFilter> ExclusionFilters => exclusionFilters;

	public static IEnumerable<NavEventErrorReporter> ErrorReporters => errorReporters;

	private static bool ExcludeFromInvokeBecauseNotPartOfUpgradeContext(NavEventSubscription sub, NavApplicationObjectBase inst)
	{
		bool flag = false;
		NavCodeunit navCodeunit = inst as NavCodeunit;
		if (navCodeunit?.MetaCodeunit != null)
		{
			NavAppUpgradeContext appUpgradeContext = NavCurrentThread.AppUpgradeContext;
			object obj;
			if (appUpgradeContext == null)
			{
				obj = null;
			}
			else
			{
				NavAppRuntimeMetadata appBeingTargeted = appUpgradeContext.AppBeingTargeted;
				obj = ((appBeingTargeted != null) ? appBeingTargeted.AppId : null);
			}
			AppId val = (AppId)obj;
			NavAppRuntimeMetadata val2 = navCodeunit.MetaCodeunit?.OwningApp;
			if (appUpgradeContext == null)
			{
				flag = val2 == null;
			}
			else if ((WrappedComparable<Guid>)(object)val != (WrappedComparable<Guid>)null)
			{
				flag = (WrappedComparable<Guid>)(object)((val2 != null) ? val2.AppId : null) == (WrappedComparable<Guid>)(object)val;
			}
		}
		return !flag;
	}

	private static void ReportIsolatedEventError(string errorMessage, Exception exception)
	{
		NavCurrentThread.Session?.IsolatedEventErrorReporter?.ReportError(errorMessage, exception);
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public static class NavInstallEventConfiguration
{
	private static readonly ApplicationObjectId SystemCodeunit = new ApplicationObjectId(ObjectType.CodeUnit, 2000000010);

	private static readonly List<NavEventExclusionFilter> exclusionFilters = new List<NavEventExclusionFilter>
	{
		new NavEventExclusionFilter(SystemCodeunit, "OnInstallAppPerDatabase", ExcludeFromInvokeBecauseNotPartOfInstallContext),
		new NavEventExclusionFilter(SystemCodeunit, "OnInstallAppPerCompany", ExcludeFromInvokeBecauseNotPartOfInstallContext)
	};

	public static IEnumerable<NavEventExclusionFilter> ExclusionFilters => exclusionFilters;

	private static bool ExcludeFromInvokeBecauseNotPartOfInstallContext(NavEventSubscription sub, NavApplicationObjectBase inst)
	{
		bool flag = false;
		NavAppInstallationContext appInstallationContext = NavCurrentThread.AppInstallationContext;
		object obj;
		if (appInstallationContext == null)
		{
			obj = null;
		}
		else
		{
			NavAppRuntimeMetadata appBeingTargeted = appInstallationContext.AppBeingTargeted;
			obj = ((appBeingTargeted != null) ? appBeingTargeted.AppId : null);
		}
		AppId val = (AppId)obj;
		if (inst is NavCodeunit navCodeunit && (WrappedComparable<Guid>)(object)val != (WrappedComparable<Guid>)null)
		{
			NCLMetaCodeunit metaCodeunit = navCodeunit.MetaCodeunit;
			object obj2;
			if (metaCodeunit == null)
			{
				obj2 = null;
			}
			else
			{
				NavAppRuntimeMetadata owningApp = metaCodeunit.OwningApp;
				obj2 = ((owningApp != null) ? owningApp.AppId : null);
			}
			flag = (WrappedComparable<Guid>)obj2 == (WrappedComparable<Guid>)(object)val;
		}
		return !flag;
	}
}

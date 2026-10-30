using System;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Exceptions.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppGroupUpdater
{
	private readonly INavAppGroupResolver navAppGroupResolver;

	public NavAppGroupUpdater(INavAppGroupResolver groupResolver)
	{
		navAppGroupResolver = groupResolver;
	}

	public void UpdateGroupForChangedTenant(NavTenant tenant)
	{
		NavAppGroup navAppGroup = navAppGroupResolver.GetOrCreateGroupForTenant(tenant.Id);
		while (tenant.DatabaseDefinedNavAppGroup != navAppGroup)
		{
			try
			{
				NavGlobal.SystemTenant.NCLMetadata.InitializeAppGroup(navAppGroup);
			}
			catch (NavNCLMetadataObjectNotFoundException)
			{
				NavAppGroup orCreateGroupForTenant = navAppGroupResolver.GetOrCreateGroupForTenant(tenant.Id);
				if (orCreateGroupForTenant != navAppGroup)
				{
					navAppGroup = orCreateGroupForTenant;
					continue;
				}
				throw;
			}
			using (new NavAppGroupCurrentThreadOverrideScope(navAppGroup))
			{
				NavGlobal.SystemTenant.EventSubscriptionMetadata.InitializeAppGroupEvents(navAppGroup);
				ConfigureRoutes(navAppGroup);
			}
			tenant.UpdateDatabaseDefinedAppGroups(navAppGroup);
		}
	}

	public void PrepareAppGroup(NavAppGroup group)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000O7", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Initializing extension group -- Group:{0}", group.GroupId);
		NavGlobal.SystemTenant.NCLMetadata.InitializeAppGroup(group);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000O8", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Initializing extension group events -- Group:{0}", group.GroupId);
		NavGlobal.SystemTenant.EventSubscriptionMetadata.InitializeAppGroupEvents(group);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000021O", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Configuring extension group HTTP routes -- Group:{0}", group.GroupId);
		ConfigureRoutes(group);
	}

	private static void ConfigureRoutes(NavAppGroup group)
	{
		try
		{
			NavGlobal.NavAppRoutesUpdater?.ConfigureRoutes(group);
		}
		catch (NavAppDuplicateApiEndpointException)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000021P", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "Configuring HTTP routes failed because of duplicated API endpoints -- Group:{0}", group.GroupId);
			throw;
		}
		catch (Exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000021Q", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "Configuring HTTP routes failed -- Group:{0}", group.GroupId);
			throw;
		}
	}
}

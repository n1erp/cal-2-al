using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class TenantDatabaseConfiguration
{
	public string Id { get; private set; }

	public string DatabaseServer { get; private set; }

	public string DatabaseName { get; private set; }

	public string DatabaseUserName { get; private set; }

	public ProtectedDatabasePassword DatabasePassword { get; private set; }

	public byte[] Timestamp { get; }

	public TenantDatabaseConfiguration(byte[] timestamp, string id, string databaseServer, string databaseName, string databaseUserName, ProtectedDatabasePassword databasePassword)
	{
		Timestamp = timestamp;
		Id = id;
		DatabaseServer = databaseServer;
		DatabaseName = databaseName;
		DatabaseUserName = databaseUserName;
		DatabasePassword = databasePassword;
	}

	public TenantDatabaseConfiguration(string id, string databaseServer, string databaseName, string databaseUserName, ProtectedDatabasePassword databasePassword)
		: this(null, id, databaseServer, databaseName, databaseUserName, databasePassword)
	{
	}

	public DatabaseInfo ToDatabaseInfo()
	{
		return new DatabaseInfo
		{
			Server = DatabaseServer,
			Name = DatabaseName,
			UserName = DatabaseUserName,
			Password = DatabasePassword
		};
	}

	public static NavTenantDatabaseSettingsDetailedState CalculateTenantDatabaseState(NavDatabase tenantDatabase, List<string> tenantDatabaseVersions, string tenantDatabaseApplicationFamily, string nstApplicationVersion, string nstApplicationFamily)
	{
		if (tenantDatabaseVersions == null)
		{
			return new NavTenantDatabaseSettingsDetailedState(NavTenantDatabaseState.PendingSync);
		}
		if (!nstApplicationFamily.Equals(tenantDatabaseApplicationFamily))
		{
			return new NavTenantDatabaseSettingsDetailedState(NavTenantDatabaseState.IncompatibleVersion, Lang.IncompatibleApplicationFamily.FormatWithResolvedCulture(tenantDatabaseApplicationFamily, nstApplicationFamily));
		}
		if (tenantDatabaseVersions.Contains(nstApplicationVersion))
		{
			return CalculateTenantDatabaseExtensionState(tenantDatabase);
		}
		if (IsApplicationVersionSynchronizable(tenantDatabaseVersions, nstApplicationVersion))
		{
			return new NavTenantDatabaseSettingsDetailedState(NavTenantDatabaseState.PendingSync);
		}
		return new NavTenantDatabaseSettingsDetailedState(NavTenantDatabaseState.IncompatibleVersion, Lang.IncompatibleTenantDatabaseVersion.FormatWithResolvedCulture(tenantDatabaseVersions.FirstOrDefault() ?? string.Empty, nstApplicationVersion));
	}

	internal static NavTenantDatabaseSettingsDetailedState CalculateTenantDatabaseExtensionState(NavDatabase tenantDatabase)
	{
		NavTenantDatabaseState state = NavTenantDatabaseState.Operational;
		Dictionary<AppId, List<NavAppRuntimeMetadata>> dictionary = (from m in NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadata()
			group m by m.AppId).ToDictionary((IGrouping<AppId, NavAppRuntimeMetadata> g) => g.Key, (IGrouping<AppId, NavAppRuntimeMetadata> g) => g.ToList());
		List<string> list = new List<string>();
		List<AppSyncTrackingRecord> list2;
		using (NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(tenantDatabase))
		{
			list2 = new DatabaseAppSyncTrackingRepository(tenantDatabase, conScope).GetAll().ToList();
		}
		foreach (AppSyncTrackingRecord syncRecord in list2)
		{
			if (dictionary.ContainsKey(syncRecord.AppId) && dictionary[syncRecord.AppId].All((NavAppRuntimeMetadata p) => p.Version > syncRecord.Version))
			{
				state = NavTenantDatabaseState.PendingExtensionSync;
				list.Add(NavAppReferenceExtensions.FriendlyDisplayName((INavAppReference)(object)syncRecord));
			}
		}
		return new NavTenantDatabaseSettingsDetailedState(state, (!list.Any()) ? string.Empty : Lang.NavAppSync_ExtensionNotSynced.FormatWithResolvedCulture(string.Join("; ", list)));
	}

	public static NavTenantDatabaseSettingsDetailedState CalculateTenantDatabaseState(NavDatabase tenantDatabase)
	{
		KeyValuePair<string, List<string>> supportedFamilyAndVersions = NavSqlTenantDatabaseVersion.GetSupportedFamilyAndVersions(tenantDatabase);
		return CalculateTenantDatabaseState(tenantDatabase, supportedFamilyAndVersions.Value, supportedFamilyAndVersions.Key, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily);
	}

	internal static bool IsApplicationVersionSynchronizable(List<string> supportedVersions, string serverApplicationVersion)
	{
		if (supportedVersions == null || !supportedVersions.Any())
		{
			return true;
		}
		if (!Version.TryParse(supportedVersions.First(), out var result))
		{
			return false;
		}
		if (!Version.TryParse(serverApplicationVersion, out var result2))
		{
			return false;
		}
		return result2 >= result;
	}

	internal static bool IsApplicationVersionMountable(List<string> supportedVersions, string serverApplicationVersion)
	{
		if (IsApplicationVersionSynchronizable(supportedVersions, serverApplicationVersion))
		{
			return true;
		}
		return supportedVersions.Contains(serverApplicationVersion);
	}

	internal static bool IsApplicationFamilyCompatible(string tenantApplicationFamily, string serverApplicationFamily)
	{
		if (string.IsNullOrEmpty(tenantApplicationFamily))
		{
			return true;
		}
		return tenantApplicationFamily.Equals(serverApplicationFamily, StringComparison.OrdinalIgnoreCase);
	}
}

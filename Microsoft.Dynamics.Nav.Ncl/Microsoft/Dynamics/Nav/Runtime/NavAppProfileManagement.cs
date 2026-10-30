using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavAppProfileManagement
{
	public static void AddProfileArtifacts(NavTenant tenant, NavSqlConnectionScope conScope, AppId appId, PackageId packageId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000754", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Adding profile artifacts -- Tenant:{0}; AppId:{1}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value);
		Stopwatch stopwatch = Stopwatch.StartNew();
		int num = 0;
		int num2 = 0;
		NavAppResourceId id = new NavAppResourceId(packageId, NavAppResourceType.Profile, NavAppProfileResourceNames.Metadata);
		NavAppResource resource = NavEnvironment.Instance.NavAppMetadataRetriever.GetResource(id);
		if (resource != NavAppResource.None)
		{
			NavAppCombinedProfileMetadata val = NavAppCombinedProfileMetadata.Deserialize(resource.Content);
			num = val.ProfileMetadata.Count;
			foreach (NavAppProfileMetadata profileMetadata in val.ProfileMetadata)
			{
				InsertProfile(tenant, conScope, appId, profileMetadata);
				num2 += InsertProfilePageMetadata(tenant, conScope, val.ProfilePageMetadata.Where((NavAppProfilePageMetadata ppm) => profileMetadata.PageExtensionObjectNames.Contains(ppm.Name)), profileMetadata.ProfileId, appId);
			}
			NavEnvironment.Instance.NavProfilePageMetadataCache.Remove(tenant.Id, ((WrappedValue<Guid>)(object)appId).Value);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000755", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Added profile artifacts -- Tenant:{0}; Profiles:{1}; ProfilePageCustomizations:{2}; Elapsed:{3}", tenant.Id, num, num2, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	public static void RemoveProfileArtifacts(NavTenant tenant, NavSqlConnectionScope conScope, AppId appId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000756", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removing profile artifacts -- Tenant:{0}; AppId:{1}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value);
		Stopwatch stopwatch = Stopwatch.StartNew();
		DeleteProfiles(tenant, conScope, ((WrappedValue<Guid>)(object)appId).Value);
		DeleteProfilePageMetadata(tenant, conScope, ((WrappedValue<Guid>)(object)appId).Value);
		NavEnvironment.Instance.NavProfilePageMetadataCache.Remove(tenant.Id, ((WrappedValue<Guid>)(object)appId).Value);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000757", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removing tenant profile metadata -- Tenant:{0}; AppId:{1}; Elapsed:{2}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	private static void InsertProfile(NavTenant tenant, NavSqlConnectionScope conScope, AppId appId, NavAppProfileMetadata profileMetadata)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "INSERT INTO [Tenant Profile] ( [Profile ID], [App ID], [Description], [Role Center ID],[Default Role Center], [Disable Personalization] {0}) VALUES (@0, @1, @2, @3, @4, @5{1})", tenant.Database.IsSharedTenantSchema ? ", \"$companyId\"" : string.Empty, tenant.Database.IsSharedTenantSchema ? ",@6" : string.Empty));
		navSqlCommand.Command.Parameters.Add(0, SqlDbType.NVarChar, 240, profileMetadata.ProfileId);
		navSqlCommand.Command.Parameters.Add(1, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)appId).Value);
		navSqlCommand.Command.Parameters.Add(2, SqlDbType.NVarChar, 240, profileMetadata.Description);
		navSqlCommand.Command.Parameters.Add(3, SqlDbType.Int, profileMetadata.RoleCenterPageId);
		navSqlCommand.Command.Parameters.Add(4, SqlDbType.TinyInt, false);
		navSqlCommand.Command.Parameters.Add(5, SqlDbType.TinyInt, false);
		if (tenant.Database.IsSharedTenantSchema)
		{
			navSqlCommand.Command.Parameters.Add(6, SqlDbType.Int, tenant.Database.GetCompanyIdForSystemTables(2000000187));
		}
		navSqlCommand.PrepareStatement();
		navSqlCommand.ExecuteNonQuery();
	}

	private static int InsertProfilePageMetadata(NavTenant tenant, NavSqlConnectionScope conScope, IEnumerable<NavAppProfilePageMetadata> entries, string profileId, AppId appId)
	{
		foreach (NavAppProfilePageMetadata entry in entries)
		{
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
			navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "INSERT INTO [Tenant Profile Page Metadata] ( [Profile ID], [Page ID],[App ID], [Owner], [Page Metadata], [Page AL]{0}) VALUES (@0, @1, @2, @3, @4, @5{1})", tenant.Database.IsSharedTenantSchema ? ", \"$companyId\"" : string.Empty, tenant.Database.IsSharedTenantSchema ? ", @6" : string.Empty));
			navSqlCommand.Command.Parameters.Add(0, SqlDbType.NVarChar, 240, profileId);
			navSqlCommand.Command.Parameters.Add(1, SqlDbType.Int, entry.TargetPageId);
			navSqlCommand.Command.Parameters.Add(2, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)appId).Value);
			navSqlCommand.Command.Parameters.Add(3, SqlDbType.Int, 0);
			byte[] bytes = Encoding.UTF8.GetBytes(entry.Metadata);
			using (NavBLOB navBLOB = NavBLOB.CreateFromObject(bytes))
			{
				object sqlWritableValue = navBLOB.GetSqlWritableValue(SqlDataType.Default, int.MaxValue, compressed: true);
				navSqlCommand.Command.Parameters.Add(4, SqlDbType.Binary, int.MaxValue, sqlWritableValue);
			}
			byte[] value = ((entry.UserALCode == null) ? new byte[0] : Encoding.UTF8.GetBytes(entry.UserALCode));
			using (NavBLOB navBLOB2 = NavBLOB.CreateFromObject(value))
			{
				object sqlWritableValue2 = navBLOB2.GetSqlWritableValue(SqlDataType.Default, int.MaxValue, compressed: true);
				navSqlCommand.Command.Parameters.Add(5, SqlDbType.Binary, int.MaxValue, sqlWritableValue2);
			}
			if (tenant.Database.IsSharedTenantSchema)
			{
				navSqlCommand.Command.Parameters.Add(6, SqlDbType.Int, tenant.Database.GetCompanyIdForSystemTables(2000000177));
			}
			navSqlCommand.PrepareStatement();
			navSqlCommand.ExecuteNonQuery();
		}
		return entries.Count();
	}

	private static void DeleteProfiles(NavTenant tenant, NavSqlConnectionScope conScope, Guid appId)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "Delete FROM [Tenant Profile] WHERE {0} [App ID] = @0 ", tenant.Database.IsSharedTenantSchema ? "\"$companyId\" = @1 AND " : string.Empty));
		navSqlCommand.Command.Parameters.Add(0, SqlDbType.UniqueIdentifier, appId);
		if (tenant.Database.IsSharedTenantSchema)
		{
			navSqlCommand.Command.Parameters.Add(1, SqlDbType.Int, tenant.Database.GetCompanyIdForSystemTables(2000000177));
		}
		navSqlCommand.PrepareStatement();
		navSqlCommand.ExecuteNonQuery();
	}

	private static void DeleteProfilePageMetadata(NavTenant tenant, NavSqlConnectionScope conScope, Guid appId)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "Delete FROM [Tenant Profile Page Metadata] WHERE {0} [App ID] = @0 AND [Owner] = @1 ", tenant.Database.IsSharedTenantSchema ? "\"$companyId\" = @2 AND " : string.Empty));
		navSqlCommand.Command.Parameters.Add(0, SqlDbType.UniqueIdentifier, appId);
		navSqlCommand.Command.Parameters.Add(1, SqlDbType.Int, ProfilePageMetadataOwner.SystemOrApp);
		if (tenant.Database.IsSharedTenantSchema)
		{
			navSqlCommand.Command.Parameters.Add(2, SqlDbType.Int, tenant.Database.GetCompanyIdForSystemTables(2000000187));
		}
		navSqlCommand.PrepareStatement();
		navSqlCommand.ExecuteNonQuery();
	}
}

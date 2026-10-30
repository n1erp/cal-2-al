using System;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class NavAppUpgradeSpecialRecordsHandler
{
	public static void HandleSpecialRecordsForUpgrade(NavTenant tenant, NavSqlConnectionScope tenantDatabaseScope, NavAppRuntimeMetadata newApp, INavAppReference oldApp)
	{
		if (tenant == null)
		{
			throw new ArgumentNullException("tenant");
		}
		if (tenantDatabaseScope == null)
		{
			throw new ArgumentNullException("tenantDatabaseScope");
		}
		if (newApp == null)
		{
			throw new ArgumentNullException("newApp");
		}
		if (oldApp == null)
		{
			throw new ArgumentNullException("oldApp");
		}
		Stream packageBlob = NavEnvironment.Instance.NavAppMetadataRetriever.GetPackageBlob(newApp.PackageId);
		NavAppInstallationManagement.RemoveInstalledAppRecord(tenant, tenantDatabaseScope, oldApp.AppId);
		NavAppPermissionManagement.RemovePermissionArtifacts(tenant, tenantDatabaseScope, oldApp.AppId);
		NavAppWebServiceManagement.RemoveWebServices(tenant, tenantDatabaseScope, oldApp.AppId, oldApp.Version);
		NavAppControlAddInManagement.RemoveControlAddins(tenant, tenantDatabaseScope, oldApp.AppId);
		NavAppReportLayoutManagement.RemoveCustomReportLayouts(tenant, tenantDatabaseScope, oldApp.AppId);
		NavAppProfileManagement.RemoveProfileArtifacts(tenant, tenantDatabaseScope, oldApp.AppId);
		NavAppInstallationManagement.AddInstalledAppRecord(tenant, tenantDatabaseScope, (INavAppMetadataReference)(object)newApp);
		NavAppPermissionManagement.AddPermissionArtifacts(tenant, tenantDatabaseScope, packageBlob, newApp.AppId);
		NavAppWebServiceManagement.AddWebServices(tenant, tenantDatabaseScope, packageBlob, newApp.AppId);
		NavAppControlAddInManagement.AddControlAddIns(tenant, tenantDatabaseScope, packageBlob, newApp.AppId);
		NavAppReportLayoutManagement.AddCustomReportLayouts(tenant, packageBlob, newApp.AppId);
		NavAppProfileManagement.AddProfileArtifacts(tenant, tenantDatabaseScope, newApp.AppId, newApp.PackageId);
	}
}

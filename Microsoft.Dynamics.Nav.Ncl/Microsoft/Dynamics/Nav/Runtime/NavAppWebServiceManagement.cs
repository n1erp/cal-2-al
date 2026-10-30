using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Apps.AppXmlObjects;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavAppWebServiceManagement
{
	public static void AddWebServices(NavTenant tenant, NavSqlConnectionScope conScope, Stream packageStream, AppId appId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000075C", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Adding tenant web service records -- Tenant:{0}; AppId:{1}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value);
		Stopwatch stopwatch = Stopwatch.StartNew();
		List<NavAppTenantWebServiceEntry> list = ExtractWebServices(packageStream);
		InsertWebServices(tenant, conScope, list);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000075D", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Added tenant web service records -- Tenant:{0}; AppId:{1}; Services:{2}; Elapsed:{3}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value, list.Count, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	public static void RemoveWebServices(NavTenant tenant, NavSqlConnectionScope conScope, Stream packageStream, AppId appId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000075E", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removing tenant web services -- Tenant:{0}; AppId:{1}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value);
		Stopwatch stopwatch = Stopwatch.StartNew();
		int num = 0;
		List<NavAppTenantWebServiceEntry> list = ExtractWebServices(packageStream);
		SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> snapshotOfAllObjects = NavGlobal.NCLMetadata.GetSnapshotOfAllObjects();
		foreach (NavAppTenantWebServiceEntry item in list)
		{
			if (!snapshotOfAllObjects.ContainsKey(item.ObjectType) || !snapshotOfAllObjects[item.ObjectType].ContainsKey(item.ObjectId))
			{
				DeleteWebService(tenant, conScope, item);
				num++;
			}
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000075F", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removed tenant web services -- Tenant:{0}; AppId:{1}; Services:{2}; Elapsed:{3}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value, num, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	public static void RemoveWebServices(NavTenant tenant, NavSqlConnectionScope conScope, AppId appId, Version version)
	{
		bool flag = false;
		NavAppRuntimeMetadata val = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadata().FirstOrDefault((NavAppRuntimeMetadata m) => (WrappedComparable<Guid>)(object)m.AppId == (WrappedComparable<Guid>)(object)appId && m.Version == version);
		if (val != null)
		{
			Stream packageBlob = NavEnvironment.Instance.NavAppMetadataRetriever.GetPackageBlob(val.PackageId);
			if (packageBlob != null)
			{
				RemoveWebServices(tenant, conScope, packageBlob, val.AppId);
				flag = true;
			}
		}
		if (!flag)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001F9", Category.Extensions, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Old package not found, cannot remove web services");
		}
	}

	private static void DeleteWebService(NavTenant tenant, NavSqlConnectionScope conScope, NavAppTenantWebServiceEntry service)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "DELETE FROM [Tenant Web Service] WHERE {0}[Object Type] = @0 AND [Service Name] = @1 AND [Object ID] = @2", tenant.Database.IsSharedTenantSchema ? "\"$companyId\" = @3 AND " : string.Empty));
		navSqlCommand.Command.Parameters.Add(0, SqlDbType.Int, service.ObjectType);
		navSqlCommand.Command.Parameters.Add(1, SqlDbType.NVarChar, 240, service.ServiceName);
		navSqlCommand.Command.Parameters.Add(2, SqlDbType.Int, service.ObjectId);
		if (tenant.Database.IsSharedTenantSchema)
		{
			navSqlCommand.Command.Parameters.Add(3, SqlDbType.Int, tenant.Database.GetCompanyIdForSystemTables(2000000168));
		}
		navSqlCommand.PrepareStatement();
		navSqlCommand.ExecuteNonQuery();
	}

	private static void InsertWebServices(NavTenant tenant, NavSqlConnectionScope conScope, IEnumerable<NavAppTenantWebServiceEntry> entries)
	{
		foreach (NavAppTenantWebServiceEntry entry in entries)
		{
			try
			{
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
				navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM [Tenant Web Service] WHERE {0} [Object Type] = @0 AND [Service Name] = @1 AND [Object ID] = @2) INSERT INTO [Tenant Web Service] ([Object Type], [Service Name], [Object ID], [Published]{1}) VALUES (@0, @1, @2, @3{2})", tenant.Database.IsSharedTenantSchema ? "\"$companyId\" = @4 AND " : string.Empty, tenant.Database.IsSharedTenantSchema ? ", \"$companyId\"" : string.Empty, tenant.Database.IsSharedTenantSchema ? ",@4" : string.Empty));
				navSqlCommand.Command.Parameters.Add(0, SqlDbType.Int, entry.ObjectType);
				navSqlCommand.Command.Parameters.Add(1, SqlDbType.NVarChar, 240, entry.ServiceName);
				navSqlCommand.Command.Parameters.Add(2, SqlDbType.Int, entry.ObjectId);
				navSqlCommand.Command.Parameters.Add(3, SqlDbType.TinyInt, entry.Published);
				if (tenant.Database.IsSharedTenantSchema)
				{
					navSqlCommand.Command.Parameters.Add(4, SqlDbType.Int, tenant.Database.GetCompanyIdForSystemTables(2000000168));
				}
				navSqlCommand.PrepareStatement();
				navSqlCommand.ExecuteNonQuery();
			}
			catch (NavCSideDuplicateKeyException innerException)
			{
				string message = string.Format(CultureInfo.CurrentCulture, Lang.TenantWebServiceEntryAlreadyExists, entry.ObjectType, entry.ServiceName);
				throw new NavAppException(message, innerException);
			}
		}
	}

	private static List<NavAppTenantWebServiceEntry> ExtractWebServices(Stream packageStream)
	{
		List<NavAppTenantWebServiceEntry> services = new List<NavAppTenantWebServiceEntry>();
		ResultExtensions.OnSuccess<List<XDocument>>(NavAppManagementUtility.GetXDocumentsFromPackage(packageStream, (NavAppPackageReader r) => r.ReadWebServicePaths()), (Action<List<XDocument>>)((List<XDocument> documents) =>
		{
			documents.ForEach((XDocument doc) =>
			{
				services.AddRange(NavAppTenantWebServiceCollection.FromXml(doc));
			});
		}));
		return services;
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Apps.AppXmlObjects;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavAppPermissionManagement
{
	public static void AddPermissionArtifacts(NavTenant tenant, NavSqlConnectionScope conScope, Stream packageStream, AppId appId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000750", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Adding tenant permissions -- Tenant:{0}; AppId:{1}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value);
		Stopwatch stopwatch = Stopwatch.StartNew();
		List<NavAppPermissionSetEntry> permissionSets = ExtractPermissions(packageStream, appId);
		int num = InsertPermissionSets(tenant, conScope, permissionSets);
		int num2 = InsertPermissions(tenant, conScope, permissionSets);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000751", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Added tenant permissions -- Tenant:{0}; AppId:{1}; PermissionSets:{2}; Permissions:{3}; Elapsed:{4}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value, num, num2, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	public static void RemovePermissionArtifacts(NavTenant tenant, NavSqlConnectionScope conScope, AppId appId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000752", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removing tenant permission sets and permissions -- Tenant:{0}; AppId:{1}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value);
		Stopwatch stopwatch = Stopwatch.StartNew();
		NavAppManagementUtility.DeleteRecordWhereAppId(tenant, conScope, appId, "Tenant Permission", 2000000166);
		NavAppManagementUtility.DeleteRecordWhereAppId(tenant, conScope, appId, "Tenant Permission Set", 2000000165);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000753", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removed tenant permission sets and permissions -- Tenant:{0}; AppId:{1}; Elapsed:{2}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	private static List<NavAppPermissionSetEntry> ExtractPermissions(Stream packageStream, AppId appId)
	{
		List<NavAppPermissionSetEntry> permissions = new List<NavAppPermissionSetEntry>();
		ResultExtensions.OnSuccess<List<XDocument>>(NavAppManagementUtility.GetXDocumentsFromPackage(packageStream, (NavAppPackageReader r) => r.ReadPermissionSetPaths()), (Action<List<XDocument>>)((List<XDocument> documents) =>
		{
			documents.ForEach((XDocument doc) =>
			{
				permissions.AddRange(NavAppPermissionSetEntry.FromXml(doc, appId));
			});
		}));
		return permissions;
	}

	private static int InsertPermissionSets(NavTenant tenant, NavSqlConnectionScope conScope, IEnumerable<NavAppPermissionSetEntry> permissionSets)
	{
		var list = permissionSets.Select((NavAppPermissionSetEntry p) => new { p.AppId, p.RoleId, p.Name }).Distinct().ToList();
		list.ForEach(set =>
		{
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
			navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM [Tenant Permission Set] WHERE {0} [App ID] = @0 AND [Role ID] = @1)INSERT INTO [Tenant Permission Set] ([App ID], [Role ID], [Name]{1}) VALUES (@0, @1, @2{2})", tenant.Database.IsSharedTenantSchema ? "\"$companyId\" = @3 AND " : string.Empty, tenant.Database.IsSharedTenantSchema ? ", \"$companyId\"" : string.Empty, tenant.Database.IsSharedTenantSchema ? ", @3" : string.Empty));
			navSqlCommand.Command.Parameters.Add(0, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)set.AppId).Value);
			navSqlCommand.Command.Parameters.Add(1, SqlDbType.NVarChar, 20, set.RoleId.ToUpperInvariant());
			navSqlCommand.Command.Parameters.Add(2, SqlDbType.NVarChar, 30, set.Name);
			if (tenant.Database.IsSharedTenantSchema)
			{
				navSqlCommand.Command.Parameters.Add(3, SqlDbType.Int, tenant.Database.GetCompanyIdForSystemTables(2000000165));
			}
			navSqlCommand.PrepareStatement();
			navSqlCommand.ExecuteNonQuery();
		});
		return list.Count;
	}

	private static int InsertPermissions(NavTenant tenant, NavSqlConnectionScope conScope, IEnumerable<NavAppPermissionSetEntry> permissionSets)
	{
		int num = 0;
		foreach (NavAppPermissionSetEntry permissionSet in permissionSets)
		{
			foreach (NavAppPermissionEntry permission in permissionSet.Permissions)
			{
				num++;
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
				NavValue value;
				try
				{
					NCLMetaField fieldByNo = NavGlobal.NCLMetadata.GetMetaTableById(2000000166).GetFieldByNo(12);
					NavTableFilterEvaluator.Instance.Evaluate(DataError.ThrowError, out value, (INavValueMetadata)fieldByNo, permission.SecurityFilter, 9);
				}
				catch (NavNCLEvaluateException)
				{
					string message = string.Format(CultureInfo.InvariantCulture, "AppID: '{0}', RoleID: '{1}', ObjectID: '{2}'", permissionSet.AppId, permissionSet.RoleId, permission.ObjectId);
					throw new NavNCLEvaluateException(message);
				}
				navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS ( SELECT * FROM [Tenant Permission] WHERE {0} [App ID] = @0 AND [Role ID] = @1 AND [Object Type] = @2 AND [Object ID] = @3)INSERT INTO [Tenant Permission] ([App ID], [Role ID], [Object Type], [Object ID], [Read Permission], [Insert Permission], [Modify Permission], [Delete Permission], [Execute Permission], [Security Filter]{1}) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9{2})", tenant.Database.IsSharedTenantSchema ? "\"$companyId\" = @10 AND " : string.Empty, tenant.Database.IsSharedTenantSchema ? ", \"$companyId\"" : string.Empty, tenant.Database.IsSharedTenantSchema ? ", @10" : string.Empty));
				navSqlCommand.Command.Parameters.Add(0, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)permissionSet.AppId).Value);
				navSqlCommand.Command.Parameters.Add(1, SqlDbType.NVarChar, 20, permissionSet.RoleId.ToUpperInvariant());
				navSqlCommand.Command.Parameters.Add(2, SqlDbType.Int, permission.ObjectType);
				navSqlCommand.Command.Parameters.Add(3, SqlDbType.Int, permission.ObjectId);
				navSqlCommand.Command.Parameters.Add(4, SqlDbType.Int, permission.ReadPermission);
				navSqlCommand.Command.Parameters.Add(5, SqlDbType.Int, permission.InsertPermission);
				navSqlCommand.Command.Parameters.Add(6, SqlDbType.Int, permission.ModifyPermission);
				navSqlCommand.Command.Parameters.Add(7, SqlDbType.Int, permission.DeletePermission);
				navSqlCommand.Command.Parameters.Add(8, SqlDbType.Int, permission.ExecutePermission);
				navSqlCommand.Command.Parameters.Add(9, SqlDbType.VarBinary, 504, value.GetSqlWritableValue(SqlDataType.Default, 504));
				if (tenant.Database.IsSharedTenantSchema)
				{
					navSqlCommand.Command.Parameters.Add(10, SqlDbType.Int, tenant.Database.GetCompanyIdForSystemTables(2000000166));
				}
				navSqlCommand.PrepareStatement();
				navSqlCommand.ExecuteNonQuery();
			}
		}
		return num;
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class AppTenantAppStorage : TableStorage
{
	internal const int TenantFieldLength = 128;

	private const string TableName = "NAV App Tenant App";

	private const string InsertStatement = "INSERT INTO {0} ([Tenant ID], [App Package ID]) VALUES (@0, @1)";

	private const string SelectTenantsWithAppStatement = "SELECT [Tenant ID] FROM {0} WHERE [App Package ID] = @0";

	private const string DeleteAllWithAppStatement = "DELETE FROM {0} WHERE [App Package ID] = @0";

	private const string SelectAppByTenantStatement = "SELECT [App Package ID] FROM {0} WHERE [Tenant ID] = @0";

	public AppTenantAppStorage(NavDatabase database, NavSqlConnectionScope connectionScope)
		: base("NAV App Tenant App", database, connectionScope)
	{
	}

	public Result AddRecord(PackageId appPackageId, string tenantId)
	{
		string tenantIdWithConsistentCasing = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("AddRecord", Database), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ([Tenant ID], [App Package ID]) VALUES (@0, @1)", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 128));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantIdWithConsistentCasing;
			cmd.Command.Parameters[1].Value = ((WrappedValue<Guid>)(object)appPackageId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}

	public Result<List<string>> GetTenantsWithAppInstalled(PackageId packageId)
	{
		List<string> list = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetTenantsWithAppInstalled", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Tenant ID] FROM {0} WHERE [App Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			List<string> list2 = new List<string>();
			while (cmd.ReadNextRow())
			{
				list2.Add(cmd.Reader.GetString(0));
			}
			return list2;
		});
		return Result.Ok<List<string>>(list);
	}

	public Result<List<PackageId>> GetAppsInstalledForTenant(string tenantId)
	{
		string tenantIdWithConsistentCasing = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		List<PackageId> list = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetAppsEnabledForTenant", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [App Package ID] FROM {0} WHERE [Tenant ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 128));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantIdWithConsistentCasing;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			List<PackageId> list2 = new List<PackageId>();
			while (cmd.ReadNextRow())
			{
				list2.Add(PackageId.op_Implicit(cmd.Reader.GetGuid(0)));
			}
			return list2;
		});
		return Result.Ok<List<PackageId>>(list);
	}

	public Result PurgeRecordsForPackage(PackageId packageId)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("PurgeRecordsForPackage", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [App Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}
}

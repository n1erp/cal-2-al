using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class AppPublishReferenceStorage : TableStorage
{
	private const string TableName = "NAV App Publish Reference";

	private const string InsertOrUpdateRecordQuery = "UPDATE {0} SET [App Package ID] = @1 WHERE [Tenant ID] = @0 AND [App Package ID] = @1 IF @@ROWCOUNT = 0 INSERT INTO {0} ([Tenant ID], [App Package ID]) VALUES (@0, @1)";

	private const string DeleteSingleRecordQuery = "DELETE FROM {0} WHERE [Tenant ID] = @0 AND [App Package ID] = @1";

	private const string DeleteForTenantQuery = "DELETE FROM {0} WHERE [Tenant ID] = @0";

	private const string DeleteForAppQuery = "DELETE FROM {0} WHERE [App Package ID] = @0";

	public AppPublishReferenceStorage(NavDatabase database, NavSqlConnectionScope connectionScope)
		: base("NAV App Publish Reference", database, connectionScope)
	{
	}

	public void InsertOrUpdatePublishReference(string tenantId, PackageId packageId)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertOrUpdatePublishReference", Database), () => string.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET [App Package ID] = @1 WHERE [Tenant ID] = @0 AND [App Package ID] = @1 IF @@ROWCOUNT = 0 INSERT INTO {0} ([Tenant ID], [App Package ID]) VALUES (@0, @1)", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantId;
			cmd.Command.Parameters[1].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}

	public void DeletePublishReference(string tenantId, PackageId packageId)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeletePublishReference", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [Tenant ID] = @0 AND [App Package ID] = @1", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantId;
			cmd.Command.Parameters[1].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}

	public void DeletePublishReferencesForTenant(string tenantId)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeletePublishReferencesForTenant", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [Tenant ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 250));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantId;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}

	public void DeletePublishReferencesForApp(PackageId packageId)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeletePublishReferencesForApp", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [App Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class AppCapabilitiesStorage : TableStorage
{
	private const string TableName = "NAV App Capabilities";

	private const string SelectRecordsForAppQuery = "SELECT [Package ID], [Capability ID] FROM {0} WHERE [Package ID] = @0";

	private const string InsertRecordQuery = "INSERT INTO {0} ([Package ID], [Capability ID]) VALUES (@0, @1)";

	private const string DeleteRecordQuery = "DELETE FROM {0} WHERE [Package ID] = @0";

	public AppCapabilitiesStorage(NavDatabase database, NavSqlConnectionScope connectionScope)
		: base("NAV App Capabilities", database, connectionScope)
	{
	}

	public Result<List<NavAppCapabilities>> GetAppCapabilities(PackageId packageId)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected Obj, but got Unknown
		NavSqlCommand navSqlCommand = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetAppCapability", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Package ID], [Capability ID] FROM {0} WHERE [Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			return cmd;
		});
		List<NavAppCapabilities> list = new List<NavAppCapabilities>();
		while (navSqlCommand.ReadNextRow())
		{
			list.Add(new NavAppCapabilities
			{
				PackageId = PackageId.op_Implicit(navSqlCommand.Reader.GetGuid(0)),
				Capability = (NavAppCapability)navSqlCommand.Reader.GetInt32(1)
			});
		}
		return Result.Ok<List<NavAppCapabilities>>(list);
	}

	public Result AddAppCapability(PackageId packageId, NavAppCapability capability)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertAppCapability", Database), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ([Package ID], [Capability ID]) VALUES (@0, @1)", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
			cmd.Command.Parameters[1].Value = capability;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}

	public Result DeleteAppCapabilities(PackageId packageId)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeleteAppCapability", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}
}

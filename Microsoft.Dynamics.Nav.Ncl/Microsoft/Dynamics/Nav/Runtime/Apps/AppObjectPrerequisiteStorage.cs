using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class AppObjectPrerequisiteStorage : TableStorage
{
	private const string TableName = "NAV App Object Prerequisites";

	private const string SelectRecordQuery = "SELECT [Package ID], [Type], [ID] FROM {0} WHERE [Package ID] = @0";

	private const string InsertRecordQuery = "INSERT INTO {0} ([Package ID], [Type], [ID]) VALUES (@0, @1, @2)";

	private const string DeleteRecordQuery = "DELETE FROM {0} WHERE [Package ID] = @0";

	public AppObjectPrerequisiteStorage(NavDatabase database, NavSqlConnectionScope connectionScope)
		: base("NAV App Object Prerequisites", database, connectionScope)
	{
	}

	public Result<List<NavAppObjectPrerequisite>> GetAppPrerequisites(PackageId packageId)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected Obj, but got Unknown
		NavSqlCommand navSqlCommand = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetAppPrerequisites", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Package ID], [Type], [ID] FROM {0} WHERE [Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
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
		List<NavAppObjectPrerequisite> list = new List<NavAppObjectPrerequisite>();
		while (navSqlCommand.ReadNextRow())
		{
			list.Add(new NavAppObjectPrerequisite
			{
				Type = (ObjectType)navSqlCommand.Reader.GetInt32(1),
				Id = navSqlCommand.Reader.GetInt32(2)
			});
		}
		return Result.Ok<List<NavAppObjectPrerequisite>>(list);
	}

	public Result AddAppPrerequisite(PackageId packageId, NavAppObjectPrerequisite prerequisite)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertAppPrerequisite", Database), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ([Package ID], [Type], [ID]) VALUES (@0, @1, @2)", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
			cmd.Command.Parameters[1].Value = (int)prerequisite.Type;
			cmd.Command.Parameters[2].Value = prerequisite.Id;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}

	public Result DeleteAppPrerequisite(PackageId packageId)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeleteAppPrerequisite", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}
}

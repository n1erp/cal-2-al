using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class AppMediaSetStorage : TableStorage
{
	private const string TableName = "Media Set";

	private const string InsertRecordQuery = "INSERT INTO {0} ([ID], [Media ID], [Company Name], [Media Index]) VALUES (@0, @1, @2, @3)";

	private const string DeleteRecordQuery = "DELETE FROM {0} WHERE [ID] = @0";

	private const string SelectMediaIdsQuery = "SELECT [Media ID] FROM {0} WHERE [ID] = @0 order by [Media Index]";

	public AppMediaSetStorage(NavDatabase database, NavSqlConnectionScope connectionScope)
		: base("Media Set", database, connectionScope)
	{
	}

	public Result AddAppScreenShot(MediaSetId appMediaSetId, NavAppMedia media)
	{
		Result result = Result.Ok();
		try
		{
			ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertAppMediaSet", Database), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ([ID], [Media ID], [Company Name], [Media Index]) VALUES (@0, @1, @2, @3)", FullTableName), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("0", SqlDbType.UniqueIdentifier));
				cmd.Command.Parameters.Add(new SqlParameter("1", SqlDbType.UniqueIdentifier));
				cmd.Command.Parameters.Add(new SqlParameter("2", SqlDbType.NVarChar, 30));
				cmd.Command.Parameters.Add(new SqlParameter("3", SqlDbType.BigInt));
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)appMediaSetId).Value;
				cmd.Command.Parameters[1].Value = ((WrappedValue<Guid>)(object)media.Id).Value;
				cmd.Command.Parameters[2].Value = string.Empty;
				cmd.Command.Parameters[3].Value = DateTime.UtcNow.Ticks;
			}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
			return result;
		}
		catch (NavSqlException ex)
		{
			return Result.Fail((Exception)ex);
		}
	}

	public Result DeleteAppMediaSet(MediaSetId mediaSetId)
	{
		if ((WrappedComparable<Guid>)(object)mediaSetId == (WrappedComparable<Guid>)null)
		{
			return Result.Ok();
		}
		Result result = Result.Ok();
		try
		{
			ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeleteAppMediaSet", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [ID] = @0", FullTableName), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)mediaSetId).Value;
			}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
			return result;
		}
		catch (NavSqlException ex)
		{
			return Result.Fail((Exception)ex);
		}
	}

	public Result<IEnumerable<MediaId>> GetAppMediaIdsForMediaSet(MediaSetId mediaSetId)
	{
		List<MediaId> list = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetMediaIdsForMediaSet", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Media ID] FROM {0} WHERE [ID] = @0 order by [Media Index]", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)mediaSetId).Value;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			List<MediaId> list2 = new List<MediaId>();
			while (cmd.ReadNextRow())
			{
				list2.Add(MediaId.op_Implicit(cmd.Reader.GetGuid(0)));
			}
			return list2;
		});
		return Result.Ok<IEnumerable<MediaId>>((IEnumerable<MediaId>)list);
	}
}

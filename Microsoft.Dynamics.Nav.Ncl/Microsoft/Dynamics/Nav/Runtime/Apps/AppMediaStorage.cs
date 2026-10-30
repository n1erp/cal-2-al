using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.Runtime.Media;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class AppMediaStorage : TableStorage
{
	private const string TableName = "Media";

	private const string InsertRecordQuery = "INSERT INTO {0} ([ID], [Description], [Content], [Mime Type], [Height], [Width], [Company Name], [Expiration Date], [Prohibit Cache], [File Name], [Security Token], [Creating User]) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11)";

	private const string DeleteRecordQuery = "DELETE FROM {0} WHERE [ID] = @0";

	public AppMediaStorage(NavDatabase database, NavSqlConnectionScope connectionScope)
		: base("Media", database, connectionScope)
	{
	}

	public Result AddAppMedia(NavAppMedia media)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertAppMedia", Database), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ([ID], [Description], [Content], [Mime Type], [Height], [Width], [Company Name], [Expiration Date], [Prohibit Cache], [File Name], [Security Token], [Creating User]) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11)", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Image, int.MaxValue));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.NVarChar, 100));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@5", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@6", SqlDbType.NVarChar, 30));
			cmd.Command.Parameters.Add(new SqlParameter("@7", SqlDbType.DateTime));
			cmd.Command.Parameters.Add(new SqlParameter("@8", SqlDbType.TinyInt));
			cmd.Command.Parameters.Add(new SqlParameter("@9", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@10", SqlDbType.NVarChar, 100));
			cmd.Command.Parameters.Add(new SqlParameter("@11", SqlDbType.NVarChar, 50));
		}, (NavSqlCommand cmd) =>
		{
			using NavBLOB navBLOB = NavBLOB.CreateFromObject(media.Blob);
			using NavMediaBase navMediaBase = NavMediaFactory.ProcessMediaObject(media.Blob);
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)media.Id).Value;
			cmd.Command.Parameters[1].Value = media.Description;
			cmd.Command.Parameters[2].Value = navBLOB.GetSqlWritableValue(SqlDataType.Default, int.MaxValue, compressed: true);
			cmd.Command.Parameters[3].Value = navMediaBase.MimeType;
			cmd.Command.Parameters[4].Value = navMediaBase.Height;
			cmd.Command.Parameters[5].Value = navMediaBase.Width;
			cmd.Command.Parameters[6].Value = string.Empty;
			cmd.Command.Parameters[7].Value = SqlDateTime.MinValue;
			cmd.Command.Parameters[8].Value = 0;
			cmd.Command.Parameters[9].Value = string.Empty;
			cmd.Command.Parameters[10].Value = string.Empty;
			cmd.Command.Parameters[11].Value = string.Empty;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}

	public Result DeleteAppMedia(MediaId mediaId)
	{
		if ((WrappedComparable<Guid>)(object)mediaId == (WrappedComparable<Guid>)null)
		{
			return Result.Ok();
		}
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeleteAppMedia", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)mediaId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}
}

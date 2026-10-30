using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class ObjectMetadataStorage : TableStorage
{
	private const string TableName = "Object Metadata";

	private const string SelectMenuSuiteQuery = "SELECT TOP 1 [Metadata], [Metadata Version], [Object Subtype] FROM {0} WHERE [Object Type] = @0 AND [Object ID] <= @1 ORDER BY [Object ID] DESC";

	private const string SelectRecordForObjectQuery = "SELECT [Metadata], [User Code], [User AL Code], [Metadata Version], [Object Subtype] FROM {0} WHERE [Object Type] = @0 AND [Object ID] = @1";

	public ObjectMetadataStorage(NavDatabase database, NavSqlConnectionScope connectionScope)
		: base("Object Metadata", database, connectionScope)
	{
	}

	public ObjectMetadataRecord GetMenuSuiteRecord(int id)
	{
		NavSqlCommand navSqlCommand = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetMenuSuiteRecord", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT TOP 1 [Metadata], [Metadata Version], [Object Subtype] FROM {0} WHERE [Object Type] = @0 AND [Object ID] <= @1 ORDER BY [Object ID] DESC", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = 7;
			cmd.Command.Parameters[1].Value = id;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			return cmd;
		});
		if (navSqlCommand.ReadNextRow())
		{
			string metadata = ReadCompressedBlobAsString(navSqlCommand, 0);
			int @int = navSqlCommand.Reader.GetInt32(1);
			string subtype = navSqlCommand.Reader.GetString(2);
			navSqlCommand.CloseReader(forceClose: false);
			return new ObjectMetadataRecord(ObjectType.MenuSuite, id, @int, metadata, null, null, subtype);
		}
		return null;
	}

	public ObjectMetadataRecord GetRecord(ObjectType type, int id)
	{
		NavSqlCommand navSqlCommand = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetRecord", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Metadata], [User Code], [User AL Code], [Metadata Version], [Object Subtype] FROM {0} WHERE [Object Type] = @0 AND [Object ID] = @1", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = type;
			cmd.Command.Parameters[1].Value = id;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			return cmd;
		});
		if (navSqlCommand.ReadNextRow())
		{
			string metadata = ReadCompressedBlobAsString(navSqlCommand, 0);
			string userCode = ReadCompressedBlobAsString(navSqlCommand, 1);
			string userALCode = ReadCompressedBlobAsString(navSqlCommand, 2);
			int @int = navSqlCommand.Reader.GetInt32(3);
			string subtype = navSqlCommand.Reader.GetString(4);
			navSqlCommand.CloseReader(forceClose: false);
			return new ObjectMetadataRecord(type, id, @int, metadata, userCode, userALCode, subtype);
		}
		return null;
	}

	private static string ReadCompressedBlobAsString(NavSqlCommand command, int ordinal)
	{
		using NavBLOB navBLOB = new NavBLOB();
		command.GetBlobDataFromReader(isCompressed: true, ordinal, navBLOB);
		string result = null;
		if (!navBLOB.IsZeroOrEmpty)
		{
			result = new StreamReader(navBLOB.GetNewStream()).ReadToEnd();
		}
		return result;
	}
}

using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class AppResourceStorage : TableStorage
{
	private const string TableName = "NAV App Resource";

	private const string SelectSingleResourceQuery = "SELECT [Content], [Hash] FROM {0} WHERE [Package ID] = @0 AND [Type] = @1 AND [Name] = @2";

	private const string InsertResourceQuery = "INSERT INTO {0} ([Package ID], [Type], [Name], [Content], [Hash]) VALUES (@0, @1, @2, @3, @4)";

	private const string DeleteForAppQuery = "DELETE FROM {0} WHERE [Package ID] = @0";

	public AppResourceStorage(NavDatabase database, NavSqlConnectionScope connectionScope)
		: base("NAV App Resource", database, connectionScope)
	{
	}

	public Result<NavAppResource> GetSingleAppResource(PackageId packageId, NavAppResourceType type, NavAppResourceName name)
	{
		NavSqlCommand navSqlCommand = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetSingleAppResource", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Content], [Hash] FROM {0} WHERE [Package ID] = @0 AND [Type] = @1 AND [Name] = @2", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.NVarChar, 250));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
			cmd.Command.Parameters[1].Value = (int)type;
			cmd.Command.Parameters[2].Value = name.Value;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			return cmd;
		});
		if (navSqlCommand.ReadNextRow())
		{
			byte[] content = ReadCompressedBlob(navSqlCommand, 0);
			string text = navSqlCommand.Reader.GetString(1);
			Sha256Hash contentHash = ((!string.IsNullOrEmpty(text)) ? Sha256Hash.CreateFromHexString(text) : null);
			NavAppResourceId id = new NavAppResourceId(PackageId.op_Implicit(((WrappedValue<Guid>)(object)packageId).Value), type, name);
			NavAppResource navAppResource = new NavAppResource(id, content, contentHash);
			return Result.Ok<NavAppResource>(navAppResource);
		}
		return Result.Ok<NavAppResource>(NavAppResource.None);
	}

	public Result AddAppResource(NavAppResource resource)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("AddAppResource", Database), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ([Package ID], [Type], [Name], [Content], [Hash]) VALUES (@0, @1, @2, @3, @4)", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Binary, int.MaxValue));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.NVarChar, 64));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)resource.Id.PackageId).Value;
			cmd.Command.Parameters[1].Value = (int)resource.Id.ResourceType;
			cmd.Command.Parameters[2].Value = resource.Id.ResourceName.Value;
			cmd.Command.Parameters[3].Value = GetSqlWritableValueForBlobField(resource.Content);
			cmd.Command.Parameters[4].Value = resource.ContentHash?.HexString ?? string.Empty;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}

	public Result DeleteAppResources(PackageId packageId)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeleteAppResources", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}

	private static object GetSqlWritableValueForBlobField(byte[] content)
	{
		object result = DBNull.Value;
		if (content != null)
		{
			using NavBLOB navBLOB = NavBLOB.CreateFromObject(content);
			result = navBLOB.GetSqlWritableValue(SqlDataType.Default, int.MaxValue, compressed: true);
		}
		return result;
	}

	private static byte[] ReadCompressedBlob(NavSqlCommand command, int ordinal)
	{
		using NavBLOB navBLOB = new NavBLOB();
		command.GetBlobDataFromReader(isCompressed: true, ordinal, navBLOB);
		return navBLOB.IsZeroOrEmpty ? null : navBLOB.GetBytes();
	}
}

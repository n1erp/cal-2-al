using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.AppXmlObjects;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.Model.Tools.IO.Nea;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class PlatformAddInStorage : TableStorage
{
	private const string TableName = "Add-in";

	private const string InsertRecordQuery = "INSERT INTO {0} ([Add-in Name], [Public Key Token], [Version], [Category], [Description], [Resource]) VALUES (@0, @1, @2, @3, @4, @5)";

	private const string SelectAllAddInQuery = "SELECT [Add-in Name], [Public Key Token], [Version], [Category] FROM {0}";

	public PlatformAddInStorage(NavDatabase database, NavSqlConnectionScope connectionScope)
		: base("Add-in", database, connectionScope)
	{
	}

	public Result<IEnumerable<ControlAddInEntry>> GetPlatformAddIns()
	{
		List<ControlAddInEntry> list = ConnectionScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("SelectAllControlAddIn", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Add-in Name], [Public Key Token], [Version], [Category] FROM {0}", FullTableName), (NavSqlCommand cmd) =>
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected Obj, but got Unknown
			cmd.Command.ExecuteReader();
			List<ControlAddInEntry> list2 = new List<ControlAddInEntry>();
			while (cmd.ReadNextRow())
			{
				list2.Add(new ControlAddInEntry
				{
					Name = cmd.Reader.GetString(0),
					Token = cmd.Reader.GetString(1),
					Version = cmd.Reader.GetString(2),
					Category = (AddInCategory)cmd.Reader.GetInt32(3)
				});
			}
			return list2;
		});
		return Result.Ok<IEnumerable<ControlAddInEntry>>((IEnumerable<ControlAddInEntry>)list);
	}

	public Result AddPlatformAddIn(ControlAddInEntry entry, string resourcePath)
	{
		if (!File.Exists(resourcePath))
		{
			return Result.Fail(Error.FromMessage(string.Format(CultureInfo.InvariantCulture, "Cannot find a control add-in with the name {0} at the specified location: {1}.", entry.FullName, resourcePath), ErrorType.InvalidOperation));
		}
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertControlAddIn", Database), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ([Add-in Name], [Public Key Token], [Version], [Category], [Description], [Resource]) VALUES (@0, @1, @2, @3, @4, @5)", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 220));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 20));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.NVarChar, 25));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@5", SqlDbType.Binary, int.MaxValue));
		}, (NavSqlCommand cmd) =>
		{
			object value = DBNull.Value;
			byte[] array;
			using (FileStream fileStream = File.OpenRead(resourcePath))
			{
				using Stream stream = (NeaStreamReader.IsSupported(fileStream) ? ((Stream)new NeaStreamReader(fileStream)) : ((Stream)fileStream));
				array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
			}
			using (NavBLOB navBLOB = NavBLOB.CreateFromObject(array))
			{
				value = navBLOB.GetSqlWritableValue(SqlDataType.Default, int.MaxValue, compressed: true);
			}
			cmd.Command.Parameters[0].Value = entry.Name;
			cmd.Command.Parameters[1].Value = entry.Token;
			cmd.Command.Parameters[2].Value = entry.Version;
			cmd.Command.Parameters[3].Value = entry.Category;
			cmd.Command.Parameters[4].Value = string.Empty;
			cmd.Command.Parameters[5].Value = value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}
}

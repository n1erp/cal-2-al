using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TableConnectionSettingsStorage
{
	private const string TableName = "$ndo$tableconnections";

	private const int IdMaxLength = 128;

	private const string CreateTableTableStatement = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[{1}].[{2}]') AND OBJECTPROPERTY(id, N'IsTable') = 1) CREATE TABLE [{0}].[{1}].[{2}]( [timestamp] TIMESTAMP, [tabletype] INT,[connectionid] NVARCHAR(128), [connectionsettings] IMAGE, [version] INT, CONSTRAINT [PK${2}] PRIMARY KEY CLUSTERED(tabletype, connectionid))";

	private const string InsertStatement = "INSERT INTO [{0}].[{1}].[{2}] ([tabletype], [connectionid], [connectionsettings], [version]) VALUES (@0,@1,@2,@3)";

	private const string DeleteStatement = "DELETE FROM [{0}].[{1}].[{2}] WHERE [tabletype] = @0 AND [connectionid] = @1";

	private const string GetAllStatement = "SELECT [version], [tabletype], [connectionsettings] FROM [{0}].[{1}].[{2}]";

	private const string GetStatement = "SELECT [version], [tabletype], [connectionsettings] FROM [{0}].[{1}].[{2}] WHERE [tabletype] = @0 AND [connectionid] = @1";

	private const int CurrentVersion = 1000;

	private const int AcceptableVersionIncreaseWithoutConversion = 9;

	private readonly NavDatabase database;

	private bool tableChecked;

	internal TableConnectionSettingsStorage(NavDatabase serviceDatabase)
	{
		database = serviceDatabase;
	}

	private void CheckTable()
	{
		if (tableChecked)
		{
			return;
		}
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		try
		{
			navSqlCommand.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[{1}].[{2}]') AND OBJECTPROPERTY(id, N'IsTable') = 1) CREATE TABLE [{0}].[{1}].[{2}]( [timestamp] TIMESTAMP, [tabletype] INT,[connectionid] NVARCHAR(128), [connectionsettings] IMAGE, [version] INT, CONSTRAINT [PK${2}] PRIMARY KEY CLUSTERED(tabletype, connectionid))", database.DatabaseName, "dbo", "$ndo$tableconnections"));
			tableChecked = true;
		}
		catch (NavSqlException ex) when (((Func<bool>)delegate
		{
			// Could not convert BlockContainer to single expression
			SqlException ex2 = (SqlException)ex.InnerException;
			return ex2 != null && ex2.Number == 2714;
		}).Invoke())
		{
		}
	}

	private static TableConnectionSettingsBase GetTableConnectionSettingsFromReader(SqlDataReader reader)
	{
		int @int = reader.GetInt32(0);
		if (@int > 1009)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "This version of {0} cannot read the {1}", ProductNames.ProductNameShort, "$ndo$tableconnections"));
		}
		TableConnectionType int2 = (TableConnectionType)reader.GetInt32(1);
		BlobReaderStream documentStream = new BlobReaderStream(reader, 2);
		DataContractSerializer dataContractSerializer = new DataContractSerializer(int2 switch
		{
			TableConnectionType.ExternalSQL => typeof(ExternalSqlTableConnectionSettings), 
			TableConnectionType.CRM => typeof(CrmTableConnectionSettings), 
			TableConnectionType.Exchange => typeof(ExchangeTableConnectionSettings), 
			_ => throw new NotSupportedException("Unsupported type "), 
		});
		using XmlReader reader2 = XmlHelper.SafeXmlReader(documentStream);
		return (TableConnectionSettingsBase)dataContractSerializer.ReadObject(reader2);
	}

	internal void Add<T>(T tableConnectionSettings) where T : TableConnectionSettingsBase
	{
		CheckTable();
		try
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true);
			navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertTableConnectionSettings", database), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO [{0}].[{1}].[{2}] ([tabletype], [connectionid], [connectionsettings], [version]) VALUES (@0,@1,@2,@3)", database.DatabaseName, "dbo", "$ndo$tableconnections"), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.Int));
				cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 128));
				cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Binary, int.MaxValue));
				cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = tableConnectionSettings.TableConnectionType;
				cmd.Command.Parameters[1].Value = tableConnectionSettings.Id.ToUpperInvariant();
				using (MemoryStream memoryStream = new MemoryStream())
				{
					DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
					using (XmlWriter writer = XmlWriter.Create(memoryStream))
					{
						dataContractSerializer.WriteObject(writer, tableConnectionSettings);
					}
					cmd.Command.Parameters[2].Value = memoryStream.ToArray();
				}
				cmd.Command.Parameters[3].Value = 1000;
			}, (NavSqlCommand cmd) =>
			{
				cmd.ExecuteNonQuery();
				return true;
			});
		}
		catch (NavCSideDuplicateKeyException)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000I1", Category.Sql, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Duplicate key insertion in table connections");
			throw new NavCSideDuplicateKeyException(string.Format(CultureInfo.CurrentCulture, Lang.TableConnectionAlreadyAdded, tableConnectionSettings.Id, tableConnectionSettings.TableConnectionType));
		}
	}

	internal void Remove(TableConnectionType tableConnectionType, string id)
	{
		CheckTable();
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true);
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("RemoveTableConnectionSettings", database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}].[{1}].[{2}] WHERE [tabletype] = @0 AND [connectionid] = @1", database.DatabaseName, "dbo", "$ndo$tableconnections"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 128));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tableConnectionType;
			cmd.Command.Parameters[1].Value = id.ToUpperInvariant();
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}

	internal TableConnectionSettingsBase Get(TableConnectionType tableConnectionType, string id)
	{
		CheckTable();
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true);
		return navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetTableConnectionSettings", database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [version], [tabletype], [connectionsettings] FROM [{0}].[{1}].[{2}] WHERE [tabletype] = @0 AND [connectionid] = @1", database.DatabaseName, "dbo", "$ndo$tableconnections"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 128));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tableConnectionType;
			cmd.Command.Parameters[1].Value = id.ToUpperInvariant();
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			if (!cmd.ReadNextRow())
			{
				return (TableConnectionSettingsBase)null;
			}
			TableConnectionSettingsBase tableConnectionSettingsFromReader = GetTableConnectionSettingsFromReader(cmd.Reader);
			cmd.CloseReader(forceClose: false);
			return tableConnectionSettingsFromReader;
		});
	}

	public IEnumerable<TableConnectionSettingsBase> GetAll()
	{
		CheckTable();
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true);
		return navSqlConnectionScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("GetAllTableConnectionSettings", database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [version], [tabletype], [connectionsettings] FROM [{0}].[{1}].[{2}]", database.DatabaseName, "dbo", "$ndo$tableconnections"), (NavSqlCommand cmd) =>
		{
			List<TableConnectionSettingsBase> list = new List<TableConnectionSettingsBase>();
			cmd.ExecuteReader();
			while (cmd.ReadNextRow())
			{
				list.Add(GetTableConnectionSettingsFromReader(cmd.Reader));
			}
			cmd.CloseReader(forceClose: false);
			return list;
		});
	}
}

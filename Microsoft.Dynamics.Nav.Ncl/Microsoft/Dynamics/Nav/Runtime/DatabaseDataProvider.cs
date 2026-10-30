using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DatabaseDataProvider : EagerVirtualDataProvider
{
	private const string SelectAvailableDatabasesCommand = "SELECT [Database Name], [Creator], [Created], [Replication Type] FROM (( SELECT [name] AS [Database Name], CAST(ISNULL(SUSER_SNAME([owner_sid]), '') AS NVARCHAR(64)) AS [Creator], CONVERT(DATETIME, CONVERT(CHAR(10), [create_date], 120)+' 00:00:00:000', 121) AS [Created], CASE WHEN [is_published] = 1 THEN 1 WHEN [is_subscribed] = 1 THEN 2 WHEN [is_merge_published] = 1 THEN 3 ELSE 0 END AS [Replication Type] FROM [sys].[databases] WHERE  [name] NOT IN ('master', 'tempdb', 'model', 'msdb') ) Databases CROSS APPLY ( SELECT HAS_DBACCESS(Databases.[Database Name]) AS [Has Access] ) HasAccess CROSS APPLY ( SELECT OBJECT_ID('\"' + Databases.[Database Name] + '\".[dbo].[$ndo$dbproperty]') AS [Valid] WHERE HasAccess.[Has Access] = 1 ) Valid )WHERE [Valid] IS NOT NULL ";

	public static readonly DatabaseDataProvider Instance = new DatabaseDataProvider();

	public override int TableId => 2000000048;

	private DatabaseDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = false;
		List<ReadOnlyRecordBuffer> records = new List<ReadOnlyRecordBuffer>();
		if (NavGlobal.AppDatabase.IsAzureDatabase)
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.SystemTableDatabaseNotSupportedInAzureSQL, 2000000048));
		}
		NCLMetaApplicationObject metaTable = MetaTable;
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Browse))
		{
			navSqlConnectionScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("SelectAvailableDatabasesCommand", NavGlobal.AppDatabase), () => "SELECT [Database Name], [Creator], [Created], [Replication Type] FROM (( SELECT [name] AS [Database Name], CAST(ISNULL(SUSER_SNAME([owner_sid]), '') AS NVARCHAR(64)) AS [Creator], CONVERT(DATETIME, CONVERT(CHAR(10), [create_date], 120)+' 00:00:00:000', 121) AS [Created], CASE WHEN [is_published] = 1 THEN 1 WHEN [is_subscribed] = 1 THEN 2 WHEN [is_merge_published] = 1 THEN 3 ELSE 0 END AS [Replication Type] FROM [sys].[databases] WHERE  [name] NOT IN ('master', 'tempdb', 'model', 'msdb') ) Databases CROSS APPLY ( SELECT HAS_DBACCESS(Databases.[Database Name]) AS [Has Access] ) HasAccess CROSS APPLY ( SELECT OBJECT_ID('\"' + Databases.[Database Name] + '\".[dbo].[$ndo$dbproperty]') AS [Valid] WHERE HasAccess.[Has Access] = 1 ) Valid )WHERE [Valid] IS NOT NULL ", (NavSqlCommand availableDatabases) =>
			{
				using (availableDatabases.ExecuteReader())
				{
					while (availableDatabases.ReadNextRow())
					{
						SqlDataReader reader = availableDatabases.Reader;
						string text = reader.GetString(0);
						ReadOnlyRecordBuffer item = new ReadOnlyRecordBuffer(metaTable, VirtualDataProvider.VirtualTimeStamp, GetTruncatedTextValue(1, text), GetTruncatedTextValue(2, reader.GetString(1)), NavDateTime.Create(new DateTime(reader.GetDateTime(2).Ticks, DateTimeKind.Utc)).DatePart, GetOptionValue(4, reader.GetInt32(3)), NavBoolean.Create(string.CompareOrdinal(NavGlobal.AppDatabase.DatabaseName, text) == 0));
						records.Add(item);
					}
					availableDatabases.CloseReader(forceClose: false);
				}
				return 0;
			});
		}
		return records;
	}
}

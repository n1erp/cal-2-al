using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Dynamics.Nav.Runtime.Apps;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavSqlMetadataCommands
{
	private const string GetAllTablesWithPartitionsCmd = "\r\n            SELECT t.name AS TableName, c.name AS PartitioningColumnName, ps.name AS PartitioningSchemeName   \r\n            FROM sys.tables AS t   \r\n            JOIN sys.indexes AS i   \r\n                ON t.[object_id] = i.[object_id]   \r\n                AND i.[type] <= 1\r\n            JOIN sys.partition_schemes AS ps   \r\n                ON ps.data_space_id = i.data_space_id   \r\n            JOIN sys.index_columns AS ic   \r\n                ON ic.[object_id] = i.[object_id]   \r\n                AND ic.index_id = i.index_id   \r\n                AND ic.partition_ordinal >= 1\r\n            JOIN sys.columns AS c   \r\n                ON t.[object_id] = c.[object_id]   \r\n                AND ic.column_id = c.column_id";

	public static SqlSortingProperties GetSqlSortingPropertiesForCollation(string collationName)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase);
		return navSqlConnectionScope.Connection.ExecuteCommandAndCacheOnSuccess(new NavSqlStringBasedCommandCacheKey("DATABASESORTINGPROPERTIES", NavGlobal.AppDatabase), () => "SELECT COLLATIONPROPERTY(@0, 'Version'),COLLATIONPROPERTY(@0, 'ComparisonStyle'),COLLATIONPROPERTY(@0, 'LCID')", (NavSqlCommand commandToExecute) =>
		{
			if (commandToExecute.Command.Parameters.Count == 0)
			{
				SqlParameter value = NavSqlStatementHelper.CreateSqlParameter("@0", NavNclType.NavText, SqlDataType.Default, 128, collationName, isWildcardValue: false, isFilterValue: false);
				commandToExecute.Command.Parameters.Add(value);
			}
			else
			{
				commandToExecute.Command.Parameters[0].Value = collationName;
			}
		}, (NavSqlCommand commandToExecute) =>
		{
			commandToExecute.ExecuteReader();
			commandToExecute.ReadNextRow();
			return new SqlSortingProperties(commandToExecute.Reader.GetByte(0), commandToExecute.Reader.GetInt32(1), commandToExecute.Reader.GetInt32(2), collationName);
		});
	}

	public static bool TableExists(NavDatabase database, NCLMetaTable table, int companyToken, NavAppTableContext tableContext)
	{
		StringBuilder stringBuilder = new StringBuilder();
		NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(stringBuilder, table, database, companyToken, tableContext);
		return TableExists(database, stringBuilder.ToString());
	}

	public static bool TableExists(NavDatabase database, string tableName)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database);
		return navSqlConnectionScope.Connection.ExecuteCommandAndCacheOnSuccess(new NavSqlStringBasedCommandCacheKey("TABLEEXISTS", database), () => "SELECT OBJECTPROPERTY(OBJECT_ID(@0), N'IsTable')", (NavSqlCommand commandToExecute) =>
		{
			if (commandToExecute.Command.Parameters.Count == 0)
			{
				SqlParameter value = NavSqlStatementHelper.CreateSqlParameter("@0", NavNclType.NavText, SqlDataType.Default, 128, tableName, isWildcardValue: false, isFilterValue: false);
				commandToExecute.Command.Parameters.Add(value);
			}
			else
			{
				commandToExecute.Command.Parameters[0].Value = tableName;
			}
		}, (NavSqlCommand commandToExecute) =>
		{
			object obj = commandToExecute.ExecuteScalar();
			return !Convert.IsDBNull(obj) && (int)obj == 1;
		});
	}

	public static bool TableExists(NavSqlConnectionScope conScope, NavDatabase database, string tableName)
	{
		return conScope.Connection.ExecuteCommandAndCacheOnSuccess(new NavSqlStringBasedCommandCacheKey("TABLEEXISTSSCOPE", database), () => "SELECT OBJECTPROPERTY(OBJECT_ID(@0), N'IsTable')", (NavSqlCommand commandToExecute) =>
		{
			if (commandToExecute.Command.Parameters.Count == 0)
			{
				SqlParameter value = NavSqlStatementHelper.CreateSqlParameter("@0", NavNclType.NavText, SqlDataType.Default, 128, tableName, isWildcardValue: false, isFilterValue: false);
				commandToExecute.Command.Parameters.Add(value);
			}
			else
			{
				commandToExecute.Command.Parameters[0].Value = tableName;
			}
		}, (NavSqlCommand commandToExecute) =>
		{
			object obj = commandToExecute.ExecuteScalar();
			return !Convert.IsDBNull(obj) && (int)obj == 1;
		});
	}

	public static string ColumnHasNumericValues(NavDatabase database, int companyToken, NCLMetaField field)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database);
		StringBuilder stringBuilder = new StringBuilder("SELECT TOP 1 ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, field.SqlColumnName);
		stringBuilder.Append(" FROM ");
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, field.Parent, database, companyToken, NavAppTableContext.Main);
		stringBuilder.Append(" WHERE ISNUMERIC(");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, field.SqlColumnName);
		stringBuilder.Append(") = 1");
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		return (string)navSqlCommand.ExecuteScalar(stringBuilder.ToString());
	}

	public static Dictionary<string, TablePartitionMetadata> GetTablesPartitionMetadata(NavDatabase database)
	{
		Dictionary<string, TablePartitionMetadata> dictionary = new Dictionary<string, TablePartitionMetadata>(StringComparer.OrdinalIgnoreCase);
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database))
		{
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			using SqlDataReader sqlDataReader = navSqlCommand.ExecuteReader("\r\n            SELECT t.name AS TableName, c.name AS PartitioningColumnName, ps.name AS PartitioningSchemeName   \r\n            FROM sys.tables AS t   \r\n            JOIN sys.indexes AS i   \r\n                ON t.[object_id] = i.[object_id]   \r\n                AND i.[type] <= 1\r\n            JOIN sys.partition_schemes AS ps   \r\n                ON ps.data_space_id = i.data_space_id   \r\n            JOIN sys.index_columns AS ic   \r\n                ON ic.[object_id] = i.[object_id]   \r\n                AND ic.index_id = i.index_id   \r\n                AND ic.partition_ordinal >= 1\r\n            JOIN sys.columns AS c   \r\n                ON t.[object_id] = c.[object_id]   \r\n                AND ic.column_id = c.column_id");
			if (sqlDataReader.HasRows)
			{
				while (sqlDataReader.Read())
				{
					dictionary.Add(sqlDataReader.GetString(0), new TablePartitionMetadata
					{
						PartitionColumnName = sqlDataReader.GetString(1),
						PartitionSchemaName = sqlDataReader.GetString(2)
					});
				}
			}
		}
		return dictionary;
	}
}

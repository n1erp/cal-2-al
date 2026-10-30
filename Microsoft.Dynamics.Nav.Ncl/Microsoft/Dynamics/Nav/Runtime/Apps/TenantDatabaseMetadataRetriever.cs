using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class TenantDatabaseMetadataRetriever
{
	private class IndexColumn
	{
		internal string TableName { get; }

		internal string IndexName { get; }

		internal string ColumnName { get; }

		public IndexColumn(string tableName, string indexName, string columnName)
		{
			TableName = tableName;
			IndexName = indexName;
			ColumnName = columnName;
		}
	}

	private readonly NavDatabase database;

	public IEnumerable<DatabaseTable> AllSqlTables
	{
		get
		{
			List<DatabaseTable> list = new List<DatabaseTable>();
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Browse);
			NavSqlConnection connection = navSqlConnectionScope.Connection;
			List<KeyValuePair<string, List<DatabaseColumn>>> tableNameWithColumns = GetTableNameWithColumns(connection);
			List<KeyValuePair<string, List<DatabaseIndex>>> tableNameWithIndexes = GetTableNameWithIndexes(connection);
			List<KeyValuePair<string, List<string>>> defaultConstraints = GetDefaultConstraints(connection);
			foreach (string tableName in tableNameWithColumns.Select((KeyValuePair<string, List<DatabaseColumn>> pair) => pair.Key))
			{
				List<DatabaseColumn> value = tableNameWithColumns.FirstOrDefault((KeyValuePair<string, List<DatabaseColumn>> column) => column.Key == tableName).Value;
				List<DatabaseIndex> value2 = tableNameWithIndexes.FirstOrDefault((KeyValuePair<string, List<DatabaseIndex>> index) => index.Key == tableName).Value;
				List<string> value3 = defaultConstraints.FirstOrDefault((KeyValuePair<string, List<string>> constraint) => constraint.Key == tableName).Value;
				DatabaseTable item = new DatabaseTable(tableName, value, value2, value3);
				list.Add(item);
			}
			return list;
		}
	}

	public TenantDatabaseMetadataRetriever(NavDatabase database)
	{
		this.database = database;
	}

	private List<KeyValuePair<string, List<string>>> GetDefaultConstraints(NavSqlConnection conn)
	{
		List<KeyValuePair<string, string>> defaultConstraints = new List<KeyValuePair<string, string>>();
		string commandText = "SELECT t.Name, dc.Name FROM sys.tables t INNER JOIN sys.default_constraints dc ON t.object_id = dc.parent_object_id ORDER BY dc.Name";
		conn.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("GetDefaultConstraints", database), () => commandText, (NavSqlCommand cmd) =>
		{
			SqlDataReader sqlDataReader = cmd.ExecuteReader();
			while (sqlDataReader.Read())
			{
				defaultConstraints.Add(new KeyValuePair<string, string>(sqlDataReader.GetString(0), sqlDataReader.GetString(1)));
			}
			return 0;
		});
		return (from c in defaultConstraints
			group c by c.Key into a
			select new KeyValuePair<string, List<string>>(a.Key, a.Select((KeyValuePair<string, string> b) => b.Value).ToList())).ToList();
	}

	private List<KeyValuePair<string, List<DatabaseColumn>>> GetTableNameWithColumns(NavSqlConnection conn)
	{
		List<KeyValuePair<string, DatabaseColumn>> allColumns = new List<KeyValuePair<string, DatabaseColumn>>();
		string commandText = "\r\nSELECT tab.name, c.name, t.Name, columnproperty(c.object_id, c.name, 'charmaxlen')\r\nFROM sys.tables tab\r\nJOIN sys.columns c ON tab.object_id = c.object_id\r\nINNER JOIN sys.types t ON c.user_type_id = t.user_type_id";
		conn.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("GetTableNameWithColumns", database), () => commandText, (NavSqlCommand cmd) =>
		{
			SqlDataReader sqlDataReader = cmd.ExecuteReader();
			while (sqlDataReader.Read())
			{
				string key = sqlDataReader.GetString(0);
				string name = sqlDataReader.GetString(1);
				string sqlType = sqlDataReader.GetString(2);
				int length = -1;
				if (!sqlDataReader.IsDBNull(3))
				{
					length = sqlDataReader.GetInt32(3);
				}
				allColumns.Add(new KeyValuePair<string, DatabaseColumn>(key, new DatabaseColumn(name, sqlType, length)));
			}
			return 0;
		});
		return (from c in allColumns
			group c by c.Key into a
			select new KeyValuePair<string, List<DatabaseColumn>>(a.Key, a.Select((KeyValuePair<string, DatabaseColumn> b) => b.Value).ToList())).ToList();
	}

	private List<KeyValuePair<string, List<DatabaseIndex>>> GetTableNameWithIndexes(NavSqlConnection conn)
	{
		List<IndexColumn> allIndexes = new List<IndexColumn>();
		string commandText = "\r\nSELECT t.name, i.name, i.type_desc, i.is_primary_key, c.name FROM sys.tables t \r\nJOIN sys.indexes i ON t.object_id = i.object_id \r\nJOIN sys.index_columns ic ON i.object_id = ic.object_id and i.index_id = ic.index_id \r\nJOIN sys.columns c on i.object_id = c.object_id and ic.column_id = c.column_id \r\nORDER BY t.name, i.index_id";
		conn.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("GetTableNameWithIndexes", database), () => commandText, (NavSqlCommand cmd) =>
		{
			SqlDataReader sqlDataReader = cmd.ExecuteReader();
			while (sqlDataReader.Read())
			{
				allIndexes.Add(new IndexColumn(sqlDataReader.GetString(0), sqlDataReader.GetString(1), sqlDataReader.GetString(4)));
			}
			return 0;
		});
		List<IGrouping<string, IndexColumn>> list = (from index in allIndexes
			group index by index.TableName).ToList();
		List<KeyValuePair<string, List<DatabaseIndex>>> list2 = new List<KeyValuePair<string, List<DatabaseIndex>>>();
		foreach (IGrouping<string, IndexColumn> item in list)
		{
			List<IGrouping<string, IndexColumn>> source = (from temp in item
				group temp by temp.IndexName).ToList();
			List<DatabaseIndex> value = source.Select((IGrouping<string, IndexColumn> index) => new DatabaseIndex(index.Key, (from columns in index.ToList()
				select columns.ColumnName).ToList())).ToList();
			list2.Add(new KeyValuePair<string, List<DatabaseIndex>>(item.Key, value));
		}
		return list2;
	}
}

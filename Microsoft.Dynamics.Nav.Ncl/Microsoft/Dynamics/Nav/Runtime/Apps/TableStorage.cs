namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal abstract class TableStorage
{
	protected readonly NavDatabase Database;

	protected readonly string FullTableName;

	protected readonly NavSqlConnectionScope ConnectionScope;

	protected TableStorage(string tableName, NavDatabase database, NavSqlConnectionScope connectionScope)
	{
		ConnectionScope = connectionScope;
		Database = database;
		FullTableName = NavSqlStatementHelper.GetQuotedTableName(Database, "dbo", tableName);
	}
}

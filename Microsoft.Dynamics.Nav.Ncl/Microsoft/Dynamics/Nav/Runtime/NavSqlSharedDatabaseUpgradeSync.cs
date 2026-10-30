#define SQLSEQUENCE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlSharedDatabaseUpgradeSync
{
	private class NclMetaTableSqlTableNameAndContextComparer : IEqualityComparer<Tuple<NCLMetaTable, NavAppTableContext>>
	{
		public bool Equals(Tuple<NCLMetaTable, NavAppTableContext> x, Tuple<NCLMetaTable, NavAppTableContext> y)
		{
			if (x == y)
			{
				return true;
			}
			if (x == null || y == null || x.Item1 == null || y.Item1 == null || x.Item2 == null || y.Item2 == null || x.Item1.SqlTableName == null || y.Item1.SqlTableName == null)
			{
				return false;
			}
			if (x.Item1.SqlTableName.Equals(y.Item1.SqlTableName, StringComparison.OrdinalIgnoreCase))
			{
				return x.Item2.CompanionTableAppOrigin.Equals(y.Item2.CompanionTableAppOrigin);
			}
			return false;
		}

		public int GetHashCode(Tuple<NCLMetaTable, NavAppTableContext> obj)
		{
			return obj.Item1.SqlTableName.GetHashCode() ^ obj.Item2.CompanionTableAppOrigin.GetHashCode();
		}
	}

	private readonly NavTenantDatabaseSyncStatus detailedDatabaseSyncState;

	private readonly NavDatabase tenantDatabase;

	private readonly bool perTable;

	private readonly NavSqlConnectionScope tenantDatabaseScope;

	public NavSqlSharedDatabaseUpgradeSync(NavDatabase tenantDatabase, NavTenantDatabaseSyncStatus detailedDatabaseSyncState, bool perTable, NavSqlConnectionScope tenantDatabaseScope)
	{
		if (tenantDatabase == null)
		{
			throw new ArgumentNullException("tenantDatabase");
		}
		if (tenantDatabase.DatabaseType != NavDatabaseType.Tenant)
		{
			throw new InvalidOperationException("Connection scope database is not a tenant database");
		}
		if (!tenantDatabase.TenantProperties.TenantLocalId.HasValue)
		{
			throw new InvalidOperationException("The upgrade to shared tenant schema must be performed with Convert-NAVTenant command.");
		}
		this.detailedDatabaseSyncState = detailedDatabaseSyncState ?? throw new ArgumentNullException("detailedDatabaseSyncState");
		this.tenantDatabase = tenantDatabase;
		this.perTable = perTable;
		this.tenantDatabaseScope = tenantDatabaseScope;
	}

	private NavSqlConnectionScope GetConnectionScope(ref IReference<NavSqlConnection> lastUsedConnection)
	{
		if (!perTable)
		{
			return NavSqlConnectionScope.Create(tenantDatabase, TransactionType.Update, ref lastUsedConnection);
		}
		return NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, TransactionType.Update, ref lastUsedConnection);
	}

	public void RunUpgrade()
	{
		TenantDatabaseTableMetadataRetriever tenantDatabaseTableMetadataRetriever = new TenantDatabaseTableMetadataRetriever(tenantDatabase);
		IReference<NavSqlConnection> lastUsedConnection = null;
		Dictionary<int, CompanyId> companyTokensAndIds;
		List<Tuple<NCLMetaTable, NavAppTableContext>> list;
		using (NavSqlConnectionScope navSqlConnectionScope = GetConnectionScope(ref lastUsedConnection))
		{
			companyTokensAndIds = GetCompanyTokensAndIds();
			list = (from t in tenantDatabaseTableMetadataRetriever.GetSnapshotOfAllTenantTablesAndContexts(navSqlConnectionScope)
				where !t.Item1.IsExternalDataSource
				select t).Distinct(new NclMetaTableSqlTableNameAndContextComparer()).ToList();
		}
		int num = 1;
		int count = list.Count;
		foreach (Tuple<NCLMetaTable, NavAppTableContext> item in list.Where((Tuple<NCLMetaTable, NavAppTableContext> t) => !t.Item1.DataPerCompany))
		{
			detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Format(CultureInfo.InvariantCulture, "Upgrading the tenant database to the shared schema model. Processing table {0} of {1}.", num, count);
			UpgradeTable(item, companyTokensAndIds, ref lastUsedConnection);
			num++;
		}
		foreach (Tuple<NCLMetaTable, NavAppTableContext> item2 in list.Where((Tuple<NCLMetaTable, NavAppTableContext> t) => t.Item1.DataPerCompany))
		{
			detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Format(CultureInfo.InvariantCulture, "Upgrading the tenant database to the shared schema model. Processing table {0} of {1}.", num, count);
			UpgradeTable(item2, companyTokensAndIds, ref lastUsedConnection);
			num++;
		}
		using (GetConnectionScope(ref lastUsedConnection))
		{
			tenantDatabase.TenantDatabaseProperties.StorePerCompanySchemaSetting(newPerCompanySchema: false);
			CreateSequences((from t in list
				where t.Item2.TableType == NavAppTableContextType.Main
				select t.Item1).ToList(), companyTokensAndIds);
		}
		RebuildAllIndexes(companyTokensAndIds);
	}

	private void RebuildAllIndexes(Dictionary<int, CompanyId> companyIds)
	{
		TenantDatabaseTableMetadataRetriever tenantDatabaseTableMetadataRetriever = new TenantDatabaseTableMetadataRetriever(tenantDatabase);
		IReference<NavSqlConnection> lastUsedConnection = null;
		List<Tuple<NCLMetaTable, NavAppTableContext>> list;
		using (NavSqlConnectionScope navSqlConnectionScope = GetConnectionScope(ref lastUsedConnection))
		{
			navSqlConnectionScope.Connection.CachedCommands.Initialize();
			list = (from t in tenantDatabaseTableMetadataRetriever.GetSnapshotOfAllTenantTablesAndContexts(navSqlConnectionScope)
				where !t.Item1.IsExternalDataSource
				select t).Distinct(new NclMetaTableSqlTableNameAndContextComparer()).ToList();
		}
		bool flag = companyIds.Count > 1;
		int num = 1;
		int count = list.Count;
		foreach (Tuple<NCLMetaTable, NavAppTableContext> item2 in list)
		{
			NCLMetaTable item = item2.Item1;
			if (item.DataPerCompany && !flag)
			{
				continue;
			}
			using (GetConnectionScope(ref lastUsedConnection))
			{
				detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Format(CultureInfo.InvariantCulture, "Upgrading the tenant database to the shared schema model. Upgrading indexes for table {0} of {1}.", num, count);
				NavSqlDdlCommands.DropSiftIndexes(tenantDatabase, item2.Item1, 0, item2.Item2);
				if (!item2.Item1.PrimaryKey.Clustered)
				{
					NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(tenantDatabase);
					navSqlDdlCommands.CreateNonSiftIndex(item.ClusterIndex, 0, item2.Item2, tenantDatabase.DefaultPartitionMetadata(item));
					navSqlDdlCommands.ExecuteNonQuery();
				}
				NavSqlDdlCommands.CreateAllIndexes(tenantDatabase, item, 0, item2.Item2, tenantDatabase.DefaultPartitionMetadata(item));
				num++;
			}
		}
	}

	private void CreateSequences(List<NCLMetaTable> tables, Dictionary<int, CompanyId> companyIds)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(tenantDatabase);
		foreach (NCLMetaTable item in tables.Where((NCLMetaTable t) => tenantDatabase.HasSequencedAutoIncrementField(t, NavAppTableContext.Main)))
		{
			foreach (KeyValuePair<int, CompanyId> companyId in companyIds)
			{
				if ((!item.DataPerCompany || companyId.Key != 0) && (item.DataPerCompany || companyId.Key == 0))
				{
					navSqlDdlCommands.RestartSequenceForAutoIncrementField(item, item.AutoIncrementField, companyId.Value);
				}
			}
		}
		navSqlDdlCommands.ExecuteNonQuery();
	}

	public Dictionary<int, CompanyId> GetCompanyTokensAndIds()
	{
		Dictionary<int, CompanyId> dictionary = new Dictionary<int, CompanyId>();
		dictionary.Add(0, tenantDatabase.TenantCompanies.GetCompanyIdForSharedCompany());
		int[] companyTokens = tenantDatabase.GetCompanyTokens();
		foreach (int num in companyTokens)
		{
			string companyNameFromToken = tenantDatabase.GetCompanyNameFromToken(num);
			dictionary.Add(num, tenantDatabase.TenantCompanies.GetCompanyId(companyNameFromToken));
		}
		return dictionary;
	}

	private void UpgradeTable(Tuple<NCLMetaTable, NavAppTableContext> tableAndContext, Dictionary<int, CompanyId> companyIds, ref IReference<NavSqlConnection> lastUsedConnection)
	{
		StringBuilder stringBuilder = new StringBuilder();
		using NavSqlConnectionScope navSqlConnectionScope = GetConnectionScope(ref lastUsedConnection);
		if (tableAndContext.Item1.DataPerCompany)
		{
			foreach (KeyValuePair<int, CompanyId> item in companyIds.Where((KeyValuePair<int, CompanyId> c) => c.Key != 0))
			{
				NavSqlDdlCommands.DropSiftIndexes(tenantDatabase, tableAndContext.Item1, item.Key, tableAndContext.Item2);
				string tableName = NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), tableAndContext.Item1, tenantDatabase, item.Key, tableAndContext.Item2).ToString();
				NavSqlDdlCommands.DisableChangeTrackingForTable(new CloudReadyReplicationStatus
				{
					TableName = tableName,
					TableId = tableAndContext.Item1.TableId,
					CompanyId = item.Value.Value,
					CompanyName = tenantDatabase.GetCompanyNameFromToken(item.Key)
				}, tenantDatabaseScope);
			}
			BuildPerCompanyTableUpgradeSql(navSqlConnectionScope, stringBuilder, tableAndContext.Item1, companyIds, tableAndContext.Item2);
		}
		else
		{
			NavSqlDdlCommands.DropSiftIndexes(tenantDatabase, tableAndContext.Item1, 0, tableAndContext.Item2);
			string tableName2 = NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), tableAndContext.Item1, tenantDatabase, 0, tableAndContext.Item2).ToString();
			NavSqlDdlCommands.DisableChangeTrackingForTable(new CloudReadyReplicationStatus
			{
				TableName = tableName2,
				TableId = tableAndContext.Item1.TableId,
				CompanyId = companyIds[0].Value,
				CompanyName = tenantDatabase.GetCompanyNameFromToken(0)
			}, tenantDatabaseScope);
			string tableName3 = string.Format(CultureInfo.InvariantCulture, "{0}.[{1}]", "dbo", GetSqlTableName(tableAndContext.Item1, tableAndContext.Item2));
			BuildAddCompanyIdColumnAndSetValueSql(tableName3, tableAndContext.Item1, companyIds[0], stringBuilder, tableAndContext.Item2);
		}
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		navSqlCommand.ExecuteNonQuery();
	}

	private void AppendRenameDefaultConstraints(StringBuilder sqlCommandText, NCLMetaTable table, int companyToken, NavAppTableContext tableContext)
	{
		foreach (NCLMetaField field in table.Fields)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			NavSqlStatementHelper.AppendSqlFieldDefaultConstraintName(stringBuilder, table, field, tenantDatabase, companyToken, tableContext, true);
			NavSqlStatementHelper.AppendSqlFieldDefaultConstraintName(stringBuilder2, table, field, tenantDatabase, companyToken, tableContext, false);
			NavSqlDdlCommands.AppendRenameConstraintIfExists(sqlCommandText, stringBuilder.ToString(), stringBuilder2.ToString());
			sqlCommandText.Append(";");
		}
	}

	private static string RemoveIdentityConstraint(NCLMetaTable table, string tableName)
	{
		NCLMetaField autoIncrementField = table.AutoIncrementField;
		if (autoIncrementField == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "EXEC('ALTER TABLE {0} ADD [$tmp${1}] ", tableName, autoIncrementField.SqlColumnName);
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, autoIncrementField.FieldNclType, autoIncrementField.SqlDataType, autoIncrementField.FieldDefinedLength, NavGlobal.AppDatabase.SqlSortingProperties.Collation);
		stringBuilder.AppendLine(" NOT NULL CONSTRAINT AUTOINCREMENT_TMP DEFAULT 0;')");
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "EXEC('UPDATE {0} SET [$tmp${1}] = [{1}]')", tableName, autoIncrementField.SqlColumnName);
		stringBuilder.AppendLine(";");
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "EXEC('ALTER TABLE {0} DROP COLUMN [{1}]')", tableName, autoIncrementField.SqlColumnName);
		stringBuilder.AppendLine(";");
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "EXEC('ALTER TABLE {0} DROP CONSTRAINT AUTOINCREMENT_TMP;')", tableName);
		stringBuilder.AppendLine(";");
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " EXEC sp_rename N'{0}.[$tmp${1}]', N'{1}', N'COLUMN'", tableName, autoIncrementField.SqlColumnName);
		stringBuilder.AppendLine(";");
		return stringBuilder.ToString();
	}

	private void BuildPerCompanyTableUpgradeSql(NavSqlConnectionScope scope, StringBuilder sqlCommandText, NCLMetaTable table, Dictionary<int, CompanyId> companyIds, NavAppTableContext tableContext)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		sqlCommandText.AppendLine("DECLARE @TableColumns varchar(MAX)\r\n                                        DECLARE @TableHasIdentityColumns BIT\r\n                                        DECLARE @SQLCMP varchar(MAX);");
		foreach (KeyValuePair<int, CompanyId> item in companyIds.Where((KeyValuePair<int, CompanyId> c) => c.Key != 0))
		{
			int key = item.Key;
			CompanyId value = item.Value;
			string tableNameWithPrefix = GetTableNameWithPrefix(stringBuilder, key, table, tableContext);
			string tableName = string.Format(CultureInfo.InvariantCulture, "{0}.[{1}]", "dbo", tableNameWithPrefix);
			if (flag)
			{
				BuildAddCompanyIdColumnAndSetValueSql(tableName, table, value, sqlCommandText, tableContext);
				sqlCommandText.AppendFormat(CultureInfo.InvariantCulture, "EXEC sp_rename N'{0}', N'{1}';", tableNameWithPrefix, GetSqlTableName(table, tableContext));
				if (NonUpgradedFieldDefaultConstrainstsExist(scope, table, key, tableContext))
				{
					AppendRenameDefaultConstraints(sqlCommandText, table, key, tableContext);
					sqlCommandText.Append(";");
				}
				flag = false;
			}
			else
			{
				sqlCommandText.AppendFormat(CultureInfo.InvariantCulture, "-- select all table columns\r\nSET @TableColumns = NULL\r\nSELECT @TableColumns = COALESCE(@TableColumns + ', ', '') + '[' + COLUMN_NAME + ']'\r\nFROM INFORMATION_SCHEMA.COLUMNS\r\nWHERE TABLE_NAME = '{0}' AND COLUMN_NAME != 'timestamp'\r\n\r\nSET @SQLCMP = 'INSERT INTO [{1}] (' + @TableColumns + ', {2}) SELECT ' + @TableColumns + ', {3} FROM [{0}]'\r\nEXEC(@SQLCMP)\r\n\r\n-- delete per company table\r\nSET @SQLCMP = 'DROP TABLE [{0}]'\r\nEXEC(@SQLCMP);\r\n", tableNameWithPrefix, GetSqlTableName(table, tableContext), "\"$companyId\"", value);
			}
		}
	}

	private static void BuildAddCompanyIdColumnAndSetValueSql(string tableName, NCLMetaTable table, CompanyId companyId, StringBuilder sqlCommandText, NavAppTableContext tableContext)
	{
		sqlCommandText.AppendFormat(CultureInfo.InvariantCulture, "\r\nDECLARE @PkName VARCHAR(255)\r\nDECLARE @PrimaryKeyColumns varchar(MAX) = ''\r\nDECLARE @PkType INT = ''\r\nDECLARE @PkNumberOfColumns INT = 0\r\nDECLARE @SQL VARCHAR(MAX) = '';\r\n\r\n-- Finding current primary key and primary key columns\r\nSELECT TOP 1 @PkName = i.Name, @PkType = i.type FROM sys.indexes i\r\nWHERE i.is_primary_key = 1\r\n    and i.object_ID = OBJECT_ID('{0}')\r\n\r\n-- Simulating so what C/SIDE is doing by removing the columns from the current primary key if the number exeeds the limit\r\nSELECT TOP {4} @PrimaryKeyColumns = COALESCE(@PrimaryKeyColumns + ', ', '') + '[' + c.name + ']'\r\n\r\nFROM sys.indexes i\r\n    inner join sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id\r\n    inner join sys.columns c ON ic.object_id = c.object_id AND c.column_id = ic.column_id\r\nWHERE i.is_primary_key = 1\r\n    and i.object_ID = OBJECT_ID('{0}')\r\n    and i.Name = @PkName\r\nORDER BY ic.index_column_id\r\n\r\n-- Adding $companyId to the primary key list of columns\r\nSET @PrimaryKeyColumns = '[{1}]'+ @PrimaryKeyColumns\r\n\r\n-- Alter table and index drop are done in dynamic SQL\r\n-- The indexes are drop in order to rebuild them with $dataset column.\r\nSELECT @SQL += 'DROP INDEX ['+ i.Name +'] ON {0}'\r\n        FROM sys.indexes i\r\n        WHERE i.is_primary_key = 0\r\n            and i.is_unique_constraint = 0\r\n            and i.object_ID = OBJECT_ID('{0}')\r\n\r\nSELECT @SQL += 'ALTER TABLE {0} DROP CONSTRAINT ['+ i.Name +']'\r\n        FROM sys.indexes i\r\n        WHERE i.is_unique_constraint = 1\r\n            and i.object_ID = OBJECT_ID('{0}')\r\n\r\nSELECT @SQL += 'ALTER TABLE {0} DROP CONSTRAINT ['+ @PkName +'];\r\n                ALTER TABLE {0} ADD [{1}] INT NOT NULL CONSTRAINT companyIdDefault DEFAULT {2};\r\n                ALTER TABLE {0} DROP CONSTRAINT companyIdDefault;'\r\n\r\nEXEC (@SQL)\r\n\r\n{3}\r\n\r\nIF @PkType = 1\r\n    SET @SQL = 'ALTER TABLE {0} ADD CONSTRAINT \"{5}\" PRIMARY KEY CLUSTERED ('+ @PrimaryKeyColumns +')'\r\nELSE\r\n    SET @SQL = 'ALTER TABLE {0} ADD CONSTRAINT \"{5}\" PRIMARY KEY NONCLUSTERED ('+ @PrimaryKeyColumns +')'\r\n\r\nEXEC (@SQL)\r\n", tableName, "$companyId", companyId, RemoveIdentityConstraint(table, tableName), NCLMetaTable.MaxSqlIndexColumns, GetSqlTableName(table, tableContext) + "$0");
	}

	private string GetTableNameWithPrefix(StringBuilder stringBuilder, int companyToken, NCLMetaTable table, NavAppTableContext tableContext)
	{
		NavSqlStatementHelper.AppendCompanyNameAndSeparator(stringBuilder, tenantDatabase, companyToken);
		stringBuilder.Append(GetSqlTableName(table, tableContext));
		string result = stringBuilder.ToString();
		stringBuilder.Clear();
		return result;
	}

	private bool NonUpgradedFieldDefaultConstrainstsExist(NavSqlConnectionScope scope, NCLMetaTable table, int companyToken, NavAppTableContext tableContext)
	{
		StringBuilder stringBuilder = new StringBuilder("SELECT TOP 1 name FROM sys.default_constraints WHERE name LIKE '");
		NavSqlStatementHelper.AppendTableNameCompanyQualified(stringBuilder, table, tenantDatabase, companyToken, tableContext);
		stringBuilder.Append("%'");
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(scope.Connection);
		return navSqlCommand.ExecuteScalar(stringBuilder.ToString()) != null;
	}

	private static string GetSqlTableName(NCLMetaTable table, NavAppTableContext tableContext)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(table.SqlTableName);
		if (tableContext.TableType == NavAppTableContextType.Companion)
		{
			stringBuilder.Append('$');
			stringBuilder.Append(tableContext.CompanionTableAppOrigin.AppId.Value);
		}
		else if (tableContext.TableType == NavAppTableContextType.Main && table.SourceExtensionType == NavExtensionType.ModernDev)
		{
			stringBuilder.Append('$');
			stringBuilder.Append(table.SourceAppId.Value);
		}
		return stringBuilder.ToString();
	}
}

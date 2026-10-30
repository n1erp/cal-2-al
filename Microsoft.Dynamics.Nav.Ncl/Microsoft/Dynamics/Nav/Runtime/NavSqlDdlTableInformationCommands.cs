using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavSqlDdlTableInformationCommands
{
	public static void CreateTableInformationStatsFunctionIfNotExists(NavDatabase database)
	{
		if (database.IsSharedTenantSchema && !TableInformationStatsFunctionExists(database))
		{
			CreateTableInformationStatsFunction(database);
		}
	}

	public static void DropTableInformationStatsFunction(NavDatabase database)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.UpdateNoLocks);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.ExecuteNonQuery(FormattableString.Invariant(FormattableStringFactory.Create("IF OBJECT_ID('[{0}].GetTableInformationStats', 'TF') IS NOT NULL\r\n                        DROP FUNCTION [{1}].GetTableInformationStats", "dbo", "dbo")));
	}

	private static void CreateTableInformationStatsFunction(NavDatabase database)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "CREATE FUNCTION [{0}].GetTableInformationStats (@TableId INT, @CompanyId INT)\r\n                RETURNS @ret TABLE\r\n                (\r\n                    [Row Count] INT NOT NULL,\r\n                    [Size] DECIMAL(18,6) NOT NULL\r\n                )\r\n                AS BEGIN ", "dbo");
		foreach (NCLMetaTable item in from t in NavGlobal.NCLMetadata.GetSnapshotOfAllNonVirtualMetaTablesExceptApplicationTables()
			where !t.IsCrossTenantTable && !t.IsExternalDataSource
			select t)
		{
			stringBuilder.Append(GetStatsQueryForTable(database, item));
		}
		stringBuilder.Append("RETURN\r\n                END");
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.UpdateNoLocks);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
	}

	private static string GetStatsQueryForTable(NavDatabase database, NCLMetaTable table)
	{
		if (!database.HasCompanyIdColumn(table))
		{
			return string.Empty;
		}
		return string.Format(CultureInfo.InvariantCulture, "\r\nIF @TableId = {1} AND OBJECT_ID('{0}', 'U') IS NOT NULL\r\nWITH stats_data AS\r\n  (SELECT count(*) AS num_rows, [{2}]\r\n   FROM [{4}].[{0}]\r\n   GROUP BY ROLLUP([{2}])),\r\n     partition_stats AS\r\n  (SELECT SUM(reserved_page_count) as reserved_page_count\r\n   FROM[sys].[dm_db_partition_stats]\r\n   WHERE object_id = OBJECT_ID('{0}', 'U')\r\n     AND(index_id < 2\r\n          OR index_id = 255) ),\r\n     avg_row_size AS\r\n  (SELECT convert(decimal, partition_stats.reserved_page_count) / nullif(stats_data.num_rows, 0) AS avg_row_size\r\n   FROM stats_data\r\n   CROSS JOIN partition_stats\r\n   WHERE stats_data.[{2}] IS NULL )\r\nINSERT INTO @ret\r\nSELECT\r\n       stats_data.num_rows AS [Row Count],\r\n       avg_row_size.avg_row_size * num_rows * 8 AS [Size]\r\nFROM[{4}].[$ndo$tenantcompany] AS tc\r\nCROSS JOIN avg_row_size\r\nLEFT OUTER JOIN stats_data ON tc.[{3}] = stats_data.[{2}]\r\nWHERE stats_data.[{2}] = @CompanyId;\r\n", table.SqlTableName, table.TableId, "$companyId", NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.CompanyId].Name, "dbo");
	}

	private static bool TableInformationStatsFunctionExists(NavDatabase database)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Browse);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		return navSqlCommand.ExecuteScalar("SELECT TOP 1 object_id FROM sys.objects  WHERE name = 'GetTableInformationStats' AND type = 'TF'") != null;
	}
}

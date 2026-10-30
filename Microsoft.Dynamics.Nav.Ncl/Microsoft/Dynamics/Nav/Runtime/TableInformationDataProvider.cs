using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TableInformationDataProvider : TranslatedQueryDataProvider
{
	private const string Template = "SELECT {0} FROM (\r\n    SELECT \r\n        CAST (0 AS timestamp) AS [Timestamp],\r\n        [Company Name],\r\n        [ID] AS [Table No_],\r\n        [Name] AS [Table Name],\r\n        CAST (CASE WHEN [Row Count] > 2147483647 THEN -1 ELSE [Row Count] END AS int) AS [No_ of Records],\r\n        CASE [Row Count] WHEN 0 THEN 0.0 ELSE [Size] * 1024.0 / [Row Count] END AS [Record Size],\r\n        CAST (CASE WHEN [Size] > 2147483647 THEN -1 ELSE ROUND([Size],0) END AS int) AS [Size (KB)] \r\n    FROM (\r\n        SELECT\r\n            [Company Name], \r\n            [ID], \r\n            [Name],\r\n            [TS].[Row Count],\r\n            [TS].[Size] \r\n        FROM (\r\n            SELECT \r\n                C.[Name] AS [Company Name], \r\n                O.[Object ID] AS [ID], \r\n                O.[Name],\r\n                OBJECT_ID({3}) AS [Table Object ID]\r\n                {6}\r\n            FROM [{1}].[{2}].[Object Metadata Snapshot] AS O\r\n            CROSS JOIN [{1}].[{2}].[Company] AS C\r\n            {7}\r\n            WHERE O.[Data Per Company] = 1 AND O.[Object Type] = 1 {5}\r\n            UNION ALL\r\n            SELECT\r\n                '' AS [Company Name], \r\n                O.[Object ID] AS [ID], \r\n                O.[Name],\r\n                OBJECT_ID({4}) AS [Table Object ID]\r\n                {8}\r\n            FROM [{1}].[{2}].[Object Metadata Snapshot] AS O \r\n            WHERE O.[Data Per Company] = 0 AND O.[Object Type] = 1\r\n        ) AS [TN]\r\n        OUTER APPLY {9} AS [TS]\r\n    ) AS [Core Query]\r\n) AS [Projected Query]";

	private readonly NavDatabase database;

	public override int TableId => 2000000028;

	protected override NavDatabase Database => database;

	public TableInformationDataProvider(NavDatabase database)
	{
		this.database = database;
		NavSqlDdlTableInformationCommands.CreateTableInformationStatsFunctionIfNotExists(this.database);
	}

	protected override string CreateStatement(FilterExpression combinedFilter, SortingFieldList sortingFields, QueryType queryType)
	{
		NavSqlParameterHandler navSqlParameterHandler = new NavSqlParameterHandler();
		string format = CreateTableNameExpression(navSqlParameterHandler);
		string text = queryType switch
		{
			QueryType.Normal => "*", 
			QueryType.FirstOnly => "TOP 1 *", 
			QueryType.Count => "COUNT(*)", 
			QueryType.Exists => "TOP 1 NULL", 
			_ => throw new NotSupportedException(), 
		};
		StringBuilder stringBuilder = new StringBuilder();
		bool perCompanySchema = database.TenantDatabaseProperties.PerCompanySchema;
		CompanyId? companyIdForSystemTables = database.GetCompanyIdForSystemTables(2000000006);
		string text2 = (perCompanySchema ? FormattableString.Invariant($"(SELECT SUM(row_count) AS[Row Count], SUM(reserved_page_count) * 8 AS[Size] \r\n                                FROM [{NavCurrentThread.Session.Database.DatabaseName}].[sys].[dm_db_partition_stats] AS[PS] \r\n                                WHERE [PS].object_id = [Table Object ID] \r\n                                    AND (index_id< 2 OR index_id = 255))") : FormattableString.Invariant(FormattableStringFactory.Create(" [{0}].GetTableInformationStats([TN].[ID], [TN].[{1}])", "dbo", "$companyId")));
		string name = NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.CompanyId].Name;
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "SELECT {0} FROM (\r\n    SELECT \r\n        CAST (0 AS timestamp) AS [Timestamp],\r\n        [Company Name],\r\n        [ID] AS [Table No_],\r\n        [Name] AS [Table Name],\r\n        CAST (CASE WHEN [Row Count] > 2147483647 THEN -1 ELSE [Row Count] END AS int) AS [No_ of Records],\r\n        CASE [Row Count] WHEN 0 THEN 0.0 ELSE [Size] * 1024.0 / [Row Count] END AS [Record Size],\r\n        CAST (CASE WHEN [Size] > 2147483647 THEN -1 ELSE ROUND([Size],0) END AS int) AS [Size (KB)] \r\n    FROM (\r\n        SELECT\r\n            [Company Name], \r\n            [ID], \r\n            [Name],\r\n            [TS].[Row Count],\r\n            [TS].[Size] \r\n        FROM (\r\n            SELECT \r\n                C.[Name] AS [Company Name], \r\n                O.[Object ID] AS [ID], \r\n                O.[Name],\r\n                OBJECT_ID({3}) AS [Table Object ID]\r\n                {6}\r\n            FROM [{1}].[{2}].[Object Metadata Snapshot] AS O\r\n            CROSS JOIN [{1}].[{2}].[Company] AS C\r\n            {7}\r\n            WHERE O.[Data Per Company] = 1 AND O.[Object Type] = 1 {5}\r\n            UNION ALL\r\n            SELECT\r\n                '' AS [Company Name], \r\n                O.[Object ID] AS [ID], \r\n                O.[Name],\r\n                OBJECT_ID({4}) AS [Table Object ID]\r\n                {8}\r\n            FROM [{1}].[{2}].[Object Metadata Snapshot] AS O \r\n            WHERE O.[Data Per Company] = 0 AND O.[Object Type] = 1\r\n        ) AS [TN]\r\n        OUTER APPLY {9} AS [TS]\r\n    ) AS [Core Query]\r\n) AS [Projected Query]", text, NavCurrentThread.Session.Database.DatabaseName, "dbo", string.Format(CultureInfo.InvariantCulture, format, perCompanySchema ? "C.[Name] + '$' + O.[Name]" : "O.[Name]"), string.Format(CultureInfo.InvariantCulture, format, "O.[Name]"), perCompanySchema ? string.Empty : FormattableString.Invariant(FormattableStringFactory.Create(" AND C.[{0}] =/*companyIdLiteral*/ {1}", "$companyId", companyIdForSystemTables.Value)), perCompanySchema ? string.Empty : FormattableString.Invariant(FormattableStringFactory.Create(",TC.[{0}] AS [{1}]", name, "$companyId")), perCompanySchema ? string.Empty : FormattableString.Invariant(FormattableStringFactory.Create("JOIN [{0}].[$ndo$tenantcompany] AS TC ON C.Name = TC.[companyname] AND TC.[$tenantlocalid] = {1}", "dbo", database.TenantProperties.TenantLocalId)), perCompanySchema ? string.Empty : FormattableString.Invariant(FormattableStringFactory.Create(",{0} AS [{1}]", companyIdForSystemTables.Value, "$companyId")), text2);
		NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, combinedFilter, navSqlParameterHandler, database.SqlSortingProperties, null);
		if (queryType != QueryType.Count)
		{
			NavSqlStatementHelper.AppendOrderByList(stringBuilder, sortingFields);
		}
		return stringBuilder.ToString();
	}

	protected override void AddorSetParameters(SqlParameterCollection parameters, FilterExpression combinedFilter, SortingFieldList sortingFields, QueryType queryType)
	{
		NavSqlParameterHandler navSqlParameterHandler = new NavSqlParameterHandler();
		SetParametersForTableNameExpression(parameters, navSqlParameterHandler);
		NavSqlWhereClauseBuilder.AddOrSetFilterParameters(combinedFilter, parameters, navSqlParameterHandler, database.SqlSortingProperties, null);
	}

	private static string CreateTableNameExpression(NavSqlParameterHandler parameterHandler)
	{
		StringBuilder stringBuilder = new StringBuilder();
		char[] array = NavGlobal.AppDatabase.SqlDatabaseProperties.InvalidIdentifierChars.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append("REPLACE(");
		}
		stringBuilder.Append("{0}");
		string arg = "@" + parameterHandler.GetNextParameterNo;
		for (int j = 0; j < array.Length; j++)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, ", @{0}, {1})", parameterHandler.GetNextParameterNo, arg);
		}
		return stringBuilder.ToString();
	}

	private static void SetParametersForTableNameExpression(SqlParameterCollection parameters, NavSqlParameterHandler parameterHandler)
	{
		int getNextParameterNo = parameterHandler.GetNextParameterNo;
		string parameterName = "@" + getNextParameterNo;
		object value = '_'.ToString(CultureInfo.InvariantCulture);
		SqlParameter value2 = NavSqlStatementHelper.CreateSqlParameter(parameterName, NavNclType.NavText, SqlDataType.Default, 1, value, isWildcardValue: false, isFilterValue: false);
		if (getNextParameterNo >= parameters.Count)
		{
			parameters.Add(value2);
		}
		char[] array = NavGlobal.AppDatabase.SqlDatabaseProperties.InvalidIdentifierChars.ToCharArray();
		char[] array2 = array;
		foreach (char c in array2)
		{
			getNextParameterNo = parameterHandler.GetNextParameterNo;
			string parameterName2 = "@" + getNextParameterNo;
			value = c.ToString(CultureInfo.InvariantCulture);
			value2 = NavSqlStatementHelper.CreateSqlParameter(parameterName2, NavNclType.NavText, SqlDataType.Default, 1, value, isWildcardValue: false, isFilterValue: false);
			if (getNextParameterNo >= parameters.Count)
			{
				parameters.Add(value2);
			}
		}
	}
}

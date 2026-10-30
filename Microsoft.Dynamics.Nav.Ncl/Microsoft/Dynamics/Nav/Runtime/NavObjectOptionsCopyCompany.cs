using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavObjectOptionsCopyCompany
{
	internal static void UpdateObjectOptionsAfterCompanyCopy(NavDatabase database, int fromCompanyToken, int toCompanyToken)
	{
		CompanyId? companyId = database.GetCompanyIdForSystemTables(2000000196);
		string companyIdWhere = string.Empty;
		string text = "UpdateObjectOptionsAfterCompanyCopy";
		if (companyId.HasValue)
		{
			companyIdWhere = "\"$companyId\" = @2 AND ";
			text += "ID";
		}
		string toCompanyName = database.GetCompanyNameFromToken(toCompanyToken);
		string fromCompanyName = database.GetCompanyNameFromToken(fromCompanyToken);
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000196);
		string sqlTableName = NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(new StringBuilder(), metaTableById, database, fromCompanyToken, NavAppTableContext.Main).ToString();
		StringBuilder sqlFieldList = new StringBuilder();
		NavSqlStatementHelper.AppendFieldList(sqlFieldList, metaTableById.AllFields.Where((NCLMetaField f) => !f.FieldIsTimestamp), null, companyId.HasValue);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Update);
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(text, database), () => FormattableString.Invariant($"\r\n                        SELECT * into #temp_table FROM {sqlTableName} WITH (READUNCOMMITTED) where {companyIdWhere}[Company Name] = @0;\r\n                        UPDATE #temp_table SET [Company Name] = @1;\r\n                        INSERT INTO {sqlTableName} ({sqlFieldList}) SELECT {sqlFieldList} FROM #temp_table;\r\n                        DROP TABLE #temp_table;"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 30));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 30));
			if (companyId.HasValue)
			{
				cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
			}
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = fromCompanyName;
			cmd.Command.Parameters[1].Value = toCompanyName;
			if (companyId.HasValue)
			{
				cmd.Command.Parameters[2].Value = companyId.Value;
			}
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteNonQuery();
			return true;
		});
	}
}

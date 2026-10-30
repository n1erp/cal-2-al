using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlDdlCommandBuilder
{
	public static string BuildCreateSiftIndexCommandText(NavDatabase database, NCLMetaKey key, int companyToken, NavAppTableContext tableContext)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string siftViewName = GetSiftViewName(database, key, companyToken, tableContext);
		string defaultMainTableAlias = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(key.Parent.TableId);
		HashSet<NavAppOrigin> hashSet = new HashSet<NavAppOrigin>();
		IEnumerable<NCLMetaField> enumerable = key.DefinedKeyFields.AsNclMetaFields();
		foreach (NCLMetaField item in enumerable.Where((NCLMetaField f) => f.IsCompanionTableField))
		{
			hashSet.Add(item.AppOrigin);
		}
		stringBuilder.Append("CREATE VIEW ");
		stringBuilder.Append(siftViewName);
		stringBuilder.Append(" WITH SCHEMABINDING AS SELECT ");
		if (database.HasCompanyIdColumn(key.Parent))
		{
			NavSqlDdlCommands.AppendCompanyIdFieldName(stringBuilder, commaPrefix: false, commaSuffix: true, tableContext, defaultMainTableAlias);
		}
		NavSqlStatementHelper.AppendFieldList(stringBuilder, enumerable, defaultMainTableAlias);
		stringBuilder.Append(",COUNT_BIG(*) AS ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, "$Cnt");
		stringBuilder.Append(',');
		foreach (NCLMetaField sumFields in key.SumFieldsList)
		{
			if (sumFields.IsCompanionTableField)
			{
				hashSet.Add(sumFields.AppOrigin);
			}
			stringBuilder.Append("SUM(");
			NavSqlStatementHelper.AppendSqlColumnName(stringBuilder, sumFields, defaultMainTableAlias);
			stringBuilder.Append(") AS ");
			stringBuilder.Append("\"");
			stringBuilder.Append("SUM$");
			stringBuilder.Append(sumFields.SqlColumnName);
			stringBuilder.Append("\"");
			stringBuilder.Append(',');
		}
		stringBuilder.Length--;
		NavSqlStatementHelper.AppendFromClause(stringBuilder, key.Parent, database, companyToken, tableContext, qualifyTableWithDatabase: false, defaultMainTableAlias);
		stringBuilder.Append(" GROUP BY ");
		if (database.HasCompanyIdColumn(key.Parent))
		{
			NavSqlDdlCommands.AppendCompanyIdFieldName(stringBuilder, commaPrefix: false, commaSuffix: true, tableContext, defaultMainTableAlias);
		}
		NavSqlStatementHelper.AppendFieldList(stringBuilder, key.DefinedKeyFields.AsNclMetaFields(), defaultMainTableAlias);
		return stringBuilder.ToString();
	}

	public static string BuildCreateSiftClusteredIndexCommandText(NavDatabase database, NCLMetaKey key, int companyToken, NavAppTableContext tableContext)
	{
		string siftViewName = GetSiftViewName(database, key, companyToken, tableContext);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("CREATE UNIQUE CLUSTERED INDEX VSIFTIDX ON ");
		stringBuilder.Append(siftViewName);
		stringBuilder.Append('(');
		if (database.HasCompanyIdColumn(key.Parent))
		{
			NavSqlDdlCommands.AppendCompanyIdFieldName(stringBuilder, commaPrefix: false, commaSuffix: true);
		}
		NavSqlStatementHelper.AppendFieldList(stringBuilder, key.DefinedKeyFields.AsNclMetaFields().Take(NCLMetaTable.MaxSqlIndexColumns), null);
		stringBuilder.Append(')');
		if (key.Parent.CompressionType == CompressionType.Page || key.Parent.CompressionType == CompressionType.Row)
		{
			stringBuilder.Append(" WITH (DATA_COMPRESSION = " + key.Parent.CompressionType.ToString() + ")");
		}
		return stringBuilder.ToString();
	}

	public static string GetSiftViewName(NavDatabase database, NCLMetaKey key, int companyToken, NavAppTableContext tableContext)
	{
		StringBuilder stringBuilder = new StringBuilder();
		NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(stringBuilder, key.Parent, database, companyToken, tableContext);
		stringBuilder.Length--;
		stringBuilder.Append("$VSIFT$");
		if (!string.IsNullOrEmpty(key.KeyName))
		{
			stringBuilder.Append(key.KeyName);
		}
		else
		{
			stringBuilder.Append(key.KeyIndex);
		}
		stringBuilder.Append("\"");
		return stringBuilder.ToString();
	}
}

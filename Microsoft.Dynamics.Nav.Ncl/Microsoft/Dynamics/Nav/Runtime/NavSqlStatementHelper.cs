using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavSqlStatementHelper
{
	internal const char IdentifierInvalidCharReplacement = '_';

	public const string IdentifierQuotingCharacter = "\"";

	public const string IsNotNull = "IS NOT NULL";

	public const string IsNotEmpty = "<>''";

	public const string AggregateSumStr = "SUM";

	public const string AggregateMinStr = "MIN";

	public const string AggregateMaxStr = "MAX";

	public const string AggregateAvgStr = "AVG";

	public const string AggregateCountStr = "COUNT";

	public const string DateDayStr = "DAY";

	public const string DateMonthStr = "MONTH";

	public const string DateYearStr = "YEAR";

	public const string SqlSortOrderAscendingStr = "ASC";

	public const string SqlSortOrderDescendingStr = "DESC";

	public const string SumFieldPrefix = "SUM$";

	public const string SumCountField = "$Cnt";

	public const string SiftViewIdentification = "$VSIFT$";

	public const string SequencePrefix = "$SEQ$";

	internal const string ObjectOwnerSchema = "dbo";

	public const char SqlZeroOrMoreCharactersWildcardCharacter = '%';

	public const char SqlSingleCharacterWildcardCharacter = '_';

	public const char SqlRangeWildcardStartCharacter = '[';

	public const int MaxSqlIdentifierNameLength = 128;

	private const string IdentifierNoMappingCharacter = "_$0123456789";

	private const string DefaultValueInt = "0";

	private const string DefaultValueDecimal = "0.0";

	private const string DefaultValueVarChar = "''";

	private const string DefaultValueNVarChar = "N''";

	private const string DefaultValueDateTime = "'1753.01.01'";

	private const string DefaultValueBinary = "0x00";

	private const string DefaultValueGuid = "'00000000-0000-0000-0000-000000000000'";

	private const string SelectDefaultConstraintsStatement = "SELECT default_constraints.name FROM sys.all_columns INNER JOIN sys.tables ON all_columns.object_id = tables.object_id INNER JOIN sys.schemas ON tables.schema_id = schemas.schema_id INNER JOIN sys.default_constraints ON all_columns.default_object_id = default_constraints.object_id WHERE schemas.name = N'{0}' AND tables.name = N'{1}'";

	private const string FullTableNamePlaceholder = "[{0}].[{1}].[{2}]";

	private const int SQLServerSystemNameAllowedMaxLength = 128;

	private NavSqlStatementHelper()
	{
	}

	public static string ConvertToSqlIdentifier(string identifier)
	{
		string invalidIdentifierChars = NavGlobal.AppDatabase.SqlDatabaseProperties.InvalidIdentifierChars;
		foreach (char oldChar in invalidIdentifierChars)
		{
			identifier = identifier.Replace(oldChar, '_');
		}
		return identifier;
	}

	public static StringBuilder AppendQuotedIdentifier(StringBuilder commandText, string identifier)
	{
		commandText.Append("\"");
		commandText.Append(identifier);
		commandText.Append("\"");
		return commandText;
	}

	public static string QuoteIdentifier(string identifier)
	{
		return "\"" + identifier + "\"";
	}

	public static string SequenceNameForAutoIncrementField(NCLMetaTable table, NCLMetaField field, CompanyId companyId)
	{
		string text = SequencePrefixForAutoIncrementField(table, field);
		CompanyId companyId2 = companyId;
		return text + "$" + companyId2.ToString();
	}

	public static string SequencePrefixForAutoIncrementField(NCLMetaTable table, NCLMetaField field)
	{
		return "$SEQ$" + table.TableId + "$" + field.FieldIndex;
	}

	public static StringBuilder AppendTableNameCompanyQualified(StringBuilder output, NCLMetaTable table, NavDatabase tenantDatabase, int companyIndex, NavAppTableContext tableContext, bool? overridePerCompanySchema = null, bool skipQualifyingV2Tables = false)
	{
		if (table.DataPerCompany && (!tenantDatabase.IsSharedTenantSchema || (overridePerCompanySchema.HasValue && overridePerCompanySchema.Value)))
		{
			if (companyIndex == 0)
			{
				NavCurrentThread.Session.Tenant.Diagnostics.SendTraceTag("00000OM", Category.Runtime, Verbosity.Warning, PrivacyClassification.SystemMetadata, "A SQL statement for a table per company has been made without a company token specified");
				companyIndex = NavCurrentThread.Session.Company.CompanyNameToken;
			}
			AppendCompanyNameAndSeparator(output, tenantDatabase, companyIndex);
		}
		output.Append(table.SqlTableName);
		if (tableContext.TableType == NavAppTableContextType.Companion)
		{
			output.Append('$');
			output.Append(tableContext.CompanionTableAppOrigin.AppId.Value);
		}
		else if (!skipQualifyingV2Tables && tableContext.TableType == NavAppTableContextType.Main && table.SourceExtensionType == NavExtensionType.ModernDev && table.TableType != TableType.ExternalSQL)
		{
			output.Append('$');
			output.Append(table.SourceAppId.Value);
		}
		return output;
	}

	public static StringBuilder AppendCompanyNameAndSeparator(StringBuilder output, NavDatabase tenantDatabase, int companyIndex)
	{
		output.Append(ConvertToSqlIdentifier(tenantDatabase.GetCompanyNameFromToken(companyIndex)));
		if (companyIndex != 0)
		{
			output.Append('$');
		}
		return output;
	}

	public static void AppendDatabaseAndSchemaQualification(StringBuilder output, NavDatabase database, string schema)
	{
		AppendQuotedIdentifier(output, database.DatabaseName);
		output.Append('.');
		output.Append(schema);
		output.Append('.');
	}

	public static StringBuilder AppendQuotedTableNameCompanyQualified(StringBuilder commandText, NCLMetaTable table, NavDatabase database, int companyIndex, NavAppTableContext tableContext)
	{
		AppendDatabaseAndSchemaQualification(commandText, database, (table.TableType == TableType.Normal) ? "dbo" : table.ExternalSchema);
		commandText.Append("\"");
		AppendTableNameCompanyQualified(commandText, table, database, companyIndex, tableContext);
		commandText.Append("\"");
		return commandText;
	}

	public static StringBuilder AppendQuotedTableNameQualifiedBySchemaAliasCompany(StringBuilder commandText, NCLMetaTable table, NavDatabase database, int companyIndex, string alias, NavAppTableContext tableContext)
	{
		string value = ((table.TableType == TableType.Normal) ? "dbo" : table.ExternalSchema);
		commandText.Append(value);
		commandText.Append('.');
		commandText.Append("\"");
		AppendTableNameCompanyQualified(commandText, table, database, companyIndex, tableContext);
		commandText.Append("\"");
		commandText.Append(' ');
		commandText.Append("\"");
		commandText.Append(alias);
		commandText.Append("\"");
		commandText.Append(' ');
		return commandText;
	}

	public static StringBuilder AppendQuotedTableNameQualifiedByDatabaseSchemaAliasCompany(StringBuilder commandText, NCLMetaTable table, NavDatabase database, int companyIndex, string alias, NavAppTableContext tableContext)
	{
		AppendDatabaseAndSchemaQualification(commandText, database, (table.TableType == TableType.Normal) ? "dbo" : table.ExternalSchema);
		commandText.Append("\"");
		AppendTableNameCompanyQualified(commandText, table, database, companyIndex, tableContext);
		commandText.Append("\"");
		commandText.Append(' ');
		commandText.Append("\"");
		commandText.Append(alias);
		commandText.Append("\"");
		commandText.Append(' ');
		return commandText;
	}

	public static StringBuilder AppendQuotedTableNameSchemaQualified(StringBuilder input, NCLMetaTable table, NavDatabase tenantDatabase, int companyIndex, NavAppTableContext tableContext)
	{
		input.Append((table.TableType == TableType.Normal) ? "dbo" : table.ExternalSchema);
		input.Append('.');
		input.Append("\"");
		AppendTableNameCompanyQualified(input, table, tenantDatabase, companyIndex, tableContext);
		input.Append("\"");
		return input;
	}

	public static void AppendFromClauseWithCompanionTableJoinsIncluded(StringBuilder statement, NCLMetaTable mainTable, NavDatabase database, int companyIndex, string hints, IEnumerable<NavAppOrigin> appOrigins, bool qualifyTableWithDatabase, string mainAlias)
	{
		statement.Append(" FROM ");
		if (qualifyTableWithDatabase)
		{
			AppendQuotedTableNameQualifiedByDatabaseSchemaAliasCompany(statement, mainTable, database, companyIndex, mainAlias, NavAppTableContext.Main);
		}
		else
		{
			AppendQuotedTableNameQualifiedBySchemaAliasCompany(statement, mainTable, database, companyIndex, mainAlias, NavAppTableContext.Main);
		}
		statement.Append(hints);
		if (!FeatureSwitches.Instance.DisableCompanionTableJoinOptimization)
		{
			if (appOrigins != null)
			{
				AppendCompanionTableJoinStatementFromAppOrigins(statement, mainTable, database, companyIndex, hints, appOrigins, qualifyTableWithDatabase, mainAlias);
			}
		}
		else
		{
			AppendCompanionTableJoinStatementFromAppOrigins(statement, mainTable, database, companyIndex, hints, appOrigins, qualifyTableWithDatabase, mainAlias);
		}
	}

	private static void AppendCompanionTableJoinStatementFromAppOrigins(StringBuilder statement, NCLMetaTable mainTable, NavDatabase database, int companyIndex, string hints, IEnumerable<NavAppOrigin> appOrigins, bool qualifyTableWithDatabase, string mainAlias)
	{
		foreach (NavAppOrigin appOrigin in appOrigins)
		{
			AppendCompanionTableJoinStatement(statement, mainTable, database, companyIndex, hints, appOrigin, qualifyTableWithDatabase, mainAlias);
		}
	}

	public static void AppendFromClause(StringBuilder statement, NCLMetaTable table, NavDatabase database, int companyIndex, NavAppTableContext tableContext, bool qualifyTableWithDatabase, string mainAlias)
	{
		string alias = ((tableContext.TableType == NavAppTableContextType.Main) ? mainAlias : NavGlobal.SystemTenant.NavSqlTableAliasMap.GetCompanionTableAlias(tableContext.CompanionTableAppOrigin, mainAlias));
		statement.Append(" FROM ");
		if (qualifyTableWithDatabase)
		{
			AppendQuotedTableNameQualifiedByDatabaseSchemaAliasCompany(statement, table, database, companyIndex, alias, tableContext);
		}
		else
		{
			AppendQuotedTableNameQualifiedBySchemaAliasCompany(statement, table, database, companyIndex, alias, tableContext);
		}
	}

	public static void AppendCompanionTableJoinStatement(StringBuilder statement, NCLMetaTable mainTable, NavDatabase database, int companyIndex, string hints, NavAppOrigin appOrigin, bool qualifyTableWithDatabase, string mainAlias)
	{
		NavAppTableContext tableContext = NavAppTableContext.Companion(appOrigin);
		statement.Append(" JOIN ");
		string companionTableAlias = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetCompanionTableAlias(appOrigin, mainAlias);
		if (qualifyTableWithDatabase)
		{
			AppendQuotedTableNameQualifiedByDatabaseSchemaAliasCompany(statement, mainTable, database, companyIndex, companionTableAlias, tableContext);
		}
		else
		{
			AppendQuotedTableNameQualifiedBySchemaAliasCompany(statement, mainTable, database, companyIndex, companionTableAlias, tableContext);
		}
		statement.Append(hints);
		statement.Append(" ON ");
		int num = 0;
		if (database.IsSharedTenantSchema)
		{
			num++;
			statement.Append('(');
			AppendQuotedIdentifier(statement, mainAlias);
			statement.Append('.');
			statement.Append("\"$companyId\"");
			statement.Append(" = ");
			AppendQuotedIdentifier(statement, companionTableAlias);
			statement.Append('.').Append("\"$companyId\"").Append(')');
		}
		foreach (NCLMetaField keyFields in mainTable.PrimaryKey.KeyFieldsList)
		{
			if (num > 0)
			{
				statement.Append(" AND ");
			}
			statement.Append('(');
			AppendQuotedIdentifier(statement, mainAlias);
			statement.Append('.');
			AppendQuotedIdentifier(statement, keyFields.SqlColumnName);
			statement.Append(" = ");
			AppendQuotedIdentifier(statement, companionTableAlias);
			statement.Append('.');
			AppendQuotedIdentifier(statement, keyFields.SqlColumnName);
			statement.Append(')');
			num++;
		}
	}

	public static StringBuilder AppendIndexName(StringBuilder input, NCLMetaKey key, NavDatabase tenantDatabase, int companyIndex, NavAppTableContext tableContext, bool useKeyName = true)
	{
		if (key.IsPrimaryKey || key.Unique)
		{
			AppendTableNameCompanyQualified(input, key.Parent, tenantDatabase, companyIndex, tableContext);
		}
		if (useKeyName && !string.IsNullOrEmpty(key.KeyName))
		{
			input.Append("$" + key.KeyName);
		}
		else
		{
			input.AppendFormat(CultureInfo.InvariantCulture, "${0}", key.SqlIndexNo);
		}
		return input;
	}

	public static StringBuilder AppendAutoIncrementUniqueConstraintName(StringBuilder input, NCLMetaTable table, NCLMetaField field)
	{
		input.AppendFormat(CultureInfo.InvariantCulture, "AI_{0}_{1}", table.SqlTableName, field.SqlColumnName);
		return input;
	}

	public static StringBuilder AppendQuotedIndexName(StringBuilder input, NCLMetaKey key, NavDatabase tenantDatabase, int companyIndex, NavAppTableContext tableContext)
	{
		input.Append("\"");
		AppendIndexName(input, key, tenantDatabase, companyIndex, tableContext);
		input.Append("\"");
		return input;
	}

	public static void AppendSiftIndexName(StringBuilder output, NCLMetaKey key, NavDatabase database, int companyIndex, NavAppTableContext tableContext)
	{
		AppendTableNameCompanyQualified(output, key.Parent, database, companyIndex, tableContext);
		output.Append("$VSIFT$");
		if (!string.IsNullOrEmpty(key.KeyName))
		{
			output.Append(key.KeyName);
		}
		else
		{
			output.Append(key.KeyIndex);
		}
	}

	public static void AppendQuotedSiftIndexNameWithSchemaQualification(StringBuilder output, NCLMetaKey key, NavDatabase database, int companyIndex, NavAppTableContext tableContext)
	{
		output.Append("dbo");
		output.Append('.');
		output.Append("\"");
		AppendSiftIndexName(output, key, database, companyIndex, tableContext);
		output.Append("\"");
	}

	public static void AppendQuotedSiftIndexNameWithDatabaseAndSchemaQualification(StringBuilder output, NCLMetaKey key, NavDatabase database, int companyIndex)
	{
		NavAppTableContext tableContext = (key.IsCompanionTableKey ? NavAppTableContext.Companion(key.AppOrigin) : NavAppTableContext.Main);
		AppendDatabaseAndSchemaQualification(output, database, (key.Parent.TableType == TableType.Normal) ? "dbo" : key.Parent.ExternalSchema);
		output.Append("\"");
		AppendSiftIndexName(output, key, database, companyIndex, tableContext);
		output.Append("\"");
	}

	public static string QueryColumnSourceExpression(NCLMetaQueryColumn column, Dictionary<NCLMetaQueryDataItem, NCLMetaKey> coveringSIFTIndexes, bool treatNullAsDefaultValue, NavSqlParameterHandler sqlParameterHandling)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if ((column.NclType == NavNclType.NavBlob) & treatNullAsDefaultValue)
		{
			treatNullAsDefaultValue = false;
		}
		bool flag = false;
		if (column.ParentDataItem.SubQueryDefinition == null)
		{
			if (column.ColumnType == QueryColumnType.ConstValue)
			{
				stringBuilder.Append(ParameterName(sqlParameterHandling.GetNextParameterNo));
				treatNullAsDefaultValue = false;
			}
			else
			{
				flag = coveringSIFTIndexes.ContainsKey(column.ParentDataItem);
				string sqlDataItemName = column.ParentDataItem.SqlDataItemName;
				string identifier = (flag ? sqlDataItemName : NavGlobal.SystemTenant.NavSqlTableAliasMap.ResolveFinalAlias(column.SourceTableField, sqlDataItemName));
				AppendQuotedIdentifier(stringBuilder, identifier);
				stringBuilder.Append('.');
				AppendQuotedIdentifier(stringBuilder, column.SourceTableField.SqlColumnName);
			}
		}
		else
		{
			AppendQuotedIdentifier(stringBuilder, column.ParentDataItem.SqlDataItemName);
			stringBuilder.Append('.');
			AppendQuotedIdentifier(stringBuilder, column.SourceColumn.SqlColumnName);
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		if (column.IsAggregated)
		{
			bool flag2 = false;
			switch (column.AggregationType)
			{
			case AggregationType.Average:
				if (flag)
				{
					stringBuilder2.Append("SUM");
					stringBuilder2.Append('(');
					string sqlDataItemName3 = column.ParentDataItem.SqlDataItemName;
					AppendQuotedIdentifier(stringBuilder2, sqlDataItemName3);
					stringBuilder2.Append('.');
					stringBuilder2.Append("\"");
					stringBuilder2.Append("SUM$");
					stringBuilder2.Append(column.SourceTableField.SqlColumnName);
					stringBuilder2.Append("\"");
					stringBuilder2.Append(")/");
					stringBuilder2.Append("SUM");
					stringBuilder2.Append('(');
					AppendQuotedIdentifier(stringBuilder2, "$Cnt");
					stringBuilder2.Append(')');
					flag2 = true;
				}
				else
				{
					stringBuilder2.Append("AVG");
				}
				break;
			case AggregationType.Count:
				if (flag)
				{
					stringBuilder2.Append("CAST(");
					stringBuilder2.Append("SUM");
					stringBuilder2.Append('(');
					AppendQuotedIdentifier(stringBuilder2, "$Cnt");
					stringBuilder2.Append(") AS INTEGER)");
					flag2 = true;
				}
				else
				{
					stringBuilder2.Append("COUNT");
				}
				break;
			case AggregationType.Max:
				stringBuilder2.Append("MAX");
				break;
			case AggregationType.Min:
				stringBuilder2.Append("MIN");
				break;
			case AggregationType.Sum:
				stringBuilder2.Append("SUM");
				if (flag)
				{
					stringBuilder2.Append('(');
					string sqlDataItemName2 = column.ParentDataItem.SqlDataItemName;
					AppendQuotedIdentifier(stringBuilder2, sqlDataItemName2);
					stringBuilder2.Append('.');
					stringBuilder2.Append("\"");
					stringBuilder2.Append("SUM$");
					stringBuilder2.Append(column.SourceTableField.SqlColumnName);
					stringBuilder2.Append("\"");
					stringBuilder2.Append(')');
					flag2 = true;
				}
				break;
			}
			if (!flag2)
			{
				stringBuilder2.Append('(');
				stringBuilder2.Append(stringBuilder);
				stringBuilder2.Append(')');
			}
		}
		if (column.IsDateFunction)
		{
			switch (column.DateMethod)
			{
			case DateMethodType.Day:
				stringBuilder2.Append("DAY");
				break;
			case DateMethodType.Month:
				stringBuilder2.Append("MONTH");
				break;
			case DateMethodType.Year:
				stringBuilder2.Append("YEAR");
				break;
			}
			stringBuilder2.Append('(');
			stringBuilder2.Append(stringBuilder);
			stringBuilder2.Append(')');
		}
		if (stringBuilder2.Length == 0)
		{
			stringBuilder2 = stringBuilder;
		}
		StringBuilder stringBuilder3 = new StringBuilder();
		if (column.ReverseSign)
		{
			if (column.NclType == NavNclType.NavBoolean)
			{
				stringBuilder3.Append("CAST(CASE ");
			}
			else
			{
				stringBuilder3.Append('-');
			}
		}
		if (treatNullAsDefaultValue)
		{
			stringBuilder3.Append("ISNULL(");
		}
		stringBuilder3.Append(stringBuilder2);
		if (treatNullAsDefaultValue)
		{
			stringBuilder3.Append(',');
			int parameterNo = ((column.ColumnType == QueryColumnType.ConstValue) ? sqlParameterHandling.GetNextParameterNo : sqlParameterHandling.GetParameterNoForEmptyValue(column));
			stringBuilder3.Append(ParameterName(parameterNo));
			stringBuilder3.Append(')');
		}
		if (column.ReverseSign && column.NclType == NavNclType.NavBoolean)
		{
			stringBuilder3.Append(" WHEN 0 THEN 1 ELSE 0 END AS TINYINT)");
		}
		return stringBuilder3.ToString();
	}

	public static string ParameterName(int parameterNo)
	{
		return string.Intern("@" + parameterNo);
	}

	public static string SqlStatementCompanyIdEqualsToParameterNo(int parameterNo)
	{
		return string.Intern("\"$companyId\"" + " = " + ParameterName(parameterNo) + " ");
	}

	internal static string GenerateSelectStatementHints(DataLockState lockState, bool noExpand, NavSqlConnection connection, bool withXLock = false)
	{
		StringBuilder stringBuilder = new StringBuilder(" WITH(");
		if (withXLock)
		{
			stringBuilder.Append("XLOCK,");
		}
		else
		{
			switch (lockState)
			{
			case DataLockState.None:
				switch (connection.CurrentTransactionType)
				{
				case TransactionType.UpdateNoLocks:
				case TransactionType.Browse:
				case TransactionType.Report:
					stringBuilder.Append("READUNCOMMITTED,");
					break;
				}
				break;
			case DataLockState.LockExclusive:
				stringBuilder.Append("UPDLOCK,");
				break;
			case DataLockState.Shared:
				stringBuilder.Append("REPEATABLEREAD,");
				break;
			}
		}
		if (noExpand)
		{
			stringBuilder.Append("NOEXPAND,");
		}
		if (stringBuilder.Length > 6)
		{
			stringBuilder.Length--;
			stringBuilder.Append(") ");
			return stringBuilder.ToString();
		}
		return string.Empty;
	}

	internal static string GenerateSelectStatementHints(bool locking, bool noExpand, NavSqlConnection connection, bool withXLock = false)
	{
		return GenerateSelectStatementHints(locking ? DataLockState.LockExclusive : DataLockState.None, noExpand, connection, withXLock);
	}

	public static void AppendSelectStatementOptions(StringBuilder statement, bool addFastOption, bool addForceOrderHint, bool addLoopJoinHint)
	{
		statement.Append(" OPTION(");
		int length = statement.Length;
		if (!ServerUserSettings.Instance.DisableQueryHintOptimizeForUnknown.Value)
		{
			statement.Append("OPTIMIZE FOR UNKNOWN, ");
		}
		if (addFastOption)
		{
			statement.Append("FAST 50, ");
		}
		if (addForceOrderHint && !ServerUserSettings.Instance.DisableQueryHintForceOrder.Value)
		{
			statement.Append("FORCE ORDER, ");
		}
		if (addLoopJoinHint && !ServerUserSettings.Instance.DisableQueryHintLoopJoin.Value)
		{
			statement.Append("LOOP JOIN, ");
		}
		if (statement.Length > length)
		{
			statement.Length -= 2;
			statement.Append(')');
		}
		else
		{
			statement.Length -= " OPTION(".Length;
		}
	}

	public static void AppendSelectStatementOptions(StringBuilder statement, bool addFastOption)
	{
		AppendSelectStatementOptions(statement, addFastOption, addForceOrderHint: false, addLoopJoinHint: false);
	}

	public static string WildcardText(string text)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			switch (text[i])
			{
			case '@':
				if (i > 0)
				{
					stringBuilder.Append(text[i]);
				}
				break;
			case '*':
				stringBuilder.Append('%');
				break;
			case '?':
				stringBuilder.Append('_');
				break;
			case '%':
			case '[':
			case '_':
				stringBuilder.Append('[');
				stringBuilder.Append(text[i]);
				stringBuilder.Append(']');
				break;
			default:
				stringBuilder.Append(text[i]);
				break;
			}
		}
		return stringBuilder.ToString();
	}

	public static void AppendFieldList(StringBuilder fieldList, IEnumerable<NCLMetaField> fields, NavSqlColumnListType columnListType, string mainTableAlias)
	{
		switch (columnListType)
		{
		case NavSqlColumnListType.SiftCount:
			fieldList.Append("SUM");
			fieldList.Append('(');
			AppendQuotedIdentifier(fieldList, "$Cnt");
			fieldList.Append(')');
			return;
		case NavSqlColumnListType.Count:
			fieldList.Append("COUNT");
			fieldList.Append("(*)");
			return;
		}
		foreach (NCLMetaField field in fields)
		{
			switch (columnListType)
			{
			case NavSqlColumnListType.DataLengthInsteadOfBlobs:
				if (field.FieldNclType == NavNclType.NavBlob)
				{
					fieldList.Append("DATALENGTH(");
					AppendSqlColumnName(fieldList, field, mainTableAlias);
					fieldList.Append(')');
				}
				else
				{
					AppendSqlColumnName(fieldList, field, mainTableAlias);
				}
				break;
			case NavSqlColumnListType.Sum:
				fieldList.Append("SUM");
				fieldList.Append('(');
				AppendSqlColumnName(fieldList, field, mainTableAlias);
				fieldList.Append(')');
				break;
			case NavSqlColumnListType.Min:
				fieldList.Append("MIN");
				fieldList.Append('(');
				AppendSqlColumnName(fieldList, field, mainTableAlias);
				fieldList.Append(')');
				break;
			case NavSqlColumnListType.Max:
				fieldList.Append("MAX");
				fieldList.Append('(');
				AppendSqlColumnName(fieldList, field, mainTableAlias);
				fieldList.Append(')');
				break;
			case NavSqlColumnListType.Average:
				fieldList.Append("AVG");
				fieldList.Append('(');
				AppendSqlColumnName(fieldList, field, mainTableAlias);
				fieldList.Append(')');
				break;
			case NavSqlColumnListType.SiftSum:
				fieldList.Append("SUM");
				fieldList.Append('(');
				fieldList.Append("\"");
				fieldList.Append("SUM$");
				fieldList.Append(field.SqlColumnName);
				fieldList.Append("\"");
				fieldList.Append(')');
				break;
			case NavSqlColumnListType.SiftAverage:
				fieldList.Append("SUM");
				fieldList.Append('(');
				fieldList.Append("\"");
				fieldList.Append("SUM$");
				fieldList.Append(field.SqlColumnName);
				fieldList.Append("\"");
				fieldList.Append(")/");
				fieldList.Append("SUM");
				fieldList.Append('(');
				AppendQuotedIdentifier(fieldList, "$Cnt");
				fieldList.Append(')');
				break;
			case NavSqlColumnListType.BaseTableAggregation:
				switch (field.CalculationFormula.CalculationMethod)
				{
				case NCLMetaCalculationMethod.Average:
					fieldList.Append("AVG");
					break;
				case NCLMetaCalculationMethod.Count:
					fieldList.Append("COUNT");
					break;
				case NCLMetaCalculationMethod.Max:
					fieldList.Append("MAX");
					break;
				case NCLMetaCalculationMethod.Min:
					fieldList.Append("MIN");
					break;
				case NCLMetaCalculationMethod.Sum:
					fieldList.Append("SUM");
					break;
				}
				fieldList.Append('(');
				if (field.CalculationFormula.CalculationMethod == NCLMetaCalculationMethod.Count)
				{
					fieldList.Append('*');
				}
				else
				{
					AppendSqlColumnName(fieldList, field.CalculationFormula.SourceField, mainTableAlias);
				}
				fieldList.Append(')');
				break;
			case NavSqlColumnListType.SiftAggregation:
				switch (field.CalculationFormula.CalculationMethod)
				{
				case NCLMetaCalculationMethod.Sum:
					fieldList.Append("SUM");
					fieldList.Append('(');
					fieldList.Append("\"");
					fieldList.Append("SUM$");
					fieldList.Append(field.CalculationFormula.SourceField.SqlColumnName);
					fieldList.Append("\"");
					fieldList.Append(')');
					break;
				case NCLMetaCalculationMethod.Average:
					fieldList.Append("SUM");
					fieldList.Append('(');
					fieldList.Append("\"");
					fieldList.Append("SUM$");
					fieldList.Append(field.CalculationFormula.SourceField.SqlColumnName);
					fieldList.Append("\"");
					fieldList.Append(")/");
					fieldList.Append("SUM");
					fieldList.Append('(');
					AppendQuotedIdentifier(fieldList, "$Cnt");
					fieldList.Append(')');
					break;
				case NCLMetaCalculationMethod.Count:
					fieldList.Append("SUM");
					fieldList.Append('(');
					AppendQuotedIdentifier(fieldList, "$Cnt");
					fieldList.Append(')');
					break;
				}
				break;
			}
			fieldList.Append(',');
		}
		if (fieldList.Length > 0)
		{
			fieldList.Length--;
		}
	}

	public static void AppendFieldList(StringBuilder fieldList, IEnumerable<NCLMetaField> fields, string mainTableAlias, bool addCompanyIdField = false)
	{
		foreach (NCLMetaField field in fields)
		{
			AppendSqlColumnName(fieldList, field, mainTableAlias);
			fieldList.Append(',');
		}
		if (fieldList.Length > 0)
		{
			fieldList.Length--;
		}
		if (addCompanyIdField)
		{
			fieldList.Append(",");
			fieldList.Append("\"$companyId\"");
		}
	}

	public static StringBuilder AppendOrderByList(StringBuilder orderByList, SortingFieldList sortingFields)
	{
		orderByList.Append(" ORDER BY ");
		foreach (INavSortingField sortingField in sortingFields)
		{
			NCLMetaField nCLMetaField = sortingField.Field as NCLMetaField;
			bool flag = nCLMetaField.FieldNclType == NavNclType.NavBlob;
			if (flag)
			{
				orderByList.Append("DATALENGTH(");
			}
			AppendQuotedIdentifier(orderByList, nCLMetaField.SqlColumnName);
			if (flag)
			{
				orderByList.Append(')');
			}
			orderByList.Append(' ');
			orderByList.Append((sortingField.Order == SortOrder.Descending) ? "DESC" : "ASC");
			orderByList.Append(',');
		}
		orderByList.Length--;
		return orderByList;
	}

	public static StringBuilder AppendCollate(StringBuilder input, string collation)
	{
		if (collation == null)
		{
			return input;
		}
		input.Append(" COLLATE ");
		input.Append(collation);
		return input;
	}

	public static StringBuilder AppendCollateForVariantParameter(StringBuilder input, string parameterName, NCLMetaField field, string collation)
	{
		input.AppendFormat(CultureInfo.InvariantCulture, "CASE SQL_VARIANT_PROPERTY({0}, 'BaseType') WHEN 'nvarchar' THEN CAST({0} AS {2}) COLLATE {1} WHEN 'varchar' THEN CAST({0} AS {2}) COLLATE {1} ELSE {0} END", parameterName, collation, AppendSqlColumnTypeAndLength(new StringBuilder(), NavNclType.NavText, SqlDataType.Default, field.FieldDefinedLength));
		return input;
	}

	public static StringBuilder AppendSqlColumnTypeAndLength(StringBuilder input, NavNclType nclType, SqlDataType sqlDataType, int definedLength = 0, string collation = null)
	{
		switch (nclType)
		{
		case NavNclType.NavInteger:
		case NavNclType.NavOption:
			input.Append("INT");
			break;
		case NavNclType.NavBoolean:
		case NavNclType.NavByte:
			input.Append("TINYINT");
			break;
		case NavNclType.NavDecimal:
			input.Append("DECIMAL(38,20)");
			break;
		case NavNclType.NavBigInteger:
		case NavNclType.NavDuration:
			if (sqlDataType == SqlDataType.Timestamp)
			{
				input.Append("TIMESTAMP");
			}
			else
			{
				input.Append("BIGINT");
			}
			break;
		case NavNclType.NavDateFormula:
			input.AppendFormat(CultureInfo.InvariantCulture, "VARCHAR({0})", 32);
			break;
		case NavNclType.NavOemText:
			input.AppendFormat(CultureInfo.InvariantCulture, "VARCHAR({0})", definedLength);
			AppendCollate(input, collation);
			break;
		case NavNclType.NavText:
			input.AppendFormat(CultureInfo.InvariantCulture, "NVARCHAR({0})", definedLength);
			AppendCollate(input, collation);
			break;
		case NavNclType.NavRecordId:
			input.AppendFormat(CultureInfo.InvariantCulture, "VARBINARY({0})", 448);
			break;
		case NavNclType.NavTableFilter:
			input.AppendFormat(CultureInfo.InvariantCulture, "VARBINARY({0})", 504);
			break;
		case NavNclType.NavMedia:
		case NavNclType.NavMediaSet:
		case NavNclType.NavGuid:
			input.Append("UNIQUEIDENTIFIER");
			break;
		case NavNclType.NavOemCode:
		case NavNclType.NavCode:
			switch (sqlDataType)
			{
			case SqlDataType.VarChar:
			case SqlDataType.Default:
				if (nclType == NavNclType.NavCode)
				{
					input.AppendFormat(CultureInfo.InvariantCulture, "NVARCHAR({0})", definedLength);
					AppendCollate(input, collation);
				}
				else
				{
					input.AppendFormat(CultureInfo.InvariantCulture, "VARCHAR({0})", definedLength);
					AppendCollate(input, collation);
				}
				break;
			case SqlDataType.Integer:
				input.Append("INT");
				break;
			case SqlDataType.BigInt:
				input.Append("BIGINT");
				break;
			case SqlDataType.Variant:
				input.Append("SQL_VARIANT");
				break;
			default:
				throw new NotSupportedException();
			}
			break;
		case NavNclType.NavDate:
		case NavNclType.NavTime:
		case NavNclType.NavDateTime:
			input.Append("DATETIME");
			break;
		case NavNclType.NavBlob:
			input.Append("IMAGE");
			break;
		default:
			throw new NotSupportedException();
		}
		return input;
	}

	public static StringBuilder AppendSqlColumnTypeAndMaxLength(StringBuilder input, NavNclType nclType, string collation = null)
	{
		if (nclType == NavNclType.NavText)
		{
			input.Append("NVARCHAR(max)");
			AppendCollate(input, collation);
			return input;
		}
		throw new NotSupportedException();
	}

	public static SqlParameter GetOrCreateSqlParameterNo(SqlParameterCollection sqlParameters, int parameterNo)
	{
		SqlParameter sqlParameter;
		if (parameterNo < sqlParameters.Count)
		{
			sqlParameter = sqlParameters[parameterNo];
		}
		else
		{
			sqlParameter = new SqlParameter();
			sqlParameter.ParameterName = ParameterName(parameterNo);
			sqlParameters.Add(sqlParameter);
		}
		return sqlParameter;
	}

	public static void SetSqlParameter(SqlParameter sqlParameter, INavFieldMetadata field, object value, bool isWildcardValue, bool isFilterValue)
	{
		SetSqlParameter(sqlParameter, field.NclType, field.SqlDataType, field.NavDefinedLengthMetadata, value, isWildcardValue, isFilterValue);
	}

	public static void SetSqlParameter(SqlParameter sqlParameter, NavNclType nclType, SqlDataType sqlDataType, int length, object value, bool isWildcardValue, bool isFilterValue)
	{
		switch (nclType)
		{
		case NavNclType.NavInteger:
		case NavNclType.NavOption:
			sqlParameter.Value = value;
			sqlParameter.SqlDbType = SqlDbType.Int;
			break;
		case NavNclType.NavBoolean:
		case NavNclType.NavByte:
			sqlParameter.Value = value;
			sqlParameter.SqlDbType = SqlDbType.TinyInt;
			break;
		case NavNclType.NavDecimal:
			sqlParameter.SqlDbType = SqlDbType.Decimal;
			sqlParameter.Precision = 38;
			sqlParameter.Scale = 20;
			sqlParameter.Value = null;
			sqlParameter.SqlValue = value;
			break;
		case NavNclType.NavBigInteger:
		case NavNclType.NavDuration:
			sqlParameter.Value = value;
			if (sqlDataType == SqlDataType.Timestamp)
			{
				sqlParameter.SqlDbType = SqlDbType.Binary;
			}
			else
			{
				sqlParameter.SqlDbType = SqlDbType.BigInt;
			}
			sqlParameter.Size = 8;
			break;
		case NavNclType.NavDateFormula:
			sqlParameter.Value = value;
			sqlParameter.SqlDbType = SqlDbType.VarChar;
			sqlParameter.Size = 32;
			break;
		case NavNclType.NavOemText:
			sqlParameter.Value = value;
			sqlParameter.SqlDbType = SqlDbType.VarChar;
			sqlParameter.Size = (isWildcardValue ? (length * 2) : length);
			break;
		case NavNclType.NavText:
			sqlParameter.Value = value;
			sqlParameter.SqlDbType = SqlDbType.NVarChar;
			sqlParameter.Size = (isWildcardValue ? (length * 2) : length);
			break;
		case NavNclType.NavRecordId:
			sqlParameter.SqlDbType = SqlDbType.VarBinary;
			sqlParameter.Size = 448;
			sqlParameter.Value = null;
			sqlParameter.SqlValue = value;
			break;
		case NavNclType.NavTableFilter:
			sqlParameter.SqlDbType = SqlDbType.VarBinary;
			sqlParameter.Size = 504;
			sqlParameter.Value = null;
			sqlParameter.SqlValue = value;
			break;
		case NavNclType.NavMedia:
		case NavNclType.NavMediaSet:
		case NavNclType.NavGuid:
			sqlParameter.Value = value;
			sqlParameter.SqlDbType = SqlDbType.UniqueIdentifier;
			break;
		case NavNclType.NavOemCode:
		case NavNclType.NavCode:
			sqlParameter.Value = value;
			switch ((!isWildcardValue) ? sqlDataType : SqlDataType.VarChar)
			{
			case SqlDataType.VarChar:
			case SqlDataType.Default:
				sqlParameter.SqlDbType = ((nclType == NavNclType.NavOemCode) ? SqlDbType.VarChar : SqlDbType.NVarChar);
				sqlParameter.Size = (isWildcardValue ? (length * 2) : length);
				break;
			case SqlDataType.Integer:
				sqlParameter.SqlDbType = SqlDbType.Int;
				break;
			case SqlDataType.BigInt:
				sqlParameter.SqlDbType = SqlDbType.BigInt;
				break;
			case SqlDataType.Variant:
				sqlParameter.SqlDbType = SqlDbType.Variant;
				if (sqlParameter.Value.GetType() == typeof(string))
				{
					sqlParameter.Size = length;
				}
				else
				{
					sqlParameter.Size = 8;
				}
				break;
			default:
				throw new NotSupportedException();
			}
			break;
		case NavNclType.NavDate:
		case NavNclType.NavTime:
		case NavNclType.NavDateTime:
			sqlParameter.Value = value;
			sqlParameter.SqlDbType = SqlDbType.DateTime;
			break;
		case NavNclType.NavBlob:
			sqlParameter.Value = value;
			if (isFilterValue)
			{
				sqlParameter.SqlDbType = SqlDbType.Int;
				break;
			}
			sqlParameter.SqlDbType = SqlDbType.Binary;
			sqlParameter.Size = int.MaxValue;
			break;
		default:
			throw new NotSupportedException();
		}
	}

	public static void SetSqlParameter(SqlParameter sqlParameter, CompanyId companyId)
	{
		sqlParameter.Value = companyId.Value;
		sqlParameter.SqlDbType = SqlDbType.Int;
	}

	public static void SetSqlParameter(SqlParameter sqlParameter, object value, SqlDbType valueSqlDbType)
	{
		sqlParameter.Value = value;
		sqlParameter.SqlDbType = valueSqlDbType;
	}

	public static void SetSqlParameterWithEmptyValue(SqlParameter sqlParameter, INavFieldMetadata field, Func<INavFieldMetadata, object> getEmptySqlValue)
	{
		if (field is NCLMetaQueryColumn nCLMetaQueryColumn && nCLMetaQueryColumn.SourceColumn.AggregationType == AggregationType.Count)
		{
			sqlParameter.Value = NavInteger.Default.ValueAsObject;
			sqlParameter.SqlDbType = SqlDbType.Int;
		}
		else
		{
			SetSqlParameter(sqlParameter, field.NclType, field.SqlDataType, field.NavDefinedLengthMetadata, getEmptySqlValue(field), isWildcardValue: false, isFilterValue: false);
		}
	}

	public static SqlParameter CreateSqlParameter(string parameterName, NavNclType nclType, SqlDataType sqlDataType, int length, object value, bool isWildcardValue, bool isFilterValue)
	{
		SqlParameter sqlParameter = new SqlParameter();
		sqlParameter.ParameterName = parameterName;
		SetSqlParameter(sqlParameter, nclType, sqlDataType, length, value, isWildcardValue, isFilterValue);
		return sqlParameter;
	}

	public static SqlParameter CreateSqlParameter(string parameterName, CompanyId value)
	{
		return CreateSqlParameter(parameterName, NavNclType.NavInteger, SqlDataType.Integer, 0, value, isWildcardValue: false, isFilterValue: false);
	}

	public static void AppendSqlDefaultValueAsLiteral(StringBuilder statement, NCLMetaField field)
	{
		switch (field.FieldNclType)
		{
		case NavNclType.NavBoolean:
		case NavNclType.NavByte:
		case NavNclType.NavInteger:
		case NavNclType.NavBigInteger:
		case NavNclType.NavOption:
		case NavNclType.NavDuration:
			if (field.SqlDataType == SqlDataType.Timestamp)
			{
				throw new NotSupportedException("Specifying default value for timestamp column");
			}
			statement.Append("0");
			break;
		case NavNclType.NavDecimal:
			statement.Append("0.0");
			break;
		case NavNclType.NavOemText:
		case NavNclType.NavDateFormula:
			statement.Append("''");
			break;
		case NavNclType.NavText:
			statement.Append("N''");
			break;
		case NavNclType.NavRecordId:
		case NavNclType.NavTableFilter:
			statement.Append("0x00");
			break;
		case NavNclType.NavMedia:
		case NavNclType.NavMediaSet:
		case NavNclType.NavGuid:
			statement.Append("'00000000-0000-0000-0000-000000000000'");
			break;
		case NavNclType.NavOemCode:
		case NavNclType.NavCode:
			switch (field.SqlDataType)
			{
			case SqlDataType.VarChar:
			case SqlDataType.Variant:
			case SqlDataType.Default:
				if (field.FieldNclType == NavNclType.NavCode)
				{
					statement.Append("N''");
				}
				else
				{
					statement.Append("''");
				}
				break;
			case SqlDataType.Integer:
			case SqlDataType.BigInt:
				statement.Append("0");
				break;
			default:
				throw new NotSupportedException();
			}
			break;
		case NavNclType.NavDate:
		case NavNclType.NavTime:
		case NavNclType.NavDateTime:
			statement.Append("'1753.01.01'");
			break;
		case NavNclType.NavBlob:
			throw new NotSupportedException("No default value for BLOB's");
		default:
			throw new NotSupportedException();
		}
	}

	public static void AppendSqlColumnName(StringBuilder statement, NCLMetaField field, string mainAlias)
	{
		string alias = null;
		if (mainAlias != null)
		{
			alias = NavGlobal.SystemTenant.NavSqlTableAliasMap.ResolveFinalAlias(field, mainAlias);
		}
		AppendSqlColumnName(statement, field.SqlColumnName, alias);
	}

	public static void ComposeSelectAllDefaultConstraintsOfTableStatement(StringBuilder commandText, NCLMetaTable table, NavDatabase tenantDatabase, int companyIndex, NavAppTableContext tableContext, string tableNamePrefix = "")
	{
		StringBuilder stringBuilder = new StringBuilder(tableNamePrefix);
		AppendTableNameCompanyQualified(stringBuilder, table, tenantDatabase, companyIndex, tableContext);
		commandText.AppendFormat(CultureInfo.InvariantCulture, "SELECT default_constraints.name FROM sys.all_columns INNER JOIN sys.tables ON all_columns.object_id = tables.object_id INNER JOIN sys.schemas ON tables.schema_id = schemas.schema_id INNER JOIN sys.default_constraints ON all_columns.default_object_id = default_constraints.object_id WHERE schemas.name = N'{0}' AND tables.name = N'{1}'", "dbo", stringBuilder.ToString());
	}

	public static void AppendSqlColumnName(StringBuilder statement, string sqlColumnName, string alias)
	{
		if (alias != null)
		{
			AppendQuotedIdentifier(statement, alias);
			statement.Append('.');
		}
		AppendQuotedIdentifier(statement, sqlColumnName);
	}

	public static void AppendSqlFieldDefaultConstraintName(StringBuilder statement, NCLMetaTable table, NCLMetaField field, NavDatabase database, int companyToken, NavAppTableContext tableContext, bool? overridePerCompanySchema = null)
	{
		string value = BuildAndFormatConstraintName(table, field, database, companyToken, tableContext, overridePerCompanySchema);
		statement.Append(value);
	}

	public static void BuildConstraintName(StringBuilder constraintNameBuilder, NCLMetaTable table, NCLMetaField field, NavDatabase database, int companyToken, NavAppTableContext tableContext, bool? overridePerCompanySchema = null)
	{
		AppendTableNameCompanyQualified(constraintNameBuilder, table, database, companyToken, tableContext, overridePerCompanySchema);
		constraintNameBuilder.Append(field.SqlColumnName);
	}

	public static string BuildAndFormatConstraintName(NCLMetaTable table, NCLMetaField field, NavDatabase database, int companyToken, NavAppTableContext tableContext, bool? overridePerCompanySchema = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		BuildConstraintName(stringBuilder, table, field, database, companyToken, tableContext, overridePerCompanySchema);
		string rawConstraintName = stringBuilder.ToString();
		return GenerateConstraintNameHash(rawConstraintName);
	}

	public static string GenerateConstraintNameHash(string rawConstraintName)
	{
		Sha256Hash sha256Hash = Sha256Hash.ComputeHashForContent(Encoding.UTF8.GetBytes(rawConstraintName));
		return "MDF_" + sha256Hash.HexString;
	}

	public static string GetQuotedTableName(NavDatabase database, string objectOwnerSchema, string tableName)
	{
		return string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}].[{2}]", database.DatabaseName, objectOwnerSchema, tableName);
	}
}

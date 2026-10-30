#define SQLSEQUENCE
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlDdlCommands : NavSqlBatchedCommand
{
	public const string CompanyIdPartitionFunctionName = "CompanyIdPartitionFunction";

	public const string CompanyIdPartitionSchemeName = "CompanyIdPartitionScheme";

	private static readonly string DropPartitionSchemeCmd = FormattableString.Invariant(FormattableStringFactory.Create("IF (EXISTS( SELECT * FROM sys.partition_schemes WHERE name = '{0}' ) )\r\n                BEGIN\r\n                    DROP PARTITION SCHEME {1}; \r\n                END", "CompanyIdPartitionScheme", "CompanyIdPartitionScheme"));

	private static readonly string DropPartitionFunctionIfChangedCmd = FormattableString.Invariant(FormattableStringFactory.Create("\r\n                    {0} \r\n                    IF (EXISTS( SELECT * FROM sys.partition_functions WHERE name = '{1}' ) )\r\n                    BEGIN\r\n                        DROP PARTITION FUNCTION CompanyIdPartitionFunction;\r\n                    END", DropPartitionSchemeCmd, "CompanyIdPartitionFunction"));

	private const string CreatePartitionFunctionIfNotExistsCmd = "\r\n                IF NOT (EXISTS( SELECT * FROM sys.partition_functions WHERE name = '{0}' ) )\r\n                BEGIN\r\n                    CREATE PARTITION FUNCTION {0} (int) \r\n                        AS RANGE RIGHT FOR VALUES ({1})\r\n                END";

	private const string CreatePartitionSchemeIfNotExistsCmd = "                \r\n                IF NOT(EXISTS( SELECT* FROM sys.partition_schemes WHERE name = '{0}') )\r\n                BEGIN\r\n                    CREATE PARTITION SCHEME {0}\r\n                    AS PARTITION {1} \r\n                    ALL TO ([PRIMARY]) ;  \r\n                END";

	private const int MaxCountOfCompaniesInOnePartition = 1000;

	private const string SqlSequenceFeature = "SQLSEQUENCE";

	private bool seqVariablesDeclared;

	private const string ChangeRetentionPeriod = "10 DAYS";

	private const string TempPrefix = "TEMP$";

	public const string CompanyIdFieldName = "$companyId";

	public const string EnableOnlineIndexes = "ONLINE=ON";

	public const string CompanyIdFieldNameQuoted = "\"$companyId\"";

	private void EnsurePartitionFunctionAndSchemeCreated(int partitionsCount, int bucketSize)
	{
		StartNewCommand();
		List<int> list = new List<int>();
		for (int i = 1; i < partitionsCount; i++)
		{
			list.Add(1000 * (1 + bucketSize * i));
		}
		CommandText.AppendLine(DropPartitionFunctionIfChangedCmd);
		CommandText.AppendLine(string.Format(CultureInfo.InvariantCulture, "\r\n                IF NOT (EXISTS( SELECT * FROM sys.partition_functions WHERE name = '{0}' ) )\r\n                BEGIN\r\n                    CREATE PARTITION FUNCTION {0} (int) \r\n                        AS RANGE RIGHT FOR VALUES ({1})\r\n                END", "CompanyIdPartitionFunction", string.Join(",", list)));
		CommandText.AppendLine(string.Format(CultureInfo.InvariantCulture, "                \r\n                IF NOT(EXISTS( SELECT* FROM sys.partition_schemes WHERE name = '{0}') )\r\n                BEGIN\r\n                    CREATE PARTITION SCHEME {0}\r\n                    AS PARTITION {1} \r\n                    ALL TO ([PRIMARY]) ;  \r\n                END", "CompanyIdPartitionScheme", "CompanyIdPartitionFunction"));
	}

	public static void EnsurePartitionFunctionAndSchemeCreated(NavDatabase tenantDatabase, int partitionsCount, int bucketSize)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(tenantDatabase);
		navSqlDdlCommands.EnsurePartitionFunctionAndSchemeCreated(partitionsCount, bucketSize);
		navSqlDdlCommands.ExecuteNonQuery();
	}

	private void EnsureSequenceVariablesDeclared()
	{
		if (!seqVariablesDeclared)
		{
			DeclareSequenceVariables();
			seqVariablesDeclared = true;
		}
	}

	[Conditional("SQLSEQUENCE")]
	public void DropSequenceIfExists(NavDatabase database, NCLMetaTable table, NCLMetaField field, int companyToken)
	{
		string name = NavSqlStatementHelper.SequenceNameForAutoIncrementField(table, field, database.GetCompanyId(table, companyToken).Value);
		DropSequenceIfExists(name);
	}

	[Conditional("SQLSEQUENCE")]
	public void CreateOrAlterSequenceForAutoIncrementField(NCLMetaTable table, NCLMetaField field, CompanyId companyId)
	{
		CreateOrAlterSequence(NavSqlStatementHelper.SequenceNameForAutoIncrementField(table, field, companyId), field.SqlDataType, 1L);
	}

	[Conditional("SQLSEQUENCE")]
	private void InitAutoIncrementValues(NCLMetaTable table, NCLMetaField autoincrementField)
	{
		StartNewCommand();
		EnsureSequenceVariablesDeclared();
		string text = NavSqlStatementHelper.SequencePrefixForAutoIncrementField(table, autoincrementField);
		CommandText.AppendFormat(CultureInfo.InvariantCulture, "\r\n                select distinct [{3}] into #companies from [{0}].[{1}];\r\n                SET @SeqSQLStatement = N'';\r\n                select @SeqSQLStatement = COALESCE(@SeqSQLStatement + '; ', '') + 'update [{0}].[{1}] set [{2}] = (NEXT VALUE FOR [{0}].[{4}$'+CAST([{3}] as nvarchar)+']) where [{3}] = ' + CAST([{3}] as nvarchar) + ' and [{2}] is NULL' from #companies\r\n                EXEC(@SeqSQLStatement);\r\n                drop table #companies;", "dbo", table.SqlTableName, autoincrementField.SqlColumnName, "$companyId", text);
	}

	[Conditional("SQLSEQUENCE")]
	internal void CreateOrAlterSequence(string name, SqlDataType type, long start, int cache = 1)
	{
		DropSequenceIfExists(name);
		StartNewCommand();
		CreateSequence(name, type, start, cache);
	}

	private void CreateSequence(string name, SqlDataType type, long start, int cache = 1)
	{
		CommandText.Append("CREATE SEQUENCE ");
		CommandText.Append("dbo");
		CommandText.Append(".[");
		CommandText.Append(name);
		CommandText.AppendLine("]");
		if (type == SqlDataType.BigInt)
		{
			CommandText.AppendLine(" AS BIGINT ");
		}
		CommandText.Append("START WITH ");
		CommandText.AppendLine(start.ToString(CultureInfo.InvariantCulture));
		CommandText.AppendLine("INCREMENT BY 1");
		if (cache > 1)
		{
			CommandText.Append("CACHE ");
			CommandText.AppendLine(cache.ToString(CultureInfo.InvariantCulture));
		}
		else
		{
			CommandText.AppendLine("NO CACHE");
		}
	}

	[Conditional("SQLSEQUENCE")]
	internal void CreateSequenceIfNotExists(string name, SqlDataType type, long start, int cache = 1)
	{
		StartNewCommand();
		CommandText.Append("IF NOT EXISTS(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'");
		CommandText.Append("dbo");
		CommandText.Append(".[");
		CommandText.Append(name);
		CommandText.Append("]') AND type = 'SO')");
		CreateSequence(name, type, start, cache);
	}

	[Conditional("SQLSEQUENCE")]
	private void DropSequenceIfExists(string name)
	{
		StartNewCommand();
		CommandText.Append("IF EXISTS(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'");
		CommandText.Append("dbo");
		CommandText.Append(".[");
		CommandText.Append(name);
		CommandText.Append("]') AND type = 'SO')");
		CommandText.Append("DROP SEQUENCE ");
		CommandText.Append("dbo");
		CommandText.Append(".[");
		CommandText.Append(name);
		CommandText.AppendLine("]");
	}

	[Conditional("SQLSEQUENCE")]
	private static void CreateSequenceForAutoIncrementField(NCLMetaTable table, NCLMetaField field, NavDatabase database, int companyToken)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		string name = NavSqlStatementHelper.SequenceNameForAutoIncrementField(table, field, database.GetCompanyId(table, companyToken).Value);
		navSqlDdlCommands.CreateOrAlterSequence(name, field.SqlDataType, 1L);
		navSqlDdlCommands.ExecuteNonQuery();
	}

	[Conditional("SQLSEQUENCE")]
	private void RestartSequenceForAutoIncrementField(NCLMetaTable table, NCLMetaField field, NavDatabase database, int companyToken)
	{
		RestartSequenceForAutoIncrementField(table, field, database.GetCompanyId(table, companyToken).Value);
	}

	[Conditional("SQLSEQUENCE")]
	private void DeclareSequenceVariables()
	{
		StartNewCommand();
		CommandText.AppendFormat(CultureInfo.InvariantCulture, "DECLARE @SeqStartValue bigint;");
		CommandText.AppendFormat(CultureInfo.InvariantCulture, "DECLARE @SeqSQLStatement NVARCHAR(MAX);");
	}

	[Conditional("SQLSEQUENCE")]
	public void RestartSequenceForAutoIncrementField(NCLMetaTable table, NCLMetaField field, CompanyId companyId)
	{
		EnsureSequenceVariablesDeclared();
		string text = NavSqlStatementHelper.SequenceNameForAutoIncrementField(table, field, companyId);
		CreateOrAlterSequence(text, field.SqlDataType, 1L);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}].[{1}", "dbo", table.SqlTableName);
		if (table.SourceExtensionType == NavExtensionType.ModernDev)
		{
			stringBuilder.Append('$');
			stringBuilder.Append(table.SourceAppId.Value);
		}
		stringBuilder.Append("]");
		string text2 = stringBuilder.ToString();
		StartNewCommand();
		CommandText.AppendFormat(CultureInfo.InvariantCulture, "SET @SeqStartValue = (select MAX([{0}]) from {1} where {3} =  /*escape*/{4});", field.SqlColumnName, text2, table.TableId, "\"$companyId\"", companyId);
		CommandText.AppendFormat(CultureInfo.InvariantCulture, "SET @SeqSQLStatement = N'ALTER SEQUENCE {0}.[{1}] RESTART WITH ' + CAST((@SeqStartValue+1) as NVARCHAR);", "dbo", text, table.TableId, companyId);
		CommandText.AppendLine("EXEC(@SeqSQLStatement);");
	}

	[Conditional("SQLSEQUENCE")]
	private void SequenceNextValueFor(NCLMetaTable table, NCLMetaField field, CompanyId? companyId)
	{
		CommandText.Append("NEXT VALUE FOR [" + NavSqlStatementHelper.SequenceNameForAutoIncrementField(table, field, companyId.Value) + "]");
	}

	public static void RestartSequencesForAllCompanies(NavDatabase tenantDatabase)
	{
		IEnumerable<NCLMetaTable> enumerable = from t in NavGlobal.NCLMetadata.GetSnapshotOfAllNonVirtualMetaTables()
			where !t.IsSystemOrVirtualTable && tenantDatabase.HasSequencedAutoIncrementField(t, NavAppTableContext.Main)
			select t;
		int[] companyTokens = tenantDatabase.GetCompanyTokens();
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(tenantDatabase);
		foreach (NCLMetaTable item in enumerable)
		{
			if (item.DataPerCompany)
			{
				int[] array = companyTokens;
				foreach (int companyToken in array)
				{
					CompanyId? companyId = tenantDatabase.GetCompanyId(item, companyToken);
					navSqlDdlCommands.RestartSequenceForAutoIncrementField(item, item.AutoIncrementField, companyId.Value);
				}
			}
			else
			{
				CompanyId? companyId2 = tenantDatabase.GetCompanyId(item, 0);
				navSqlDdlCommands.RestartSequenceForAutoIncrementField(item, item.AutoIncrementField, companyId2.Value);
			}
		}
		navSqlDdlCommands.ExecuteNonQuery();
	}

	public override void ExecuteNonQuery()
	{
		base.ExecuteNonQuery();
		seqVariablesDeclared = false;
	}

	public NavSqlDdlCommands(NavDatabase tenantDatabase)
		: base(tenantDatabase)
	{
	}

	private void AppendNewFieldClause(NCLMetaField field, bool allowNull, NavAppTableContext tableContext)
	{
		NavSqlStatementHelper.AppendQuotedIdentifier(CommandText, field.SqlColumnName);
		CommandText.Append(' ');
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(CommandText, field.FieldNclType, field.SqlDataType, field.FieldDefinedLength, NavGlobal.AppDatabase.SqlSortingProperties.Collation);
		if (!Database.HasSequencedAutoIncrementField(field.Parent, tableContext) && field.FieldIsAutoIncrement && tableContext.TableType != NavAppTableContextType.Companion)
		{
			CommandText.Append(" IDENTITY(1,1) ");
		}
		CommandText.Append(allowNull ? " NULL " : " NOT NULL");
	}

	private void AppendCompanyIdField()
	{
		CommandText.Append("\"$companyId\"");
		CommandText.Append(' ');
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(CommandText, NavNclType.NavInteger, SqlDataType.Integer);
		CommandText.Append(',');
	}

	internal static void AppendCompanyIdFieldName(StringBuilder sqlCommand, bool commaPrefix, bool commaSuffix, NavAppTableContext tableContext = null, string mainTableAlias = null)
	{
		if (commaPrefix)
		{
			sqlCommand.Append(',');
		}
		if (mainTableAlias != null && tableContext != null && tableContext.TableType == NavAppTableContextType.Companion)
		{
			mainTableAlias = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetCompanionTableAlias(tableContext.CompanionTableAppOrigin, mainTableAlias);
		}
		NavSqlStatementHelper.AppendSqlColumnName(sqlCommand, "$companyId", mainTableAlias);
		if (commaSuffix)
		{
			sqlCommand.Append(',');
		}
	}

	public static SqlParameter GetDefaultParameter(NCLMetaField field, int nextParameterNo)
	{
		SqlParameter sqlParameter = new SqlParameter();
		sqlParameter.ParameterName = string.Format(CultureInfo.InvariantCulture, "@{0}", nextParameterNo);
		NavSqlStatementHelper.SetSqlParameter(sqlParameter, field, field.EmptyValue.GetSqlWritableValue(field.SqlDataType, field.FieldDefinedLength), isWildcardValue: false, isFilterValue: false);
		return sqlParameter;
	}

	private void AlterTableAddFields(NCLMetaTable table, ICollection<NCLMetaField> fields, int companyToken, NavAppTableContext tableContext)
	{
		if (!fields.Any())
		{
			return;
		}
		StartNewCommand();
		CommandText.Append("ALTER TABLE ");
		NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(CommandText, table, Database, companyToken, tableContext);
		CommandText.Append(" ADD ");
		NCLMetaField nCLMetaField = null;
		foreach (NCLMetaField field in fields)
		{
			bool flag = false;
			if (field.FieldIsAutoIncrement && Database.HasSequencedAutoIncrementField(table, tableContext))
			{
				nCLMetaField = field;
				flag = true;
			}
			AppendNewFieldClause(field, (field.FieldNclType == NavNclType.NavBlob) | flag, tableContext);
			if (field.FieldNclType != NavNclType.NavBlob && !field.FieldIsAutoIncrement && !field.FieldIsTimestamp && !field.FieldIsUserDefinedTimestamp)
			{
				CommandText.Append(" CONSTRAINT ");
				CommandText.Append("\"");
				NavSqlStatementHelper.AppendSqlFieldDefaultConstraintName(CommandText, table, field, Database, companyToken, tableContext);
				CommandText.Append("\"");
				CommandText.Append(" DEFAULT ");
				NavSqlStatementHelper.AppendSqlDefaultValueAsLiteral(CommandText, field);
			}
			CommandText.Append(',');
		}
		CommandText.Length--;
		ExecuteNonQuery();
		if (nCLMetaField != null)
		{
			RestartTableSequenceForAllTenants(table, nCLMetaField);
			ExecuteNonQuery();
		}
		if (nCLMetaField != null)
		{
			InitAutoIncrementValues(table, nCLMetaField);
			CommandText.Append("ALTER TABLE ");
			NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(CommandText, table, Database, companyToken, tableContext);
			CommandText.Append(" ALTER COLUMN ");
			AppendNewFieldClause(nCLMetaField, allowNull: false, tableContext);
			ExecuteNonQuery();
		}
		List<NCLMetaField> list = fields.Where((NCLMetaField field) => field.FieldNclType != NavNclType.NavBlob && !field.FieldIsAutoIncrement && !field.SourceAppId.HasValue).ToList();
		if (!Database.TenantDatabaseProperties.PerCompanySchema || !list.Any())
		{
			return;
		}
		CommandText.Append("ALTER TABLE ");
		NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(CommandText, table, Database, companyToken, tableContext);
		CommandText.Append(" DROP CONSTRAINT ");
		foreach (NCLMetaField item in list)
		{
			CommandText.Append("\"");
			NavSqlStatementHelper.AppendSqlFieldDefaultConstraintName(CommandText, table, item, Database, companyToken, tableContext);
			CommandText.Append("\"");
			CommandText.Append(',');
		}
		CommandText.Length--;
		ExecuteNonQuery();
	}

	private List<string> FindAllTableDefaultConstraints(NCLMetaTable table, int oldCompanyToken, NavAppTableContext tableContext, bool isTempTable = false)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001FP", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Finding default constraints of table:{0} - isTempTable:{1}", table.TableName, isTempTable);
		List<string> list = new List<string>();
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(Database, updateOrlockingActivity: false))
		{
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			StringBuilder stringBuilder = new StringBuilder();
			NavSqlStatementHelper.ComposeSelectAllDefaultConstraintsOfTableStatement(stringBuilder, table, Database, oldCompanyToken, tableContext, isTempTable ? "TEMP$" : string.Empty);
			using (navSqlCommand.ExecuteReader(stringBuilder.ToString()))
			{
				while (navSqlCommand.ReadNextRow())
				{
					list.Add(navSqlCommand.Reader.GetString(0));
				}
				navSqlCommand.CloseReader(forceClose: false);
			}
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001FQ", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Found default constraints for table:{0} - isTempTable:{1} = {2}", table.TableName, isTempTable, string.Join(", ", list.ToArray()));
		return list;
	}

	private void AlterTableAddFieldDefaultConstraints(NCLMetaTable table, IEnumerable<NCLMetaField> fields, int companyToken, NavAppTableContext tableContext)
	{
		List<NCLMetaField> list = fields.Where((NCLMetaField f) => !f.FieldIsTimestamp && !f.FieldIsUserDefinedTimestamp && !f.FieldIsPartOfPrimaryKey && f.FieldNclType != NavNclType.NavBlob && !f.FieldIsAutoIncrement && f.FieldClass == FieldClass.Normal).ToList();
		if (!list.Any())
		{
			return;
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001FR", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Adding default constraints  for table:{0} - fields : {1}", table.TableName, string.Join(", ", list.Select((NCLMetaField field) => field.SqlColumnName).ToArray()));
		StartNewCommand();
		CommandText.Append("ALTER TABLE ");
		NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(CommandText, table, Database, companyToken, tableContext);
		CommandText.Append(" ADD ");
		foreach (NCLMetaField item in list)
		{
			CommandText.Append(" CONSTRAINT ");
			CommandText.Append("\"");
			NavSqlStatementHelper.AppendSqlFieldDefaultConstraintName(CommandText, table, item, Database, companyToken, tableContext);
			CommandText.Append("\"");
			CommandText.Append(" DEFAULT ");
			NavSqlStatementHelper.AppendSqlDefaultValueAsLiteral(CommandText, item);
			CommandText.Append(" FOR ");
			NavSqlStatementHelper.AppendQuotedIdentifier(CommandText, item.SqlColumnName);
			CommandText.Append(',');
		}
		CommandText.Length--;
		ExecuteNonQuery();
	}

	private void AlterTableDropConstraintIfExists(NCLMetaTable table, IEnumerable<NCLMetaField> fields, int companyToken, NavAppTableContext tableContext)
	{
		if (!fields.Any() || !fields.Any((NCLMetaField field) => field.FieldNclType != NavNclType.NavBlob && !field.FieldIsAutoIncrement))
		{
			return;
		}
		foreach (NCLMetaField item in fields.Where((NCLMetaField field) => field.FieldNclType != NavNclType.NavBlob && !field.FieldIsAutoIncrement))
		{
			StartNewCommand();
			StringBuilder stringBuilder = new StringBuilder();
			NavSqlStatementHelper.AppendSqlFieldDefaultConstraintName(stringBuilder, table, item, Database, companyToken, tableContext);
			CommandText.Append(string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT TOP 1 name FROM sys.default_constraints WHERE name = N'{0}') ", stringBuilder));
			CommandText.Append("ALTER TABLE ");
			NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(CommandText, table, Database, companyToken, tableContext);
			CommandText.Append(" DROP CONSTRAINT ");
			CommandText.Append("\"");
			CommandText.Append(stringBuilder);
			CommandText.Append("\"");
		}
		ExecuteNonQuery();
	}

	private void AlterTableChangeCompressionType(NCLMetaTable table, int companyToken, NavAppTableContext tableContext, CompressionType compressionType)
	{
		StartNewCommand();
		CommandText.Append("ALTER TABLE ");
		NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(CommandText, table, Database, companyToken, tableContext);
		CommandText.Append(string.Format(CultureInfo.InvariantCulture, " REBUILD PARTITION = all WITH (data_compression = {0})", compressionType));
		ExecuteNonQuery();
	}

	private void AlterNonSiftIndexChangeCompressionType(int companyToken, NavAppTableContext tableContext, NCLMetaKey key, CompressionType compressionType)
	{
		StartNewCommand();
		CommandText.Append("IF (SELECT INDEXPROPERTY(OBJECT_ID(N'");
		AppendTableOrTempName(key.Parent, companyToken, tempName: false, tableContext);
		CommandText.Append("'), N'");
		NavSqlStatementHelper.AppendIndexName(CommandText, key, Database, companyToken, tableContext);
		CommandText.Append("', 'IndexId')) IS NOT NULL ALTER INDEX ");
		NavSqlStatementHelper.AppendQuotedIndexName(CommandText, key, Database, companyToken, tableContext);
		CommandText.Append(" ON ");
		NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(CommandText, key.Parent, Database, companyToken, tableContext);
		CommandText.AppendFormat(CultureInfo.InvariantCulture, " REBUILD PARTITION = all WITH (data_compression = {0})", compressionType);
		ExecuteNonQuery();
	}

	private void AlterSiftIndexChangeCompressionType(int companyToken, NavAppTableContext tableContext, NCLMetaKey key, CompressionType compressionType)
	{
		StartNewCommand();
		string siftViewName = NavSqlDdlCommandBuilder.GetSiftViewName(Database, key, companyToken, tableContext);
		CommandText.Append("IF (SELECT INDEXPROPERTY(OBJECT_ID(N'");
		CommandText.Append(siftViewName);
		CommandText.Append("'), 'VSIFTIDX', 'IndexId')) IS NOT NULL ALTER INDEX VSIFTIDX ON ");
		CommandText.Append(siftViewName);
		CommandText.AppendFormat(CultureInfo.InvariantCulture, " REBUILD PARTITION = all WITH (data_compression = {0})", compressionType);
		ExecuteNonQuery();
	}

	private void AlterTableDropFields(NCLMetaTable oldTable, NCLMetaTable newTable, ICollection<NCLMetaField> fields, int companyToken, NavAppTableContext tableContext)
	{
		if (!fields.Any())
		{
			return;
		}
		StartNewCommand();
		CommandText.Append("ALTER TABLE ");
		NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(CommandText, newTable, Database, companyToken, tableContext);
		CommandText.Append(" DROP COLUMN ");
		NCLMetaField nCLMetaField = null;
		foreach (NCLMetaField field in fields)
		{
			NavSqlStatementHelper.AppendQuotedIdentifier(CommandText, field.SqlColumnName);
			CommandText.Append(',');
			if (field.FieldIsAutoIncrement)
			{
				nCLMetaField = field;
			}
		}
		CommandText.Length--;
		if (nCLMetaField != null && Database.HasSequencedAutoIncrementField(oldTable, tableContext))
		{
			StartNewCommand();
			DropTableSequenceForAllTenants(oldTable, nCLMetaField);
		}
		ExecuteNonQuery();
	}

	private void AlterTableChangeFieldsLength(NCLMetaTable table, ICollection<NavSqlFieldAndChanges> fields, int companyToken, NavAppTableContext tableContext, bool ignoreWarning)
	{
		if (!fields.Any())
		{
			return;
		}
		StartNewCommand();
		if (ignoreWarning)
		{
			CommandText.Append("SET ANSI_WARNINGS OFF;");
		}
		foreach (NavSqlFieldAndChanges field in fields)
		{
			NCLMetaField newField = field.NewField;
			if (!newField.IsStringType)
			{
				throw new InvalidOperationException("Only string type can have their length modified");
			}
			if (newField.SqlDataType == SqlDataType.Variant)
			{
				throw new InvalidOperationException("Variant fields cannot have their length modified.");
			}
			CommandText.Append("ALTER TABLE ");
			NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(CommandText, table, Database, companyToken, tableContext);
			CommandText.Append(" ALTER COLUMN ");
			AppendNewFieldClause(newField, allowNull: false, tableContext);
			CommandText.AppendLine(";");
		}
		if (ignoreWarning)
		{
			CommandText.Append("SET ANSI_WARNINGS ON;");
		}
		ExecuteNonQuery();
	}

	private void RenameConstraintsIfExist(NCLMetaTable newTable, NCLMetaTable oldTable, IEnumerable<NavSqlFieldAndChanges> fieldsAndChanges, int newCompanyToken, int oldCompanyToken, NavAppTableContext tableContext)
	{
		if (fieldsAndChanges == null || !fieldsAndChanges.Any())
		{
			return;
		}
		IEnumerable<NavSqlFieldAndChanges> enumerable = fieldsAndChanges.Where((NavSqlFieldAndChanges field) => (field == null || field.OldField?.FieldNclType != NavNclType.NavBlob) && field != null && field.OldField?.FieldIsAutoIncrement == false);
		foreach (NavSqlFieldAndChanges item in enumerable)
		{
			StringBuilder stringBuilder = new StringBuilder(string.Empty);
			StringBuilder stringBuilder2 = new StringBuilder(string.Empty);
			NavSqlStatementHelper.AppendSqlFieldDefaultConstraintName(stringBuilder, oldTable, item.OldField, Database, oldCompanyToken, tableContext);
			NavSqlStatementHelper.AppendSqlFieldDefaultConstraintName(stringBuilder2, newTable, item.Excessive ? item.OldField : item.NewField, Database, newCompanyToken, tableContext);
			StartNewCommand();
			AppendRenameConstraintIfExists(CommandText, stringBuilder.ToString(), stringBuilder2.ToString());
		}
		ExecuteNonQuery();
	}

	internal static StringBuilder AppendRenameConstraintIfExists(StringBuilder commandText, string oldConstraintName, string newConstraintName)
	{
		commandText.Append(string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT TOP 1 name FROM sys.default_constraints WHERE name = N'{0}') ", oldConstraintName));
		commandText.Append("exec sp_rename ");
		NavSqlStatementHelper.AppendQuotedIdentifier(commandText, oldConstraintName);
		commandText.Append(',');
		NavSqlStatementHelper.AppendQuotedIdentifier(commandText, newConstraintName);
		commandText.Append(',');
		commandText.Append("'OBJECT'");
		return commandText;
	}

	private void RenameFields(NCLMetaTable table, IEnumerable<NavSqlFieldAndChanges> fieldsAndChanges, int companyToken, NavAppTableContext tableContext)
	{
		for (int i = 0; i <= 1; i++)
		{
			foreach (NavSqlFieldAndChanges fieldsAndChange in fieldsAndChanges)
			{
				StartNewCommand();
				CommandText.AppendFormat(CultureInfo.InvariantCulture, "exec sp_rename N'\"{0}\".\"{1}\".\"{2}{3}\"', N'{4}{5}', N'COLUMN'", "dbo", NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), table, Database, companyToken, tableContext), (i == 0) ? string.Empty : "TEMP$", (i == 0) ? fieldsAndChange.OldField.SqlColumnName : fieldsAndChange.NewField.SqlColumnName, (i == 0) ? "TEMP$" : string.Empty, fieldsAndChange.NewField.SqlColumnName);
			}
			ExecuteNonQuery();
		}
	}

	private void DropStatisticsOnFields(NCLMetaTable table, ICollection<NCLMetaField> fields, int companyToken, NavAppTableContext tableContext)
	{
		if (!fields.Any())
		{
			return;
		}
		StartNewCommand();
		CommandText.Append("DECLARE @StatisticsToDrop NVARCHAR(MAX);\r\n                                      SELECT @StatisticsToDrop = COALESCE(@StatisticsToDrop +', ', '') + N'[");
		NavSqlStatementHelper.AppendTableNameCompanyQualified(CommandText, table, Database, companyToken, tableContext);
		CommandText.Append("].['+ s.name + ']' FROM \r\n                                      sys.stats AS s\r\n                                      INNER JOIN sys.stats_columns AS sc ON (s.stats_id = sc.stats_id AND sc.object_id = s.object_id)\r\n                                      INNER JOIN sys.all_columns AS c ON (c.column_id = sc.column_id AND c.object_id = s.object_id)\r\n                                      LEFT OUTER JOIN sys.indexes AS i ON (s.name = i.name AND i.object_id = s.object_id)\r\n                                      WHERE s.object_id = OBJECT_ID(N'");
		NavSqlStatementHelper.AppendTableNameCompanyQualified(CommandText, table, Database, companyToken, tableContext);
		CommandText.AppendLine("')");
		CommandText.AppendLine("AND NOT (i.object_id IS NOT NULL AND i.type = 2 AND s.auto_created = 0 AND s.user_created = 0)");
		CommandText.Append("AND (");
		foreach (NCLMetaField item in fields.DistinctBy((NCLMetaField f) => f.FieldNo))
		{
			CommandText.Append("c.name = N'").Append(item.SqlColumnName).AppendLine("' OR ");
		}
		CommandText.Length -= 5;
		CommandText.AppendLine(")");
		CommandText.AppendLine("IF @@ROWCOUNT > 0 BEGIN\r\n                                            SET @StatisticsToDrop = CONCAT('drop statistics ', @StatisticsToDrop)\r\n                                            EXECUTE sp_executesql @StatisticsToDrop\r\n                                          END");
		ExecuteNonQuery();
	}

	public void CreateNonSiftIndex(NCLMetaKey key, int companyToken, NavAppTableContext tableContext, TablePartitionMetadata tablePartitionMetadata)
	{
		DropNonSiftIndexIfExists(key, companyToken, tableContext);
		StartNewCommand();
		IEnumerable<NCLMetaField> enumerable;
		if (!key.Unique)
		{
			IEnumerable<NCLMetaField> sqlIndexFields = key.SqlIndexFields;
			enumerable = sqlIndexFields ?? key.KeyFieldsWithPrimaryKeyFields.AsNclMetaFields();
		}
		else
		{
			IEnumerable<NCLMetaField> sqlIndexFields = key.KeyFieldsList;
			enumerable = sqlIndexFields;
		}
		IEnumerable<NCLMetaField> source = enumerable;
		if (key.Unique)
		{
			CommandText.Append("ALTER TABLE ");
			NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(CommandText, key.Parent, Database, companyToken, tableContext);
			CommandText.Append(" ADD CONSTRAINT ");
			NavSqlStatementHelper.AppendQuotedIndexName(CommandText, key, Database, companyToken, tableContext);
			CommandText.Append(" UNIQUE ");
			CommandText.Append(key.Clustered ? "CLUSTERED" : "NONCLUSTERED");
		}
		else
		{
			CommandText.Append("CREATE ");
			if (key.SqlIndexFields == null)
			{
				CommandText.Append("UNIQUE ");
			}
			CommandText.Append(key.Clustered ? "CLUSTERED " : "NONCLUSTERED ");
			CommandText.Append("INDEX ");
			NavSqlStatementHelper.AppendQuotedIndexName(CommandText, key, Database, companyToken, tableContext);
			CommandText.Append(" ON ");
			NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(CommandText, key.Parent, Database, companyToken, tableContext);
		}
		CommandText.Append(" (");
		if (Database.HasCompanyIdColumn(key.Parent))
		{
			AppendCompanyIdFieldName(CommandText, commaPrefix: false, commaSuffix: true);
		}
		foreach (NCLMetaField item in source.Take(NCLMetaTable.MaxSqlIndexColumns))
		{
			if (item.FieldBelongsInPhysicalTable(tableContext))
			{
				NavSqlStatementHelper.AppendQuotedIdentifier(CommandText, item.SqlColumnName);
				CommandText.Append(',');
			}
		}
		CommandText.Length--;
		CommandText.Append(')');
		bool flag = Database.HasCapability(SupportedCapability.CreateOnlineIndex) && (!key.Clustered || (key.Clustered && key.Parent.BlobFieldCount == 0));
		bool flag2 = key.Parent.CompressionType == CompressionType.Page || key.Parent.CompressionType == CompressionType.Row;
		if (flag | flag2)
		{
			CommandText.Append(" WITH (");
			if (flag)
			{
				CommandText.Append("ONLINE=ON");
			}
			if (flag & flag2)
			{
				CommandText.Append(",");
			}
			if (flag2)
			{
				CommandText.Append("DATA_COMPRESSION=");
				CommandText.Append(key.Parent.CompressionType.ToString());
			}
			CommandText.Append(")");
		}
		AppendTablePartitionSchemeIfNeeded(tablePartitionMetadata);
		int num = source.Count();
		if (num > NCLMetaTable.MaxSqlIndexColumns)
		{
			StringBuilder stringBuilder = new StringBuilder();
			NavSqlStatementHelper.AppendQuotedIndexName(stringBuilder, key, Database, companyToken, tableContext);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000O4", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Index - name:{0} - field limit reached - number of fields {1} exeeds the limit of {2}", stringBuilder.ToString(), num, NCLMetaTable.MaxSqlIndexColumns);
		}
	}

	public void CreateNonSiftIndexes(NCLMetaTable table, int companyToken, NavAppTableContext tableContext, TablePartitionMetadata tablePartitionMetadata)
	{
		IEnumerable<NCLMetaKey> source = table.Keys.Where((NCLMetaKey key) => key.MaintainSqlIndex && !key.IsPrimaryKey && !key.Clustered);
		foreach (NCLMetaKey item in source.Where((NCLMetaKey key) => key.OriginatingTableContext.Equals(tableContext)))
		{
			CreateNonSiftIndex(item, companyToken, tableContext, tablePartitionMetadata);
		}
		ExecuteNonQuery();
	}

	public void DropNonSiftIndexIfExists(NCLMetaKey key, int companyToken, NavAppTableContext tableContext, bool isTemp = false)
	{
		StartNewCommand();
		if (key.IsPrimaryKey || key.Unique)
		{
			CommandText.Append("IF (OBJECT_ID(N'");
			NavSqlStatementHelper.AppendQuotedIndexName(CommandText, key, Database, companyToken, NavAppTableContext.Main);
			CommandText.Append("', '");
			CommandText.Append(key.IsPrimaryKey ? "PK" : "UQ");
			CommandText.Append("')) IS NOT NULL ");
			CommandText.Append("ALTER TABLE ");
			AppendTableOrTempName(key.Parent, companyToken, isTemp, tableContext);
			CommandText.Append(" DROP CONSTRAINT ");
			NavSqlStatementHelper.AppendQuotedIndexName(CommandText, key, Database, companyToken, NavAppTableContext.Main);
		}
		else
		{
			CommandText.Append("IF (SELECT INDEXPROPERTY(OBJECT_ID(N'");
			AppendTableOrTempName(key.Parent, companyToken, isTemp, tableContext);
			CommandText.Append("'), N'");
			NavSqlStatementHelper.AppendIndexName(CommandText, key, Database, companyToken, tableContext);
			CommandText.Append("', 'IndexId')) IS NOT NULL ");
			CommandText.Append("DROP INDEX ");
			NavSqlStatementHelper.AppendQuotedIndexName(CommandText, key, Database, companyToken, tableContext);
			CommandText.Append(" ON ");
			AppendTableOrTempName(key.Parent, companyToken, isTemp, tableContext);
		}
	}

	private void CreateSiftIndex(NCLMetaKey key, int companyToken, NavAppTableContext tableContext)
	{
		string value = NavSqlDdlCommandBuilder.BuildCreateSiftIndexCommandText(Database, key, companyToken, tableContext);
		string value2 = NavSqlDdlCommandBuilder.BuildCreateSiftClusteredIndexCommandText(Database, key, companyToken, tableContext);
		StartNewExclusiveCommand();
		CommandText.Append(value);
		ExecuteNonQuery();
		StartNewCommand();
		CommandText.Append(value2);
		StringBuilder stringBuilder = new StringBuilder();
		NavSqlStatementHelper.AppendQuotedSiftIndexNameWithSchemaQualification(stringBuilder, key, Database, companyToken, tableContext);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000N4", Category.Runtime, Verbosity.Normal, "Creating SIFT index -- Name:{0}", DiagnosticParameter.Oii(stringBuilder));
		int num = key.DefinedKeyFields.AsNclMetaFields().Count();
		if (num > NCLMetaTable.MaxSqlIndexColumns)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000O5", Category.Runtime, Verbosity.Normal, "SIFT index - name:{0} - field limit reached - number of fields {1} exeeds the limit of {2}", DiagnosticParameter.Oii(stringBuilder), DiagnosticParameter.SystemMetadata(num), DiagnosticParameter.SystemMetadata(NCLMetaTable.MaxSqlIndexColumns));
		}
	}

	private void CreateSiftIndexes(NCLMetaTable table, int companyToken, NavAppTableContext tableContext)
	{
		IEnumerable<NCLMetaKey> source = table.Keys.Where((NCLMetaKey key) => key.MaintainSumIndex);
		foreach (NCLMetaKey item in source.Where((NCLMetaKey key) => key.OriginatingTableContext.Equals(tableContext)))
		{
			CreateSiftIndex(item, companyToken, tableContext);
		}
	}

	private void DropSiftIndexIfExists(NCLMetaKey key, int companyToken, NavAppTableContext tableContext)
	{
		StartNewCommand();
		CommandText.Append("IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'");
		NavSqlStatementHelper.AppendQuotedSiftIndexNameWithSchemaQualification(CommandText, key, Database, companyToken, tableContext);
		CommandText.Append("'), N'IsView'))=1 DROP VIEW ");
		NavSqlStatementHelper.AppendQuotedSiftIndexNameWithSchemaQualification(CommandText, key, Database, companyToken, tableContext);
		StringBuilder stringBuilder = new StringBuilder();
		NavSqlStatementHelper.AppendQuotedSiftIndexNameWithSchemaQualification(stringBuilder, key, Database, companyToken, tableContext);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000N5", Category.Runtime, Verbosity.Normal, "Dropping SIFT index -- Name:{0}", DiagnosticParameter.Oii(stringBuilder));
		ExecuteNonQuery();
	}

	private void DropSiftIndexes(NCLMetaTable table, int companyToken, NavAppTableContext tableContext)
	{
		IEnumerable<NCLMetaKey> enumerable = table.Keys.Where((NCLMetaKey k) => k.MaintainSumIndex && ((tableContext.TableType != NavAppTableContextType.Main) ? k.OriginatesFrom(tableContext) : (!k.IsCompanionTableKey)));
		foreach (NCLMetaKey item in enumerable)
		{
			DropSiftIndexIfExists(item, companyToken, tableContext);
		}
	}

	private void SafeRenamePK(StringBuilder oldObjectName, StringBuilder newObjectName)
	{
		StartNewCommand();
		CommandText.AppendFormat(CultureInfo.InvariantCulture, "IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'{0}.\"{1}\"'), N'IsPrimaryKey'))=1 BEGIN EXEC sp_rename N'{0}.\"{1}\"', N'{2}' END", "dbo", oldObjectName, newObjectName);
	}

	private void RenameObject(StringBuilder oldObjectName, StringBuilder newObjectName)
	{
		StartNewCommand();
		CommandText.AppendFormat(CultureInfo.InvariantCulture, "exec sp_rename N'\"{0}\".\"{1}\"', N'{2}'", "dbo", oldObjectName, newObjectName);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000N6", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "SQL Object Rename -- Old:{0}; New:{1}", oldObjectName, newObjectName);
	}

	private void CreateTableAndCreateDefaultConstraintsIfAny(NavSqlTableAndChange tableAndChange, NCLMetaTable table, int companyToken, NavAppTableContext tableContext, TablePartitionMetadata tablePartitionMetadata)
	{
		CreateTable(table, companyToken, tableContext, tablePartitionMetadata);
		if ((!tableAndChange.Missing && tableAndChange.RebuildTable) || tableAndChange.TableRenamed)
		{
			IEnumerable<NCLMetaField> fields = from f in table.NormalFieldsForContext(tableContext)
				where !f.FieldIsUserDefinedTimestamp
				select f;
			RecreateDefaultConstraintsFromTempTableIfNeeded(table, fields, companyToken, tableContext);
		}
	}

	private void CreateTable(NCLMetaTable table, int companyToken, NavAppTableContext tableContext, TablePartitionMetadata tablePartitionMetadata)
	{
		IEnumerable<NCLMetaField> enumerable = from f in table.NormalFieldsForContext(tableContext)
			where !f.FieldIsUserDefinedTimestamp
			select f;
		StartNewCommand();
		CommandText.Append("CREATE TABLE ");
		NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(CommandText, table, Database, companyToken, tableContext);
		CommandText.Append(" (");
		if (Database.HasCompanyIdColumn(table))
		{
			AppendCompanyIdField();
		}
		foreach (NCLMetaField item in enumerable)
		{
			AppendNewFieldClause(item, item.FieldNclType == NavNclType.NavBlob, tableContext);
			CommandText.Append(',');
		}
		CommandText.Append("CONSTRAINT ");
		NavSqlStatementHelper.AppendQuotedIndexName(CommandText, table.PrimaryKey, Database, companyToken, tableContext);
		CommandText.Append(" PRIMARY KEY ");
		CommandText.Append(table.PrimaryKey.Clustered ? "CLUSTERED " : "NONCLUSTERED ");
		CommandText.Append('(');
		if (Database.HasCompanyIdColumn(table))
		{
			AppendCompanyIdFieldName(CommandText, commaPrefix: false, commaSuffix: true);
		}
		ReadOnlyCollection<NCLMetaField> source = table.PrimaryKey.SqlIndexFields ?? table.PrimaryKey.KeyFieldsList;
		NavSqlStatementHelper.AppendFieldList(CommandText, source.Take(NCLMetaTable.MaxSqlIndexColumns), null);
		CommandText.Append(")");
		if (table.CompressionType == CompressionType.Page || table.CompressionType == CompressionType.Row)
		{
			CommandText.Append(" WITH (DATA_COMPRESSION = " + table.CompressionType.ToString() + ")");
		}
		CommandText.Append(")");
		AppendTablePartitionSchemeIfNeeded(tablePartitionMetadata);
		if (!table.PrimaryKey.Clustered)
		{
			CreateNonSiftIndex(table.ClusterIndex, companyToken, tableContext, tablePartitionMetadata);
		}
		int num = source.Count();
		if (num > NCLMetaTable.MaxSqlIndexColumns)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000O6", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Primary key - table name:{0} - field limit reached - number of fields {1} exeeds the limit of {2}", table.TableName, num, NCLMetaTable.MaxSqlIndexColumns);
		}
		if (Database.HasSequencedAutoIncrementField(table, tableContext))
		{
			CreateSequenceForAllTenants(table);
			StringBuilder input = new StringBuilder();
			NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(input, table, Database, companyToken, tableContext);
		}
		StringBuilder stringBuilder = new StringBuilder();
		NavSqlStatementHelper.AppendTableNameCompanyQualified(stringBuilder, table, Database, companyToken, tableContext);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000013K", Category.Runtime, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Creating table -- TableId:{0}; Name:{1}", table.TableId, stringBuilder);
	}

	private void RecreateDefaultConstraintsFromTempTableIfNeeded(NCLMetaTable table, IEnumerable<NCLMetaField> fields, int companyToken, NavAppTableContext tableContext)
	{
		List<string> tableConstraints = FindAllTableDefaultConstraints(table, companyToken, tableContext, isTempTable: true);
		if (tableConstraints.Any())
		{
			StringBuilder tempTableCompanyName = new StringBuilder("TEMP$");
			NavSqlStatementHelper.AppendTableNameCompanyQualified(tempTableCompanyName, table, Database, companyToken, tableContext);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001FS", Category.Runtime, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Trying to re-create default constraints of table {0} using default constraints of temporary table:{1}", table.TableName, tempTableCompanyName);
			IEnumerable<NCLMetaField> fields2 = fields.Where((NCLMetaField field) => tableConstraints.Contains(tempTableCompanyName.ToString() + field.SqlColumnName));
			AlterTableAddFieldDefaultConstraints(table, fields2, companyToken, tableContext);
		}
	}

	private void AppendTablePartitionSchemeIfNeeded(TablePartitionMetadata tablePartitionMetadata)
	{
		if (tablePartitionMetadata != null && Database.HasCapability(SupportedCapability.TablePartition))
		{
			string partitionSchemaName = tablePartitionMetadata.PartitionSchemaName;
			if (!string.IsNullOrWhiteSpace(partitionSchemaName))
			{
				CommandText.Append(" ON ");
				NavSqlStatementHelper.AppendQuotedIdentifier(CommandText, partitionSchemaName);
				CommandText.Append(" ([" + tablePartitionMetadata.PartitionColumnName + "])");
			}
		}
	}

	private void DropTableIfExists(NCLMetaTable table, int companyToken, bool isTemp, NavAppTableContext tableContext)
	{
		StartNewCommand();
		CommandText.Append("IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'");
		AppendTableOrTempName(table, companyToken, isTemp, tableContext);
		CommandText.Append("'), N'IsTable'))=1 DROP TABLE");
		AppendTableOrTempName(table, companyToken, isTemp, tableContext);
		StringBuilder stringBuilder = new StringBuilder();
		NavSqlStatementHelper.AppendTableNameCompanyQualified(stringBuilder, table, Database, companyToken, tableContext);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000013L", Category.Runtime, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Dropping table -- TableId:{0}; Name:{1}", table.TableId, stringBuilder);
		if (Database.HasSequencedAutoIncrementField(table, tableContext) && !isTemp)
		{
			DropTableSequenceForAllTenants(table, table.AutoIncrementField);
		}
	}

	private void RenameTableToTemp(NavSqlTableAndChange tableAndChange, int companyToken)
	{
		NCLMetaTable oldTable = tableAndChange.OldTable;
		NavAppTableContext tableContext = tableAndChange.TableContext;
		RenameObject(NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), oldTable, Database, companyToken, tableContext), NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder("TEMP$"), oldTable, Database, companyToken, tableContext));
		SafeRenamePK(NavSqlStatementHelper.AppendIndexName(new StringBuilder(), oldTable.PrimaryKey, Database, companyToken, tableContext), NavSqlStatementHelper.AppendIndexName(new StringBuilder("TEMP$"), oldTable.PrimaryKey, Database, companyToken, tableContext));
		SafeRenamePK(NavSqlStatementHelper.AppendIndexName(new StringBuilder(), oldTable.PrimaryKey, Database, companyToken, tableContext, useKeyName: false), NavSqlStatementHelper.AppendIndexName(new StringBuilder("TEMP$"), oldTable.PrimaryKey, Database, companyToken, tableContext, useKeyName: false));
		ExecuteNonQuery();
	}

	private void RenameTable(NCLMetaTable oldTable, int oldCompanyToken, NCLMetaTable newTable, int newCompanyToken, NavAppTableContext tableContext)
	{
		DropSiftIndexes(oldTable, oldCompanyToken, tableContext);
		RenameObject(NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), oldTable, Database, oldCompanyToken, tableContext), NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), newTable, Database, newCompanyToken, tableContext));
		RenameObject(NavSqlStatementHelper.AppendIndexName(new StringBuilder(), oldTable.PrimaryKey, Database, oldCompanyToken, tableContext), NavSqlStatementHelper.AppendIndexName(new StringBuilder(), newTable.PrimaryKey, Database, newCompanyToken, tableContext));
		IEnumerable<NavSqlFieldAndChanges> fieldsAndChanges = from oldField in oldTable.Fields
			where oldField.OriginatingTableContext.Equals(tableContext)
			select new NavSqlFieldAndChanges(oldField, FieldChangeTypes.None, newTable.Fields.FirstOrDefault((NCLMetaField newField) => newField.SourceAppId == oldField.SourceAppId && newField.FieldNo == oldField.FieldNo));
		RenameConstraintsIfExist(newTable, oldTable, fieldsAndChanges, newCompanyToken, oldCompanyToken, tableContext);
		ExecuteNonQuery();
		RenameIntelligentCloudStatus(Database, oldTable, oldCompanyToken, newTable, newCompanyToken, tableContext);
	}

	private static void AppendTableOrTempName(StringBuilder commandText, NavDatabase database, NCLMetaTable table, int companyToken, bool tempName, NavAppTableContext tableContext)
	{
		NavSqlStatementHelper.AppendQuotedIdentifier(commandText, "dbo");
		commandText.Append('.');
		commandText.Append("\"");
		if (tempName)
		{
			commandText.Append("TEMP$");
		}
		NavSqlStatementHelper.AppendTableNameCompanyQualified(commandText, table, database, companyToken, tableContext);
		commandText.Append("\"");
	}

	private void AppendTableOrTempName(NCLMetaTable table, int companyToken, bool tempName, NavAppTableContext tableContext)
	{
		AppendTableOrTempName(CommandText, Database, table, companyToken, tempName, tableContext);
	}

	internal static string FieldWithConversion(NavSqlFieldAndChanges fieldAndChanges, SqlSortingProperties sortingProperties, Func<NCLMetaField, string> defaultValueParameter)
	{
		if (fieldAndChanges.Missing || fieldAndChanges.DataTypeChanged)
		{
			return defaultValueParameter(fieldAndChanges.NewField);
		}
		StringBuilder stringBuilder;
		if (fieldAndChanges.SqlDataTypeChanged)
		{
			string text = defaultValueParameter(fieldAndChanges.NewField);
			if (fieldAndChanges.NewField.SqlDataTypeWithDefaultMapping == SqlDataType.Variant && fieldAndChanges.OldField.SqlDataTypeWithDefaultMapping == SqlDataType.VarChar)
			{
				stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "CASE ISNUMERIC({0}) WHEN 1 THEN {1} ELSE {0} END", NavSqlStatementHelper.AppendQuotedIdentifier(new StringBuilder(), fieldAndChanges.OldField.SqlColumnName), text);
			}
			else
			{
				stringBuilder = new StringBuilder(text);
			}
		}
		else
		{
			stringBuilder = NavSqlStatementHelper.AppendQuotedIdentifier(new StringBuilder(), fieldAndChanges.OldField.SqlColumnName);
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		bool flag = fieldAndChanges.LengthChanged && fieldAndChanges.OldField.SqlDataTypeWithDefaultMapping != SqlDataType.Variant && fieldAndChanges.NewField.FieldDefinedLength < fieldAndChanges.OldField.FieldDefinedLength;
		if (flag)
		{
			stringBuilder2.Append("SUBSTRING(");
		}
		if (fieldAndChanges.SortingChanged)
		{
			if (fieldAndChanges.OldField.SqlDataTypeWithDefaultMapping == SqlDataType.Variant)
			{
				stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, "CASE SQL_VARIANT_PROPERTY({0}, 'BaseType') WHEN 'nvarchar' THEN CAST({0} AS NVARCHAR({1})) COLLATE {2} WHEN 'varchar' THEN CAST({0} AS NVARCHAR({1})) COLLATE {2} ELSE {0} END", stringBuilder, fieldAndChanges.NewField.FieldDefinedLength, sortingProperties.Collation);
			}
			else
			{
				stringBuilder2.Append(stringBuilder).Append(" COLLATE ").Append(sortingProperties.Collation);
			}
		}
		else
		{
			stringBuilder2.Append(stringBuilder);
		}
		if (flag)
		{
			stringBuilder2.Append(",1,").Append(fieldAndChanges.NewField.FieldDefinedLength).Append(')');
		}
		return stringBuilder2.ToString();
	}

	private static void ChangeCompressionForBLOBsIfNeeded(NavDatabase database, NavSqlTableAndChange tableAndChange, int newCompanyToken)
	{
		if (tableAndChange.FieldsAndChanges == null)
		{
			return;
		}
		NavSqlFieldAndChanges[] array = tableAndChange.FieldsAndChanges.Where((NavSqlFieldAndChanges fieldAndChange) => fieldAndChange.CompressionChanged).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		ReadOnlyCollection<NCLMetaField> keyFieldsList = tableAndChange.NewTable.PrimaryKey.KeyFieldsList;
		string value = NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(new StringBuilder(), tableAndChange.NewTable, database, newCompanyToken, tableAndChange.TableContext).ToString();
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		using NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		StringBuilder getBlobStatement = new StringBuilder("SELECT ");
		getBlobStatement.Join(",", array, (NavSqlFieldAndChanges changedBlobField) =>
		{
			NavSqlStatementHelper.AppendQuotedIdentifier(getBlobStatement, changedBlobField.NewField.SqlColumnName);
		});
		getBlobStatement.Append(',');
		getBlobStatement.Join(",", keyFieldsList, (NCLMetaField nCLMetaField) =>
		{
			NavSqlStatementHelper.AppendQuotedIdentifier(getBlobStatement, nCLMetaField.SqlColumnName);
		});
		getBlobStatement.Append(" FROM ");
		getBlobStatement.Append(value);
		StringBuilder stringBuilder = new StringBuilder("UPDATE ");
		stringBuilder.Append(value);
		stringBuilder.Append(" SET ");
		int num = 0;
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			NCLMetaField newField = array[num2].NewField;
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, newField.SqlColumnName);
			stringBuilder.Append('=');
			SqlParameter sqlParameter = new SqlParameter(NavSqlStatementHelper.ParameterName(num), SqlDbType.Binary, int.MaxValue);
			navSqlCommand2.Command.Parameters.Add(sqlParameter);
			stringBuilder.Append(sqlParameter.ParameterName);
			stringBuilder.Append(',');
			num++;
		}
		stringBuilder.Length--;
		stringBuilder.Append(" WHERE ");
		foreach (NCLMetaField item in keyFieldsList)
		{
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, item.SqlColumnName);
			stringBuilder.Append('=');
			SqlParameter sqlParameter2 = new SqlParameter(NavSqlStatementHelper.ParameterName(num), SqlDbType.Binary, int.MaxValue);
			navSqlCommand2.Command.Parameters.Add(sqlParameter2);
			stringBuilder.Append(sqlParameter2.ParameterName);
			stringBuilder.Append(" AND ");
			num++;
		}
		stringBuilder.Length -= 5;
		navSqlCommand2.SetCommandText(stringBuilder.ToString());
		navSqlCommand2.PrepareStatement();
		using (navSqlCommand.ExecuteReader(getBlobStatement.ToString()))
		{
			while (navSqlCommand.ReadNextRow())
			{
				for (int num3 = 0; num3 < array.Length; num3++)
				{
					NCLMetaField oldField = array[num3].OldField;
					NCLMetaField newField2 = array[num3].NewField;
					NavBLOB navBLOB = new NavBLOB();
					navSqlCommand.GetBlobDataFromReader(oldField.FieldIsCompressed, num3, navBLOB);
					NavSqlStatementHelper.SetSqlParameter(navSqlCommand2.Command.Parameters[num3], newField2, navBLOB.GetSqlWritableValue(SqlDataType.Default, 0, newField2.FieldIsCompressed), isWildcardValue: false, isFilterValue: false);
				}
				for (int num4 = 0; num4 < keyFieldsList.Count; num4++)
				{
					NCLMetaField field = keyFieldsList[num4];
					NavSqlStatementHelper.SetSqlParameter(navSqlCommand2.Command.Parameters[num4 + array.Length], field, navSqlCommand.Reader.GetValue(num4 + array.Length), isWildcardValue: false, isFilterValue: false);
				}
				navSqlCommand2.ExecuteNonQuery();
			}
		}
	}

	private void TransferData(NavDatabase database, NavSqlTableAndChange tableAndChange, int oldCompanyToken, int newCompanyToken, bool transferFromTemp, NavAppTableContext tableContext, bool transferEntireTable, bool excludeCompanyIdsContainingNonUniquePrimaryKeyData = false)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000015I", Category.Sql, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Transferring table data -- TableId:{0}; NewName:{1}; OldName:{2}; NewCompany:{3}; OldCompany:{4}", tableAndChange.NewTable.TableId, tableAndChange.NewTable.TableName, tableAndChange.OldTable.TableName, newCompanyToken, oldCompanyToken);
		StartNewCommand();
		bool flag = tableContext.TableType == NavAppTableContextType.Main && tableAndChange.NewTable.AutoIncrementField != null && !tableAndChange.FieldsAndChanges.Any((NavSqlFieldAndChanges fieldAndChange) => fieldAndChange.Missing && fieldAndChange.NewField.FieldIsAutoIncrement);
		if (!database.IsSharedTenantSchema & flag)
		{
			CommandText.Append("SET IDENTITY_INSERT ");
			CommandText.Append("\"");
			NavSqlStatementHelper.AppendTableNameCompanyQualified(CommandText, tableAndChange.NewTable, Database, newCompanyToken, tableContext);
			CommandText.Append("\"");
			CommandText.Append(" ON; ");
		}
		CommandText.Append("INSERT INTO ");
		AppendTableOrTempName(tableAndChange.NewTable, newCompanyToken, tempName: false, tableContext);
		CommandText.Append(" (");
		CompanyId? companyId = null;
		CompanyId? companyId2 = null;
		if (!transferEntireTable)
		{
			companyId = Database.GetCompanyId(tableAndChange.NewTable, newCompanyToken);
			companyId2 = Database.GetCompanyId(tableAndChange.OldTable, oldCompanyToken);
			if (companyId.HasValue && companyId2.HasValue)
			{
				AppendCompanyIdFieldName(CommandText, commaPrefix: false, commaSuffix: true);
			}
		}
		else
		{
			AppendCompanyIdFieldName(CommandText, commaPrefix: false, commaSuffix: true);
		}
		foreach (NavSqlFieldAndChanges item in tableAndChange.FieldsAndChanges.Where((NavSqlFieldAndChanges field) => !field.Excessive && !field.NewField.FieldIsTimestamp))
		{
			if ((tableContext.TableType != NavAppTableContextType.Main || database.HasSequencedAutoIncrementField(tableAndChange.NewTable, tableContext) || !item.NewField.FieldIsAutoIncrement || flag) && item.NewField.FieldBelongsInPhysicalTable(tableContext))
			{
				CommandText.Append("\"");
				CommandText.Append(item.NewField.SqlColumnName);
				CommandText.Append("\"");
				CommandText.Append(',');
			}
		}
		CommandText.Length--;
		CommandText.Append(") SELECT ");
		if (companyId.HasValue && companyId2.HasValue)
		{
			CommandText.Append(companyId.Value).Append(',');
		}
		else if (transferEntireTable)
		{
			AppendCompanyIdFieldName(CommandText, commaPrefix: false, commaSuffix: true);
		}
		foreach (NavSqlFieldAndChanges fieldsAndChange in tableAndChange.FieldsAndChanges)
		{
			if (fieldsAndChange.Excessive)
			{
				continue;
			}
			if (!transferEntireTable && fieldsAndChange.NewField.FieldIsAutoIncrement && !flag && tableContext.TableType == NavAppTableContextType.Main)
			{
				if (!database.HasSequencedAutoIncrementField(tableAndChange.NewTable, tableContext))
				{
					continue;
				}
				SequenceNextValueFor(tableAndChange.NewTable, fieldsAndChange.NewField, companyId);
			}
			else
			{
				if (!fieldsAndChange.NewField.FieldBelongsInPhysicalTable(tableContext))
				{
					continue;
				}
				CommandText.Append(FieldWithConversion(fieldsAndChange, NavGlobal.AppDatabase.SqlSortingProperties, (NCLMetaField field) =>
				{
					SqlParameter defaultParameter = GetDefaultParameter(field, ParameterCount);
					AddParameter(defaultParameter);
					return defaultParameter.ParameterName;
				}));
			}
			CommandText.Append(',');
		}
		CommandText.Length--;
		CommandText.Append(" FROM ");
		AppendTableOrTempName(tableAndChange.OldTable, oldCompanyToken, transferFromTemp, tableContext);
		if (!transferEntireTable && companyId.HasValue && companyId2.HasValue)
		{
			CommandText.Append(" WHERE ").Append("\"$companyId\"").Append(" =  /*companyIdLiteral*/")
				.Append(companyId2.Value);
		}
		else if ((excludeCompanyIdsContainingNonUniquePrimaryKeyData & transferEntireTable) && database.HasCompanyIdColumn(tableAndChange.NewTable))
		{
			CommandText.Append("WHERE ");
			AppendCompanyIdFieldName(CommandText, commaPrefix: false, commaSuffix: false);
			CommandText.Append(" NOT IN ( SELECT ");
			AppendCompanyIdFieldName(CommandText, commaPrefix: false, commaSuffix: false);
			CommandText.Append(" FROM ");
			AppendTableOrTempName(CommandText, database, tableAndChange.OldTable, oldCompanyToken, transferFromTemp, tableContext);
			CommandText.Append(" GROUP BY ");
			CommandText.Append(GetNewKeyFieldList(database, tableAndChange, tableAndChange.NewTable.PrimaryKey, () => ParameterCount, AddParameter));
			CommandText.Length--;
			CommandText.Append(" HAVING COUNT(*) > 1) ");
		}
		if (!database.IsSharedTenantSchema & flag)
		{
			CommandText.Append("; SET IDENTITY_INSERT ");
			CommandText.Append("\"");
			NavSqlStatementHelper.AppendTableNameCompanyQualified(CommandText, tableAndChange.NewTable, Database, newCompanyToken, tableContext);
			CommandText.Append("\"");
			CommandText.Append(" OFF");
		}
		if (database.HasSequencedAutoIncrementField(tableAndChange.NewTable, tableContext))
		{
			NCLMetaTable newTable = tableAndChange.NewTable;
			if (!transferEntireTable)
			{
				RestartSequenceForAutoIncrementField(newTable, newTable.AutoIncrementField, database, newCompanyToken);
			}
			else
			{
				RestartTableSequenceForAllTenants(newTable, newTable.AutoIncrementField);
			}
		}
	}

	private void TransferEntireTableData(NavDatabase database, NavSqlTableAndChange tableAndChange, bool transferFromTemp, NavAppTableContext tableContext, bool excludeCompanyIdsContainingNonUniquePrimaryKeyData = false)
	{
		if (!database.IsSharedTenantSchema)
		{
			throw new InvalidOperationException("This method only works on shared schema database");
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000015J", Category.Sql, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Transferring table Data -- TableId:{0}; NewName:{1}; OldName:{2}", tableAndChange.NewTable.TableId, tableAndChange.NewTable.TableName, tableAndChange.OldTable.TableName);
		StringBuilder stringBuilder = new StringBuilder();
		AppendTableOrTempName(stringBuilder, database, tableAndChange.OldTable, int.MinValue, transferFromTemp, tableContext);
		string text = stringBuilder.ToString();
		StringBuilder stringBuilder2 = new StringBuilder();
		StringBuilder stringBuilder3 = new StringBuilder();
		StringBuilder stringBuilder4 = new StringBuilder();
		StartNewCommand();
		CommandText.Append("INSERT INTO ");
		AppendTableOrTempName(tableAndChange.NewTable, int.MinValue, tempName: false, tableContext);
		CommandText.Append(" (");
		AppendCompanyIdFieldName(CommandText, commaPrefix: false, commaSuffix: true);
		foreach (NavSqlFieldAndChanges item in tableAndChange.FieldsAndChanges.Where((NavSqlFieldAndChanges field) => !field.Excessive && !field.NewField.FieldIsTimestamp))
		{
			if (item.NewField.FieldBelongsInPhysicalTable(tableContext))
			{
				CommandText.Append("\"");
				CommandText.Append(item.NewField.SqlColumnName);
				CommandText.Append("\"");
				CommandText.Append(',');
			}
		}
		CommandText.Length--;
		CommandText.Append(") SELECT ");
		if (tableAndChange.DataPerCompanyChanged)
		{
			string name = NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.CompanyName].Name;
			string name2 = NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.CompanyId].Name;
			string name3 = NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.TenantLocalId].Name;
			string text2 = "[dbo].[$ndo$tenantcompany]";
			if (tableAndChange.NewTable.DataPerCompany)
			{
				CommandText.Append("CompanyIds.[").Append(NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.CompanyId].Name).Append("], ");
				stringBuilder2.AppendLine(FormattableString.Invariant(FormattableStringFactory.Create("\r\n                        INNER JOIN {0} TC ON TC.[{1}] = {2}.{3}\r\n                        CROSS JOIN (SELECT [{4}], [{5}] FROM {6} WHERE [{7}] <> '') AS CompanyIds", text2, name2, text, "\"$companyId\"", name2, name3, text2, name)));
				stringBuilder3.AppendLine(FormattableString.Invariant($"CompanyIds.[{name3}] = TC.[{name3}]"));
			}
			else
			{
				CommandText.Append("CompanyIds.SharedCompanyId, ");
				stringBuilder2.Append(FormattableString.Invariant(FormattableStringFactory.Create("\r\n                        INNER JOIN {0} AS TC ON TC.[{1}] = {2}.{3}\r\n                        INNER JOIN (SELECT [{4}] AS SharedCompanyId, [{5}]\r\n                                      FROM {6}\r\n                                      WHERE [{7}] = ''\r\n                                      GROUP BY [{8}], [{9}]\r\n                                   ) AS CompanyIds ON CompanyIds.[{10}] = TC.[{11}]\r\n                        ", text2, name2, text, "\"$companyId\"", name2, name3, text2, name, name2, name3, name3, name3)));
				stringBuilder3.AppendLine(FormattableString.Invariant(FormattableStringFactory.Create("{0}.{1} IN (\r\n                                                SELECT MIN([{2}]) FROM {3} AS TC2 WHERE [{4}] <> '' AND EXISTS (\r\n                                                    SELECT 1 FROM {5} AS T WHERE TC2.[{6}] = T.{7}\r\n                                                ) GROUP BY [{8}])", text, "\"$companyId\"", name2, text2, name, text, name2, "\"$companyId\"", name3)));
			}
		}
		else
		{
			AppendCompanyIdFieldName(CommandText, commaPrefix: false, commaSuffix: true);
		}
		foreach (NavSqlFieldAndChanges fieldsAndChange in tableAndChange.FieldsAndChanges)
		{
			if (!fieldsAndChange.Excessive && fieldsAndChange.NewField.FieldBelongsInPhysicalTable(tableContext))
			{
				CommandText.Append(FieldWithConversion(fieldsAndChange, NavGlobal.AppDatabase.SqlSortingProperties, (NCLMetaField field) =>
				{
					SqlParameter defaultParameter = GetDefaultParameter(field, ParameterCount);
					AddParameter(defaultParameter);
					return defaultParameter.ParameterName;
				}));
				CommandText.Append(',');
			}
		}
		CommandText.Length--;
		if (excludeCompanyIdsContainingNonUniquePrimaryKeyData)
		{
			if (stringBuilder3.Length > 0)
			{
				stringBuilder3.Append(" AND ");
			}
			stringBuilder3.Append(text).Append(".");
			AppendCompanyIdFieldName(stringBuilder3, commaPrefix: false, commaSuffix: false);
			stringBuilder3.Append(" NOT IN ( SELECT ");
			AppendCompanyIdFieldName(stringBuilder3, commaPrefix: false, commaSuffix: false);
			stringBuilder3.Append(" FROM ");
			AppendTableOrTempName(stringBuilder3, database, tableAndChange.OldTable, int.MinValue, transferFromTemp, tableContext);
			stringBuilder4.Append(GetNewKeyFieldList(database, tableAndChange, tableAndChange.NewTable.PrimaryKey, () => ParameterCount, AddParameter));
			stringBuilder4.Length--;
			stringBuilder4.Append(" HAVING COUNT(*) > 1) ");
		}
		CommandText.Append(" FROM ").Append(text);
		if (stringBuilder2.Length > 0)
		{
			CommandText.AppendLine(stringBuilder2.ToString());
		}
		if (stringBuilder3.Length > 0)
		{
			CommandText.Append(" WHERE ").Append(stringBuilder3.ToString());
		}
		if (stringBuilder4.Length > 0)
		{
			CommandText.Append(" GROUP BY ").Append(stringBuilder4.ToString());
		}
		if (Database.HasSequencedAutoIncrementField(tableAndChange.NewTable, tableContext))
		{
			NCLMetaTable newTable = tableAndChange.NewTable;
			RestartTableSequenceForAllTenants(newTable, newTable.AutoIncrementField);
		}
	}

	private void TransferTenantData(NCLMetaTable table, NavAppTableContext tableContext, NavTenant fromTenant, Dictionary<int, CompanyId> companyIds)
	{
		StartNewCommand();
		IEnumerable<KeyValuePair<int, CompanyId>> enumerable = (table.DataPerCompany ? companyIds.Where((KeyValuePair<int, CompanyId> c) => c.Key != 0) : companyIds.Where((KeyValuePair<int, CompanyId> c) => c.Key == 0));
		foreach (KeyValuePair<int, CompanyId> item in enumerable)
		{
			int key = item.Key;
			CompanyId value = item.Value;
			CompanyId? companyId = fromTenant.Database.GetCompanyId(table, key);
			if (!companyId.HasValue)
			{
				throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.TransferTenantDataCompanyIdNotFound, table, fromTenant.Database.TenantProperties.TenantLocalId));
			}
			BuildTransferTenantDataCommand(fromTenant, table, tableContext, companyId.Value, key, value);
		}
	}

	private void BuildTransferTenantDataCommand(NavTenant fromTenant, NCLMetaTable table, NavAppTableContext tableContext, CompanyId templateCompanyId, int companyToken, CompanyId newCompanyId)
	{
		string identifier = NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), table, fromTenant.Database, companyToken, tableContext).ToString();
		CommandText.Append("INSERT INTO ");
		NavSqlStatementHelper.AppendQuotedIdentifier(CommandText, identifier);
		CommandText.Append(" (");
		AppendCompanyIdFieldName(CommandText, commaPrefix: false, commaSuffix: true);
		List<NCLMetaField> list = table.Fields.Where((NCLMetaField field) => !field.FieldIsTimestamp && !field.FieldIsUserDefinedTimestamp && field.FieldClass == FieldClass.Normal).ToList();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (NCLMetaField item in list)
		{
			stringBuilder.Append("\"");
			stringBuilder.Append(item.SqlColumnName);
			stringBuilder.Append("\"");
			stringBuilder.Append(',');
		}
		stringBuilder.Length--;
		CommandText.Append(stringBuilder);
		CommandText.Append(") SELECT ").Append(newCompanyId).Append(',');
		CommandText.Append(stringBuilder);
		CommandText.Append(" FROM ");
		NavSqlStatementHelper.AppendQuotedIdentifier(CommandText, identifier);
		CommandText.Append(" WHERE ").Append("\"$companyId\"").Append(" =  /*companyIdLiteral*/")
			.Append(templateCompanyId);
		CommandText.AppendLine();
		if (Database.HasSequencedAutoIncrementField(table, tableContext))
		{
			RestartSequenceForAutoIncrementField(table, table.AutoIncrementField, newCompanyId);
		}
	}

	private void CreateSequenceForAllTenants(NCLMetaTable table)
	{
		List<CompanyId> list = (table.DataPerCompany ? NavSqlTenantCompanies.ReadAllNormalCompanyIdsFromAllTenants(Database) : NavSqlTenantCompanies.ReadAllSharedCompanyIdsFromAllTenants(Database));
		foreach (CompanyId item in list)
		{
			CreateOrAlterSequence(NavSqlStatementHelper.SequenceNameForAutoIncrementField(table, table.AutoIncrementField, item), table.AutoIncrementField.SqlDataType, 1L);
		}
	}

	private void DropTableSequenceForAllTenants(NCLMetaTable table, NCLMetaField field)
	{
		List<CompanyId> list = (table.DataPerCompany ? NavSqlTenantCompanies.ReadAllNormalCompanyIdsFromAllTenants(Database) : NavSqlTenantCompanies.ReadAllSharedCompanyIdsFromAllTenants(Database));
		foreach (CompanyId item in list)
		{
			DropSequenceIfExists(NavSqlStatementHelper.SequenceNameForAutoIncrementField(table, field, item));
		}
	}

	private void RestartTableSequenceForAllTenants(NCLMetaTable table, NCLMetaField field)
	{
		List<CompanyId> list = (table.DataPerCompany ? NavSqlTenantCompanies.ReadAllNormalCompanyIdsFromAllTenants(Database) : NavSqlTenantCompanies.ReadAllSharedCompanyIdsFromAllTenants(Database));
		foreach (CompanyId item in list)
		{
			RestartSequenceForAutoIncrementField(table, field, item);
		}
	}

	public static void UpdateCompanionTableRecordsForExtensionInstall(NCLMetaTable table, NavDatabase tenantDatabase, int companyIndex, NavAppTableContext companionTableContext)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(tenantDatabase);
		navSqlDdlCommands.RemoveExcessiveRecordsFromCompanionTable(table, companyIndex, companionTableContext);
		navSqlDdlCommands.ExecuteNonQuery();
		navSqlDdlCommands.InsertDefaultRecordsIntoCompanionTable(table, companyIndex, companionTableContext);
		navSqlDdlCommands.ExecuteNonQuery();
	}

	private void RemoveExcessiveRecordsFromCompanionTable(NCLMetaTable table, int companyIndex, NavAppTableContext companionTableContext)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000012C", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removing excessive companion records -- Table:{0}", NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(new StringBuilder(), table, Database, companyIndex, companionTableContext));
		StartNewCommand();
		List<NCLMetaField> list = table.PrimaryKey.KeyFieldsList.ToList();
		CommandText.Append("DELETE FROM ");
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(CommandText, table, Database, companyIndex, companionTableContext);
		CommandText.Append(" WHERE NOT EXISTS (SELECT 1 FROM ");
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(CommandText, table, Database, companyIndex, NavAppTableContext.Main);
		CommandText.Append(" WHERE ");
		for (int i = 0; i < list.Count; i++)
		{
			NCLMetaField field = list[i];
			if (i > 0)
			{
				CommandText.Append(" AND ");
			}
			NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(CommandText, table, Database, companyIndex, companionTableContext);
			CommandText.Append('.');
			NavSqlStatementHelper.AppendSqlColumnName(CommandText, field, null);
			CommandText.Append(" = ");
			NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(CommandText, table, Database, companyIndex, NavAppTableContext.Main);
			CommandText.Append('.');
			NavSqlStatementHelper.AppendSqlColumnName(CommandText, field, null);
		}
		CommandText.Append(")");
		if (Database.IsSharedTenantSchema && !table.IsCrossTenantTable && !table.IsAppTable)
		{
			CommandText.Append(" AND ");
			CommandText.Append("\"$companyId\"");
			CommandText.Append(" = @companyID");
			SqlParameter sqlParameter = new SqlParameter("@companyID", SqlDbType.Int);
			sqlParameter.Value = Database.GetCompanyId(table, companyIndex);
			AddParameter(sqlParameter);
		}
	}

	private void InsertDefaultRecordsIntoCompanionTable(NCLMetaTable table, int companyIndex, NavAppTableContext companionTableContext)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000012D", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Inserting default companion records -- Table:{0}", NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(new StringBuilder(), table, Database, companyIndex, companionTableContext));
		StartNewCommand();
		List<NCLMetaField> list = table.PrimaryKey.KeyFieldsList.ToList();
		List<NCLMetaField> list2 = (from f in table.NormalFieldsOwnedByCompanionTables
			where f.OriginatesFrom(companionTableContext)
			where !f.FieldIsAutoIncrement && f.FieldNclType != NavNclType.NavBlob
			select f).ToList();
		CommandText.Append("INSERT INTO ");
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(CommandText, table, Database, companyIndex, companionTableContext);
		CommandText.Append(" (");
		if (Database.IsSharedTenantSchema && !table.IsCrossTenantTable && !table.IsAppTable)
		{
			CommandText.Append("\"$companyId\"");
			CommandText.Append(",");
		}
		foreach (NCLMetaField item in list.Concat(list2))
		{
			NavSqlStatementHelper.AppendQuotedIdentifier(CommandText, item.SqlColumnName);
			CommandText.Append(',');
		}
		CommandText.Length--;
		CommandText.Append(") SELECT ");
		if (Database.IsSharedTenantSchema && !table.IsCrossTenantTable && !table.IsAppTable)
		{
			CommandText.Append("\"$companyId\"");
			CommandText.Append(",");
		}
		foreach (NCLMetaField item2 in list)
		{
			NavSqlStatementHelper.AppendQuotedIdentifier(CommandText, item2.SqlColumnName);
			CommandText.Append(',');
		}
		foreach (NCLMetaField item3 in list2)
		{
			NavSqlStatementHelper.AppendSqlDefaultValueAsLiteral(CommandText, item3);
			CommandText.Append(',');
		}
		CommandText.Length--;
		CommandText.Append(" FROM ");
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(CommandText, table, Database, companyIndex, NavAppTableContext.Main);
		CommandText.Append(" WHERE NOT EXISTS (SELECT 1 FROM ");
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(CommandText, table, Database, companyIndex, companionTableContext);
		CommandText.Append(" WHERE ");
		for (int num = 0; num < list.Count; num++)
		{
			NCLMetaField field = list[num];
			if (num > 0)
			{
				CommandText.Append(" AND ");
			}
			NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(CommandText, table, Database, companyIndex, companionTableContext);
			CommandText.Append('.');
			NavSqlStatementHelper.AppendSqlColumnName(CommandText, field, null);
			CommandText.Append(" = ");
			NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(CommandText, table, Database, companyIndex, NavAppTableContext.Main);
			CommandText.Append('.');
			NavSqlStatementHelper.AppendSqlColumnName(CommandText, field, null);
		}
		CommandText.Append(")");
	}

	public static void PurgeNavAppDataFromCompanionTable(NCLMetaTable table, NavDatabase tenantDatabase, int companyIndex, NavAppTableContext tableContext)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(tenantDatabase);
		navSqlDdlCommands.DeleteCompanyDataFromCompanionTable(table, companyIndex, tableContext);
		navSqlDdlCommands.ExecuteNonQuery();
	}

	private void DeleteCompanyDataFromCompanionTable(NCLMetaTable table, int companyIndex, NavAppTableContext companionTableContext)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000012U", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Purging companion records -- Table:{0}", NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(new StringBuilder(), table, Database, companyIndex, companionTableContext));
		StartNewCommand();
		CommandText.Append("DELETE FROM ");
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(CommandText, table, Database, companyIndex, companionTableContext);
		if (Database.IsSharedTenantSchema && !table.IsCrossTenantTable && !table.IsAppTable)
		{
			CommandText.Append(" WHERE ");
			CommandText.Append("\"$companyId\"");
			CommandText.Append(" =  /*companyIdLiteral*/" + Database.GetCompanyId(table, companyIndex).ToString());
		}
	}

	public static void CreateTableNoIndexes(NavDatabase database, NCLMetaTable table, int companyToken, NavAppTableContext tableContext, TablePartitionMetadata tablePartitionMetadata)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		navSqlDdlCommands.CreateTable(table, companyToken, tableContext, tablePartitionMetadata);
		navSqlDdlCommands.ExecuteNonQuery();
	}

	public static void CreateAllIndexes(NavDatabase database, NCLMetaTable table, int companyToken, NavAppTableContext tableContext, TablePartitionMetadata tablePartitionMetadata)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		navSqlDdlCommands.CreateNonSiftIndexes(table, companyToken, tableContext, tablePartitionMetadata);
		navSqlDdlCommands.CreateSiftIndexes(table, companyToken, tableContext);
		navSqlDdlCommands.ExecuteNonQuery();
	}

	public static void CreateTableAndIndexes(NavDatabase database, NCLMetaTable table, int companyToken, NavAppTableContext tableContext, TablePartitionMetadata tablePartitionMetadata)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		navSqlDdlCommands.CreateTableAndIndexes(table, companyToken, tableContext, tablePartitionMetadata);
		navSqlDdlCommands.ExecuteNonQuery();
	}

	private void CreateTableAndIndexes(NCLMetaTable table, int companyToken, NavAppTableContext tableContext, TablePartitionMetadata tablePartitionMetadata)
	{
		CreateTable(table, companyToken, tableContext, tablePartitionMetadata);
		CreateNonSiftIndexes(table, companyToken, tableContext, tablePartitionMetadata);
		CreateSiftIndexes(table, companyToken, tableContext);
	}

	public static void CreateTableInCompany(NavDatabase database, NCLMetaTable table, int companyToken, NavAppTableContext tableContext, TablePartitionMetadata tablePartitionMetadata)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		navSqlDdlCommands.CreateTableAndIndexes(table, companyToken, tableContext, tablePartitionMetadata);
		if (CompanyOperationNeedsToCreateConstraints(table, tableContext))
		{
			navSqlDdlCommands.AlterTableAddFieldDefaultConstraints(table, table.Fields.Where((NCLMetaField f) => f.OriginatingTableContext.Equals(tableContext)), companyToken, tableContext);
		}
		navSqlDdlCommands.ExecuteNonQuery();
		SetupTableForIntelligentCloud(database, table, companyToken, tableContext);
	}

	public static void DropTableAndIndexes(NavDatabase database, NCLMetaTable table, int companyToken, NavAppTableContext tableContext)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		navSqlDdlCommands.DropSiftIndexes(table, companyToken, tableContext);
		navSqlDdlCommands.DropTableIfExists(table, companyToken, isTemp: false, tableContext);
		navSqlDdlCommands.ExecuteNonQuery();
		DeleteIntelligentCloudStatus(database, table, companyToken, tableContext);
	}

	public static void DropSiftIndexes(NavDatabase database, NCLMetaTable table, int companyToken, NavAppTableContext tableContext)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		navSqlDdlCommands.DropSiftIndexes(table, companyToken, tableContext);
		navSqlDdlCommands.ExecuteNonQuery();
	}

	public static void MoveTableToNewCompany(NavDatabase database, NCLMetaTable table, int oldCompanyToken, int newCompanyToken, NavAppTableContext tableContext)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		navSqlDdlCommands.RenameTable(table, oldCompanyToken, table, newCompanyToken, tableContext);
		navSqlDdlCommands.CreateSiftIndexes(table, newCompanyToken, tableContext);
		navSqlDdlCommands.ExecuteNonQuery();
	}

	public static void CopyTableToNewCompany(NavDatabase database, NCLMetaTable table, int fromCompanyToken, int toCompanyToken, NavAppTableContext tableContext, TablePartitionMetadata tablePartitionMetadata)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		if (database.TenantDatabaseProperties.PerCompanySchema)
		{
			navSqlDdlCommands.CreateTable(table, toCompanyToken, tableContext, tablePartitionMetadata);
			if (CompanyOperationNeedsToCreateConstraints(table, tableContext))
			{
				navSqlDdlCommands.AlterTableAddFieldDefaultConstraints(table, table.Fields.Where((NCLMetaField f) => f.OriginatingTableContext.Equals(tableContext)), toCompanyToken, tableContext);
			}
		}
		NavSqlTableAndChange tableAndChange = new NavSqlTableAndChange(table, TableChangeType.Changed, table, NavText.Empty, NavText.Empty, 0, changedSortingProperties: false, databaseConvertedFromNav2013: false);
		navSqlDdlCommands.TransferData(database, tableAndChange, fromCompanyToken, toCompanyToken, transferFromTemp: false, tableContext, transferEntireTable: false);
		if (database.TenantDatabaseProperties.PerCompanySchema)
		{
			navSqlDdlCommands.CreateNonSiftIndexes(table, toCompanyToken, tableContext, tablePartitionMetadata);
			navSqlDdlCommands.CreateSiftIndexes(table, toCompanyToken, tableContext);
		}
		navSqlDdlCommands.ExecuteNonQuery();
	}

	private static bool CompanyOperationNeedsToCreateConstraints(NCLMetaTable table, NavAppTableContext tableContext)
	{
		if (tableContext.TableType == NavAppTableContextType.Main && table.AppOrigin.AppId.HasValue && table.SourceExtensionType == NavExtensionType.ModernDev)
		{
			return true;
		}
		if (tableContext.TableType == NavAppTableContextType.Companion)
		{
			return true;
		}
		return false;
	}

	public static void CopyTenantToNewTenant(NavDatabase database, NCLMetaTable table, NavAppTableContext tableContext, NavTenant fromTenant, Dictionary<int, CompanyId> companyIds)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		navSqlDdlCommands.TransferTenantData(table, tableContext, fromTenant, companyIds);
		navSqlDdlCommands.ExecuteNonQuery();
	}

	public static void PurgeAppArchiveRecordsAndTablesForApp(NavDatabase database, Guid appId)
	{
		INavAppDataArchiveRepository navAppDataArchiveRepository = new DatabaseNavAppDataArchiveRepository(database);
		IEnumerable<NavAppDataArchiveRecord> recordsForApp = navAppDataArchiveRepository.GetRecordsForApp(appId);
		foreach (NavAppDataArchiveRecord item in recordsForApp)
		{
			NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
			navSqlDdlCommands.DropAppArchiveTableIfExists(item.ArchiveTableName);
			navSqlDdlCommands.ExecuteNonQuery();
			navAppDataArchiveRepository.DeleteRecord(item);
		}
	}

	public static void PurgeAppArchiveRecordAndTable(NavDatabase database, Guid appId, string companyName, int tableId)
	{
		INavAppDataArchiveRepository navAppDataArchiveRepository = new DatabaseNavAppDataArchiveRepository(database);
		NavAppDataArchiveRecord record = navAppDataArchiveRepository.GetRecord(appId, tableId, companyName);
		if (record != null)
		{
			NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
			navSqlDdlCommands.DropAppArchiveTableIfExists(record.ArchiveTableName);
			navSqlDdlCommands.ExecuteNonQuery();
			navAppDataArchiveRepository.DeleteRecord(record);
		}
	}

	public static void DeleteAllInstalledAppEntriesAsPartOfUninstallAllProcess(NavDatabase database)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		navSqlDdlCommands.DeleteAllInstalledAppEntriesAsPartOfUninstallAllProcess();
		navSqlDdlCommands.ExecuteNonQuery();
	}

	public static void PrepareOldTable(NavDatabase database, NavSqlTableAndChange tableAndChange, int companyToken, SyncMode syncMode, NavAppSyncMode? appSyncMode)
	{
		if (tableAndChange.Missing)
		{
			return;
		}
		string content = (database.TenantDatabaseProperties.PerCompanySchema ? string.Format(CultureInfo.InvariantCulture, "Preparing old table {0} in company {1} ({2})", tableAndChange.OldTable.TableId, companyToken, database.GetCompanyNameFromToken(companyToken)) : string.Format(CultureInfo.InvariantCulture, "Preparing old table {0}", tableAndChange.OldTable.TableId));
		using (DiagnosticsResolver.CreateTimingScope("00000RD", Category.DatabaseSync, DiagnosticParameter.Oii(content)))
		{
			if (tableAndChange.DataUpgradeSetup == DataUpgradeSetup.OldAndNewPopulated || tableAndChange.DataUpgradeSetup == DataUpgradeSetup.OldPopulatedNewEmpty)
			{
				tableAndChange.ValidateTableSchemasSupportMoving();
			}
			bool flag = appSyncMode.HasValue && appSyncMode.Value == NavAppSyncMode.Clean;
			bool flag2 = false;
			if (!flag && (tableAndChange.Excessive || tableAndChange.DataPerCompanyChanged))
			{
				flag2 = DataExistsInTable(database, tableAndChange.OldTable, companyToken, tableAndChange.TableContext);
			}
			if (flag2 && tableAndChange.Excessive && !tableAndChange.AllowDeletionOfFieldsAndTablesWithContent && syncMode != SyncMode.ForceSync)
			{
				throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.AttemptToRemoveNonEmptyTable, tableAndChange.OldTable.TableName, database.DatabaseName, database.TenantDatabaseProperties.PerCompanySchema ? database.GetCompanyNameFromToken(companyToken) : string.Empty));
			}
			if (tableAndChange.ChangesAffectSqlSchema && tableAndChange.Changed && syncMode != SyncMode.ForceSync && !tableAndChange.AllowDeletionOfFieldsAndTablesWithContent)
			{
				NavSqlDatabaseSync.CheckColumnChanges(database, companyToken, tableAndChange);
			}
			if (tableAndChange.DataPerCompanyChanged & flag2)
			{
				if (database.TenantDatabaseProperties.PerCompanySchema)
				{
					if (tableAndChange.DataTransferFromCompanyToken >= 0)
					{
						if (!tableAndChange.AllowDataLossOnChangeFromPerCompanyToGlobal && syncMode != SyncMode.ForceSync)
						{
							throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.DataExistsForTableInMoreThanOneCompany, tableAndChange.OldTable.TableName, database.DatabaseName, database.GetCompanyNameFromToken(tableAndChange.DataTransferFromCompanyToken), database.GetCompanyNameFromToken(companyToken)));
						}
					}
					else
					{
						tableAndChange.DataTransferFromCompanyToken = companyToken;
					}
				}
				else
				{
					if (!tableAndChange.AllowDataLossOnChangeFromPerCompanyToGlobal && syncMode != SyncMode.ForceSync && DataExistsInMoreThanOneCompany(database, tableAndChange.OldTable, out var tenantAndCompaniesWithData))
					{
						StringBuilder sb = new StringBuilder();
						tenantAndCompaniesWithData.ForEach((KeyValuePair<string, string> t) =>
						{
							sb.Append(t.Key).Append('/').Append(t.Value)
								.Append(", ");
						});
						sb.Length -= 2;
						throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.DataExistsForTableInMoreThanOneCompanyNotPerCompanySchema, tableAndChange.OldTable.TableName, database.DatabaseName, sb.ToString()));
					}
					tableAndChange.DataTransferFromCompanyToken = companyToken;
				}
			}
			if (syncMode == SyncMode.CheckOnly)
			{
				return;
			}
			if (tableAndChange.RebuildTable)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001BO", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Rebuilding Table -- TableId:{0}", tableAndChange.OldTable.TableId);
				NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
				navSqlDdlCommands.DropSiftIndexes(tableAndChange.OldTable, companyToken, tableAndChange.TableContext);
				if (!flag)
				{
					navSqlDdlCommands.RenameTableToTemp(tableAndChange, companyToken);
				}
			}
			else
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001BP", Category.DatabaseSync, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Altering Table -- TableId:{0}", tableAndChange.OldTable.TableId);
				AlterTableInitial(database, tableAndChange, companyToken);
			}
		}
	}

	public static void RemoveExistingUpgradeTable(NavDatabase database, NavSqlTableAndChange tableAndChange, int companyToken, SyncMode syncMode)
	{
		if (syncMode == SyncMode.CheckOnly)
		{
			return;
		}
		string content = (database.TenantDatabaseProperties.PerCompanySchema ? string.Format(CultureInfo.InvariantCulture, "Removing existing upgrade table for table {0} in company {1} ({2})", tableAndChange.NewTable.TableId, companyToken, database.GetCompanyNameFromToken(companyToken)) : string.Format(CultureInfo.InvariantCulture, "Removing existing upgrade table for table {0}", tableAndChange.NewTable.TableId));
		using (DiagnosticsResolver.CreateTimingScope("00000RE", Category.DatabaseSync, DiagnosticParameter.Oii(content)))
		{
			NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
			navSqlDdlCommands.DropSiftIndexes(tableAndChange.NewTable, companyToken, tableAndChange.TableContext);
			navSqlDdlCommands.DropTableIfExists(tableAndChange.NewTable, companyToken, isTemp: false, tableAndChange.TableContext);
			if (!tableAndChange.RebuildTable)
			{
				foreach (NavSqlIndexAndChanges indexesAndChange in tableAndChange.IndexesAndChanges)
				{
					if ((indexesAndChange.Excessive || indexesAndChange.IndexFields || indexesAndChange.UniqueChanged) && !indexesAndChange.OldSqlIndex.IsPrimaryKey)
					{
						navSqlDdlCommands.DropNonSiftIndexIfExists(indexesAndChange.OldSqlIndex, companyToken, tableAndChange.TableContext, isTemp: true);
					}
				}
			}
			navSqlDdlCommands.ExecuteNonQuery();
		}
	}

	private static bool PerformNewKeyUniquenessCheck(NavDatabase tenantDatabase, NavSqlTableAndChange tableAndChange, NCLMetaKey key, int companyToken, SyncMode syncMode)
	{
		if (!tableAndChange.ChangesAffectSqlSchema || !tableAndChange.IndexesAndChanges.Any((NavSqlIndexAndChanges indexAndChanges) => indexAndChanges.PrimaryKey || indexAndChanges.IsPotentialUniqueConstraintViolation))
		{
			return true;
		}
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase))
		{
			NavSqlCommand command = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			try
			{
				StringBuilder stringBuilder = new StringBuilder("SELECT TOP 1 COUNT(*),");
				StringBuilder newKeyFieldList = GetNewKeyFieldList(tenantDatabase, tableAndChange, key, () => command.Command.Parameters.Count, (SqlParameter parm) =>
				{
					command.Command.Parameters.Add(parm);
				});
				stringBuilder.Append(newKeyFieldList);
				stringBuilder.Length--;
				stringBuilder.Append(" FROM ");
				AppendTableOrTempName(stringBuilder, tenantDatabase, tableAndChange.OldTable, companyToken, syncMode != SyncMode.CheckOnly && (tableAndChange.RebuildTable || tableAndChange.TableRenamed), tableAndChange.TableContext);
				if (newKeyFieldList.Length > 0)
				{
					stringBuilder.Append(" GROUP BY ");
					stringBuilder.Append(newKeyFieldList);
					stringBuilder.Length--;
				}
				stringBuilder.Append(" HAVING COUNT(*) > 1");
				if (command.ExecuteScalar(stringBuilder.ToString()) != null)
				{
					if (!tableAndChange.AllowDeleteTableContentOnNonUniqueKey && (syncMode == SyncMode.CheckOnly || syncMode == SyncMode.Sync))
					{
						throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, key.IsPrimaryKey ? Lang.NewPrimaryKeyIsNotUnique : Lang.UniqueKeyIsNotUnique, tableAndChange.NewTable.TableName, tenantDatabase.Tenant.Id, tenantDatabase.DatabaseName, tenantDatabase.TenantDatabaseProperties.PerCompanySchema ? tenantDatabase.GetCompanyNameFromToken(companyToken) : string.Empty, string.IsNullOrEmpty(key.KeyName) ? key.KeyIndex.ToString(CultureInfo.InvariantCulture) : key.KeyName));
					}
					return false;
				}
			}
			finally
			{
				if (command != null)
				{
					((IDisposable)command).Dispose();
				}
			}
		}
		return true;
	}

	private static StringBuilder GetNewKeyFieldList(NavDatabase tenantDatabase, NavSqlTableAndChange tableAndChange, NCLMetaKey key, Func<int> parameterCount, Action<SqlParameter> parameterAdd)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (tenantDatabase.HasCompanyIdColumn(tableAndChange.NewTable))
		{
			AppendCompanyIdFieldName(stringBuilder, commaPrefix: false, commaSuffix: true);
		}
		foreach (NCLMetaField newKeyField in key.KeyFieldsList)
		{
			NavSqlFieldAndChanges navSqlFieldAndChanges = tableAndChange.FieldsAndChanges.First((NavSqlFieldAndChanges item) => item.NewField == newKeyField);
			if (!navSqlFieldAndChanges.Missing && !navSqlFieldAndChanges.DataTypeChanged && (!navSqlFieldAndChanges.SqlDataTypeChanged || navSqlFieldAndChanges.NewField.SqlDataTypeWithDefaultMapping == SqlDataType.Variant))
			{
				stringBuilder.Append(FieldWithConversion(navSqlFieldAndChanges, NavGlobal.AppDatabase.SqlSortingProperties, (NCLMetaField field) =>
				{
					SqlParameter defaultParameter = GetDefaultParameter(field, parameterCount());
					parameterAdd(defaultParameter);
					return defaultParameter.ParameterName;
				}));
				stringBuilder.Append(',');
			}
		}
		return stringBuilder;
	}

	public static void PrepareNewTable(NavDatabase database, NavSqlTableAndChange tableAndChange, int companyToken, SyncMode syncMode)
	{
		if (tableAndChange.Excessive)
		{
			return;
		}
		string text = ((tableAndChange.TableContext.TableType == NavAppTableContextType.Main) ? "" : "companion ");
		string content = (database.TenantDatabaseProperties.PerCompanySchema ? string.Format(CultureInfo.InvariantCulture, "Preparing new {0}table {1} in company {2} ({3})", text, tableAndChange.NewTable.TableId, companyToken, database.GetCompanyNameFromToken(companyToken)) : string.Format(CultureInfo.InvariantCulture, "Preparing new {0}table {1}", text, tableAndChange.NewTable.TableId));
		using (DiagnosticsResolver.CreateTimingScope("00000RF", Category.DatabaseSync, DiagnosticParameter.Oii(content)))
		{
			int num = (tableAndChange.DataPerCompanyChanged ? tableAndChange.DataTransferFromCompanyToken : companyToken);
			bool flag = num != -1;
			bool flag2 = false;
			if (flag && !tableAndChange.Missing)
			{
				flag2 = true;
				foreach (NCLMetaKey item in (from change in tableAndChange.IndexesAndChanges
					where change.IsPotentialUniqueConstraintViolation
					select change.NewKey).Append(tableAndChange.NewTable.PrimaryKey))
				{
					flag2 &= PerformNewKeyUniquenessCheck(database, tableAndChange, item, num, syncMode);
				}
			}
			if (syncMode == SyncMode.CheckOnly)
			{
				return;
			}
			if (!tableAndChange.RebuildTable)
			{
				if (!flag2 && syncMode == SyncMode.ForceSync)
				{
					TruncateExistingTable(database, tableAndChange, companyToken);
				}
				AlterTableFinalize(database, tableAndChange, companyToken, syncMode, tableAndChange.TablePartitionMetadata);
				return;
			}
			NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
			navSqlDdlCommands.CreateTableAndCreateDefaultConstraintsIfAny(tableAndChange, tableAndChange.NewTable, companyToken, tableAndChange.TableContext, tableAndChange.TablePartitionMetadata);
			if (((tableAndChange.ChangesAffectSqlSchema & flag2) && tableAndChange.DataUpgradeSetup != DataUpgradeSetup.OldPopulatedNewEmpty) & flag)
			{
				if (database.TenantDatabaseProperties.PerCompanySchema)
				{
					navSqlDdlCommands.TransferData(database, tableAndChange, num, companyToken, transferFromTemp: true, tableAndChange.TableContext, transferEntireTable: false);
				}
				else
				{
					navSqlDdlCommands.TransferEntireTableData(database, tableAndChange, transferFromTemp: true, tableAndChange.TableContext, syncMode == SyncMode.ForceSync);
					navSqlDdlCommands.ExecuteNonQuery();
				}
			}
			navSqlDdlCommands.ExecuteNonQuery();
		}
	}

	private void DropAppArchiveTableIfExists(string tableNameWithoutSchema)
	{
		StartNewCommand();
		string value = string.Format(CultureInfo.InvariantCulture, "{0}.{1}{2}{1}", "dbo", "\"", tableNameWithoutSchema);
		CommandText.Append("IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'").Append(value).Append("'), N'IsTable'))=1 DROP TABLE ")
			.Append(value);
	}

	private void DeleteAllInstalledAppEntriesAsPartOfUninstallAllProcess()
	{
		StartNewCommand();
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000153);
		string value = string.Format(CultureInfo.InvariantCulture, "{0}.{1}{2}{1}", "dbo", "\"", metaTableById.SqlTableName);
		CommandText.Append("IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'").Append(value).Append("'), N'IsTable'))=1 TRUNCATE TABLE ")
			.Append(value);
	}

	private static void TruncateExistingTable(NavDatabase database, NavSqlTableAndChange tableAndChange, int companyToken)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		navSqlDdlCommands.CommandText.Append("IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'");
		AppendTableOrTempName(navSqlDdlCommands.CommandText, database, tableAndChange.OldTable, companyToken, tableAndChange.RebuildTable || tableAndChange.TableRenamed, NavAppTableContext.Main);
		navSqlDdlCommands.CommandText.Append("'), N'IsTable'))=1 DELETE FROM ");
		AppendTableOrTempName(navSqlDdlCommands.CommandText, database, tableAndChange.OldTable, companyToken, tableAndChange.RebuildTable || tableAndChange.TableRenamed, NavAppTableContext.Main);
		navSqlDdlCommands.ExecuteNonQuery();
	}

	public static void CleanupOldTable(NavDatabase database, NavSqlTableAndChange tableAndChange, int oldCompanyToken, SyncMode syncMode, NavAppSyncMode? appSyncMode)
	{
		if (tableAndChange.Missing || syncMode == SyncMode.CheckOnly)
		{
			return;
		}
		bool flag = appSyncMode.HasValue && appSyncMode.Value == NavAppSyncMode.Clean;
		string content = (database.TenantDatabaseProperties.PerCompanySchema ? string.Format(CultureInfo.InvariantCulture, "Cleaning up old table {0} in company {1} ({2})", tableAndChange.OldTable.TableId, oldCompanyToken, database.GetCompanyNameFromToken(oldCompanyToken)) : string.Format(CultureInfo.InvariantCulture, "Cleaning up old table {0}", tableAndChange.OldTable.TableId));
		using (DiagnosticsResolver.CreateTimingScope("00000RG", Category.DatabaseSync, DiagnosticParameter.Oii(content)))
		{
			if ((tableAndChange.DataUpgradeSetup == DataUpgradeSetup.None || tableAndChange.DataUpgradeSetup == DataUpgradeSetup.ForceSync) && tableAndChange.RebuildTable)
			{
				bool isTemp = !flag;
				NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
				navSqlDdlCommands.DropTableIfExists(tableAndChange.OldTable, oldCompanyToken, isTemp, tableAndChange.TableContext);
				navSqlDdlCommands.ExecuteNonQuery();
				DeleteIntelligentCloudStatus(database, tableAndChange.OldTable, oldCompanyToken, tableAndChange.TableContext);
			}
		}
	}

	public static void FinalizeTable(NavDatabase database, NavSqlTableAndChange tableAndChange, int newCompanyToken, SyncMode syncMode)
	{
		if (tableAndChange.Excessive || syncMode == SyncMode.CheckOnly)
		{
			return;
		}
		string content = (database.TenantDatabaseProperties.PerCompanySchema ? string.Format(CultureInfo.InvariantCulture, "Finalizing new table {0} in company {1} ({2})", tableAndChange.NewTable.TableId, newCompanyToken, database.GetCompanyNameFromToken(newCompanyToken)) : string.Format(CultureInfo.InvariantCulture, "Finalizing new table {0}", tableAndChange.NewTable.TableId));
		using (DiagnosticsResolver.CreateTimingScope("00000RH", Category.DatabaseSync, DiagnosticParameter.Oii(content)))
		{
			ChangeCompressionForBLOBsIfNeeded(database, tableAndChange, newCompanyToken);
			if (tableAndChange.RebuildTable)
			{
				NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
				navSqlDdlCommands.CreateNonSiftIndexes(tableAndChange.NewTable, newCompanyToken, tableAndChange.TableContext, tableAndChange.TablePartitionMetadata);
				navSqlDdlCommands.CreateSiftIndexes(tableAndChange.NewTable, newCompanyToken, tableAndChange.TableContext);
				navSqlDdlCommands.ExecuteNonQuery();
			}
		}
	}

	public static void AlterTableInitial(NavDatabase database, NavSqlTableAndChange tableAndChange, int companyToken)
	{
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		foreach (NavSqlIndexAndChanges indexesAndChange in tableAndChange.IndexesAndChanges)
		{
			if ((indexesAndChange.Excessive || indexesAndChange.IndexFields || indexesAndChange.UniqueChanged) && !indexesAndChange.OldSqlIndex.IsPrimaryKey)
			{
				navSqlDdlCommands.DropNonSiftIndexIfExists(indexesAndChange.OldSqlIndex, companyToken, tableAndChange.TableContext);
			}
			if (indexesAndChange.SiftExcessive || indexesAndChange.SumFields)
			{
				navSqlDdlCommands.DropSiftIndexIfExists(indexesAndChange.OldSiftIndex, companyToken, tableAndChange.TableContext);
			}
		}
		if (tableAndChange.TableRenamed)
		{
			navSqlDdlCommands.RenameObject(NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), tableAndChange.OldTable, database, companyToken, tableAndChange.TableContext), NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder("TEMP$"), tableAndChange.OldTable, database, companyToken, tableAndChange.TableContext));
			navSqlDdlCommands.RenameObject(NavSqlStatementHelper.AppendIndexName(new StringBuilder(), tableAndChange.OldTable.PrimaryKey, database, companyToken, tableAndChange.TableContext), NavSqlStatementHelper.AppendIndexName(new StringBuilder("TEMP$"), tableAndChange.OldTable.PrimaryKey, database, companyToken, tableAndChange.TableContext));
		}
		navSqlDdlCommands.ExecuteNonQuery();
	}

	public static void AlterTableFinalize(NavDatabase database, NavSqlTableAndChange tableAndChange, int companyToken, SyncMode syncMode, TablePartitionMetadata tablePartitionMetadata)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000015K", Category.Sql, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Altering Table -- TableId:{0}; NewName:{1}; OldName:{2}", tableAndChange.OldTable.TableId, tableAndChange.NewTable?.TableName, tableAndChange.OldTable.TableName);
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(database);
		if (tableAndChange.TableRenamed || tableAndChange.IsUpgradeChange)
		{
			navSqlDdlCommands.RenameObject(NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder("TEMP$"), tableAndChange.OldTable, database, companyToken, tableAndChange.TableContext), NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), tableAndChange.NewTable, database, companyToken, tableAndChange.TableContext));
			navSqlDdlCommands.RenameObject(NavSqlStatementHelper.AppendIndexName(new StringBuilder("TEMP$"), tableAndChange.OldTable.PrimaryKey, database, companyToken, tableAndChange.TableContext), NavSqlStatementHelper.AppendIndexName(new StringBuilder(), tableAndChange.NewTable.PrimaryKey, database, companyToken, tableAndChange.TableContext));
			navSqlDdlCommands.RenameConstraintsIfExist(tableAndChange.NewTable, tableAndChange.OldTable, tableAndChange.FieldsAndChanges, companyToken, companyToken, tableAndChange.TableContext);
			navSqlDdlCommands.ExecuteNonQuery();
		}
		List<NCLMetaField> list = (from fieldAndChange in tableAndChange.FieldsAndChanges
			where fieldAndChange.Excessive
			select fieldAndChange.OldField).ToList();
		List<NavSqlFieldAndChanges> list2 = tableAndChange.FieldsAndChanges.Where((NavSqlFieldAndChanges fieldAndChange) => fieldAndChange.NameChanged).ToList();
		List<NavSqlFieldAndChanges> list3 = tableAndChange.FieldsAndChanges.Where((NavSqlFieldAndChanges f) => !f.Excessive && f.LengthChanged && f.NewField.IsStringType && f.NewField.SqlDataType != SqlDataType.Variant).ToList();
		List<NCLMetaField> list4 = new List<NCLMetaField>();
		list4.AddRange(list);
		list4.AddRange(list2.Select((NavSqlFieldAndChanges f) => f.OldField));
		list4.AddRange(list3.Select((NavSqlFieldAndChanges f) => f.OldField));
		if (!FeatureSwitches.Instance.DisableDropStatisticsOnSync)
		{
			navSqlDdlCommands.DropStatisticsOnFields(tableAndChange.NewTable, list4, companyToken, tableAndChange.TableContext);
		}
		navSqlDdlCommands.AlterTableDropConstraintIfExists(tableAndChange.NewTable, list, companyToken, tableAndChange.TableContext);
		navSqlDdlCommands.AlterTableDropFields(tableAndChange.OldTable, tableAndChange.NewTable, list, companyToken, tableAndChange.TableContext);
		navSqlDdlCommands.RenameConstraintsIfExist(tableAndChange.NewTable, tableAndChange.OldTable, list2, companyToken, companyToken, tableAndChange.TableContext);
		navSqlDdlCommands.RenameFields(tableAndChange.NewTable, list2, companyToken, tableAndChange.TableContext);
		List<NCLMetaField> fields = (from fieldAndChange in tableAndChange.FieldsAndChanges
			where fieldAndChange.Missing
			select fieldAndChange.NewField).ToList();
		navSqlDdlCommands.AlterTableAddFields(tableAndChange.NewTable, fields, companyToken, tableAndChange.TableContext);
		navSqlDdlCommands.AlterTableChangeFieldsLength(tableAndChange.NewTable, list3, companyToken, tableAndChange.TableContext, syncMode == SyncMode.ForceSync);
		HashSet<NCLMetaKey> hashSet = new HashSet<NCLMetaKey>();
		HashSet<NCLMetaKey> hashSet2 = new HashSet<NCLMetaKey>();
		foreach (NavSqlIndexAndChanges indexesAndChange in tableAndChange.IndexesAndChanges)
		{
			if ((indexesAndChange.Missing || indexesAndChange.IndexFields || indexesAndChange.UniqueChanged) && !indexesAndChange.NewKey.IsPrimaryKey && !indexesAndChange.NewKey.Clustered)
			{
				navSqlDdlCommands.CreateNonSiftIndex(indexesAndChange.NewKey, companyToken, tableAndChange.TableContext, tablePartitionMetadata);
				hashSet2.Add(indexesAndChange.NewKey);
			}
			if (indexesAndChange.SiftMissing || indexesAndChange.SumFields)
			{
				navSqlDdlCommands.CreateSiftIndex(indexesAndChange.NewKey, companyToken, tableAndChange.TableContext);
				hashSet.Add(indexesAndChange.NewKey);
			}
		}
		if (tableAndChange.CompressionChanged)
		{
			navSqlDdlCommands.AlterTableChangeCompressionType(tableAndChange.NewTable, companyToken, tableAndChange.TableContext, tableAndChange.NewTable.CompressionType);
			IEnumerable<NCLMetaKey> enumerable = tableAndChange.NewTable.Keys.Where((NCLMetaKey key) => !key.IsPrimaryKey && !key.Clustered);
			foreach (NCLMetaKey item in enumerable)
			{
				if (item.MaintainSqlIndex && !hashSet2.Contains(item))
				{
					navSqlDdlCommands.AlterNonSiftIndexChangeCompressionType(companyToken, tableAndChange.TableContext, item, tableAndChange.NewTable.CompressionType);
				}
				if (item.MaintainSumIndex && !hashSet.Contains(item))
				{
					navSqlDdlCommands.AlterSiftIndexChangeCompressionType(companyToken, tableAndChange.TableContext, item, tableAndChange.NewTable.CompressionType);
				}
			}
		}
		navSqlDdlCommands.ExecuteNonQuery();
	}

	internal static bool DataExistsInTableColumn(NavDatabase database, NCLMetaTable table, NCLMetaField field, int companyToken, NavAppTableContext tableContext)
	{
		return GetDataExists(database, table, companyToken, tableContext, (StringBuilder selectStmt, NavSqlCommand command) =>
		{
			FilterExpression filter = new UnaryFilterExpression(FilterExpressionType.NotEqual, field.EmptyValue, field.ExpressionContext);
			NavSqlWhereClauseBuilder.AppendFilterClause(selectStmt, filter, new NavSqlParameterHandler(), database.SqlSortingProperties, null);
			NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter, command.Command.Parameters, new NavSqlParameterHandler(), database.SqlSortingProperties, null);
		});
	}

	private static bool DataExistsInTable(NavDatabase database, NCLMetaTable table, int companyToken, NavAppTableContext tableContext)
	{
		return GetDataExists(database, table, companyToken, tableContext, null);
	}

	private static bool GetDataExists(NavDatabase database, NCLMetaTable table, int companyToken, NavAppTableContext tableContext, Action<StringBuilder, NavSqlCommand> filtersToAppend)
	{
		if (table.TableId == 2000000079)
		{
			return false;
		}
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		StringBuilder stringBuilder = new StringBuilder("SELECT TOP 1 0 FROM ");
		AppendTableOrTempName(stringBuilder, database, table, companyToken, tempName: false, tableContext);
		filtersToAppend?.Invoke(stringBuilder, navSqlCommand);
		NavSqlStatementHelper.AppendSelectStatementOptions(stringBuilder, addFastOption: false);
		return navSqlCommand.ExecuteScalar(stringBuilder.ToString()) != null;
	}

	private static bool DataExistsInMoreThanOneCompany(NavDatabase database, NCLMetaTable table, out Dictionary<string, string> tenantAndCompaniesWithData)
	{
		string name = NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.CompanyName].Name;
		string name2 = NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.CompanyId].Name;
		string name3 = NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.TenantLocalId].Name;
		string text = NavSqlTenantProperties.FieldNames[4];
		string text2 = NavSqlTenantProperties.FieldNames[0];
		StringBuilder stringBuilder = new StringBuilder();
		AppendTableOrTempName(stringBuilder, database, table, int.MinValue, tempName: false, NavAppTableContext.Main);
		string text3 = FormattableString.Invariant(FormattableStringFactory.Create("\r\n                SELECT TP.[{0}], A.[{1}]\r\n                    FROM {2} A\r\n                    INNER JOIN (\r\n                        SELECT [{3}] FROM {4}\r\n                        WHERE EXISTS (\r\n                            SELECT 1 FROM {5} AS T\r\n                            WHERE {6}.[{7}] = T.[{8}]\r\n                        )\r\n                    ) AS B ON A.[{9}] = B.[{10}]\r\n                INNER JOIN {11} AS TP ON TP.[{12}] = A.[{13}]", text2, name, "[dbo].[$ndo$tenantcompany]", name2, "[dbo].[$ndo$tenantcompany]", stringBuilder, "[dbo].[$ndo$tenantcompany]", name2, "$companyId", name2, name2, "[dbo].[$ndo$tenantproperty]", text, name3));
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.SetCommandText(text3);
		using (navSqlCommand.ExecuteReader())
		{
			tenantAndCompaniesWithData = new Dictionary<string, string>();
			bool result = false;
			while (navSqlCommand.ReadNextRow())
			{
				string key = navSqlCommand.Reader.GetString(0);
				string text4 = navSqlCommand.Reader.GetString(1);
				if (!tenantAndCompaniesWithData.TryGetValue(key, out var value))
				{
					tenantAndCompaniesWithData.Add(key, text4);
					continue;
				}
				tenantAndCompaniesWithData[key] = value + ", " + text4;
				result = true;
			}
			return result;
		}
	}

	public bool IsCreatingIndexOnline()
	{
		return CommandText.ToString().Contains("ONLINE=ON");
	}

	private static void SetEnableChangeTrackingForTableCommand(CloudReadyReplicationStatus cloudReadyReplicationStatus, NavSqlConnectionScope conScope)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000026R", Category.Runtime, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Enabling Change tracking for cloud ready in -- TableId:{0}; Name:{1}", cloudReadyReplicationStatus.TableId, cloudReadyReplicationStatus.TableName);
		try
		{
			if (TableExists(cloudReadyReplicationStatus, conScope))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("IF NOT EXISTS(SELECT * FROM sys.change_tracking_tables WHERE object_id = Object_ID('");
				stringBuilder.Append(GetCloudReadyTableNameWithDatabaseSchema(cloudReadyReplicationStatus.TableName));
				stringBuilder.Append("')) ALTER TABLE ");
				stringBuilder.Append(GetCloudReadyTableNameWithDatabaseSchema(cloudReadyReplicationStatus.TableName));
				stringBuilder.Append(" ENABLE CHANGE_TRACKING");
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
				navSqlCommand.SetCommandText(stringBuilder.ToString());
				navSqlCommand.ExecuteNonQuery();
				return;
			}
		}
		catch (Exception exception)
		{
			conScope.Connection.Database.Tenant.Diagnostics.SendExceptionTag("000024R", Category.Sql, exception);
			throw;
		}
	}

	public static void SetDisableChangeTrackingForTableCommand(CloudReadyReplicationStatus cloudReadyReplicationStatus, NavSqlConnectionScope conScope)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007KT", Category.Runtime, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Disabling Change tracking for cloud ready in -- TableId:{0}; Name:{1}", cloudReadyReplicationStatus.TableId, cloudReadyReplicationStatus.TableName);
		try
		{
			if (TableExists(cloudReadyReplicationStatus, conScope))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("IF EXISTS(SELECT * FROM sys.change_tracking_tables WHERE object_id = Object_ID('");
				stringBuilder.Append(GetCloudReadyTableNameWithDatabaseSchema(cloudReadyReplicationStatus.TableName));
				stringBuilder.Append("')) ALTER TABLE ");
				stringBuilder.Append(GetCloudReadyTableNameWithDatabaseSchema(cloudReadyReplicationStatus.TableName));
				stringBuilder.Append(" DISABLE CHANGE_TRACKING");
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
				navSqlCommand.SetCommandText(stringBuilder.ToString());
				navSqlCommand.ExecuteNonQuery();
				return;
			}
		}
		catch (Exception exception)
		{
			conScope.Connection.Database.Tenant.Diagnostics.SendExceptionTag("000025R", Category.Sql, exception);
		}
	}

	private static bool TableExists(CloudReadyReplicationStatus cloudReadyReplicationStatus, NavSqlConnectionScope conScope)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("IF Object_ID('");
		stringBuilder.Append(GetCloudReadyTableNameWithDatabaseSchema(cloudReadyReplicationStatus.TableName));
		stringBuilder.Append("', 'U') is not null SELECT 1 ELSE SELECT 0");
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		return navSqlCommand.ExecuteScalar<int>() == 1;
	}

	private static string GetCloudReadyTableNameWithDatabaseSchema(string tableName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("dbo");
		stringBuilder.Append('.');
		stringBuilder.Append("\"");
		stringBuilder.Append(tableName);
		stringBuilder.Append("\"");
		return stringBuilder.ToString();
	}

	public static void EnableChangeTrackingForDatabase(NavDatabase database, NavSqlConnectionScope conScope, bool force = false)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000028R", Category.Runtime, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Enabling Change tracking for cloud ready in -- Databasename:{0}", database.DatabaseName);
		StringBuilder stringBuilder = new StringBuilder();
		if (!force)
		{
			stringBuilder.Append("IF NOT EXISTS(SELECT * FROM sys.change_tracking_databases t JOIN sys.databases d ON d.database_id = t.database_id WHERE d.name = '");
			stringBuilder.Append(database.DatabaseName);
			stringBuilder.Append("') ");
		}
		stringBuilder.Append("ALTER DATABASE [");
		stringBuilder.Append(database.DatabaseName);
		stringBuilder.Append("] SET CHANGE_TRACKING = ON (CHANGE_RETENTION = ");
		stringBuilder.Append("10 DAYS");
		stringBuilder.Append(", AUTO_CLEANUP = ON)");
		conScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
	}

	public static void DisableChangeTrackingForTable(CloudReadyReplicationStatus cloudReadyReplicationStatus, NavSqlConnectionScope conScope)
	{
		if (NavEnvironment.Topology.IsServiceRunningInLocalEnvironment)
		{
			SetDisableChangeTrackingForTableCommand(cloudReadyReplicationStatus, conScope);
		}
	}

	public static void EnableChangeTrackingForTable(CloudReadyReplicationStatus cloudReadyReplicationStatus, NavSqlConnectionScope conScope)
	{
		if (NavEnvironment.Topology.IsServiceRunningInLocalEnvironment)
		{
			SetEnableChangeTrackingForTableCommand(cloudReadyReplicationStatus, conScope);
		}
	}

	private static void DeleteIntelligentCloudStatus(NavDatabase database, NCLMetaTable table, int companyToken, NavAppTableContext tableContext)
	{
		if (!database.IsSharedTenantSchema && table.ReplicateData && table.DataPerCompany)
		{
			string companyNameFromToken = database.GetCompanyNameFromToken(companyToken);
			string tableName = NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), table, database, companyToken, tableContext).ToString();
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
			NavSqlCloudReadyReplicationStatus.Delete(conScope, companyNameFromToken, tableName);
		}
	}

	private static void RenameIntelligentCloudStatus(NavDatabase database, NCLMetaTable oldTable, int oldCompanyToken, NCLMetaTable newTable, int newCompanyToken, NavAppTableContext tableContext)
	{
		if (database.IsSharedTenantSchema || !newTable.ReplicateData)
		{
			return;
		}
		string companyNameFromToken = database.GetCompanyNameFromToken(oldCompanyToken);
		string companyNameFromToken2 = database.GetCompanyNameFromToken(newCompanyToken);
		string tableName = NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), oldTable, database, oldCompanyToken, tableContext).ToString();
		string tableName2 = NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), newTable, database, newCompanyToken, tableContext).ToString();
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		CloudReadyReplicationStatus cloudReadyReplicationStatus = NavSqlCloudReadyReplicationStatus.GetCloudReadyReplicationStatus(conScope, tableName, companyNameFromToken);
		if (cloudReadyReplicationStatus != null)
		{
			NavSqlCloudReadyReplicationStatus.Delete(conScope, companyNameFromToken, tableName);
			cloudReadyReplicationStatus.TableName = tableName2;
			cloudReadyReplicationStatus.CompanyName = companyNameFromToken2;
			NavSqlCloudReadyReplicationStatus.Insert(conScope, cloudReadyReplicationStatus);
		}
	}

	public static void SetupTableForIntelligentCloud(NavDatabase database, NCLMetaTable table, int companyToken, NavAppTableContext tableContext)
	{
		if (database.IsSharedTenantSchema || !table.ReplicateData || !table.DataPerCompany)
		{
			return;
		}
		string companyNameFromToken = database.GetCompanyNameFromToken(companyToken);
		string tableName = NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), table, database, companyToken, tableContext).ToString();
		CloudReadyReplicationStatus cloudReadyReplicationStatus = new CloudReadyReplicationStatus
		{
			TableName = tableName,
			CompanyName = companyNameFromToken,
			CompanyId = companyToken,
			TableId = table.TableId
		};
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		NavSqlCloudReadyReplicationStatus.Insert(conScope, cloudReadyReplicationStatus);
		if (database.Tenant.IsIntelligentCloudReplicationEnabled)
		{
			EnableChangeTrackingForTable(cloudReadyReplicationStatus, conScope);
		}
	}
}

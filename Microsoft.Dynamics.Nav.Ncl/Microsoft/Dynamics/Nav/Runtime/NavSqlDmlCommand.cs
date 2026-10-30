using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlDmlCommand : NavSqlCommand
{
	private enum DmlCommandType
	{
		insert,
		insertRecordSet,
		modify,
		modifyWithTimestampCheck,
		modifyAll,
		delete,
		deleteWithTimestampCheck,
		deleteAll,
		modifyWithTimestampEqualsCheck
	}

	private class DmlCommandCacheKey : NavSqlCollationSensitiveCommandCacheKey
	{
		private FiltersAndMarks filter;

		private readonly int companyIndex;

		public DmlCommandType DmlType { get; private set; }

		public NCLMetaTable Table { get; private set; }

		public FieldList ModifiedFields { get; private set; }

		public bool IsIdentityInsert { get; private set; }

		public bool RetrieveTimestamp { get; private set; }

		public NavAppTableContext TableContext { get; private set; }

		public DmlCommandCacheKey(DmlCommandType dmlType, NCLMetaTable table, NavDatabase database, int companyIndex, FiltersAndMarks filter, FieldList modifiedFields, bool isIdentityInsert, bool retrieveTimestamp, int hashCode, NavAppTableContext tableContext)
			: base(CachedCommandType.Dml, database, hashCode)
		{
			DmlType = dmlType;
			Table = table;
			this.companyIndex = companyIndex;
			this.filter = filter;
			ModifiedFields = modifiedFields;
			IsIdentityInsert = isIdentityInsert;
			TableContext = tableContext ?? NavAppTableContext.Main;
			RetrieveTimestamp = retrieveTimestamp && TableContext.TableType == NavAppTableContextType.Main;
		}

		public DmlCommandCacheKey(DmlCommandType dmlType, NCLMetaTable table, NavDatabase database, int companyIndex, FiltersAndMarks filter, NavAppTableContext tableContext)
			: this(dmlType, table, database, companyIndex, filter, null, isIdentityInsert: false, retrieveTimestamp: false, (table.TableId % 20000 << 8) | ((int)dmlType << 3) | (companyIndex ^ filter.GetValueInsensitiveHashCode()), tableContext)
		{
		}

		public DmlCommandCacheKey(DmlCommandType dmlType, NCLMetaTable table, NavDatabase database, int companyIndex, bool isIdentityInsert, bool retrieveTimestampy, NavAppTableContext tableContext)
			: this(dmlType, table, database, companyIndex, null, null, isIdentityInsert, retrieveTimestampy, (table.TableId % 20000 << 8) | ((int)dmlType << 3) | companyIndex, tableContext)
		{
		}

		public DmlCommandCacheKey(DmlCommandType dmlType, NCLMetaTable table, NavDatabase database, int companyIndex, FieldList modifiedFields, bool retrieveTimestampy, NavAppTableContext tableContext)
			: this(dmlType, table, database, companyIndex, null, modifiedFields, isIdentityInsert: false, retrieveTimestampy, HashCodeHelper.CombineHashCodes((table.TableId % 20000 << 8) | ((int)dmlType << 3) | companyIndex, modifiedFields.GetHashCode()), tableContext)
		{
		}

		public DmlCommandCacheKey(DmlCommandType dmlType, NCLMetaTable table, NavDatabase database, int companyIndex, FiltersAndMarks filter, FieldList modifiedFields)
			: this(dmlType, table, database, companyIndex, filter, modifiedFields, isIdentityInsert: false, retrieveTimestamp: false, HashCodeHelper.CombineHashCodes((table.TableId % 20000 << 8) | ((int)dmlType << 3) | companyIndex, filter.GetValueInsensitiveHashCode(), modifiedFields.GetHashCode()), NavAppTableContext.Main)
		{
		}

		public override bool Equals(object obj)
		{
			if (!(obj is DmlCommandCacheKey dmlCommandCacheKey))
			{
				return false;
			}
			if (!base.Equals(dmlCommandCacheKey) || DmlType != dmlCommandCacheKey.DmlType || companyIndex != dmlCommandCacheKey.companyIndex || !Table.Equals(dmlCommandCacheKey.Table) || IsIdentityInsert != dmlCommandCacheKey.IsIdentityInsert || RetrieveTimestamp != dmlCommandCacheKey.RetrieveTimestamp || TableContext.CompanionTableAppOrigin != dmlCommandCacheKey.TableContext.CompanionTableAppOrigin)
			{
				return false;
			}
			if (filter == null != (dmlCommandCacheKey.filter == null))
			{
				return false;
			}
			if (filter != null && !filter.ValueInsensitiveEquals(dmlCommandCacheKey.filter))
			{
				return false;
			}
			if (ModifiedFields == null != (dmlCommandCacheKey.ModifiedFields == null))
			{
				return false;
			}
			if (ModifiedFields != null)
			{
				if (ModifiedFields.Count != dmlCommandCacheKey.ModifiedFields.Count)
				{
					return false;
				}
				return ModifiedFields.AreAllFieldsContainedInOtherAssumingSameParent(dmlCommandCacheKey.ModifiedFields);
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(base.ToString());
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, ", DmlType {0}, Table {1}", DmlType, Table.TableName);
			if (ModifiedFields != null)
			{
				stringBuilder.Append(", ModifiedFields ");
				foreach (INavFieldMetadata modifiedField in ModifiedFields)
				{
					stringBuilder.Append(modifiedField.DescriptiveName);
					stringBuilder.Append(',');
				}
				stringBuilder.Length--;
			}
			if (filter != null)
			{
				stringBuilder.Append(", Filter ");
				stringBuilder.Append(filter.ToString());
			}
			return stringBuilder.ToString();
		}
	}

	private const string InsertStr = "INSERT INTO ";

	private const string ValuesStr = " VALUES ";

	private const string DeleteStr = "DELETE FROM ";

	private const string UpdateStr = "UPDATE ";

	private const string SetStr = " SET ";

	private const string SelectOutputStr = " SELECT ";

	private const string SetIdentityInsertStr = "SET IDENTITY_INSERT ";

	private const string SelectIdentityCastAsInt32Str = "CAST(SCOPE_IDENTITY() AS INTEGER)";

	private const string SelectIdentityCastAsInt64Str = "CAST(SCOPE_IDENTITY() AS BIGINT)";

	private const string SelectTimestampStr = "@@DBTS";

	private const string DefaultValuesStr = " DEFAULT VALUES ";

	private DmlCommandCacheKey cacheKey;

	private NavValue lastIdentityValue;

	private NavBigInteger lastTimestampValue;

	public override bool Stale => cacheKey.Table.Stale;

	private NavSqlDmlCommand(NavSqlConnection parentConnection, DmlCommandCacheKey cacheKey)
		: base(parentConnection)
	{
		this.cacheKey = cacheKey;
	}

	private static NavSqlDmlCommand Create(NavSqlConnection parentConnection, DmlCommandCacheKey cacheKey)
	{
		return new NavSqlDmlCommand(parentConnection, cacheKey);
	}

	private static IEnumerable<NCLMetaField> WritableFields(NCLMetaTable table, bool isIdentityInsert)
	{
		foreach (NCLMetaField normalField in table.NormalFields)
		{
			if (!normalField.FieldIsTimestamp && (!normalField.FieldIsAutoIncrement | isIdentityInsert) && !normalField.FieldIsUserDefinedTimestamp)
			{
				yield return normalField;
			}
		}
	}

	private static IEnumerable<NCLMetaField> WritableFieldsForSinglePhysicalTable(NCLMetaTable table, bool isIdentityInsert, NavAppTableContext tableContext)
	{
		bool isIdentityInsert2 = isIdentityInsert || tableContext.TableType == NavAppTableContextType.Companion;
		IEnumerable<NCLMetaField> fields = WritableFields(table, isIdentityInsert2);
		return FieldsForSinglePhysicalTable(fields, tableContext);
	}

	private static IEnumerable<NCLMetaField> FieldsForSinglePhysicalTable(IEnumerable<NCLMetaField> fields, NavAppTableContext tableContext)
	{
		foreach (NCLMetaField field in fields)
		{
			if (field.FieldBelongsInPhysicalTable(tableContext))
			{
				yield return field;
			}
		}
	}

	private void AppendOutputValues(StringBuilder sqlStatement)
	{
		bool flag = cacheKey.Table.AutoIncrementField != null && !cacheKey.IsIdentityInsert && cacheKey.TableContext.TableType == NavAppTableContextType.Main;
		if (!flag && !cacheKey.RetrieveTimestamp)
		{
			return;
		}
		sqlStatement.Append(" SELECT ");
		if (cacheKey.RetrieveTimestamp)
		{
			sqlStatement.Append("@@DBTS");
			sqlStatement.Append(',');
		}
		if (!Connection.Database.HasSequencedAutoIncrementField(cacheKey.Table, cacheKey.TableContext) && flag)
		{
			if (cacheKey.Table.AutoIncrementField.FieldNclType == NavNclType.NavInteger)
			{
				sqlStatement.Append("CAST(SCOPE_IDENTITY() AS INTEGER)");
			}
			else
			{
				sqlStatement.Append("CAST(SCOPE_IDENTITY() AS BIGINT)");
			}
			sqlStatement.Append(',');
		}
		sqlStatement.Length--;
	}

	private static object GetWritableSqlValue(NCLMetaField field, NavValue value)
	{
		try
		{
			return value.GetSqlWritableValue(field.SqlDataType, field.FieldDefinedLength, field.FieldIsCompressed);
		}
		catch (NavCSideException ex) when (ex.ErrorCode == 22928068)
		{
			throw new NavCSideException(22928068, string.Format(CultureInfo.CurrentCulture, Lang.BadCSideData, value.ToString(), field.FieldName, field.Parent.TableName));
		}
	}

	private int ExecuteAndRetrieveOutputParameters()
	{
		lastIdentityValue = null;
		lastTimestampValue = null;
		bool flag = cacheKey.Table.AutoIncrementField != null && !Connection.Database.HasSequencedAutoIncrementField(cacheKey.Table, cacheKey.TableContext) && !cacheKey.IsIdentityInsert && cacheKey.DmlType == DmlCommandType.insert && cacheKey.TableContext.TableType == NavAppTableContextType.Main;
		bool flag2 = (cacheKey.DmlType == DmlCommandType.modify || cacheKey.DmlType == DmlCommandType.modifyWithTimestampCheck || cacheKey.DmlType == DmlCommandType.modifyWithTimestampEqualsCheck) && cacheKey.TableContext.TableType == NavAppTableContextType.Main;
		if (!flag && !cacheKey.RetrieveTimestamp)
		{
			if (ExecuteNonQuery() == 0)
			{
				return 85132272;
			}
			return 0;
		}
		if (!flag2 && (flag ^ cacheKey.RetrieveTimestamp))
		{
			object obj = ExecuteScalar();
			if (flag)
			{
				switch (cacheKey.Table.AutoIncrementField.FieldNclType)
				{
				case NavNclType.NavInteger:
					lastIdentityValue = NavInteger.CreateFromObject(obj);
					break;
				case NavNclType.NavBigInteger:
					lastIdentityValue = NavBigInteger.CreateFromObject(obj);
					break;
				default:
					throw new NotSupportedException();
				}
			}
			if (cacheKey.RetrieveTimestamp)
			{
				byte[] buffer = (byte[])obj;
				lastTimestampValue = NavBigInteger.Create(NavSqlCommand.TimestampToInt64(buffer));
			}
			return 0;
		}
		int result = 0;
		ExecuteReader();
		if (flag2 && Reader.RecordsAffected == 0)
		{
			result = 85132272;
		}
		ReadNextRow();
		int num = 0;
		for (int i = 0; i < 2; i++)
		{
			switch (i)
			{
			case 0:
				if (cacheKey.RetrieveTimestamp)
				{
					lastTimestampValue = (NavBigInteger)CreateNavValueFromReader(cacheKey.Table.TimestampField, num);
					num++;
				}
				break;
			case 1:
				if (flag)
				{
					lastIdentityValue = CreateNavValueFromReader(cacheKey.Table.AutoIncrementField, num);
					num++;
				}
				break;
			}
		}
		CloseReader(forceClose: false);
		return result;
	}

	private int ExecuteInsert(IRecordBuffer record, NavAppTableContext tableContext, NavValue resolvedIdentityValue, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		List<NCLMetaField> list = WritableFieldsForSinglePhysicalTable(cacheKey.Table, cacheKey.IsIdentityInsert, tableContext).ToList();
		int num;
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (cacheKey.IsIdentityInsert && !Connection.Database.HasSequencedAutoIncrementField(cacheKey.Table, cacheKey.TableContext))
			{
				stringBuilder.Append("SET IDENTITY_INSERT ");
				NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, cacheKey.Table, database, companyIndex, tableContext);
				stringBuilder.Append(" ON; ");
			}
			stringBuilder.Append("INSERT INTO ");
			NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, cacheKey.Table, database, companyIndex, tableContext);
			if (!list.Any())
			{
				stringBuilder.Append(" DEFAULT VALUES ");
			}
			else
			{
				stringBuilder.Append(" (");
				NavSqlStatementHelper.AppendFieldList(stringBuilder, list, null);
				if (companyId.HasValue)
				{
					NavSqlDdlCommands.AppendCompanyIdFieldName(stringBuilder, commaPrefix: true, commaSuffix: false);
				}
				stringBuilder.Append(')');
				stringBuilder.Append(" VALUES ");
				stringBuilder.Append('(');
				num = 0;
				foreach (NCLMetaField item in list)
				{
					if ((item.FieldNclType == NavNclType.NavCode || item.FieldNclType == NavNclType.NavOemCode) && item.SqlDataType == SqlDataType.Variant)
					{
						NavSqlStatementHelper.AppendCollateForVariantParameter(stringBuilder, NavSqlStatementHelper.ParameterName(num), item, cacheKey.SqlSortingProperties.Collation);
					}
					else
					{
						stringBuilder.Append(NavSqlStatementHelper.ParameterName(num));
					}
					stringBuilder.Append(',');
					num++;
				}
				if (companyId.HasValue)
				{
					stringBuilder.Append(NavSqlStatementHelper.ParameterName(num));
				}
				else
				{
					stringBuilder.Length--;
				}
				stringBuilder.Append(')');
			}
			if (cacheKey.IsIdentityInsert && !Connection.Database.HasSequencedAutoIncrementField(cacheKey.Table, cacheKey.TableContext))
			{
				stringBuilder.Append("; ");
				stringBuilder.Append("SET IDENTITY_INSERT ");
				NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, cacheKey.Table, database, companyIndex, tableContext);
				stringBuilder.Append(" OFF ");
			}
			AppendOutputValues(stringBuilder);
			SetCommandText(stringBuilder.ToString());
		}
		num = 0;
		bool flag = Command.Parameters.Count == 0;
		foreach (NCLMetaField item2 in list)
		{
			if (flag)
			{
				SqlParameter sqlParameter = new SqlParameter();
				sqlParameter.ParameterName = NavSqlStatementHelper.ParameterName(num);
				Command.Parameters.Add(sqlParameter);
			}
			if (item2.FieldIsAutoIncrement && tableContext.TableType == NavAppTableContextType.Companion)
			{
				NavSqlStatementHelper.SetSqlParameter(Command.Parameters[num], item2, GetWritableSqlValue(item2, resolvedIdentityValue), isWildcardValue: false, isFilterValue: false);
			}
			else
			{
				NavSqlStatementHelper.SetSqlParameter(Command.Parameters[num], item2, GetWritableSqlValue(item2, record[item2.FieldIndex]), isWildcardValue: false, isFilterValue: false);
			}
			num++;
		}
		if (companyId.HasValue)
		{
			if (flag)
			{
				SqlParameter sqlParameter2 = new SqlParameter();
				sqlParameter2.ParameterName = NavSqlStatementHelper.ParameterName(num);
				Command.Parameters.Add(sqlParameter2);
			}
			SqlParameter sqlParameter3 = Command.Parameters[num];
			sqlParameter3.SqlDbType = SqlDbType.Int;
			sqlParameter3.Value = companyId.Value;
		}
		PrepareStatement();
		ExecuteAndRetrieveOutputParameters();
		return 0;
	}

	private int ExecuteInsertRecordSet(IEnumerable<ReadOnlyRecordBuffer> recordSet, int maxBufferedInsertRecords, out ReadOnlyRecordBuffer lastRecordProcessed, NavAppTableContext tableContext, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		lastRecordProcessed = null;
		List<NCLMetaField> list = WritableFieldsForSinglePhysicalTable(cacheKey.Table, cacheKey.IsIdentityInsert, tableContext).ToList();
		int num;
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder("INSERT INTO ");
			NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, cacheKey.Table, database, companyIndex, tableContext);
			stringBuilder.Append(" (");
			if (companyId.HasValue)
			{
				NavSqlDdlCommands.AppendCompanyIdFieldName(stringBuilder, commaPrefix: false, commaSuffix: true);
			}
			NavSqlStatementHelper.AppendFieldList(stringBuilder, list, null);
			stringBuilder.Append(')');
			stringBuilder.Append(" VALUES ");
			num = 0;
			for (int i = 0; i < maxBufferedInsertRecords; i++)
			{
				stringBuilder.Append('(');
				if (companyId.HasValue)
				{
					stringBuilder.Append(NavSqlStatementHelper.ParameterName(num));
					stringBuilder.Append(',');
					num++;
				}
				foreach (NCLMetaField item in list)
				{
					if ((item.FieldNclType == NavNclType.NavCode || item.FieldNclType == NavNclType.NavOemCode) && item.SqlDataType == SqlDataType.Variant)
					{
						NavSqlStatementHelper.AppendCollateForVariantParameter(stringBuilder, NavSqlStatementHelper.ParameterName(num), item, cacheKey.SqlSortingProperties.Collation);
					}
					else
					{
						stringBuilder.Append(NavSqlStatementHelper.ParameterName(num));
					}
					stringBuilder.Append(',');
					num++;
				}
				stringBuilder.Length--;
				stringBuilder.Append("),");
			}
			stringBuilder.Length--;
			AppendOutputValues(stringBuilder);
			SetCommandText(stringBuilder.ToString());
		}
		num = 0;
		bool flag = Command.Parameters.Count == 0;
		foreach (ReadOnlyRecordBuffer item2 in recordSet)
		{
			ReadOnlyRecordBuffer readOnlyRecordBuffer = (lastRecordProcessed = item2);
			if (companyId.HasValue)
			{
				if (flag)
				{
					SqlParameter sqlParameter = new SqlParameter();
					sqlParameter.ParameterName = NavSqlStatementHelper.ParameterName(num);
					Command.Parameters.Add(sqlParameter);
				}
				SqlParameter sqlParameter2 = Command.Parameters[num];
				sqlParameter2.SqlDbType = SqlDbType.Int;
				sqlParameter2.Value = companyId.Value;
				num++;
			}
			foreach (NCLMetaField item3 in list)
			{
				if (flag)
				{
					SqlParameter sqlParameter3 = new SqlParameter();
					sqlParameter3.ParameterName = NavSqlStatementHelper.ParameterName(num);
					Command.Parameters.Add(sqlParameter3);
				}
				NavSqlStatementHelper.SetSqlParameter(Command.Parameters[num], item3, GetWritableSqlValue(item3, readOnlyRecordBuffer[item3.FieldIndex]), isWildcardValue: false, isFilterValue: false);
				num++;
			}
		}
		PrepareStatement();
		ExecuteAndRetrieveOutputParameters();
		return 0;
	}

	private int ExecuteDelete(FilterExpression filter, NavAppTableContext tableContext, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder("DELETE FROM ");
			NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, cacheKey.Table, database, companyIndex, tableContext);
			NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, filter, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId);
			SetCommandText(stringBuilder.ToString());
		}
		NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter, Command.Parameters, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId);
		PrepareStatement();
		if (ExecuteNonQuery() == 0)
		{
			return 85132272;
		}
		return 0;
	}

	private int ExecuteModify(FilterExpression modifyFilter, MutableRecordBuffer record, NavAppTableContext tableContext, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		List<NCLMetaField> list = (from f in cacheKey.ModifiedFields
			select (NCLMetaField)f into f
			where f.FieldBelongsInPhysicalTable(tableContext)
			select f).ToList();
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder("UPDATE ");
			NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, cacheKey.Table, database, companyIndex, tableContext);
			stringBuilder.Append(" SET ");
			for (int num = 0; num < list.Count; num++)
			{
				NCLMetaField nCLMetaField = list[num];
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, nCLMetaField.SqlColumnName);
				stringBuilder.Append('=');
				if ((nCLMetaField.FieldNclType == NavNclType.NavCode || nCLMetaField.FieldNclType == NavNclType.NavOemCode) && nCLMetaField.SqlDataType == SqlDataType.Variant)
				{
					NavSqlStatementHelper.AppendCollateForVariantParameter(stringBuilder, NavSqlStatementHelper.ParameterName(num), nCLMetaField, cacheKey.SqlSortingProperties.Collation);
				}
				else
				{
					stringBuilder.Append(NavSqlStatementHelper.ParameterName(num));
				}
				stringBuilder.Append(',');
			}
			stringBuilder.Length--;
			NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, modifyFilter, new NavSqlParameterHandler(list.Count), cacheKey.SqlSortingProperties, companyId);
			AppendOutputValues(stringBuilder);
			SetCommandText(stringBuilder.ToString());
		}
		bool flag = Command.Parameters.Count == 0;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			NCLMetaField nCLMetaField2 = list[num2];
			SqlParameter sqlParameter;
			if (flag)
			{
				sqlParameter = new SqlParameter();
				sqlParameter.ParameterName = NavSqlStatementHelper.ParameterName(num2);
				Command.Parameters.Add(sqlParameter);
			}
			else
			{
				sqlParameter = Command.Parameters[num2];
			}
			NavSqlStatementHelper.SetSqlParameter(sqlParameter, nCLMetaField2, GetWritableSqlValue(nCLMetaField2, record[nCLMetaField2.FieldIndex]), isWildcardValue: false, isFilterValue: false);
		}
		NavSqlWhereClauseBuilder.AddOrSetFilterParameters(modifyFilter, Command.Parameters, new NavSqlParameterHandler(list.Count), cacheKey.SqlSortingProperties, companyId);
		PrepareStatement();
		return ExecuteAndRetrieveOutputParameters();
	}

	private int ExecuteDeleteMultiple(FiltersAndMarks filter, NavAppTableContext tableContext, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder("DELETE FROM ");
			NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, cacheKey.Table, database, companyIndex, tableContext);
			NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, filter.GetCombinedFilter(), new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId);
			SetCommandText(stringBuilder.ToString());
		}
		NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter.GetCombinedFilter(), Command.Parameters, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId);
		PrepareStatement();
		ExecuteNonQuery();
		return 0;
	}

	private int ExecuteModifyMultiple(FieldDictionary<NavValue> modifiedFields, FiltersAndMarks filter, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder("UPDATE ");
			NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, cacheKey.Table, database, companyIndex, NavAppTableContext.Main);
			stringBuilder.Append(" SET ");
			for (int i = 0; i < cacheKey.ModifiedFields.Count; i++)
			{
				NCLMetaField nCLMetaField = (NCLMetaField)cacheKey.ModifiedFields[i];
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, nCLMetaField.SqlColumnName);
				stringBuilder.Append('=');
				stringBuilder.Append(NavSqlStatementHelper.ParameterName(i));
				stringBuilder.Append(',');
			}
			stringBuilder.Length--;
			NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, filter.GetCombinedFilter(), new NavSqlParameterHandler(cacheKey.ModifiedFields.Count), cacheKey.SqlSortingProperties, companyId);
			SetCommandText(stringBuilder.ToString());
		}
		bool flag = Command.Parameters.Count == 0;
		for (int j = 0; j < cacheKey.ModifiedFields.Count; j++)
		{
			NCLMetaField nCLMetaField2 = (NCLMetaField)cacheKey.ModifiedFields[j];
			SqlParameter sqlParameter;
			if (flag)
			{
				sqlParameter = new SqlParameter();
				sqlParameter.ParameterName = NavSqlStatementHelper.ParameterName(j);
				Command.Parameters.Add(sqlParameter);
			}
			else
			{
				sqlParameter = Command.Parameters[j];
			}
			NavSqlStatementHelper.SetSqlParameter(sqlParameter, nCLMetaField2, GetWritableSqlValue(nCLMetaField2, modifiedFields[nCLMetaField2]), isWildcardValue: false, isFilterValue: false);
		}
		NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter.GetCombinedFilter(), Command.Parameters, new NavSqlParameterHandler(cacheKey.ModifiedFields.Count), cacheKey.SqlSortingProperties, companyId);
		PrepareStatement();
		ExecuteNonQuery();
		return 0;
	}

	internal static InsertResult Insert(NavDatabase database, NCLMetaTable table, int companyToken, IRecordBuffer recordBuffer, ref NavValue identityValue, out NavBigInteger timestampValue, bool throwOnDuplicate = false)
	{
		try
		{
			int num = Insert(database, table, companyToken, recordBuffer, retrieveTimestamp: true, ref identityValue, out timestampValue);
			if (num == 0)
			{
				return InsertResult.Success;
			}
			throw new NavCSideException(num);
		}
		catch (NavCSideDuplicateKeyException)
		{
			if (!database.HasSequencedAutoIncrementField(table, NavAppTableContext.Main))
			{
				identityValue = null;
			}
			timestampValue = null;
			if (throwOnDuplicate)
			{
				throw;
			}
			return InsertResult.RecordAlreadyExists;
		}
	}

	internal static int Insert(NavDatabase database, NCLMetaTable table, int companyIndex, IRecordBuffer record, bool retrieveTimestamp, ref NavValue identityValue, out NavBigInteger timestampValue)
	{
		database.CheckTableInSync(table);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, TransactionType.UpdateNoLocks);
		try
		{
			bool isIdentityInsert = false;
			NCLMetaField autoIncrementField = table.AutoIncrementField;
			if (autoIncrementField != null)
			{
				isIdentityInsert = autoIncrementField.FieldNclType switch
				{
					NavNclType.NavInteger => ((NavInteger)record[autoIncrementField.FieldIndex]).Value != 0, 
					NavNclType.NavBigInteger => ((NavBigInteger)record[autoIncrementField.FieldIndex]).Value != 0, 
					_ => throw new NotSupportedException(), 
				};
			}
			NavSqlDmlCommand dmlCommand = null;
			DmlCommandCacheKey commandCacheKey = new DmlCommandCacheKey(DmlCommandType.insert, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), isIdentityInsert, retrieveTimestamp, NavAppTableContext.Main);
			int num = conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) =>
			{
				dmlCommand = (NavSqlDmlCommand)command;
				return dmlCommand.ExecuteInsert(record, NavAppTableContext.Main, null, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex));
			});
			if (identityValue == null)
			{
				if (!database.HasSequencedAutoIncrementField(table, NavAppTableContext.Main) && dmlCommand.lastIdentityValue != null)
				{
					identityValue = dmlCommand.lastIdentityValue;
				}
				else if (autoIncrementField != null)
				{
					identityValue = record[autoIncrementField.FieldIndex];
				}
			}
			timestampValue = dmlCommand.lastTimestampValue;
			if (num == 0)
			{
				int num2 = DoInsertForCompanionTables(database, table, companyIndex, record, conScope, identityValue);
				if (num2 != 0)
				{
					return num2;
				}
				if (retrieveTimestamp)
				{
					conScope.Connection.AddKnownXLockedTimestamp(timestampValue.ToInt64());
				}
			}
			return num;
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}

	private static int DoInsertForCompanionTables(NavDatabase database, NCLMetaTable table, int companyIndex, IRecordBuffer record, NavSqlConnectionScope conScope, NavValue identityValue)
	{
		foreach (NavAppOrigin distinctCompanionTableOrigin in table.DistinctCompanionTableOrigins)
		{
			NavAppTableContext companionTableContext = NavAppTableContext.Companion(distinctCompanionTableOrigin);
			DmlCommandCacheKey extensionCommandCacheKey = new DmlCommandCacheKey(DmlCommandType.insert, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), isIdentityInsert: false, retrieveTimestampy: false, companionTableContext);
			int num = conScope.Connection.ExecuteCommandAndCacheOnSuccess(extensionCommandCacheKey, () => Create(conScope.Connection, extensionCommandCacheKey), (NavSqlCommand command) =>
			{
				NavSqlDmlCommand navSqlDmlCommand = (NavSqlDmlCommand)command;
				return navSqlDmlCommand.ExecuteInsert(record, companionTableContext, identityValue, database, companyIndex, database.GetCompanyId(extensionCommandCacheKey.Table, companyIndex));
			});
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	internal static int InsertRecordSet(NavDatabase database, NCLMetaTable table, int companyIndex, ICollection<ReadOnlyRecordBuffer> recordSet, bool retrieveTimestamp, out NavBigInteger timestampValue)
	{
		if (recordSet.Count >= SqlTableDataProvider.BufferedInsertRowCount(table))
		{
			ReadOnlyRecordBuffer mainRecordInError = null;
			ReadOnlyRecordBuffer lastRecordProcessed = null;
			try
			{
				NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, TransactionType.UpdateNoLocks);
				try
				{
					int maxBufferedInsertRecords = SqlTableDataProvider.BufferedInsertRowCount(table);
					NavSqlDmlCommand dmlCommand = null;
					DmlCommandCacheKey commandCacheKey = new DmlCommandCacheKey(DmlCommandType.insertRecordSet, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), isIdentityInsert: false, retrieveTimestamp, NavAppTableContext.Main);
					int result = conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) =>
					{
						dmlCommand = (NavSqlDmlCommand)command;
						return dmlCommand.ExecuteInsertRecordSet(recordSet, maxBufferedInsertRecords, out mainRecordInError, NavAppTableContext.Main, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex));
					});
					timestampValue = dmlCommand.lastTimestampValue;
					DoInsertRecordSetForCompanionTables(recordSet, maxBufferedInsertRecords, out lastRecordProcessed, database, table, companyIndex, conScope);
					return result;
				}
				finally
				{
					if (conScope != null)
					{
						((IDisposable)conScope).Dispose();
					}
				}
			}
			catch (NavCSideDuplicateKeyException)
			{
			}
			catch (NavCSideException originalException)
			{
				ReadOnlyRecordBuffer recordBuffer = lastRecordProcessed ?? mainRecordInError;
				RecordImplementationHelper.MapException(originalException, database, companyIndex, table, recordBuffer, makeNewExceptionRemappable: false);
				throw;
			}
		}
		timestampValue = null;
		NavValue identityValue = null;
		foreach (ReadOnlyRecordBuffer item in recordSet)
		{
			try
			{
				int num = Insert(database, table, companyIndex, item, retrieveTimestamp, ref identityValue, out timestampValue);
				if (num != 0)
				{
					return num;
				}
			}
			catch (NavCSideException originalException2)
			{
				RecordImplementationHelper.MapException(originalException2, database, companyIndex, table, item, makeNewExceptionRemappable: false);
				throw;
			}
		}
		return 0;
	}

	private static int DoInsertRecordSetForCompanionTables(IEnumerable<ReadOnlyRecordBuffer> recordSet, int maxBufferedInsertRecords, out ReadOnlyRecordBuffer lastRecordProcessed, NavDatabase database, NCLMetaTable table, int companyIndex, NavSqlConnectionScope conScope)
	{
		lastRecordProcessed = null;
		foreach (NavAppOrigin distinctCompanionTableOrigin in table.DistinctCompanionTableOrigins)
		{
			ReadOnlyRecordBuffer lastCompanionRecordProcessed = null;
			NavAppTableContext companionTableContext = NavAppTableContext.Companion(distinctCompanionTableOrigin);
			DmlCommandCacheKey extensionCommandCacheKey = new DmlCommandCacheKey(DmlCommandType.insertRecordSet, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), isIdentityInsert: false, retrieveTimestampy: false, companionTableContext);
			conScope.Connection.ExecuteCommandAndCacheOnSuccess(extensionCommandCacheKey, () => Create(conScope.Connection, extensionCommandCacheKey), (NavSqlCommand command) =>
			{
				NavSqlDmlCommand navSqlDmlCommand = (NavSqlDmlCommand)command;
				return navSqlDmlCommand.ExecuteInsertRecordSet(recordSet, maxBufferedInsertRecords, out lastCompanionRecordProcessed, companionTableContext, database, companyIndex, database.GetCompanyId(extensionCommandCacheKey.Table, companyIndex));
			});
			lastRecordProcessed = lastCompanionRecordProcessed;
		}
		return 0;
	}

	internal static DeleteResult Delete(NavDatabase database, NCLMetaTable table, int companyToken, MutableRecordBuffer recordBuffer, bool recordConsistentWithCurrentTransaction)
	{
		int num = DeleteImpl(database, table, companyToken, recordBuffer, recordConsistentWithCurrentTransaction);
		return num switch
		{
			0 => DeleteResult.Success, 
			85132272 => DeleteResult.RecordNotFound, 
			85132286 => DeleteResult.RecordStale, 
			_ => throw new NavCSideException(num), 
		};
	}

	private static int DeleteImpl(NavDatabase database, NCLMetaTable table, int companyIndex, MutableRecordBuffer record, bool recordConsistentWithCurrentTransaction)
	{
		database.CheckTableInSync(table);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, TransactionType.UpdateNoLocks);
		try
		{
			DmlCommandType dmlCommandType = DmlCommandType.delete;
			if (!recordConsistentWithCurrentTransaction)
			{
				NavValue navValue = record[table.TimestampField.FieldIndex];
				if (navValue != null && navValue.ToInt64() != 0L)
				{
					dmlCommandType = DmlCommandType.deleteWithTimestampCheck;
				}
			}
			FilterExpression mainFilter;
			if (dmlCommandType == DmlCommandType.deleteWithTimestampCheck)
			{
				mainFilter = NavSqlWhereClauseBuilder.CreateRecordClauseWithTimestampCheck(table, FilterExpressionType.Equal, record, table.PrimaryKey.SortingFieldsWithPrimaryKeyFields, strictEqualityTimestampCheck: false);
			}
			else
			{
				mainFilter = NavSqlWhereClauseBuilder.CreateRecordClause(FilterExpressionType.Equal, record, table.PrimaryKey.SortingFieldsWithPrimaryKeyFields, useOriginalValues: true);
			}
			DmlCommandCacheKey commandCacheKey = new DmlCommandCacheKey(dmlCommandType, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), isIdentityInsert: false, retrieveTimestampy: false, NavAppTableContext.Main);
			int num = conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlDmlCommand)command).ExecuteDelete(mainFilter, NavAppTableContext.Main, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex)));
			if (num == 85132272 && dmlCommandType == DmlCommandType.deleteWithTimestampCheck)
			{
				if (NavSqlExistsCommand.RecordExists(database, table, companyIndex, record, locking: true) == 0)
				{
					return 85132286;
				}
				return 85132272;
			}
			if (num == 0)
			{
				int num2 = DoDeleteForCompanionTables(database, table, record, companyIndex, conScope);
				if (num2 != 0)
				{
					return num2;
				}
				if (dmlCommandType == DmlCommandType.deleteWithTimestampCheck)
				{
					conScope.Connection.RemoveKnownXLockedTimestamp(record[table.TimestampField.FieldIndex].ToInt64());
				}
				else
				{
					conScope.Connection.ClearKnownXLockedTimestamps();
				}
			}
			return num;
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}

	private static int DoDeleteForCompanionTables(NavDatabase database, NCLMetaTable table, MutableRecordBuffer record, int companyIndex, NavSqlConnectionScope conScope)
	{
		FilterExpression companionFilter = NavSqlWhereClauseBuilder.CreateRecordClause(FilterExpressionType.Equal, record, table.PrimaryKey.SortingFieldsWithPrimaryKeyFields, useOriginalValues: true);
		foreach (NavAppOrigin distinctCompanionTableOrigin in table.DistinctCompanionTableOrigins)
		{
			NavAppTableContext companionTableContext = NavAppTableContext.Companion(distinctCompanionTableOrigin);
			DmlCommandCacheKey extensionCommandCacheKey = new DmlCommandCacheKey(DmlCommandType.delete, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), isIdentityInsert: false, retrieveTimestampy: false, companionTableContext);
			int num = conScope.Connection.ExecuteCommandAndCacheOnSuccess(extensionCommandCacheKey, () => Create(conScope.Connection, extensionCommandCacheKey), (NavSqlCommand command) =>
			{
				NavSqlDmlCommand navSqlDmlCommand = (NavSqlDmlCommand)command;
				return navSqlDmlCommand.ExecuteDelete(companionFilter, companionTableContext, database, companyIndex, database.GetCompanyId(extensionCommandCacheKey.Table, companyIndex));
			});
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	internal static ModifyResult Modify(NavDatabase database, int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction, out NavBigInteger timestampValue, bool throwOnDuplicate = false)
	{
		try
		{
			int num = Modify(database, recordBuffer.MetaTable, companyToken, recordBuffer, retrieveTimestamp: true, recordHasBeenReadWithLockInsideThisTransaction, out timestampValue);
			return num switch
			{
				0 => ModifyResult.Success, 
				85132286 => ModifyResult.RecordStale, 
				85132272 => ModifyResult.RecordNotFound, 
				_ => throw new NavCSideException(num), 
			};
		}
		catch (NavCSideDuplicateKeyException)
		{
			timestampValue = null;
			if (throwOnDuplicate)
			{
				throw;
			}
			return ModifyResult.RecordAlreadyExists;
		}
	}

	private static int ModifyImpl(NavDatabase database, NCLMetaTable table, int companyIndex, MutableRecordBuffer record, FieldList modifiedFields, bool retrieveTimestamp, out NavBigInteger timestampValue, bool recordConsistentWithCurrentTransaction)
	{
		database.CheckTableInSync(table);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, TransactionType.UpdateNoLocks);
		try
		{
			DmlCommandType dmlCommandType = DmlCommandType.modify;
			if (!recordConsistentWithCurrentTransaction && table.SqlHasTimestamp)
			{
				NavValue navValue = record[table.TimestampField.FieldIndex];
				if (navValue != null && navValue.ToInt64() != 0L)
				{
					dmlCommandType = DmlCommandType.modifyWithTimestampEqualsCheck;
				}
			}
			if (table.AutoIncrementField != null && record.GetChangedFieldValue(table.AutoIncrementField.FieldIndex) != null && !record.ReadOnlyBuffer[table.AutoIncrementField.FieldIndex].Equals(record.GetChangedFieldValue(table.AutoIncrementField.FieldIndex)))
			{
				timestampValue = null;
				return 18023796;
			}
			if (modifiedFields.Count == 0)
			{
				if (dmlCommandType == DmlCommandType.modifyWithTimestampCheck || dmlCommandType == DmlCommandType.modifyWithTimestampEqualsCheck)
				{
					timestampValue = (NavBigInteger)record[table.TimestampField.FieldIndex];
					if (conScope.Connection.IsKnownXLockedTimestampsIsKnownLatestTimestamp(timestampValue.Value))
					{
						return 0;
					}
					FilterExpression filter = FilterExpression.And(NavSqlWhereClauseBuilder.BuildKeyFilterFromRecord(record, table.PrimaryKey.DefinedSortingFields, FilterExpressionType.Equal, useOriginalValues: false), NavSqlWhereClauseBuilder.BuildTimestampCheckFilter(table, record, strictEqualityTimestampCheck: true, useOriginalValue: false));
					DiagnosticsResolver.GetMostSpecificInstance(NavCurrentThread.Session).SendTraceTag("000093G", Category.AL, Verbosity.Normal, PrivacyClassification.SystemMetadata, "MODIFY called with no modified fields." + Environment.NewLine + SessionContextHelper.GetALCallStack());
					int num = NavSqlExistsCommand.Exists(database, table, companyIndex, filter, locking: true, null, withXLock: true);
					switch (num)
					{
					case 0:
						conScope.Connection.AddKnownXLockedTimestamp(timestampValue.Value);
						break;
					case 85132272:
						num = 85132286;
						break;
					}
					return num;
				}
				NCLMetaField fieldForUpdateWhenNoChanges = table.FieldForUpdateWhenNoChanges;
				if (fieldForUpdateWhenNoChanges == null)
				{
					timestampValue = null;
					return 18023796;
				}
				modifiedFields = new FieldList(fieldForUpdateWhenNoChanges);
			}
			if (modifiedFields.Count > 0 && modifiedFields.AsNclMetaFields().All((NCLMetaField f) => f.IsCompanionTableField))
			{
				INavFieldMetadata fieldForUpdateWhenNoChanges2 = table.FieldForUpdateWhenNoChanges;
				if (fieldForUpdateWhenNoChanges2 == null)
				{
					timestampValue = null;
					return 18023796;
				}
				INavFieldMetadata[] fields = new INavFieldMetadata[1] { fieldForUpdateWhenNoChanges2 }.Concat(modifiedFields).ToArray();
				modifiedFields = new FieldList(fields);
			}
			FilterExpression modifyFilter;
			if (dmlCommandType == DmlCommandType.modifyWithTimestampCheck || dmlCommandType == DmlCommandType.modifyWithTimestampEqualsCheck)
			{
				modifyFilter = NavSqlWhereClauseBuilder.CreateRecordClauseWithTimestampCheck(table, FilterExpressionType.Equal, record, table.PrimaryKey.SortingFieldsWithPrimaryKeyFields, dmlCommandType == DmlCommandType.modifyWithTimestampEqualsCheck);
			}
			else
			{
				modifyFilter = NavSqlWhereClauseBuilder.CreateRecordClause(FilterExpressionType.Equal, record, table.PrimaryKey.SortingFieldsWithPrimaryKeyFields, useOriginalValues: true);
			}
			NavSqlDmlCommand dmlCommand = null;
			DmlCommandCacheKey commandCacheKey = new DmlCommandCacheKey(dmlCommandType, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), modifiedFields, retrieveTimestamp, NavAppTableContext.Main);
			int num2 = conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) =>
			{
				dmlCommand = (NavSqlDmlCommand)command;
				return dmlCommand.ExecuteModify(modifyFilter, record, NavAppTableContext.Main, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex));
			});
			timestampValue = dmlCommand.lastTimestampValue;
			if (num2 == 85132272 && (dmlCommandType == DmlCommandType.modifyWithTimestampCheck || dmlCommandType == DmlCommandType.modifyWithTimestampEqualsCheck))
			{
				return 85132286;
			}
			if (num2 == 0)
			{
				int num3 = DoModifyForCompanionTables(database, table, companyIndex, record, modifiedFields, conScope);
				if (num3 != 0)
				{
					return num3;
				}
				if (dmlCommandType == DmlCommandType.modifyWithTimestampCheck || dmlCommandType == DmlCommandType.modifyWithTimestampEqualsCheck)
				{
					conScope.Connection.RemoveKnownXLockedTimestamp(record[table.TimestampField.FieldIndex].ToInt64());
				}
				else
				{
					conScope.Connection.ClearKnownXLockedTimestamps();
				}
				if (retrieveTimestamp)
				{
					conScope.Connection.AddKnownXLockedTimestamp(timestampValue);
				}
			}
			return num2;
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}

	private static int DoModifyForCompanionTables(NavDatabase database, NCLMetaTable table, int companyIndex, MutableRecordBuffer record, FieldList modifiedFields, NavSqlConnectionScope conScope)
	{
		FilterExpression companionFilter = NavSqlWhereClauseBuilder.CreateRecordClause(FilterExpressionType.Equal, record, table.PrimaryKey.SortingFieldsWithPrimaryKeyFields, useOriginalValues: true);
		IEnumerable<NCLMetaField> source = (record.IsPrimaryKeyDirty ? table.NormalFields : modifiedFields.AsNclMetaFields());
		List<NavAppOrigin> list = (from f in source
			where f.IsCompanionTableField
			select f.AppOrigin).Distinct().ToList();
		foreach (NavAppOrigin item in list)
		{
			NavAppTableContext companionTableContext = NavAppTableContext.Companion(item);
			DmlCommandCacheKey extensionCommandCacheKey = new DmlCommandCacheKey(DmlCommandType.modify, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), modifiedFields, retrieveTimestampy: false, companionTableContext);
			int num = conScope.Connection.ExecuteCommandAndCacheOnSuccess(extensionCommandCacheKey, () => Create(conScope.Connection, extensionCommandCacheKey), (NavSqlCommand command) =>
			{
				NavSqlDmlCommand navSqlDmlCommand = (NavSqlDmlCommand)command;
				return navSqlDmlCommand.ExecuteModify(companionFilter, record, companionTableContext, database, companyIndex, database.GetCompanyId(extensionCommandCacheKey.Table, companyIndex));
			});
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	internal static int Modify(NavDatabase database, NCLMetaTable table, int companyIndex, MutableRecordBuffer record, bool retrieveTimestamp, bool recordConsistentWithCurrentTransaction, out NavBigInteger timestampValue)
	{
		List<NCLMetaField> list = new List<NCLMetaField>();
		foreach (NCLMetaField item in WritableFields(table, isIdentityInsert: false))
		{
			NavValue changedFieldValue = record.GetChangedFieldValue(item.FieldIndex);
			if (changedFieldValue != null && (item.FieldNclType != NavNclType.NavBlob || ((NavBLOB)changedFieldValue).IsDirty))
			{
				list.Add(item);
			}
		}
		INavFieldMetadata[] fields = list.ToArray();
		return ModifyImpl(database, table, companyIndex, record, new FieldList(fields), retrieveTimestamp, out timestampValue, recordConsistentWithCurrentTransaction);
	}

	internal static void DeleteMultiple(NavDatabase database, NCLMetaTable table, int companyIndex, FiltersAndMarks filtersAndMarks, NavAppTableContext tableContext)
	{
		if (!filtersAndMarks.IsEmpty && table.NormalFieldsOwnedByCompanionTables.Any())
		{
			throw new NavNotSupportedException(Lang.CannotDeleteMultipleWithFiltersWithCompanionTables);
		}
		database.CheckTableInSync(table);
		DmlCommandCacheKey commandCacheKey = new DmlCommandCacheKey(DmlCommandType.deleteAll, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), filtersAndMarks, tableContext);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, TransactionType.UpdateNoLocks);
		try
		{
			if (conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlDmlCommand)command).ExecuteDeleteMultiple(filtersAndMarks, tableContext, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex))) == 0 && tableContext == NavAppTableContext.Main && DoDeleteMultipleForCompanionTables(database, table, companyIndex, conScope, filtersAndMarks) == 0)
			{
				conScope.Connection.ClearKnownXLockedTimestamps();
			}
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}

	private static int DoDeleteMultipleForCompanionTables(NavDatabase database, NCLMetaTable table, int companyIndex, NavSqlConnectionScope conScope, FiltersAndMarks filtersAndMarks)
	{
		foreach (NavAppOrigin distinctCompanionTableOrigin in table.DistinctCompanionTableOrigins)
		{
			NavAppTableContext companionTableContext = NavAppTableContext.Companion(distinctCompanionTableOrigin);
			DmlCommandCacheKey extensionCommandCacheKey = new DmlCommandCacheKey(DmlCommandType.deleteAll, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), filtersAndMarks, companionTableContext);
			int num = conScope.Connection.ExecuteCommandAndCacheOnSuccess(extensionCommandCacheKey, () => Create(conScope.Connection, extensionCommandCacheKey), (NavSqlCommand command) =>
			{
				NavSqlDmlCommand navSqlDmlCommand = (NavSqlDmlCommand)command;
				return navSqlDmlCommand.ExecuteDeleteMultiple(filtersAndMarks, companionTableContext, database, companyIndex, database.GetCompanyId(extensionCommandCacheKey.Table, companyIndex));
			});
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	internal static void ModifyMultiple(NavDatabase database, NCLMetaTable table, int companyIndex, FieldDictionary<NavValue> modifiedFields, FiltersAndMarks filter)
	{
		NCLMetaField[] array = new NCLMetaField[modifiedFields.Items.Length];
		for (int i = 0; i < modifiedFields.Items.Length; i++)
		{
			Tuple<INavFieldMetadata, NavValue> tuple = modifiedFields.Items[i];
			array[i] = (NCLMetaField)tuple.Item1;
		}
		INavFieldMetadata[] fields = array;
		FieldList modifiedFields2 = new FieldList(fields);
		DmlCommandCacheKey commandCacheKey = new DmlCommandCacheKey(DmlCommandType.modifyAll, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), filter, modifiedFields2);
		database.CheckTableInSync(table);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, TransactionType.UpdateNoLocks);
		try
		{
			if (conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlDmlCommand)command).ExecuteModifyMultiple(modifiedFields, filter, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex))) == 0)
			{
				conScope.Connection.ClearKnownXLockedTimestamps();
			}
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}
}

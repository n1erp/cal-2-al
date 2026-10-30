using System.Data.SqlClient;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavSqlWhereClauseBuilder
{
	internal static FilterExpression BuildKeyFilterFromRecord(MutableRecordBuffer record, SortingFieldList keyFields, FilterExpressionType clauseType, bool useOriginalValues)
	{
		FilterExpression filterExpression = null;
		int count = keyFields.Count;
		int num = ((clauseType == FilterExpressionType.Equal) ? 1 : count);
		for (int i = 0; i < num; i++)
		{
			int num2 = count - i;
			FilterExpression filterExpression2 = null;
			for (int j = 1; j <= num2; j++)
			{
				NCLMetaField nCLMetaField = (NCLMetaField)keyFields[j - 1];
				FilterExpressionType filterExpressionType = ((j >= num2 && clauseType != FilterExpressionType.Equal) ? clauseType : FilterExpressionType.Equal);
				NavValue value = (useOriginalValues ? record.GetOriginalValue(nCLMetaField.FieldIndex) : record[nCLMetaField.FieldIndex]);
				UnaryFilterExpression unaryFilterExpression = new UnaryFilterExpression(filterExpressionType, value, nCLMetaField.ExpressionContext);
				filterExpression2 = ((filterExpression2 == null) ? unaryFilterExpression : FilterExpression.And(filterExpression2, unaryFilterExpression));
			}
			filterExpression = ((filterExpression == null) ? filterExpression2 : FilterExpression.Or(filterExpression, filterExpression2));
		}
		return filterExpression;
	}

	internal static FilterExpression BuildTimestampCheckFilter(NCLMetaTable table, IRecordBuffer record, bool strictEqualityTimestampCheck, bool useOriginalValue)
	{
		return new UnaryFilterExpression((!strictEqualityTimestampCheck) ? FilterExpressionType.LessThanOrEqual : FilterExpressionType.Equal, useOriginalValue ? record.GetOriginalValue(table.TimestampField.FieldIndex) : record[table.TimestampField.FieldIndex], table.TimestampField.ExpressionContext);
	}

	public static void AppendFilterClause(StringBuilder whereClause, FilterExpression filter, NavSqlParameterHandler sqlParameterHandling, SqlSortingProperties sqlSortingProperties, CompanyId? companyId, bool shouldAliasQualifyFields = false, string companyIdAlias = null)
	{
		if (filter != null)
		{
			whereClause.Append(" WHERE ");
			if (companyId.HasValue)
			{
				if (companyIdAlias != null)
				{
					whereClause.Append('"').Append(companyIdAlias).Append("\".");
				}
				whereClause.Append(NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(sqlParameterHandling.GetCompanyParameterNo(companyId.Value))).Append(" AND ");
			}
			whereClause.Append('(');
			NavSqlWhereClauseVisitorInputOutput input = NavSqlWhereClauseVisitorInputOutput.CreateForTextualWhereClauseGeneration(whereClause, sqlParameterHandling, null, treatNullAsDefaultValue: false, string.Empty, sqlSortingProperties, shouldAliasQualifyFields);
			filter.Visit(NavSqlWhereClauseVisitor.DefaultVisitor, input);
			whereClause.Append(')');
		}
		else if (companyId.HasValue)
		{
			whereClause.Append(" WHERE ");
			if (companyIdAlias != null)
			{
				whereClause.Append('"').Append(companyIdAlias).Append("\".");
			}
			whereClause.Append(NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(sqlParameterHandling.GetCompanyParameterNo(companyId.Value)));
		}
	}

	internal static FilterExpression CreateFilterBasedOnPrimaryKeyValuesFromRecordAndFilter(FilterExpression filter, FilterExpressionType clauseType, MutableRecordBuffer record, SortingFieldList keyFields, bool useOriginalValues)
	{
		FilterExpression filterExpression = BuildKeyFilterFromRecord(record, keyFields, clauseType, useOriginalValues);
		if (filter != null)
		{
			filterExpression = FilterExpression.And(filter, filterExpression);
		}
		return filterExpression;
	}

	internal static FilterExpression CreateRecordClause(FilterExpressionType clauseType, MutableRecordBuffer record, SortingFieldList sortingFields, bool useOriginalValues)
	{
		return BuildKeyFilterFromRecord(record, sortingFields, clauseType, useOriginalValues);
	}

	internal static FilterExpression CreateRecordClauseWithTimestampCheck(NCLMetaTable table, FilterExpressionType clauseType, MutableRecordBuffer record, SortingFieldList sortingFields, bool strictEqualityTimestampCheck)
	{
		return FilterExpression.And(BuildKeyFilterFromRecord(record, sortingFields, clauseType, useOriginalValues: true), BuildTimestampCheckFilter(table, record, strictEqualityTimestampCheck, useOriginalValue: false));
	}

	public static void AddOrSetFilterParameters(FilterExpression filter, SqlParameterCollection parameters, NavSqlParameterHandler sqlParameterHandling, SqlSortingProperties sqlSortingProperties, CompanyId? companyId)
	{
		if (companyId.HasValue)
		{
			NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(parameters, sqlParameterHandling.GetCompanyParameterNo(companyId.Value)), companyId.Value);
		}
		if (filter != null)
		{
			NavSqlWhereClauseVisitorInputOutput input = NavSqlWhereClauseVisitorInputOutput.CreateForParameterAssignment(parameters, sqlParameterHandling, null, treatNullAsDefaultValue: false, sqlSortingProperties);
			filter.Visit(NavSqlWhereClauseVisitor.DefaultVisitor, input);
		}
	}
}

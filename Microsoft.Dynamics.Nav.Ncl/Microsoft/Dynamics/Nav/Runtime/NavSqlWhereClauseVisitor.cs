using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlWhereClauseVisitor : FilterExpressionVisitor<NavSqlWhereClauseVisitorInputOutput, NavSqlWhereClauseVisitorInputOutput>
{
	private static NavSqlWhereClauseVisitor defaultVisitor = new NavSqlWhereClauseVisitor();

	public static NavSqlWhereClauseVisitor DefaultVisitor => defaultVisitor;

	private static object GetSqlparameterValue(INavFieldMetadata field, NavValue value)
	{
		if (value == null)
		{
			return DBNull.Value;
		}
		return value.GetSqlWhereClauseValue(field.SqlDataType);
	}

	private static void AppendOperator(FilterExpressionType expressionType, bool negate, NavSqlWhereClauseVisitorInputOutput input)
	{
		switch (expressionType)
		{
		case FilterExpressionType.And:
			input.WhereClause.Append(" AND ");
			break;
		case FilterExpressionType.Or:
			input.WhereClause.Append(" OR ");
			break;
		case FilterExpressionType.Equal:
		case FilterExpressionType.FieldEqualsField:
			input.WhereClause.Append('=');
			break;
		case FilterExpressionType.GreaterThan:
			input.WhereClause.Append('>');
			break;
		case FilterExpressionType.GreaterThanOrEqual:
		case FilterExpressionType.RangeFromInclusive:
			input.WhereClause.Append(">=");
			break;
		case FilterExpressionType.LessThan:
		case FilterExpressionType.RangeToInclusive:
			input.WhereClause.Append('<');
			break;
		case FilterExpressionType.LessThanOrEqual:
			input.WhereClause.Append("<=");
			break;
		case FilterExpressionType.NotEqual:
			input.WhereClause.Append("<>");
			break;
		case FilterExpressionType.Wildcard:
			input.WhereClause.Append(negate ? " NOT LIKE " : " LIKE ");
			break;
		default:
			throw new NotSupportedException();
		}
	}

	private NavSqlWhereClauseVisitorInputOutput Parenthesize(FilterExpression filterExpression, NavSqlWhereClauseVisitorInputOutput input)
	{
		input.WhereClause.Append('(');
		Visit(filterExpression, input);
		input.WhereClause.Append(')');
		return input;
	}

	private static void AppendParameterName(NavSqlWhereClauseVisitorInputOutput input, int parameterNo)
	{
		input.WhereClause.Append(NavSqlStatementHelper.ParameterName(parameterNo));
	}

	private static void AddNullCheckParameterIfApplicable(INavFieldMetadata field, NavSqlWhereClauseVisitorInputOutput input)
	{
		if (input.TreatNullAsDefaultValue && field.NclType != NavNclType.NavBlob)
		{
			int parameterNoForEmptyValue = input.SqlParameterHandling.GetParameterNoForEmptyValue(field);
			SqlParameter sqlParameter = input.GetSqlParameter(parameterNoForEmptyValue);
			Func<INavFieldMetadata, object> getEmptySqlValue = (INavFieldMetadata f) => f.EmptyValue.GetSqlWhereClauseValue(f.SqlDataType);
			NavSqlStatementHelper.SetSqlParameterWithEmptyValue(sqlParameter, field, getEmptySqlValue);
		}
	}

	private static void AppendColumnName(NavSqlWhereClauseVisitorInputOutput input, StringBuilder whereClause, INavFieldMetadata metaData, bool treatNullAsDefaultValue, string fixedParentName)
	{
		if (metaData is NCLMetaQueryColumn column)
		{
			whereClause.Append(NavSqlStatementHelper.QueryColumnSourceExpression(column, input.CoveringSIFTIndexes, treatNullAsDefaultValue, input.SqlParameterHandling));
			return;
		}
		NCLMetaField nCLMetaField = (NCLMetaField)metaData;
		if (input.ShouldAliasQualifyFields)
		{
			string mainAlias = (string.IsNullOrWhiteSpace(fixedParentName) ? NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(nCLMetaField.Parent.TableId) : fixedParentName);
			NavSqlStatementHelper.AppendSqlColumnName(whereClause, nCLMetaField, mainAlias);
		}
		else if (!string.IsNullOrEmpty(fixedParentName))
		{
			NavSqlStatementHelper.AppendQuotedIdentifier(whereClause, NavSqlStatementHelper.ConvertToSqlIdentifier(fixedParentName));
			whereClause.Append('.');
			NavSqlStatementHelper.AppendQuotedIdentifier(whereClause, nCLMetaField.SqlColumnName);
		}
		else
		{
			NavSqlStatementHelper.AppendQuotedIdentifier(whereClause, nCLMetaField.SqlColumnName);
		}
	}

	private NavSqlWhereClauseVisitorInputOutput VisitOr(BinaryFilterExpression binaryExpression, NavSqlWhereClauseVisitorInputOutput input)
	{
		Visit(binaryExpression.Left, input);
		AppendOperator(FilterExpressionType.Or, negate: false, input);
		if (binaryExpression.Right.ExpressionType == FilterExpressionType.Or)
		{
			return Parenthesize(binaryExpression.Right, input);
		}
		return Visit(binaryExpression.Right, input);
	}

	private NavSqlWhereClauseVisitorInputOutput VisitAnd(BinaryFilterExpression binaryExpression, NavSqlWhereClauseVisitorInputOutput input)
	{
		if (binaryExpression.Left.ExpressionType == FilterExpressionType.Or)
		{
			Parenthesize(binaryExpression.Left, input);
		}
		else
		{
			Visit(binaryExpression.Left, input);
		}
		AppendOperator(FilterExpressionType.And, negate: false, input);
		if (binaryExpression.Right.ExpressionType == FilterExpressionType.Or || binaryExpression.Right.ExpressionType == FilterExpressionType.And)
		{
			Parenthesize(binaryExpression.Right, input);
		}
		else
		{
			Visit(binaryExpression.Right, input);
		}
		return input;
	}

	protected override NavSqlWhereClauseVisitorInputOutput VisitBinary(BinaryFilterExpression binaryExpression, NavSqlWhereClauseVisitorInputOutput input)
	{
		if (input.CreateTextualWhereClause)
		{
			switch (binaryExpression.ExpressionType)
			{
			case FilterExpressionType.And:
				VisitAnd(binaryExpression, input);
				break;
			case FilterExpressionType.Or:
				VisitOr(binaryExpression, input);
				break;
			default:
				throw new NotSupportedException();
			}
			return input;
		}
		Visit(binaryExpression.Left, input);
		Visit(binaryExpression.Right, input);
		return input;
	}

	protected override NavSqlWhereClauseVisitorInputOutput VisitUnary(UnaryFilterExpression unaryExpression, NavSqlWhereClauseVisitorInputOutput input)
	{
		if (input.CreateTextualWhereClause)
		{
			bool flag = unaryExpression.ExpressionContext.Metadata.NclType == NavNclType.NavBlob;
			if (flag)
			{
				input.WhereClause.Append("CASE ISNULL(DATALENGTH(");
			}
			AppendColumnName(input, input.WhereClause, unaryExpression.ExpressionContext.Metadata, input.TreatNullAsDefaultValue, input.FixedParentName);
			if (flag)
			{
				input.WhereClause.Append("), 0) WHEN 0 THEN 0 ELSE 1 END");
			}
			AppendOperator(unaryExpression.ExpressionType, negate: false, input);
			AppendParameterName(input, input.SqlParameterHandling.GetParameterNoFromValueToken(unaryExpression.ValueToken));
			return input;
		}
		AddNullCheckParameterIfApplicable(unaryExpression.ExpressionContext.Metadata, input);
		object sqlparameterValue = GetSqlparameterValue(unaryExpression.ExpressionContext.Metadata, unaryExpression.Value);
		NavSqlStatementHelper.SetSqlParameter(input.GetSqlParameter(input.SqlParameterHandling.GetParameterNoFromValueToken(unaryExpression.ValueToken)), unaryExpression.ExpressionContext.Metadata, sqlparameterValue, isWildcardValue: false, isFilterValue: true);
		return input;
	}

	protected override NavSqlWhereClauseVisitorInputOutput VisitWildcard(WildcardFilterExpression wildcardExpression, NavSqlWhereClauseVisitorInputOutput input)
	{
		if (input.CreateTextualWhereClause)
		{
			bool flag = wildcardExpression.ExpressionContext.Metadata.NclType == NavNclType.NavOemCode || wildcardExpression.ExpressionContext.Metadata.NclType == NavNclType.NavCode;
			bool flag2 = flag && wildcardExpression.ExpressionContext.Metadata.SqlDataType == SqlDataType.Variant;
			if (flag2)
			{
				input.WhereClause.Append("CAST(");
			}
			AppendColumnName(input, input.WhereClause, wildcardExpression.ExpressionContext.Metadata, input.TreatNullAsDefaultValue, input.FixedParentName);
			if (flag2)
			{
				input.WhereClause.Append((wildcardExpression.ExpressionContext.Metadata.NclType == NavNclType.NavOemCode) ? " AS VARCHAR)" : " AS NVARCHAR)");
			}
			if (wildcardExpression.IsCaseAndAccentInsensitive && !NavGlobal.AppDatabase.SqlSortingProperties.IsAccentAndCaseInsensitive)
			{
				INavFieldMetadata metadata = wildcardExpression.ExpressionContext.Metadata;
				if (!flag || (metadata.SqlDataType != SqlDataType.Integer && metadata.SqlDataType != SqlDataType.BigInt))
				{
					NavSqlStatementHelper.AppendCollate(input.WhereClause, input.SqlSortingProperties.AccentAndCaseInsensitiveCollation);
				}
			}
			AppendOperator(wildcardExpression.ExpressionType, wildcardExpression.IsNegated, input);
			AppendParameterName(input, input.SqlParameterHandling.GetNextParameterNo);
			return input;
		}
		AddNullCheckParameterIfApplicable(wildcardExpression.ExpressionContext.Metadata, input);
		object value = NavSqlStatementHelper.WildcardText(wildcardExpression.Pattern);
		NavSqlStatementHelper.SetSqlParameter(input.GetSqlParameter(input.SqlParameterHandling.GetNextParameterNo), wildcardExpression.ExpressionContext.Metadata, value, isWildcardValue: true, isFilterValue: true);
		return input;
	}

	protected override NavSqlWhereClauseVisitorInputOutput VisitBooleanConstant(BooleanConstantFilterExpression booleanConstantExpression, NavSqlWhereClauseVisitorInputOutput input)
	{
		if (input.CreateTextualWhereClause)
		{
			input.WhereClause.Append("1=");
			AppendParameterName(input, input.SqlParameterHandling.GetNextParameterNo);
			return input;
		}
		SqlParameter sqlParameter = input.GetSqlParameter(input.SqlParameterHandling.GetNextParameterNo);
		sqlParameter.SqlDbType = SqlDbType.TinyInt;
		sqlParameter.Value = booleanConstantExpression.Value;
		return input;
	}

	protected override NavSqlWhereClauseVisitorInputOutput VisitFieldEqualsField(FieldEqualsFieldFilterExpression fieldEqualsFieldExpression, NavSqlWhereClauseVisitorInputOutput input)
	{
		if (input.CreateTextualWhereClause)
		{
			AppendColumnName(input, input.WhereClause, fieldEqualsFieldExpression.ExpressionContext.Metadata, treatNullAsDefaultValue: false, input.FixedParentName);
			AppendOperator(fieldEqualsFieldExpression.ExpressionType, negate: false, input);
			AppendColumnName(input, input.WhereClause, fieldEqualsFieldExpression.FieldToCompareTo, treatNullAsDefaultValue: false, input.FixedParentName);
		}
		return input;
	}
}

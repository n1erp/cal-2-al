using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class FilterStringVisitor : FilterExpressionVisitor<Tuple<FilterExpressionStringOptions, StringBuilder>, StringBuilder>
{
	internal static readonly FilterStringVisitor Instance = new FilterStringVisitor();

	private FilterStringVisitor()
	{
	}

	protected override StringBuilder VisitBinary(BinaryFilterExpression binaryExpression, Tuple<FilterExpressionStringOptions, StringBuilder> input)
	{
		return binaryExpression.ExpressionType switch
		{
			FilterExpressionType.And => VisitAnd(binaryExpression, input), 
			FilterExpressionType.Or => VisitOr(binaryExpression, input), 
			_ => throw new NotSupportedException(), 
		};
	}

	private StringBuilder VisitOr(BinaryFilterExpression binaryExpression, Tuple<FilterExpressionStringOptions, StringBuilder> input)
	{
		bool flag = (input.Item1 & FilterExpressionStringOptions.AlwaysParenthesize) != 0;
		if (flag && (binaryExpression.Left.ExpressionType == FilterExpressionType.Or || binaryExpression.Left.ExpressionType == FilterExpressionType.And))
		{
			Parenthesize(binaryExpression.Left, input);
		}
		else
		{
			Visit(binaryExpression.Left, input);
		}
		input.Item2.Append('|');
		if (binaryExpression.Right.ExpressionType == FilterExpressionType.Or || (flag && binaryExpression.Right.ExpressionType == FilterExpressionType.And))
		{
			return Parenthesize(binaryExpression.Right, input);
		}
		return Visit(binaryExpression.Right, input);
	}

	private StringBuilder VisitAnd(BinaryFilterExpression binaryExpression, Tuple<FilterExpressionStringOptions, StringBuilder> input)
	{
		bool flag = (input.Item1 & FilterExpressionStringOptions.AlwaysParenthesize) != 0;
		if (binaryExpression.Left.ExpressionType == FilterExpressionType.Or || (flag && binaryExpression.Left.ExpressionType == FilterExpressionType.And))
		{
			Parenthesize(binaryExpression.Left, input);
		}
		else
		{
			Visit(binaryExpression.Left, input);
		}
		StringBuilder item = input.Item2;
		item.Append('&');
		if (binaryExpression.Right.ExpressionType == FilterExpressionType.Or || binaryExpression.Right.ExpressionType == FilterExpressionType.And)
		{
			Parenthesize(binaryExpression.Right, input);
		}
		else
		{
			Visit(binaryExpression.Right, input);
		}
		return item;
	}

	protected override StringBuilder VisitUnary(UnaryFilterExpression unaryExpression, Tuple<FilterExpressionStringOptions, StringBuilder> input)
	{
		string text = unaryExpression.ExpressionContext.QuoteLiteralIfNecessary(unaryExpression.ExpressionContext.ValueToString(unaryExpression.Value, input.Item1), input.Item1);
		if (UnaryExpressionRequiresOperator(text, unaryExpression, input.Item1))
		{
			input.Item2.Append(unaryExpression.ExpressionType.OperatorString());
		}
		return input.Item2.Append(text);
	}

	protected override StringBuilder VisitRange(RangeFilterExpression rangeFilterExpression, Tuple<FilterExpressionStringOptions, StringBuilder> input)
	{
		return rangeFilterExpression.ExpressionType switch
		{
			FilterExpressionType.RangeBetweenInclusive => input.Item2.Append(rangeFilterExpression.ExpressionContext.QuoteLiteralIfNecessary(rangeFilterExpression.ExpressionContext.ValueToString(rangeFilterExpression.LowValue, input.Item1))).Append("..").Append(rangeFilterExpression.ExpressionContext.QuoteLiteralIfNecessary(rangeFilterExpression.ExpressionContext.ValueToString(rangeFilterExpression.HighValue, input.Item1))), 
			FilterExpressionType.RangeFromInclusive => input.Item2.Append(rangeFilterExpression.ExpressionContext.QuoteLiteralIfNecessary(rangeFilterExpression.ExpressionContext.ValueToString(rangeFilterExpression.LowValue, input.Item1))).Append(".."), 
			FilterExpressionType.RangeToInclusive => input.Item2.Append("..").Append(rangeFilterExpression.ExpressionContext.QuoteLiteralIfNecessary(rangeFilterExpression.ExpressionContext.ValueToString(rangeFilterExpression.HighValue, input.Item1))), 
			_ => throw new NotSupportedException(), 
		};
	}

	protected override StringBuilder VisitWildcard(WildcardFilterExpression wildcardExpression, Tuple<FilterExpressionStringOptions, StringBuilder> input)
	{
		StringBuilder item = input.Item2;
		if (wildcardExpression.IsNegated)
		{
			item.Append(FilterExpressionType.NotEqual.OperatorString());
		}
		string text = wildcardExpression.Pattern;
		if (wildcardExpression.IsCaseAndAccentInsensitive)
		{
			text = "@" + text;
		}
		return item.Append(wildcardExpression.ExpressionContext.QuoteLiteralIfNecessary(text, FilterExpressionStringOptions.DoNotQuoteWildcards));
	}

	protected override StringBuilder VisitBooleanConstant(BooleanConstantFilterExpression booleanConstantExpression, Tuple<FilterExpressionStringOptions, StringBuilder> input)
	{
		if ((input.Item1 & FilterExpressionStringOptions.BooleanConstantTrueAsEmpty) != FilterExpressionStringOptions.BooleanConstantTrueAsEmpty || !booleanConstantExpression.Value)
		{
			input.Item2.Append(booleanConstantExpression.Value ? "==" : "<><>");
		}
		return input.Item2;
	}

	protected override StringBuilder VisitFieldEqualsField(FieldEqualsFieldFilterExpression fieldEqualsFieldExpression, Tuple<FilterExpressionStringOptions, StringBuilder> input)
	{
		string tableOrColumnName;
		string tableOrColumnName2;
		if (fieldEqualsFieldExpression.FieldToCompareTo is NCLMetaField nCLMetaField)
		{
			tableOrColumnName = nCLMetaField.Parent.TableName;
			tableOrColumnName2 = nCLMetaField.FieldName;
		}
		else
		{
			if (!(fieldEqualsFieldExpression.FieldToCompareTo is NCLMetaQueryColumn nCLMetaQueryColumn))
			{
				throw new NotSupportedException();
			}
			tableOrColumnName = nCLMetaQueryColumn.ParentDataItem.Name;
			tableOrColumnName2 = nCLMetaQueryColumn.Name;
		}
		StringBuilder item = input.Item2;
		item.Append("=Field[");
		AppendAndQuoteTableOrColumnNameIfNecessary(item, tableOrColumnName);
		item.Append('.');
		AppendAndQuoteTableOrColumnNameIfNecessary(item, tableOrColumnName2);
		item.Append(']');
		return item;
	}

	private static void AppendAndQuoteTableOrColumnNameIfNecessary(StringBuilder stringBuilder, string tableOrColumnName)
	{
		if (Regex.IsMatch(tableOrColumnName, "[ \\]]"))
		{
			stringBuilder.Append('"').Append(tableOrColumnName.Replace("\"", "\"\"")).Append('"');
		}
		else
		{
			stringBuilder.Append(tableOrColumnName);
		}
	}

	private StringBuilder Parenthesize(FilterExpression filterExpression, Tuple<FilterExpressionStringOptions, StringBuilder> input)
	{
		StringBuilder item = input.Item2;
		item.Append('(');
		Visit(filterExpression, input);
		return item.Append(')');
	}

	private static bool UnaryExpressionRequiresOperator(string valueString, UnaryFilterExpression unaryExpression, FilterExpressionStringOptions stringOptions)
	{
		if (unaryExpression.ExpressionType != FilterExpressionType.Equal)
		{
			return true;
		}
		if ((stringOptions & FilterExpressionStringOptions.ConstExpressionImplied) == FilterExpressionStringOptions.ConstExpressionImplied)
		{
			return false;
		}
		if (unaryExpression.ExpressionContext.SupportsWildcards && FilterExpressionParser.IsWildcard(valueString))
		{
			return true;
		}
		return false;
	}
}

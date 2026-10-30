using System;
using System.IO;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class FilterExpressionExtensions
{
	public static string OperatorString(this FilterExpressionType filterExpressionType)
	{
		return filterExpressionType switch
		{
			FilterExpressionType.Equal => "=", 
			FilterExpressionType.NotEqual => "<>", 
			FilterExpressionType.GreaterThan => ">", 
			FilterExpressionType.LessThan => "<", 
			FilterExpressionType.GreaterThanOrEqual => ">=", 
			FilterExpressionType.LessThanOrEqual => "<=", 
			FilterExpressionType.And => "&", 
			FilterExpressionType.Or => "|", 
			_ => throw new NotSupportedException(), 
		};
	}

	internal static int OperatorPrecedence(this FilterToken token)
	{
		return token.TokenType switch
		{
			FilterTokenType.DotDot => 3, 
			FilterTokenType.And => 2, 
			FilterTokenType.Or => 1, 
			_ => -1, 
		};
	}

	internal static FilterExpressionType ToFilterExpressionType(this FilterTokenType tokenType)
	{
		return tokenType switch
		{
			FilterTokenType.Equal => FilterExpressionType.Equal, 
			FilterTokenType.NotEqual => FilterExpressionType.NotEqual, 
			FilterTokenType.GreaterThan => FilterExpressionType.GreaterThan, 
			FilterTokenType.LessThan => FilterExpressionType.LessThan, 
			FilterTokenType.GreaterThanOrEqual => FilterExpressionType.GreaterThanOrEqual, 
			FilterTokenType.LessThanOrEqual => FilterExpressionType.LessThanOrEqual, 
			FilterTokenType.And => FilterExpressionType.And, 
			FilterTokenType.Or => FilterExpressionType.Or, 
			_ => throw new NotSupportedException(), 
		};
	}

	public static FilterExpression Visit(this FilterExpression filterExpression, params ModifyingExpressionVisitor<object>[] visitors)
	{
		return visitors.Aggregate(filterExpression, (FilterExpression expression, ModifyingExpressionVisitor<object> visitor) => visitor.Visit(expression, null));
	}

	public static TReturn Visit<TInput, TReturn>(this FilterExpression filterExpression, FilterExpressionVisitor<TInput, TReturn> visitor, TInput input)
	{
		return visitor.Visit(filterExpression, input);
	}

	public static void WriteToBinary(this FilterExpression filterExpression, BinaryWriter writer)
	{
		FilterBinaryWriterVisitor.WriteToBinaryWriter(filterExpression, writer);
	}

	public static FilterExpression OptimizeForEvaluation(this FilterExpression filterExpression)
	{
		return Optimize(filterExpression, tryBalanced: true);
	}

	public static FilterExpression Simplify(this FilterExpression filterExpression)
	{
		return Optimize(filterExpression, tryBalanced: false);
	}

	private static FilterExpression Optimize(FilterExpression filterExpression, bool tryBalanced)
	{
		if (filterExpression == null)
		{
			return null;
		}
		FilterExpressionType expressionType = filterExpression.ExpressionType;
		if ((uint)(expressionType - 6) <= 1u)
		{
			try
			{
				return filterExpression.ToRangeList().ToFilterExpression(tryBalanced);
			}
			catch (CannotConvertWildcardIntoRangeException)
			{
				return filterExpression;
			}
		}
		return filterExpression;
	}

	public static FilterExpression ToFilterExpression(this RangeList rangeList, bool optimizeForEvaluation)
	{
		return RangeListExpressionBuilder.BuildExpression(rangeList, optimizeForEvaluation);
	}

	public static FilterExpression ToFilterExpression(this Range range)
	{
		if (range.IsFullRange)
		{
			return FilterExpression.BooleanConstant(value: true);
		}
		if (range.IsEmptyRange)
		{
			return FilterExpression.BooleanConstant(value: false);
		}
		if (!range.IsLowIsMinimum && !range.IsHighMaximum && range.IsLowIncluded && range.IsHighIncluded)
		{
			if (range.ExpressionContext.Compare(range.LowValue, range.HighValue) == 0)
			{
				return FilterExpression.Equal(range.LowValue, range.ExpressionContext);
			}
			return FilterExpression.RangeBetweenInclusive(range.LowValue, range.HighValue, range.ExpressionContext);
		}
		FilterExpression filterExpression;
		if (range.IsLowIsMinimum)
		{
			filterExpression = null;
		}
		else
		{
			filterExpression = (range.IsLowIncluded ? FilterExpression.GreaterThanOrEqual(range.LowValue, range.ExpressionContext) : FilterExpression.GreaterThan(range.LowValue, range.ExpressionContext));
		}
		FilterExpression filterExpression2;
		if (range.IsHighMaximum)
		{
			filterExpression2 = null;
		}
		else
		{
			filterExpression2 = (range.IsHighIncluded ? FilterExpression.LessThanOrEqual(range.HighValue, range.ExpressionContext) : FilterExpression.LessThan(range.HighValue, range.ExpressionContext));
		}
		if (filterExpression2 == null)
		{
			return filterExpression;
		}
		if (filterExpression == null)
		{
			return filterExpression2;
		}
		return FilterExpression.And(filterExpression, filterExpression2);
	}

	public static FilterExpression TransferToDifferentContext(this FilterExpression inputExpression, FilterExpressionContext newExpressionContext)
	{
		return inputExpression.Visit(TransferExpressionContextVisitor.Instance, newExpressionContext);
	}

	public static FilterExpression Negate(this FilterExpression inputExpression)
	{
		return NegatingExpressionVisitor.Instance.Visit(inputExpression, null);
	}

	public static bool IsNullOrConstantTrue(this FilterExpression expression)
	{
		if (expression != null)
		{
			if (expression.ExpressionType == FilterExpressionType.BooleanConstant)
			{
				return ((BooleanConstantFilterExpression)expression).Value;
			}
			return false;
		}
		return true;
	}

	public static bool IsConstantFalse(this FilterExpression expression)
	{
		if (expression.ExpressionType == FilterExpressionType.BooleanConstant)
		{
			return !((BooleanConstantFilterExpression)expression).Value;
		}
		return false;
	}

	public static bool IsNotNullAndConstantTrue(this FilterExpression expression)
	{
		if (expression != null && expression.ExpressionType == FilterExpressionType.BooleanConstant)
		{
			return ((BooleanConstantFilterExpression)expression).Value;
		}
		return false;
	}
}

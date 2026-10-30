using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class FilterExpression : ISizableObject, IEquatable<FilterExpression>
{
	public const int UndefinedValueToken = -1;

	public const int DefaultValueTokens = 25;

	public const int TimestampValueToken = 26;

	public const int VariantValueToken = 27;

	public const int ReservedValueTokens = 28;

	private readonly FilterExpressionType expressionType;

	public virtual int ApproximateByteSize => SizableObject.GetApproximateByteSize(1);

	public FilterExpressionType ExpressionType => expressionType;

	protected FilterExpression(FilterExpressionType expressionType)
	{
		this.expressionType = expressionType;
	}

	public abstract bool Evaluate(NavValue input);

	public abstract RangeList ToRangeList();

	protected static void ValidateNotNull(object argument, string parameterName)
	{
		if (argument == null)
		{
			throw new ArgumentNullException(parameterName);
		}
	}

	protected static void ValidateNull(object argument, string parameterName)
	{
		if (argument != null)
		{
			throw new ArgumentOutOfRangeException(parameterName);
		}
	}

	[Conditional("DEBUG")]
	protected static void ValidateTypesMatch(INavValueMetadata value, FilterExpressionContext expressionContext)
	{
		if (value.NclType != expressionContext.Metadata.NclType)
		{
			throw new ArgumentException("Type mismatch between value and expression context.");
		}
	}

	public bool Equals(FilterExpression other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null || expressionType != other.expressionType || GetHashCode() != other.GetHashCode())
		{
			return false;
		}
		switch (expressionType)
		{
		case FilterExpressionType.Equal:
		case FilterExpressionType.NotEqual:
		case FilterExpressionType.GreaterThan:
		case FilterExpressionType.LessThan:
		case FilterExpressionType.GreaterThanOrEqual:
		case FilterExpressionType.LessThanOrEqual:
			return ((UnaryFilterExpression)this).Equals((UnaryFilterExpression)other);
		case FilterExpressionType.And:
		case FilterExpressionType.Or:
			return ((BinaryFilterExpression)this).Equals((BinaryFilterExpression)other);
		case FilterExpressionType.RangeBetweenInclusive:
		case FilterExpressionType.RangeFromInclusive:
		case FilterExpressionType.RangeToInclusive:
			return ((RangeFilterExpression)this).Equals((RangeFilterExpression)other);
		case FilterExpressionType.Wildcard:
			return ((WildcardFilterExpression)this).Equals((WildcardFilterExpression)other);
		case FilterExpressionType.BooleanConstant:
			return ((BooleanConstantFilterExpression)this).Equals((BooleanConstantFilterExpression)other);
		case FilterExpressionType.FieldEqualsField:
			return ((FieldEqualsFieldFilterExpression)this).Equals((FieldEqualsFieldFilterExpression)other);
		default:
			return false;
		}
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as FilterExpression);
	}

	public abstract override int GetHashCode();

	public override string ToString()
	{
		return ToString(FilterExpressionStringOptions.None);
	}

	public string ToString(FilterExpressionStringOptions stringOptions)
	{
		StringBuilder stringBuilder = this.Visit(FilterStringVisitor.Instance, Tuple.Create(stringOptions, new StringBuilder()));
		if ((stringOptions & FilterExpressionStringOptions.TwoQuotesAsEmpty) == FilterExpressionStringOptions.TwoQuotesAsEmpty && stringBuilder.Length == 2 && stringBuilder[0] == '\'' && stringBuilder[1] == '\'')
		{
			return string.Empty;
		}
		return stringBuilder.ToString();
	}

	public static FilterExpression And(FilterExpression left, FilterExpression right)
	{
		if (left != null)
		{
			if (right != null)
			{
				return new BinaryFilterExpression(FilterExpressionType.And, left, right);
			}
			return left;
		}
		return right;
	}

	public static FilterExpression And(IEnumerable<FilterExpression> expressions)
	{
		return expressions.Aggregate<FilterExpression, FilterExpression>(null, And);
	}

	public static FilterExpression And(params FilterExpression[] expressions)
	{
		return expressions.Aggregate<FilterExpression, FilterExpression>(null, And);
	}

	public static FilterExpression Or(FilterExpression left, FilterExpression right)
	{
		if (left != null && right != null)
		{
			return new BinaryFilterExpression(FilterExpressionType.Or, left, right);
		}
		return null;
	}

	public static FilterExpression Or(IEnumerable<FilterExpression> expressions)
	{
		FilterExpression filterExpression = null;
		bool flag = true;
		foreach (FilterExpression expression in expressions)
		{
			if (expression == null)
			{
				return null;
			}
			if (flag)
			{
				flag = false;
				filterExpression = expression;
			}
			else
			{
				filterExpression = Or(filterExpression, expression);
			}
		}
		return filterExpression;
	}

	public static FilterExpression Or(params FilterExpression[] expressions)
	{
		return Or((IEnumerable<FilterExpression>)(expressions ?? new FilterExpression[0]));
	}

	public static FilterExpression BalancedOr(IEnumerable<FilterExpression> expressions)
	{
		return Balanced(expressions, Or);
	}

	public static FilterExpression BalancedAnd(IEnumerable<FilterExpression> expressions)
	{
		return Balanced(expressions, And);
	}

	public static FilterExpression Balanced(IEnumerable<FilterExpression> expressions, Func<FilterExpression, FilterExpression, FilterExpression> combiningFunc)
	{
		IEnumerator<FilterExpression> enumerator = expressions.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return null;
		}
		FilterExpression filterExpression = null;
		bool flag = true;
		int num = 0;
		while (flag)
		{
			flag = ConsumeHeight(enumerator, num, combiningFunc, out var expression);
			if (filterExpression == null)
			{
				filterExpression = expression;
				continue;
			}
			filterExpression = combiningFunc(filterExpression, expression);
			num++;
		}
		return filterExpression;
	}

	private static bool ConsumeHeight(IEnumerator<FilterExpression> expressions, int targetHeight, Func<FilterExpression, FilterExpression, FilterExpression> combiningFunc, out FilterExpression expression)
	{
		if (targetHeight == 0)
		{
			expression = expressions.Current;
			return expressions.MoveNext();
		}
		if (!ConsumeHeight(expressions, targetHeight - 1, combiningFunc, out var expression2))
		{
			expression = expression2;
			return false;
		}
		bool result = ConsumeHeight(expressions, targetHeight - 1, combiningFunc, out var expression3);
		expression = combiningFunc(expression2, expression3);
		return result;
	}

	public static UnaryFilterExpression Equal(NavValue value, FilterExpressionContext expressionContext)
	{
		return new UnaryFilterExpression(FilterExpressionType.Equal, value, expressionContext);
	}

	public static UnaryFilterExpression NotEqual(NavValue value, FilterExpressionContext expressionContext)
	{
		return new UnaryFilterExpression(FilterExpressionType.NotEqual, value, expressionContext);
	}

	public static UnaryFilterExpression GreaterThan(NavValue value, FilterExpressionContext expressionContext)
	{
		return new UnaryFilterExpression(FilterExpressionType.GreaterThan, value, expressionContext);
	}

	public static UnaryFilterExpression GreaterThanOrEqual(NavValue value, FilterExpressionContext expressionContext)
	{
		return new UnaryFilterExpression(FilterExpressionType.GreaterThanOrEqual, value, expressionContext);
	}

	public static UnaryFilterExpression LessThan(NavValue value, FilterExpressionContext expressionContext)
	{
		return new UnaryFilterExpression(FilterExpressionType.LessThan, value, expressionContext);
	}

	public static UnaryFilterExpression LessThanOrEqual(NavValue value, FilterExpressionContext expressionContext)
	{
		return new UnaryFilterExpression(FilterExpressionType.LessThanOrEqual, value, expressionContext);
	}

	public static WildcardFilterExpression Wildcard(bool isNegated, string pattern, bool caseAndAccentInsensitive, FilterExpressionContext expressionContext)
	{
		return new WildcardFilterExpression(isNegated, pattern, caseAndAccentInsensitive, expressionContext);
	}

	public static BooleanConstantFilterExpression BooleanConstant(bool value)
	{
		return BooleanConstantFilterExpression.Get(value);
	}

	public static RangeFilterExpression RangeBetweenInclusive(NavValue lowValue, NavValue highValue, FilterExpressionContext expressionContext)
	{
		return new RangeFilterExpression(FilterExpressionType.RangeBetweenInclusive, lowValue, highValue, expressionContext);
	}

	public static RangeFilterExpression RangeFromInclusive(NavValue lowValue, FilterExpressionContext expressionContext)
	{
		return new RangeFilterExpression(FilterExpressionType.RangeFromInclusive, lowValue, null, expressionContext);
	}

	public static RangeFilterExpression RangeToInclusive(NavValue highValue, FilterExpressionContext expressionContext)
	{
		return new RangeFilterExpression(FilterExpressionType.RangeToInclusive, null, highValue, expressionContext);
	}

	public static FieldEqualsFieldFilterExpression FieldEqualsField(INavFieldMetadata fieldToCompareTo, FilterExpressionContext expressionContext)
	{
		return new FieldEqualsFieldFilterExpression(fieldToCompareTo, expressionContext);
	}
}

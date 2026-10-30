using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ValueInsensitiveFilterExpressionEqualityComparer : IEqualityComparer<FilterExpression>
{
	public static readonly ValueInsensitiveFilterExpressionEqualityComparer Instance = new ValueInsensitiveFilterExpressionEqualityComparer();

	private ValueInsensitiveFilterExpressionEqualityComparer()
	{
	}

	private static int AddOrGetValueToken(Dictionary<Tuple<SqlDataType, NavValue>, int> valueTokens, SqlDataType sqlDataType, NavValue value)
	{
		if (value == null)
		{
			return -1;
		}
		Tuple<SqlDataType, NavValue> key = Tuple.Create(sqlDataType, value);
		if (!valueTokens.TryGetValue(key, out var value2))
		{
			value2 = valueTokens.Count + 28;
			valueTokens.Add(key, value2);
		}
		return value2;
	}

	public bool Equals(FilterExpression x, FilterExpression y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null)
		{
			return y == null;
		}
		if (y == null)
		{
			return false;
		}
		if (x.ExpressionType != y.ExpressionType)
		{
			switch (x.ExpressionType)
			{
			case FilterExpressionType.GreaterThanOrEqual:
			{
				if (y.ExpressionType != FilterExpressionType.RangeFromInclusive)
				{
					return false;
				}
				UnaryFilterExpression unaryFilterExpression2 = (UnaryFilterExpression)x;
				RangeFilterExpression rangeFilterExpression2 = (RangeFilterExpression)y;
				if (unaryFilterExpression2.ExpressionContext.Equals(rangeFilterExpression2.ExpressionContext))
				{
					return unaryFilterExpression2.ValueToken == rangeFilterExpression2.LowValueToken;
				}
				return false;
			}
			case FilterExpressionType.LessThanOrEqual:
			{
				if (y.ExpressionType != FilterExpressionType.RangeToInclusive)
				{
					return false;
				}
				UnaryFilterExpression unaryFilterExpression4 = (UnaryFilterExpression)x;
				RangeFilterExpression rangeFilterExpression4 = (RangeFilterExpression)y;
				if (unaryFilterExpression4.ExpressionContext.Equals(rangeFilterExpression4.ExpressionContext))
				{
					return unaryFilterExpression4.ValueToken == rangeFilterExpression4.HighValueToken;
				}
				return false;
			}
			case FilterExpressionType.RangeFromInclusive:
			{
				if (y.ExpressionType != FilterExpressionType.GreaterThanOrEqual)
				{
					return false;
				}
				RangeFilterExpression rangeFilterExpression3 = (RangeFilterExpression)x;
				UnaryFilterExpression unaryFilterExpression3 = (UnaryFilterExpression)y;
				if (rangeFilterExpression3.ExpressionContext.Equals(unaryFilterExpression3.ExpressionContext))
				{
					return rangeFilterExpression3.LowValueToken == unaryFilterExpression3.ValueToken;
				}
				return false;
			}
			case FilterExpressionType.RangeToInclusive:
			{
				if (y.ExpressionType != FilterExpressionType.LessThanOrEqual)
				{
					return false;
				}
				RangeFilterExpression rangeFilterExpression = (RangeFilterExpression)x;
				UnaryFilterExpression unaryFilterExpression = (UnaryFilterExpression)y;
				if (rangeFilterExpression.ExpressionContext.Equals(unaryFilterExpression.ExpressionContext))
				{
					return rangeFilterExpression.HighValueToken == unaryFilterExpression.ValueToken;
				}
				return false;
			}
			default:
				return false;
			}
		}
		switch (x.ExpressionType)
		{
		case FilterExpressionType.Equal:
		case FilterExpressionType.NotEqual:
		case FilterExpressionType.GreaterThan:
		case FilterExpressionType.LessThan:
		case FilterExpressionType.GreaterThanOrEqual:
		case FilterExpressionType.LessThanOrEqual:
		{
			UnaryFilterExpression unaryFilterExpression5 = (UnaryFilterExpression)x;
			UnaryFilterExpression unaryFilterExpression6 = (UnaryFilterExpression)y;
			if (unaryFilterExpression5.ExpressionContext.Equals(unaryFilterExpression6.ExpressionContext))
			{
				return unaryFilterExpression5.ValueToken == unaryFilterExpression6.ValueToken;
			}
			return false;
		}
		case FilterExpressionType.RangeBetweenInclusive:
		case FilterExpressionType.RangeFromInclusive:
		case FilterExpressionType.RangeToInclusive:
		{
			RangeFilterExpression rangeFilterExpression5 = (RangeFilterExpression)x;
			RangeFilterExpression rangeFilterExpression6 = (RangeFilterExpression)y;
			if (rangeFilterExpression5.ExpressionContext.Equals(rangeFilterExpression6.ExpressionContext) && rangeFilterExpression5.LowValueToken == rangeFilterExpression6.LowValueToken)
			{
				return rangeFilterExpression5.HighValueToken == rangeFilterExpression6.HighValueToken;
			}
			return false;
		}
		case FilterExpressionType.Wildcard:
		{
			WildcardFilterExpression wildcardFilterExpression = (WildcardFilterExpression)x;
			WildcardFilterExpression wildcardFilterExpression2 = (WildcardFilterExpression)y;
			if (wildcardFilterExpression.IsNegated == wildcardFilterExpression2.IsNegated && wildcardFilterExpression.IsCaseAndAccentInsensitive == wildcardFilterExpression2.IsCaseAndAccentInsensitive)
			{
				return wildcardFilterExpression.ExpressionContext.Equals(wildcardFilterExpression2.ExpressionContext);
			}
			return false;
		}
		case FilterExpressionType.And:
		case FilterExpressionType.Or:
		{
			BinaryFilterExpression binaryFilterExpression = (BinaryFilterExpression)x;
			BinaryFilterExpression binaryFilterExpression2 = (BinaryFilterExpression)y;
			if (Equals(binaryFilterExpression.Left, binaryFilterExpression2.Left))
			{
				return Equals(binaryFilterExpression.Right, binaryFilterExpression2.Right);
			}
			return false;
		}
		case FilterExpressionType.FieldEqualsField:
			return x.Equals(y);
		default:
			return true;
		}
	}

	private static FilterExpression GetExpressionWithValueTokensInternal(Dictionary<Tuple<SqlDataType, NavValue>, int> valueTokens, FilterExpression filter)
	{
		switch (filter.ExpressionType)
		{
		case FilterExpressionType.Equal:
		case FilterExpressionType.NotEqual:
		case FilterExpressionType.GreaterThan:
		case FilterExpressionType.LessThan:
		case FilterExpressionType.GreaterThanOrEqual:
		case FilterExpressionType.LessThanOrEqual:
		{
			UnaryFilterExpression unaryFilterExpression = (UnaryFilterExpression)filter;
			return new UnaryFilterExpression(unaryFilterExpression, AddOrGetValueToken(valueTokens, unaryFilterExpression.ExpressionContext.Metadata.SqlDataType, unaryFilterExpression.Value));
		}
		case FilterExpressionType.RangeBetweenInclusive:
		case FilterExpressionType.RangeFromInclusive:
		case FilterExpressionType.RangeToInclusive:
		{
			RangeFilterExpression rangeFilterExpression = (RangeFilterExpression)filter;
			return new RangeFilterExpression(rangeFilterExpression, AddOrGetValueToken(valueTokens, rangeFilterExpression.ExpressionContext.Metadata.SqlDataType, rangeFilterExpression.LowValue), AddOrGetValueToken(valueTokens, rangeFilterExpression.ExpressionContext.Metadata.SqlDataType, rangeFilterExpression.HighValue));
		}
		case FilterExpressionType.And:
		case FilterExpressionType.Or:
		{
			BinaryFilterExpression binaryFilterExpression = (BinaryFilterExpression)filter;
			return new BinaryFilterExpression(binaryFilterExpression.ExpressionType, GetExpressionWithValueTokensInternal(valueTokens, binaryFilterExpression.Left), GetExpressionWithValueTokensInternal(valueTokens, binaryFilterExpression.Right));
		}
		case FilterExpressionType.Wildcard:
		case FilterExpressionType.FieldEqualsField:
			return filter;
		default:
			return filter;
		}
	}

	public static FilterExpression GetExpressionWithValueTokens(FilterExpression filter)
	{
		if (filter == null)
		{
			return null;
		}
		return GetExpressionWithValueTokensInternal(new Dictionary<Tuple<SqlDataType, NavValue>, int>(NavValueTypeCheckedEqualityComparer.Instance), filter);
	}

	public int GetHashCode(FilterExpression obj)
	{
		if (obj == null)
		{
			return int.MaxValue;
		}
		uint hashCode = 0u;
		int bitPosition = 0;
		WriteNode(obj, ref hashCode, ref bitPosition);
		return (int)hashCode;
	}

	private static void WriteNode(FilterExpression expression, ref uint hashCode, ref int bitPosition)
	{
		if (bitPosition >= 32)
		{
			return;
		}
		uint num;
		switch (expression.ExpressionType)
		{
		case FilterExpressionType.Equal:
			num = 0u;
			break;
		case FilterExpressionType.NotEqual:
			num = 1u;
			break;
		case FilterExpressionType.GreaterThan:
			num = 2u;
			break;
		case FilterExpressionType.LessThan:
			num = 3u;
			break;
		case FilterExpressionType.GreaterThanOrEqual:
		case FilterExpressionType.RangeFromInclusive:
			num = 4u;
			break;
		case FilterExpressionType.LessThanOrEqual:
		case FilterExpressionType.RangeToInclusive:
			num = 5u;
			break;
		case FilterExpressionType.And:
		{
			BinaryFilterExpression binaryFilterExpression2 = (BinaryFilterExpression)expression;
			if (binaryFilterExpression2.Left.ExpressionType == FilterExpressionType.GreaterThan)
			{
				if (binaryFilterExpression2.Right.ExpressionType == FilterExpressionType.LessThan)
				{
					num = 11u;
					break;
				}
				if (binaryFilterExpression2.Right.ExpressionType == FilterExpressionType.LessThanOrEqual || binaryFilterExpression2.Right.ExpressionType == FilterExpressionType.RangeToInclusive)
				{
					num = 12u;
					break;
				}
			}
			else if ((binaryFilterExpression2.Left.ExpressionType == FilterExpressionType.GreaterThanOrEqual || binaryFilterExpression2.Left.ExpressionType == FilterExpressionType.RangeFromInclusive) && binaryFilterExpression2.Right.ExpressionType == FilterExpressionType.LessThan)
			{
				num = 13u;
				break;
			}
			WriteNode(binaryFilterExpression2.Left, ref hashCode, ref bitPosition);
			WriteNode(binaryFilterExpression2.Right, ref hashCode, ref bitPosition);
			if (bitPosition >= 32)
			{
				return;
			}
			num = 6u;
			break;
		}
		case FilterExpressionType.Or:
		{
			BinaryFilterExpression binaryFilterExpression = (BinaryFilterExpression)expression;
			if (binaryFilterExpression.Left.ExpressionType == FilterExpressionType.LessThanOrEqual || binaryFilterExpression.Left.ExpressionType == FilterExpressionType.RangeToInclusive)
			{
				if (binaryFilterExpression.Right.ExpressionType == FilterExpressionType.GreaterThanOrEqual || binaryFilterExpression.Right.ExpressionType == FilterExpressionType.RangeFromInclusive)
				{
					num = 14u;
					break;
				}
				if (binaryFilterExpression.Right.ExpressionType == FilterExpressionType.GreaterThan)
				{
					num = 15u;
					break;
				}
			}
			WriteNode(binaryFilterExpression.Left, ref hashCode, ref bitPosition);
			WriteNode(binaryFilterExpression.Right, ref hashCode, ref bitPosition);
			if (bitPosition >= 32)
			{
				return;
			}
			num = 7u;
			break;
		}
		case FilterExpressionType.RangeBetweenInclusive:
			num = 8u;
			break;
		case FilterExpressionType.Wildcard:
			num = 9u;
			break;
		case FilterExpressionType.BooleanConstant:
			num = 2147483647u;
			break;
		case FilterExpressionType.FieldEqualsField:
			num = 10u;
			break;
		default:
			throw new NotSupportedException();
		}
		hashCode |= num << bitPosition;
		bitPosition += 4;
	}
}

using System;
using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class UnaryFilterExpression : FilterExpression, IEquatable<UnaryFilterExpression>
{
	private readonly NavValue value;

	private readonly int valueToken;

	private readonly FilterExpressionContext expressionContext;

	private readonly int hashCode;

	public override int ApproximateByteSize => base.ApproximateByteSize + SizableObject.GetApproximateByteSize(expressionContext) + SizableObject.GetApproximateByteSize(value) + SizableObject.GetApproximateByteSize(4) + 24;

	public NavValue Value
	{
		[DebuggerStepThrough]
		get
		{
			return value;
		}
	}

	public int ValueToken
	{
		[DebuggerStepThrough]
		get
		{
			return valueToken;
		}
	}

	public FilterExpressionContext ExpressionContext
	{
		[DebuggerStepThrough]
		get
		{
			return expressionContext;
		}
	}

	public UnaryFilterExpression(FilterExpressionType filterExpressionType, NavValue value, FilterExpressionContext expressionContext, int valueToken = -1)
		: base(filterExpressionType)
	{
		FilterExpression.ValidateNotNull(value, "value");
		FilterExpression.ValidateNotNull(expressionContext, "expressionContext");
		if ((uint)filterExpressionType > 5u)
		{
			throw new ArgumentOutOfRangeException("filterExpressionType");
		}
		this.value = value;
		this.expressionContext = expressionContext;
		hashCode = ComputeHashCode();
		this.valueToken = valueToken;
	}

	public UnaryFilterExpression(UnaryFilterExpression unaryFilterExpression, int valueToken)
		: base(unaryFilterExpression.ExpressionType)
	{
		value = unaryFilterExpression.value;
		expressionContext = unaryFilterExpression.expressionContext;
		hashCode = unaryFilterExpression.hashCode;
		this.valueToken = valueToken;
	}

	public override bool Evaluate(NavValue input)
	{
		int num = expressionContext.Compare(input, Value);
		return ExpressionType switch
		{
			FilterExpressionType.Equal => num == 0, 
			FilterExpressionType.NotEqual => num != 0, 
			FilterExpressionType.GreaterThan => num > 0, 
			FilterExpressionType.LessThan => num < 0, 
			FilterExpressionType.GreaterThanOrEqual => num >= 0, 
			FilterExpressionType.LessThanOrEqual => num <= 0, 
			_ => throw new NotSupportedException(), 
		};
	}

	public override RangeList ToRangeList()
	{
		return ExpressionType switch
		{
			FilterExpressionType.Equal => RangeList.Create(Range.CreateForSingleValue(Value, expressionContext)), 
			FilterExpressionType.NotEqual => RangeList.CreateForEverythingExceptASingleValue(Value, expressionContext), 
			FilterExpressionType.GreaterThan => RangeList.Create(Range.CreateToMaximum(Value, lowIsInclusive: false, expressionContext)), 
			FilterExpressionType.LessThan => RangeList.Create(Range.CreateFromMinimum(Value, highIsInclusive: false, expressionContext)), 
			FilterExpressionType.GreaterThanOrEqual => RangeList.Create(Range.CreateToMaximum(Value, lowIsInclusive: true, expressionContext)), 
			FilterExpressionType.LessThanOrEqual => RangeList.Create(Range.CreateFromMinimum(Value, highIsInclusive: true, expressionContext)), 
			_ => throw new NotSupportedException(), 
		};
	}

	public bool Equals(UnaryFilterExpression other)
	{
		if (other != null)
		{
			if (this != other)
			{
				if (hashCode == other.hashCode && ExpressionType == other.ExpressionType && expressionContext.Equals(other.expressionContext))
				{
					return value.Equals(other.value);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return hashCode;
	}

	private int ComputeHashCode()
	{
		return HashCodeHelper.CombineHashCodes((int)ExpressionType, expressionContext.GetHashCode(), value.GetHashCode());
	}
}

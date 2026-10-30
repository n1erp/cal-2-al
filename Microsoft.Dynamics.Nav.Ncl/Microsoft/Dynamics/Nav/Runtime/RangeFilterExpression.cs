using System;
using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class RangeFilterExpression : FilterExpression, IEquatable<RangeFilterExpression>
{
	private readonly NavValue highValue;

	private readonly int highValueToken;

	private readonly FilterExpressionContext expressionContext;

	private readonly NavValue lowValue;

	private readonly int lowValueToken;

	private readonly int hashCode;

	public override int ApproximateByteSize => base.ApproximateByteSize + SizableObject.GetApproximateByteSize(expressionContext) + SizableObject.GetApproximateByteSize(highValue, lowValue) + SizableObject.GetApproximateByteSize(6) + 24;

	public NavValue LowValue
	{
		[DebuggerStepThrough]
		get
		{
			return lowValue;
		}
	}

	public int LowValueToken
	{
		[DebuggerStepThrough]
		get
		{
			return lowValueToken;
		}
	}

	public NavValue HighValue
	{
		[DebuggerStepThrough]
		get
		{
			return highValue;
		}
	}

	public int HighValueToken
	{
		[DebuggerStepThrough]
		get
		{
			return highValueToken;
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

	public RangeFilterExpression(FilterExpressionType filterExpressionType, NavValue lowValue, NavValue highValue, FilterExpressionContext expressionContext)
		: base(filterExpressionType)
	{
		FilterExpression.ValidateNotNull(expressionContext, "expressionContext");
		switch (filterExpressionType)
		{
		case FilterExpressionType.RangeBetweenInclusive:
			FilterExpression.ValidateNotNull(lowValue, "lowValue");
			FilterExpression.ValidateNotNull(highValue, "highValue");
			break;
		case FilterExpressionType.RangeFromInclusive:
			FilterExpression.ValidateNotNull(lowValue, "lowValue");
			FilterExpression.ValidateNull(highValue, "highValue");
			break;
		case FilterExpressionType.RangeToInclusive:
			FilterExpression.ValidateNotNull(highValue, "highValue");
			FilterExpression.ValidateNull(lowValue, "lowValue");
			break;
		default:
			throw new ArgumentOutOfRangeException("filterExpressionType");
		}
		this.highValue = highValue;
		this.lowValue = lowValue;
		this.expressionContext = expressionContext;
		hashCode = ComputeHashCode();
		lowValueToken = -1;
		highValueToken = -1;
	}

	public RangeFilterExpression(RangeFilterExpression rangeFilterExpression, int lowValueToken, int highValueToken)
		: base(rangeFilterExpression.ExpressionType)
	{
		highValue = rangeFilterExpression.highValue;
		lowValue = rangeFilterExpression.lowValue;
		expressionContext = rangeFilterExpression.expressionContext;
		hashCode = rangeFilterExpression.hashCode;
		this.lowValueToken = lowValueToken;
		this.highValueToken = highValueToken;
	}

	public override bool Evaluate(NavValue input)
	{
		switch (ExpressionType)
		{
		case FilterExpressionType.RangeBetweenInclusive:
			if (expressionContext.Compare(input, lowValue) >= 0)
			{
				return expressionContext.Compare(input, highValue) <= 0;
			}
			return false;
		case FilterExpressionType.RangeFromInclusive:
			return expressionContext.Compare(input, lowValue) >= 0;
		case FilterExpressionType.RangeToInclusive:
			return expressionContext.Compare(input, highValue) <= 0;
		default:
			throw new NotSupportedException();
		}
	}

	public override RangeList ToRangeList()
	{
		return ExpressionType switch
		{
			FilterExpressionType.RangeBetweenInclusive => RangeList.Create(Range.Create(LowValue, lowIsInclusive: true, HighValue, highIsInclusive: true, expressionContext)), 
			FilterExpressionType.RangeFromInclusive => RangeList.Create(Range.CreateToMaximum(LowValue, lowIsInclusive: true, expressionContext)), 
			FilterExpressionType.RangeToInclusive => RangeList.Create(Range.CreateFromMinimum(HighValue, highIsInclusive: true, expressionContext)), 
			_ => throw new NotSupportedException(), 
		};
	}

	public bool Equals(RangeFilterExpression other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null || hashCode != other.hashCode || ExpressionType != other.ExpressionType || !expressionContext.Equals(other.ExpressionContext))
		{
			return false;
		}
		switch (ExpressionType)
		{
		case FilterExpressionType.RangeBetweenInclusive:
			if (expressionContext.Compare(lowValue, other.lowValue) == 0)
			{
				return expressionContext.Compare(highValue, other.highValue) == 0;
			}
			return false;
		case FilterExpressionType.RangeFromInclusive:
			return expressionContext.Compare(lowValue, other.lowValue) == 0;
		case FilterExpressionType.RangeToInclusive:
			return expressionContext.Compare(highValue, other.highValue) == 0;
		default:
			return false;
		}
	}

	public override int GetHashCode()
	{
		return hashCode;
	}

	private int ComputeHashCode()
	{
		return ExpressionType switch
		{
			FilterExpressionType.RangeBetweenInclusive => HashCodeHelper.CombineHashCodes((int)ExpressionType, expressionContext.GetHashCode(), lowValue.GetHashCode(), highValue.GetHashCode()), 
			FilterExpressionType.RangeFromInclusive => HashCodeHelper.CombineHashCodes((int)ExpressionType, expressionContext.GetHashCode(), lowValue.GetHashCode()), 
			FilterExpressionType.RangeToInclusive => HashCodeHelper.CombineHashCodes((int)ExpressionType, expressionContext.GetHashCode(), highValue.GetHashCode()), 
			_ => throw new NotSupportedException(), 
		};
	}
}

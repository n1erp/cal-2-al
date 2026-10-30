using System;
using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class BinaryFilterExpression : FilterExpression, IEquatable<BinaryFilterExpression>
{
	private readonly FilterExpression left;

	private readonly FilterExpression right;

	private readonly int hashCode;

	public override int ApproximateByteSize => base.ApproximateByteSize + SizableObject.GetApproximateByteSize(left, right) + SizableObject.GetApproximateByteSize(3) + 24;

	public FilterExpression Left
	{
		[DebuggerStepThrough]
		get
		{
			return left;
		}
	}

	public FilterExpression Right
	{
		[DebuggerStepThrough]
		get
		{
			return right;
		}
	}

	public BinaryFilterExpression(FilterExpressionType filterExpressionType, FilterExpression left, FilterExpression right)
		: base(filterExpressionType)
	{
		FilterExpression.ValidateNotNull(left, "left");
		FilterExpression.ValidateNotNull(right, "right");
		if ((uint)(filterExpressionType - 6) > 1u)
		{
			throw new ArgumentOutOfRangeException("filterExpressionType");
		}
		this.left = left;
		this.right = right;
		hashCode = ComputeHashCode();
	}

	public override bool Evaluate(NavValue input)
	{
		switch (ExpressionType)
		{
		case FilterExpressionType.And:
			if (Left.Evaluate(input))
			{
				return Right.Evaluate(input);
			}
			return false;
		case FilterExpressionType.Or:
			if (!Left.Evaluate(input))
			{
				return Right.Evaluate(input);
			}
			return true;
		default:
			throw new NotSupportedException();
		}
	}

	public override RangeList ToRangeList()
	{
		RangeList rangeList = Left.ToRangeList();
		RangeList rangeList2 = Right.ToRangeList();
		return ExpressionType switch
		{
			FilterExpressionType.And => RangeList.Intersect(rangeList, rangeList2), 
			FilterExpressionType.Or => RangeList.Union(rangeList, rangeList2), 
			_ => throw new NotSupportedException(), 
		};
	}

	public bool Equals(BinaryFilterExpression other)
	{
		if (other != null)
		{
			if (this != other)
			{
				if (hashCode == other.hashCode && other.ExpressionType == ExpressionType && left.Equals(other.left))
				{
					return right.Equals(other.right);
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

	internal BinaryFilterExpression Update(FilterExpressionType newExpressionType, FilterExpression newLeft, FilterExpression newRight)
	{
		if (newLeft == Left && newRight == Right && newExpressionType == ExpressionType)
		{
			return this;
		}
		return new BinaryFilterExpression(newExpressionType, newLeft, newRight);
	}

	private int ComputeHashCode()
	{
		return HashCodeHelper.CombineHashCodes((int)ExpressionType, left.GetHashCode(), right.GetHashCode());
	}
}

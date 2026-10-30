using System;
using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class BooleanConstantFilterExpression : FilterExpression, IEquatable<BooleanConstantFilterExpression>
{
	private static readonly int trueHashCode = HashCodeHelper.CombineHashCodes(FilterExpressionType.BooleanConstant.GetHashCode(), true.GetHashCode());

	private static readonly int falseHashCode = HashCodeHelper.CombineHashCodes(FilterExpressionType.BooleanConstant.GetHashCode(), false.GetHashCode());

	private static readonly BooleanConstantFilterExpression trueExpression = new BooleanConstantFilterExpression(value: true);

	private static readonly BooleanConstantFilterExpression falseExpression = new BooleanConstantFilterExpression(value: false);

	private readonly bool value;

	public override int ApproximateByteSize => base.ApproximateByteSize + SizableObject.GetApproximateByteSize(1) + 24;

	public bool Value
	{
		[DebuggerStepThrough]
		get
		{
			return value;
		}
	}

	public BooleanConstantFilterExpression(bool value)
		: base(FilterExpressionType.BooleanConstant)
	{
		this.value = value;
	}

	public override bool Evaluate(NavValue input)
	{
		return Value;
	}

	public override RangeList ToRangeList()
	{
		if (!Value)
		{
			return RangeList.CreateEmpty();
		}
		return RangeList.CreateFull();
	}

	public bool Equals(BooleanConstantFilterExpression other)
	{
		if (other != null)
		{
			return value == other.value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (!value)
		{
			return falseHashCode;
		}
		return trueHashCode;
	}

	public static BooleanConstantFilterExpression Get(bool value)
	{
		if (!value)
		{
			return falseExpression;
		}
		return trueExpression;
	}
}

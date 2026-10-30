using System;
using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class FieldEqualsFieldFilterExpression : FilterExpression, IEquatable<FieldEqualsFieldFilterExpression>
{
	private readonly INavFieldMetadata fieldToCompareTo;

	private readonly FilterExpressionContext expressionContext;

	private readonly int hashCode;

	public override int ApproximateByteSize => base.ApproximateByteSize + SizableObject.GetApproximateByteSize(expressionContext) + SizableObject.GetApproximateByteSize(3) + 24;

	public INavFieldMetadata FieldToCompareTo
	{
		[DebuggerStepThrough]
		get
		{
			return fieldToCompareTo;
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

	public FieldEqualsFieldFilterExpression(INavFieldMetadata fieldToCompareTo, FilterExpressionContext expressionContext)
		: base(FilterExpressionType.FieldEqualsField)
	{
		FilterExpression.ValidateNotNull(fieldToCompareTo, "fieldToCompareTo");
		FilterExpression.ValidateNotNull(expressionContext, "expressionContext");
		this.fieldToCompareTo = fieldToCompareTo;
		this.expressionContext = expressionContext;
		hashCode = HashCodeHelper.CombineHashCodes(this.fieldToCompareTo.GetHashCode(), this.expressionContext.GetHashCode());
	}

	public override bool Evaluate(NavValue input)
	{
		throw new NotSupportedException();
	}

	public override RangeList ToRangeList()
	{
		throw new NotSupportedException();
	}

	public bool Equals(FieldEqualsFieldFilterExpression other)
	{
		if (other != null)
		{
			if (this != other)
			{
				if (hashCode == other.hashCode && expressionContext.Equals(other.expressionContext))
				{
					return fieldToCompareTo.Equals(other.fieldToCompareTo);
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
}

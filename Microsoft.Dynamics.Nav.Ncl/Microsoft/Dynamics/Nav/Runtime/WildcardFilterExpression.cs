using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class WildcardFilterExpression : FilterExpression, IEquatable<WildcardFilterExpression>
{
	private readonly WildcardEvaluator evaluator;

	private readonly bool isCaseAndAccentInsensitive;

	private readonly FilterExpressionContext expressionContext;

	private readonly bool isNegated;

	private readonly string pattern;

	private readonly int hashCode;

	public override int ApproximateByteSize => base.ApproximateByteSize + SizableObject.GetApproximateByteSize(expressionContext) + SizableObject.GetApproximateByteSize(6) + 24 + ((pattern != null) ? (pattern.Length * 2) : 0);

	public string Pattern
	{
		[DebuggerStepThrough]
		get
		{
			return pattern;
		}
	}

	public bool IsCaseAndAccentInsensitive
	{
		[DebuggerStepThrough]
		get
		{
			return isCaseAndAccentInsensitive;
		}
	}

	public bool IsNegated
	{
		[DebuggerStepThrough]
		get
		{
			return isNegated;
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

	public WildcardFilterExpression(bool isNegated, string pattern, bool isCaseAndAccentInsensitive, FilterExpressionContext expressionContext)
		: base(FilterExpressionType.Wildcard)
	{
		FilterExpression.ValidateNotNull(pattern, "pattern");
		FilterExpression.ValidateNotNull(expressionContext, "expressionContext");
		this.isNegated = isNegated;
		this.isCaseAndAccentInsensitive = isCaseAndAccentInsensitive;
		this.expressionContext = expressionContext;
		this.pattern = pattern;
		if (this.expressionContext.Metadata.NclType == NavNclType.NavCode)
		{
			this.pattern = this.pattern.ToUpper(CultureInfo.InvariantCulture);
		}
		evaluator = new WildcardEvaluator(this);
		hashCode = ComputeHashCode();
	}

	public override bool Evaluate(NavValue input)
	{
		return evaluator.Evaluate(input.ToString(), expressionContext);
	}

	public override RangeList ToRangeList()
	{
		throw new CannotConvertWildcardIntoRangeException();
	}

	public bool Equals(WildcardFilterExpression other)
	{
		if (other != null)
		{
			if (this != other)
			{
				if (hashCode == other.hashCode && expressionContext.Equals(other.expressionContext) && pattern == other.pattern && isNegated == other.isNegated)
				{
					return isCaseAndAccentInsensitive == other.isCaseAndAccentInsensitive;
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
		return HashCodeHelper.CombineHashCodes((int)ExpressionType, expressionContext.GetHashCode(), pattern.GetHashCode(), Convert.ToInt32(isCaseAndAccentInsensitive) | (Convert.ToInt32(isNegated) << 1));
	}
}

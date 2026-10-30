using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class Range : IEquatable<Range>
{
	[Flags]
	private enum RangeFlags
	{
		LowIsMinimum = 1,
		LowIsIncluded = 2,
		HighIsMaximum = 4,
		HighIsIncluded = 8,
		FullRange = LowIsMinimum | LowIsIncluded | HighIsMaximum | HighIsIncluded,
		EmptyRange = 0x80
	}

	private static readonly Range emptyRange = new Range(RangeFlags.EmptyRange);

	private static readonly Range fullRange = new Range(RangeFlags.FullRange);

	private readonly RangeFlags flags;

	private readonly NavValue highValue;

	private readonly NavValue lowValue;

	private readonly FilterExpressionContext expressionContext;

	public static Range EmptyRange => emptyRange;

	public static Range FullRange => fullRange;

	public NavValue LowValue => lowValue;

	public NavValue HighValue => highValue;

	public FilterExpressionContext ExpressionContext => expressionContext;

	public bool IsEmptyRange => (flags & RangeFlags.EmptyRange) != 0;

	public bool IsFullRange => flags == RangeFlags.FullRange;

	public bool IsLowIsMinimum => (flags & RangeFlags.LowIsMinimum) != 0;

	public bool IsLowDefined => (flags & RangeFlags.LowIsMinimum) == 0;

	public bool IsLowIncluded => (flags & RangeFlags.LowIsIncluded) != 0;

	public bool IsLowExcluded => (flags & RangeFlags.LowIsIncluded) == 0;

	public bool IsHighMaximum => (flags & RangeFlags.HighIsMaximum) != 0;

	public bool IsHighDefined => (flags & RangeFlags.HighIsMaximum) == 0;

	public bool IsHighIncluded => (flags & RangeFlags.HighIsIncluded) != 0;

	public bool IsHighExcluded => (flags & RangeFlags.HighIsIncluded) == 0;

	private Range(RangeFlags flags)
	{
		this.flags = flags;
		if (IsEmptyRange)
		{
			this.flags = RangeFlags.EmptyRange;
		}
		else if (IsLowIsMinimum)
		{
			if (IsHighMaximum)
			{
				this.flags = RangeFlags.FullRange;
			}
			else
			{
				this.flags |= RangeFlags.LowIsIncluded;
			}
		}
		else if (IsHighMaximum)
		{
			this.flags |= RangeFlags.HighIsIncluded;
		}
	}

	private Range(Range lowSource, Range highSource)
	{
		if (lowSource.IsEmptyRange || highSource.IsEmptyRange)
		{
			flags = EmptyRange.flags;
			return;
		}
		if (lowSource.IsLowIsMinimum && highSource.IsHighMaximum)
		{
			flags = FullRange.flags;
			return;
		}
		expressionContext = ValidateAndGetExpressionContext(lowSource, highSource);
		if (lowSource.IsLowDefined)
		{
			lowValue = lowSource.LowValue;
		}
		if (highSource.IsHighDefined)
		{
			highValue = highSource.HighValue;
		}
		flags = (lowSource.flags & (RangeFlags.LowIsMinimum | RangeFlags.LowIsIncluded)) | (highSource.flags & (RangeFlags.HighIsMaximum | RangeFlags.HighIsIncluded));
	}

	private Range(NavValue value, FilterExpressionContext expressionContext)
	{
		lowValue = value;
		highValue = value;
		flags = RangeFlags.LowIsIncluded | RangeFlags.HighIsIncluded;
		this.expressionContext = expressionContext;
	}

	private Range(NavValue lowValue, NavValue highValue, RangeFlags flags, FilterExpressionContext expressionContext)
		: this(flags)
	{
		this.lowValue = lowValue;
		this.highValue = highValue;
		this.expressionContext = expressionContext;
	}

	public static Range CreateFromMinimum(NavValue highValue, bool highIsInclusive, FilterExpressionContext expressionContext)
	{
		return new Range(null, highValue, (RangeFlags)(1 | (highIsInclusive ? 8 : 0)), expressionContext);
	}

	public static Range CreateToMaximum(NavValue lowValue, bool lowIsInclusive, FilterExpressionContext expressionContext)
	{
		return new Range(lowValue, null, (RangeFlags)(4 | (lowIsInclusive ? 2 : 0)), expressionContext);
	}

	public static Range Create(NavValue lowValue, bool lowIsInclusive, NavValue highValue, bool highIsInclusive, FilterExpressionContext expressionContext)
	{
		return new Range(lowValue, highValue, (RangeFlags)((lowIsInclusive ? 2 : 0) | (highIsInclusive ? 8 : 0)), expressionContext);
	}

	public static Range CreateForSingleValue(NavValue value, FilterExpressionContext expressionContext)
	{
		return new Range(value, expressionContext);
	}

	public static Range CreateFull()
	{
		return FullRange;
	}

	public static Range Create(Range lowSource, Range highSource)
	{
		return new Range(lowSource, highSource);
	}

	public RangeComparison CompareForIntersection(Range other)
	{
		return Compare(other, attemptToCombineContiguousRanges: false);
	}

	public RangeComparison CompareForUnion(Range other)
	{
		return Compare(other, attemptToCombineContiguousRanges: true);
	}

	internal bool GetInclusiveIntegerBounds(int minimum, int maximum, out int low, out int high)
	{
		if (IsLowIsMinimum)
		{
			low = minimum;
		}
		else
		{
			low = LowValue.ToInt32();
			if (IsLowExcluded)
			{
				if (low == int.MaxValue)
				{
					high = int.MaxValue;
					return false;
				}
				low++;
			}
			if (low < minimum)
			{
				low = minimum;
			}
		}
		if (IsHighMaximum)
		{
			high = maximum;
		}
		else
		{
			high = HighValue.ToInt32();
			if (IsHighExcluded)
			{
				if (low == int.MinValue)
				{
					return false;
				}
				high--;
			}
			if (high > maximum)
			{
				high = maximum;
			}
		}
		return low <= high;
	}

	private RangeComparison Compare(Range other, bool attemptToCombineContiguousRanges)
	{
		if (IsFullRange)
		{
			if (!other.IsFullRange)
			{
				return RangeComparison.Subset;
			}
			return RangeComparison.SubsetEndsEqual;
		}
		if (other.IsFullRange)
		{
			return RangeComparison.Superset;
		}
		int num = CompareLowEnds(other);
		int num2 = CompareHighEnds(other);
		if (num > 0)
		{
			if (num2 > 0)
			{
				if (CompareHighAndLow(other, this, attemptToCombineContiguousRanges) < 0)
				{
					return RangeComparison.DisjointLower;
				}
				return RangeComparison.BeginsLower;
			}
			if (num2 < 0)
			{
				if (attemptToCombineContiguousRanges && AreHighEndsContiguouslyEquivaluent(other, this, expressionContext))
				{
					return RangeComparison.BeginsLowerEndsEqual;
				}
				return RangeComparison.Superset;
			}
			if (num2 == 0)
			{
				return RangeComparison.BeginsLowerEndsEqual;
			}
		}
		if (num2 > 0)
		{
			if (attemptToCombineContiguousRanges && AreHighEndsContiguouslyEquivaluent(this, other, expressionContext))
			{
				return RangeComparison.SubsetEndsEqual;
			}
			return RangeComparison.Subset;
		}
		if (num2 == 0)
		{
			return RangeComparison.SubsetEndsEqual;
		}
		if (CompareHighAndLow(this, other, attemptToCombineContiguousRanges) < 0)
		{
			return RangeComparison.DisjointHigher;
		}
		if (AreHighEndsContiguouslyEquivaluent(other, this, expressionContext))
		{
			return RangeComparison.SubsetEndsEqual;
		}
		return RangeComparison.EndsHigher;
	}

	private int CompareHighEnds(Range other)
	{
		if (IsHighMaximum)
		{
			if (!other.IsHighMaximum)
			{
				return 1;
			}
			return 0;
		}
		if (other.IsHighMaximum)
		{
			return -1;
		}
		int num = expressionContext.Compare(highValue, other.highValue);
		if (num == 0)
		{
			if (IsHighIncluded && other.IsHighExcluded)
			{
				num = 1;
			}
			if (IsHighExcluded && other.IsHighIncluded)
			{
				num = -1;
			}
		}
		return num;
	}

	private static bool AreHighEndsContiguouslyEquivaluent(Range greater, Range lesser, FilterExpressionContext expressionContext)
	{
		if (expressionContext.IsDiscrete && !greater.IsHighMaximum && greater.IsHighExcluded && lesser.IsHighIncluded)
		{
			return FilterExpressionContext.AreDiscreteValuesContiguous(lesser.highValue, greater.highValue);
		}
		return false;
	}

	private int CompareLowEnds(Range other)
	{
		if (IsLowIsMinimum)
		{
			if (!other.IsLowIsMinimum)
			{
				return -1;
			}
			return 0;
		}
		if (other.IsLowIsMinimum)
		{
			return 1;
		}
		int num = expressionContext.Compare(lowValue, other.lowValue);
		if (num == 0)
		{
			if (IsLowExcluded && other.IsLowIncluded)
			{
				return 1;
			}
			if (IsLowIncluded && other.IsLowExcluded)
			{
				return -1;
			}
		}
		return num;
	}

	private static int CompareHighAndLow(Range highSource, Range lowSource, bool attemptToCombine)
	{
		if (highSource.IsHighMaximum || lowSource.IsLowIsMinimum)
		{
			return 1;
		}
		FilterExpressionContext filterExpressionContext = highSource.expressionContext;
		int num = filterExpressionContext.Compare(highSource.highValue, lowSource.lowValue);
		if (num == 0)
		{
			if (lowSource.IsLowIncluded && highSource.IsHighIncluded)
			{
				return 0;
			}
			if (!attemptToCombine || (lowSource.IsLowExcluded && highSource.IsHighExcluded))
			{
				return -1;
			}
			return 0;
		}
		if (!filterExpressionContext.IsDiscrete)
		{
			return num;
		}
		if (num > 0)
		{
			if (highSource.IsHighExcluded && lowSource.IsLowExcluded && FilterExpressionContext.AreDiscreteValuesContiguous(lowSource.lowValue, highSource.highValue))
			{
				return -1;
			}
		}
		else if (attemptToCombine && lowSource.IsLowIncluded && highSource.IsHighIncluded && FilterExpressionContext.AreDiscreteValuesContiguous(highSource.highValue, lowSource.lowValue))
		{
			return 0;
		}
		return num;
	}

	private static FilterExpressionContext ValidateAndGetExpressionContext(Range range1, Range range2)
	{
		if (range1.expressionContext == null || range2.expressionContext == null || !range1.expressionContext.Equals(range2.expressionContext))
		{
			throw new NotSupportedException("Cannot compare ranges from different expression contexts.");
		}
		return range1.expressionContext;
	}

	public bool Equals(Range other)
	{
		if (this != other)
		{
			if (other != null && other.flags == flags && object.Equals(lowValue, other.lowValue) && object.Equals(highValue, other.highValue))
			{
				return object.Equals(expressionContext, other.expressionContext);
			}
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		return (IsLowIncluded ? '[' : '(') + (IsLowIsMinimum ? "" : ExpressionContext.QuoteLiteralIfNecessary(lowValue.ToString())) + ", " + (IsHighMaximum ? "" : ExpressionContext.QuoteLiteralIfNecessary(highValue.ToString())) + (IsHighIncluded ? ']' : ')');
	}
}

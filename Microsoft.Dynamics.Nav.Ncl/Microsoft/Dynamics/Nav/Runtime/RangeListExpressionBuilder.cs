using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class RangeListExpressionBuilder
{
	[Flags]
	private enum BoundaryAlreadyChecked
	{
		None = 0,
		Low = 1,
		High = 2
	}

	public static FilterExpression BuildExpression(RangeList rangeList, bool optimizeForEvaluation)
	{
		ReadOnlyCollection<Range> ranges = rangeList.Ranges;
		if (ranges.Count == 0)
		{
			return FilterExpression.BooleanConstant(value: false);
		}
		if (ranges[0].IsFullRange)
		{
			return FilterExpression.BooleanConstant(value: true);
		}
		FilterExpressionContext expressionContext = ranges[0].ExpressionContext;
		if (!optimizeForEvaluation || !expressionContext.CanCalculateMinMaxAndMid)
		{
			return BuildSimpleExpression(rangeList);
		}
		return BuildSearchExpression(rangeList, expressionContext);
	}

	private static FilterExpression BuildSimpleExpression(RangeList rangeList)
	{
		IEnumerable<IList<Range>> source = GroupByNotEqualChain(rangeList.Ranges);
		return FilterExpression.BalancedOr(source.Select(NotEqualChainToExpression));
	}

	private static IEnumerable<IList<Range>> GroupByNotEqualChain(IEnumerable<Range> ranges)
	{
		List<Range> list = new List<Range>();
		foreach (Range range in ranges)
		{
			if (list.Count == 0)
			{
				list.Add(range);
				continue;
			}
			Range highSource = list[list.Count - 1];
			if (IsNotEqualPattern(highSource, range))
			{
				list.Add(range);
				continue;
			}
			yield return list;
			list = new List<Range> { range };
		}
		if (list.Count != 0)
		{
			yield return list;
		}
	}

	private static FilterExpression NotEqualChainToExpression(IList<Range> notEqualChain)
	{
		if (notEqualChain.Count == 1)
		{
			return notEqualChain[0].ToFilterExpression();
		}
		Range range = Range.Create(notEqualChain[0], notEqualChain[notEqualChain.Count - 1]);
		FilterExpression filterExpression = FilterExpression.BalancedAnd(from r in notEqualChain.Take(notEqualChain.Count - 1)
			select FilterExpression.NotEqual(r.HighValue, r.ExpressionContext));
		if (!range.IsFullRange)
		{
			return FilterExpression.And(range.ToFilterExpression(), filterExpression);
		}
		return filterExpression;
	}

	private static FilterExpression BuildSearchExpression(RangeList rangeList, FilterExpressionContext expressionContext)
	{
		FilterExpression filterExpression = ComputeBalancedSearchExpression(rangeList.Ranges.ToList(), expressionContext.MinValue, expressionContext.MaxValue, BoundaryAlreadyChecked.None);
		return filterExpression.Visit(BasicSimplifyingVisitor.Instance);
	}

	private static FilterExpression ComputeBalancedSearchExpression(List<Range> ranges, NavValue minValue, NavValue maxValue, BoundaryAlreadyChecked boundaryAlreadyChecked)
	{
		switch (ranges.Count)
		{
		case 0:
			return FilterExpression.BooleanConstant(value: false);
		case 1:
			return boundaryAlreadyChecked switch
			{
				BoundaryAlreadyChecked.None => ranges[0].ToFilterExpression(), 
				BoundaryAlreadyChecked.Low => ranges[0].ToHighFilter(), 
				BoundaryAlreadyChecked.High => ranges[0].ToLowFilter(), 
				BoundaryAlreadyChecked.Low | BoundaryAlreadyChecked.High => null, 
				_ => throw new NotSupportedException(), 
			};
		default:
		{
			NavValue midPoint = ranges[0].ExpressionContext.Midpoint(minValue, maxValue);
			List<Range> list = ranges.Partition(out var notMatched, (Range r) => ValueIsInOrGreaterThanRange(r, midPoint));
			if (list.Count == 0)
			{
				if ((boundaryAlreadyChecked & BoundaryAlreadyChecked.Low) == 0 && !notMatched[0].IsLowIsMinimum)
				{
					return CallComputeBalancedSearchOnPartition(notMatched, leftPartition: false, isPartinioningOnNotEqual: false, minValue, maxValue, boundaryAlreadyChecked);
				}
				list.Add(notMatched[0]);
				notMatched.RemoveAt(0);
			}
			if (notMatched.Count == 0)
			{
				if ((boundaryAlreadyChecked & BoundaryAlreadyChecked.High) == 0 && !list.Last().IsHighMaximum)
				{
					return CallComputeBalancedSearchOnPartition(list, leftPartition: true, isPartinioningOnNotEqual: false, minValue, maxValue, boundaryAlreadyChecked);
				}
				notMatched.Add(list.Last());
				list.RemoveAt(list.Count - 1);
			}
			bool isPartinioningOnNotEqual = IsNotEqualPattern(list.Last(), notMatched.First());
			FilterExpression filterExpression = CallComputeBalancedSearchOnPartition(list, leftPartition: true, isPartinioningOnNotEqual, minValue, maxValue, boundaryAlreadyChecked);
			FilterExpression filterExpression2 = CallComputeBalancedSearchOnPartition(notMatched, leftPartition: false, isPartinioningOnNotEqual, minValue, maxValue, boundaryAlreadyChecked);
			double num = RelativeSizeOfRangeList(list);
			double num2 = RelativeSizeOfRangeList(notMatched);
			if (!(num >= num2))
			{
				return FilterExpression.Or(filterExpression2, filterExpression);
			}
			return FilterExpression.Or(filterExpression, filterExpression2);
		}
		}
	}

	private static FilterExpression CallComputeBalancedSearchOnPartition(List<Range> ranges, bool leftPartition, bool isPartinioningOnNotEqual, NavValue minValue, NavValue maxValue, BoundaryAlreadyChecked boundaryAlreadyChecked)
	{
		bool flag;
		BoundaryAlreadyChecked boundaryAlreadyChecked2;
		FilterExpression left;
		NavValue minValue2;
		NavValue maxValue2;
		if (leftPartition)
		{
			Range range = ranges.Last();
			flag = isPartinioningOnNotEqual || range.IsSingleValue();
			boundaryAlreadyChecked2 = (flag ? (boundaryAlreadyChecked & ~BoundaryAlreadyChecked.High) : (boundaryAlreadyChecked | BoundaryAlreadyChecked.High));
			left = range.ToHighFilter();
			minValue2 = minValue;
			maxValue2 = range.HighValue;
		}
		else
		{
			Range range = ranges[0];
			flag = isPartinioningOnNotEqual || range.IsSingleValue();
			boundaryAlreadyChecked2 = (flag ? (boundaryAlreadyChecked & ~BoundaryAlreadyChecked.Low) : (boundaryAlreadyChecked | BoundaryAlreadyChecked.Low));
			left = range.ToLowFilter();
			minValue2 = range.LowValue;
			maxValue2 = maxValue;
		}
		FilterExpression filterExpression = ComputeBalancedSearchExpression(ranges, minValue2, maxValue2, boundaryAlreadyChecked2);
		if (flag)
		{
			return filterExpression;
		}
		return FilterExpression.And(left, filterExpression);
	}

	private static bool IsNotEqualPattern(Range highSource, Range lowSource)
	{
		if (!highSource.IsHighMaximum && !lowSource.IsLowIsMinimum && highSource.IsHighExcluded && lowSource.IsLowExcluded)
		{
			return highSource.ExpressionContext.Compare(highSource.HighValue, lowSource.LowValue) == 0;
		}
		return false;
	}

	private static double RelativeSizeOfRangeList(IEnumerable<Range> ranges)
	{
		return ranges.Sum((Range r) => FilterExpressionContext.RelativeDistanceBetwenValues(r.IsLowIsMinimum ? r.ExpressionContext.MinValue : r.LowValue, r.IsHighMaximum ? r.ExpressionContext.MaxValue : r.HighValue));
	}

	private static bool ValueIsInOrGreaterThanRange(Range range, NavValue value)
	{
		if (range.IsLowIsMinimum)
		{
			return true;
		}
		int num = range.ExpressionContext.Compare(range.LowValue, value);
		if (num >= 0)
		{
			if (num == 0)
			{
				return range.IsLowIncluded;
			}
			return false;
		}
		return true;
	}

	private static List<T> Partition<T>(this IEnumerable<T> source, out List<T> notMatched, Func<T, bool> predicate)
	{
		List<T> list = new List<T>();
		List<T> list2 = new List<T>();
		foreach (T item in source)
		{
			if (predicate(item))
			{
				list.Add(item);
			}
			else
			{
				list2.Add(item);
			}
		}
		notMatched = list2;
		return list;
	}

	private static FilterExpression ToHighFilter(this Range range)
	{
		if (!range.IsHighMaximum)
		{
			if (!range.IsHighIncluded)
			{
				return FilterExpression.LessThan(range.HighValue, range.ExpressionContext);
			}
			return FilterExpression.LessThanOrEqual(range.HighValue, range.ExpressionContext);
		}
		return null;
	}

	private static FilterExpression ToLowFilter(this Range range)
	{
		if (!range.IsLowIsMinimum)
		{
			if (!range.IsLowIncluded)
			{
				return FilterExpression.GreaterThan(range.LowValue, range.ExpressionContext);
			}
			return FilterExpression.GreaterThanOrEqual(range.LowValue, range.ExpressionContext);
		}
		return null;
	}

	private static bool IsSingleValue(this Range range)
	{
		if (!range.IsLowIsMinimum && !range.IsHighMaximum && range.IsLowIncluded && range.IsHighIncluded)
		{
			return range.ExpressionContext.Compare(range.LowValue, range.HighValue) == 0;
		}
		return false;
	}
}

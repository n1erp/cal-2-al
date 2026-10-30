using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class RangeList : IEquatable<RangeList>
{
	private static readonly RangeList empty = new RangeList();

	private static readonly RangeList full = Create(Range.CreateFull());

	private readonly List<Range> ranges;

	public ReadOnlyCollection<Range> Ranges => ranges.AsReadOnly();

	public bool IsEmpty => ranges.Count == 0;

	public bool IsFull
	{
		get
		{
			if (this != full)
			{
				if (Ranges.Count == 1)
				{
					return Ranges[0].IsFullRange;
				}
				return false;
			}
			return true;
		}
	}

	private RangeList(Range firstRange)
		: this()
	{
		ranges.Add(firstRange);
	}

	private RangeList(Range firstRange, Range secondRange)
		: this()
	{
		ranges.Add(firstRange);
		ranges.Add(secondRange);
	}

	private RangeList()
		: this(new List<Range>())
	{
	}

	private RangeList(List<Range> ranges)
	{
		this.ranges = ranges;
	}

	public static RangeList Create(Range singleRange)
	{
		return new RangeList(singleRange);
	}

	public static RangeList CreateForEverythingExceptASingleValue(NavValue value, FilterExpressionContext expressionContext)
	{
		return new RangeList(Range.CreateFromMinimum(value, highIsInclusive: false, expressionContext), Range.CreateToMaximum(value, lowIsInclusive: false, expressionContext));
	}

	public static RangeList CreateFull()
	{
		return full;
	}

	public static RangeList CreateEmpty()
	{
		return empty;
	}

	public static RangeList Union(RangeList rangeList1, RangeList rangeList2)
	{
		if (rangeList1.ranges.Count == 0)
		{
			return rangeList2;
		}
		if (rangeList2.ranges.Count == 0)
		{
			return rangeList1;
		}
		List<Range> list = rangeList1.ranges;
		List<Range> list2 = rangeList2.ranges;
		List<Range> list3 = new List<Range>(list.Count + list2.Count);
		if (list.Count > 2 && list2.Count > 2 && list[list.Count - 1].CompareForUnion(list2[0]) == RangeComparison.DisjointHigher)
		{
			list3.AddRange(list);
			list3.AddRange(list2);
			return new RangeList(list3);
		}
		Range range = list[0];
		Range range2 = list2[0];
		int num = 0;
		int num2 = 0;
		while (range != null && range2 != null)
		{
			bool flag = false;
			bool flag2 = false;
			switch (range.CompareForUnion(range2))
			{
			case RangeComparison.DisjointLower:
				list3.Add(range2);
				flag2 = true;
				break;
			case RangeComparison.BeginsLowerEndsEqual:
				list3.Add(range2);
				flag2 = true;
				flag = true;
				break;
			case RangeComparison.BeginsLower:
				range = Range.Create(range2, range);
				flag2 = true;
				break;
			case RangeComparison.SubsetEndsEqual:
				list3.Add(range);
				flag2 = true;
				flag = true;
				break;
			case RangeComparison.Subset:
				flag2 = true;
				break;
			case RangeComparison.EndsHigher:
				range2 = Range.Create(range, range2);
				flag = true;
				break;
			case RangeComparison.DisjointHigher:
				list3.Add(range);
				flag = true;
				break;
			case RangeComparison.Superset:
				flag = true;
				break;
			default:
				throw new NotSupportedException();
			}
			if (flag)
			{
				range = ((++num < list.Count) ? list[num] : null);
			}
			if (flag2)
			{
				range2 = ((++num2 < list2.Count) ? list2[num2] : null);
			}
		}
		while (range2 != null)
		{
			list3.Add(range2);
			range2 = ((++num2 < list2.Count) ? list2[num2] : null);
		}
		while (range != null)
		{
			list3.Add(range);
			range = ((++num < list.Count) ? list[num] : null);
		}
		return new RangeList(list3);
	}

	public static RangeList Union(RangeList rangeList1, RangeList rangeList2, params RangeList[] other)
	{
		RangeList rangeList3 = Union(rangeList1, rangeList2);
		if (other != null)
		{
			foreach (RangeList rangeList4 in other)
			{
				rangeList3 = Union(rangeList3, rangeList4);
			}
		}
		return rangeList3;
	}

	public static RangeList Intersect(RangeList rangeList1, RangeList rangeList2)
	{
		List<Range> list = new List<Range>(Math.Max(rangeList1.ranges.Count, rangeList2.ranges.Count));
		int num = 0;
		int num2 = 0;
		while (num < rangeList1.ranges.Count && num2 < rangeList2.ranges.Count)
		{
			Range range = rangeList1.ranges[num];
			Range range2 = rangeList2.ranges[num2];
			switch (range.CompareForIntersection(range2))
			{
			case RangeComparison.DisjointLower:
				num2++;
				break;
			case RangeComparison.BeginsLower:
				list.Add(Range.Create(range, range2));
				num2++;
				break;
			case RangeComparison.BeginsLowerEndsEqual:
				list.Add(Range.Create(range, range2));
				num++;
				num2++;
				break;
			case RangeComparison.Subset:
				list.Add(range2);
				num2++;
				break;
			case RangeComparison.SubsetEndsEqual:
				list.Add(range2);
				num++;
				num2++;
				break;
			case RangeComparison.EndsHigher:
				list.Add(Range.Create(range2, range));
				num++;
				break;
			case RangeComparison.DisjointHigher:
				num++;
				break;
			case RangeComparison.Superset:
				list.Add(range);
				num++;
				break;
			default:
				throw new NotSupportedException();
			}
		}
		if (list.Count != 0)
		{
			return new RangeList(list);
		}
		return empty;
	}

	public bool Equals(RangeList other)
	{
		if (other == null || ranges.Count != other.ranges.Count)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		for (int i = 0; i < Ranges.Count; i++)
		{
			Range range = Ranges[i];
			Range other2 = other.Ranges[i];
			if (!range.Equals(other2))
			{
				return false;
			}
		}
		return true;
	}

	public override string ToString()
	{
		return string.Join(" | ", ranges.Select((Range r) => r.ToString()));
	}
}

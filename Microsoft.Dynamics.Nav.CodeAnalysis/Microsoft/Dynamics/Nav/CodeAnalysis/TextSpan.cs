using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public struct TextSpan : IEquatable<TextSpan>, IComparable<TextSpan>
{
	private readonly int start;

	private readonly int length;

	public int Start => start;

	public int End => start + Length;

	public int Length => length;

	public bool IsEmpty => Length == 0;

	public TextSpan(int start, int length)
	{
		if (start < 0)
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (start + length < start)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		this.start = start;
		this.length = length;
	}

	public bool Contains(int position)
	{
		return (uint)(position - Start) <= (uint)Length;
	}

	public bool Contains(TextSpan span)
	{
		if (span.Start >= Start)
		{
			return span.End <= End;
		}
		return false;
	}

	public bool OverlapsWith(TextSpan span)
	{
		int num = Math.Max(Start, span.Start);
		int num2 = Math.Min(End, span.End);
		return num < num2;
	}

	public TextSpan? Overlap(TextSpan span)
	{
		int num = Math.Max(Start, span.Start);
		int num2 = Math.Min(End, span.End);
		if (num >= num2)
		{
			return null;
		}
		return FromBounds(num, num2);
	}

	public bool IntersectsWith(TextSpan span)
	{
		if (span.Start <= End)
		{
			return span.End >= Start;
		}
		return false;
	}

	public bool IntersectsWith(int position)
	{
		return (uint)(position - Start) <= (uint)Length;
	}

	public TextSpan? Intersection(TextSpan span)
	{
		int num = Math.Max(Start, span.Start);
		int num2 = Math.Min(End, span.End);
		if (num > num2)
		{
			return null;
		}
		return FromBounds(num, num2);
	}

	public static TextSpan FromBounds(int start, int end)
	{
		if (start < 0)
		{
			throw new ArgumentOutOfRangeException("start", CodeAnalysisResources.StartMustNotBeNegative);
		}
		if (end < start)
		{
			throw new ArgumentOutOfRangeException("end", CodeAnalysisResources.EndMustNotBeLessThanStart);
		}
		return new TextSpan(start, end - start);
	}

	public static bool operator ==(TextSpan left, TextSpan right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TextSpan left, TextSpan right)
	{
		return !left.Equals(right);
	}

	public bool Equals(TextSpan other)
	{
		if (Start == other.Start)
		{
			return Length == other.Length;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is TextSpan)
		{
			return Equals((TextSpan)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Start ^ Length;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "({0}..{1})", Start, End);
	}

	public int CompareTo(TextSpan other)
	{
		int num = Start - other.Start;
		if (num != 0)
		{
			return num;
		}
		return Length - other.Length;
	}

	public static bool operator >(TextSpan left, TextSpan right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator <(TextSpan left, TextSpan right)
	{
		return left.CompareTo(right) < 0;
	}
}

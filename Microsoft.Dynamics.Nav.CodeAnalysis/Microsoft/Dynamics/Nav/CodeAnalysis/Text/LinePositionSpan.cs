using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

public struct LinePositionSpan : IEquatable<LinePositionSpan>
{
	private readonly LinePosition start;

	private readonly LinePosition end;

	public LinePosition Start => start;

	public LinePosition End => end;

	public LinePositionSpan(LinePosition start, LinePosition end)
	{
		if (end < start)
		{
			throw new ArgumentException(CodeAnalysisResources.EndMustNotBeLessThanStart, "end");
		}
		this.start = start;
		this.end = end;
	}

	public override bool Equals(object obj)
	{
		if (obj is LinePositionSpan)
		{
			return Equals((LinePositionSpan)obj);
		}
		return false;
	}

	public bool Equals(LinePositionSpan other)
	{
		if (start.Equals(other.start))
		{
			return end.Equals(other.end);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(start.GetHashCode(), end.GetHashCode());
	}

	public static bool operator ==(LinePositionSpan left, LinePositionSpan right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(LinePositionSpan left, LinePositionSpan right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "({0})-({1})", start, end);
	}
}

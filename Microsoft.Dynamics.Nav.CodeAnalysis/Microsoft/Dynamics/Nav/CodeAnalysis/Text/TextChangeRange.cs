using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

public struct TextChangeRange : IEquatable<TextChangeRange>
{
	public TextSpan Span { get; }

	public int NewLength { get; }

	public static IReadOnlyList<TextChangeRange> NoChanges => SpecializedCollections.EmptyReadOnlyList<TextChangeRange>();

	public TextChangeRange(TextSpan span, int newLength)
	{
		this = default;
		if (newLength < 0)
		{
			throw new ArgumentOutOfRangeException("newLength");
		}
		Span = span;
		NewLength = newLength;
	}

	public bool Equals(TextChangeRange other)
	{
		if (other.Span == Span)
		{
			return other.NewLength == NewLength;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is TextChangeRange)
		{
			return Equals((TextChangeRange)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(NewLength, Span.GetHashCode());
	}

	public static bool operator ==(TextChangeRange left, TextChangeRange right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TextChangeRange left, TextChangeRange right)
	{
		return !(left == right);
	}

	public static TextChangeRange Collapse(IEnumerable<TextChangeRange> changes)
	{
		int num = 0;
		int num2 = int.MaxValue;
		int num3 = 0;
		foreach (TextChangeRange change in changes)
		{
			num += change.NewLength - change.Span.Length;
			if (change.Span.Start < num2)
			{
				num2 = change.Span.Start;
			}
			if (change.Span.End > num3)
			{
				num3 = change.Span.End;
			}
		}
		if (num2 > num3)
		{
			return default;
		}
		TextSpan span = TextSpan.FromBounds(num2, num3);
		int newLength = span.Length + num;
		return new TextChangeRange(span, newLength);
	}
}

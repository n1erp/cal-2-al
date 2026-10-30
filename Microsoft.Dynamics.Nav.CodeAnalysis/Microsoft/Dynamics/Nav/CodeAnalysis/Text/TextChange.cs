using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

public struct TextChange : IEquatable<TextChange>
{
	public TextSpan Span { get; }

	public string NewText { get; }

	public static IReadOnlyList<TextChange> NoChanges => SpecializedCollections.EmptyReadOnlyList<TextChange>();

	public TextChange(TextSpan span, string newText)
	{
		this = default;
		if (newText == null)
		{
			throw new ArgumentNullException("newText");
		}
		Span = span;
		NewText = newText;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}: {{ {1}, \"{2}\" }}", GetType().Name, Span, NewText);
	}

	public override bool Equals(object obj)
	{
		if (obj is TextChange)
		{
			return Equals((TextChange)obj);
		}
		return false;
	}

	public bool Equals(TextChange other)
	{
		if (EqualityComparer<TextSpan>.Default.Equals(Span, other.Span))
		{
			return EqualityComparer<string>.Default.Equals(NewText, other.NewText);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Span.GetHashCode(), NewText.GetHashCode());
	}

	public static bool operator ==(TextChange left, TextChange right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TextChange left, TextChange right)
	{
		return !(left == right);
	}

	public static implicit operator TextChangeRange(TextChange change)
	{
		return new TextChangeRange(change.Span, change.NewText.Length);
	}
}

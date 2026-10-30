using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

public struct TextLine : IEquatable<TextLine>
{
	private readonly SourceText text;

	private readonly int start;

	private readonly int endIncludingBreaks;

	public SourceText Text => text;

	public int LineNumber
	{
		get
		{
			if (text == null)
			{
				return 0;
			}
			return text.Lines.IndexOf(start);
		}
	}

	public int Start => start;

	public int End => endIncludingBreaks - LineBreakLength;

	private int LineBreakLength
	{
		get
		{
			if (text == null || text.Length == 0 || endIncludingBreaks == start)
			{
				return 0;
			}
			TextUtilities.GetStartAndLengthOfLineBreakEndingAt(text, endIncludingBreaks - 1, out var _, out var lengthLineBreak);
			return lengthLineBreak;
		}
	}

	public int EndIncludingLineBreak => endIncludingBreaks;

	public TextSpan Span => TextSpan.FromBounds(Start, End);

	public TextSpan SpanIncludingLineBreak => TextSpan.FromBounds(Start, EndIncludingLineBreak);

	private TextLine(SourceText text, int start, int endIncludingBreaks)
	{
		this.text = text;
		this.start = start;
		this.endIncludingBreaks = endIncludingBreaks;
	}

	public static TextLine FromSpan(SourceText text, TextSpan span)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (span.Start > text.Length || span.Start < 0 || span.End > text.Length)
		{
			throw new ArgumentOutOfRangeException("span");
		}
		if (text.Length > 0)
		{
			if (span.Start > 0 && !TextUtilities.IsAnyLineBreakCharacter(text[span.Start - 1]))
			{
				throw new ArgumentOutOfRangeException("span", CodeAnalysisResources.SpanDoesNotIncludeStartOfLine);
			}
			bool flag = false;
			if (span.End > span.Start)
			{
				flag = TextUtilities.IsAnyLineBreakCharacter(text[span.End - 1]);
			}
			if (!flag && span.End < text.Length)
			{
				int lengthOfLineBreak = TextUtilities.GetLengthOfLineBreak(text, span.End);
				if (lengthOfLineBreak > 0)
				{
					flag = true;
					span = new TextSpan(span.Start, span.Length + lengthOfLineBreak);
				}
			}
			if (span.End < text.Length && !flag)
			{
				throw new ArgumentOutOfRangeException("span", CodeAnalysisResources.SpanDoesNotIncludeEndOfLine);
			}
			return new TextLine(text, span.Start, span.End);
		}
		return new TextLine(text, 0, 0);
	}

	public override string ToString()
	{
		if (text == null || text.Length == 0)
		{
			return string.Empty;
		}
		return text.ToString(Span);
	}

	public static bool operator ==(TextLine left, TextLine right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TextLine left, TextLine right)
	{
		return !left.Equals(right);
	}

	public bool Equals(TextLine other)
	{
		if (other.text == text && other.start == start)
		{
			return other.endIncludingBreaks == endIncludingBreaks;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is TextLine)
		{
			return Equals((TextLine)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(text.GetHashCode(), start.GetHashCode(), endIncludingBreaks.GetHashCode());
	}
}

using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Text;

public readonly struct TextLine : IEquatable<TextLine>
{
	private readonly SourceText _text;

	private readonly int _start;

	private readonly int _endIncludingBreaks;

	public SourceText Text => _text;

	public int LineNumber => _text?.Lines.IndexOf(_start) ?? 0;

	public int Start => _start;

	public int End => _endIncludingBreaks - LineBreakLength;

	private int LineBreakLength
	{
		get
		{
			if (_text == null || _text.Length == 0 || _endIncludingBreaks == _start)
			{
				return 0;
			}
			TextUtilities.GetStartAndLengthOfLineBreakEndingAt(_text, _endIncludingBreaks - 1, out var _, out var lengthLinebreak);
			return lengthLinebreak;
		}
	}

	public int EndIncludingLineBreak => _endIncludingBreaks;

	public TextSpan Span => TextSpan.FromBounds(Start, End);

	public TextSpan SpanIncludingLineBreak => TextSpan.FromBounds(Start, EndIncludingLineBreak);

	private TextLine(SourceText text, int start, int endIncludingBreaks)
	{
		_text = text;
		_start = start;
		_endIncludingBreaks = endIncludingBreaks;
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
		if (_text == null || _text.Length == 0)
		{
			return string.Empty;
		}
		return _text.ToString(Span);
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
		if (other._text == _text && other._start == _start)
		{
			return other._endIncludingBreaks == _endIncludingBreaks;
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
		return Hash.Combine(_text, Hash.Combine(_start, _endIncludingBreaks));
	}
}

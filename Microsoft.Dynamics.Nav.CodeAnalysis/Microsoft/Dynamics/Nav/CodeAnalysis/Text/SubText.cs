using System;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

internal sealed class SubText : SourceText
{
	public override Encoding Encoding => UnderlyingText.Encoding;

	public SourceText UnderlyingText { get; private set; }

	public TextSpan UnderlyingSpan { get; private set; }

	public override int Length => UnderlyingSpan.Length;

	public override char this[int position]
	{
		get
		{
			if (position < 0 || position > Length)
			{
				throw new ArgumentOutOfRangeException("position");
			}
			return UnderlyingText[UnderlyingSpan.Start + position];
		}
	}

	public SubText(SourceText text, TextSpan span)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (span.Start < 0 || span.Start >= text.Length || span.End < 0 || span.End > text.Length)
		{
			throw new ArgumentOutOfRangeException("span");
		}
		UnderlyingText = text;
		UnderlyingSpan = span;
	}

	public override string ToString(TextSpan span)
	{
		CheckSubSpan(span);
		return UnderlyingText.ToString(GetCompositeSpan(span.Start, span.Length));
	}

	public override SourceText GetSubText(TextSpan span)
	{
		CheckSubSpan(span);
		return new SubText(UnderlyingText, GetCompositeSpan(span.Start, span.Length));
	}

	public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		TextSpan compositeSpan = GetCompositeSpan(sourceIndex, count);
		UnderlyingText.CopyTo(compositeSpan.Start, destination, destinationIndex, compositeSpan.Length);
	}

	private TextSpan GetCompositeSpan(int start, int length)
	{
		int num = Math.Min(UnderlyingText.Length, UnderlyingSpan.Start + start);
		int num2 = Math.Min(UnderlyingText.Length, num + length);
		return new TextSpan(num, num2 - num);
	}
}

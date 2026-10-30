using System;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

internal sealed class StringBuilderText : SourceText
{
	private readonly StringBuilder builder;

	private readonly Encoding encoding;

	public override Encoding Encoding => encoding;

	internal StringBuilder Builder => builder;

	public override int Length => builder.Length;

	public override char this[int position]
	{
		get
		{
			if (position < 0 || position >= builder.Length)
			{
				throw new ArgumentOutOfRangeException("position");
			}
			return builder[position];
		}
	}

	public StringBuilderText(StringBuilder builder, Encoding encoding)
	{
		DebugAssertHelper.Assert(builder != null);
		this.builder = builder;
		this.encoding = encoding;
	}

	public override string ToString(TextSpan span)
	{
		if (span.End > builder.Length)
		{
			throw new ArgumentOutOfRangeException("span");
		}
		return builder.ToString(span.Start, span.Length);
	}

	public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		builder.CopyTo(sourceIndex, destination, destinationIndex, count);
	}
}

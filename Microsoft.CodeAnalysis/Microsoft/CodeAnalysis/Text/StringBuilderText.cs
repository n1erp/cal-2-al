using System;
using System.Text;

namespace Microsoft.CodeAnalysis.Text;

internal sealed class StringBuilderText : SourceText
{
	private readonly StringBuilder _builder;

	private readonly Encoding _encodingOpt;

	public override Encoding Encoding => _encodingOpt;

	internal StringBuilder Builder => _builder;

	public override int Length => _builder.Length;

	public override char this[int position]
	{
		get
		{
			if (position < 0 || position >= _builder.Length)
			{
				throw new ArgumentOutOfRangeException("position");
			}
			return _builder[position];
		}
	}

	public StringBuilderText(StringBuilder builder, Encoding encodingOpt, SourceHashAlgorithm checksumAlgorithm)
		: base(default, checksumAlgorithm)
	{
		_builder = builder;
		_encodingOpt = encodingOpt;
	}

	public override string ToString(TextSpan span)
	{
		if (span.End > _builder.Length)
		{
			throw new ArgumentOutOfRangeException("span");
		}
		return _builder.ToString(span.Start, span.Length);
	}

	public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		_builder.CopyTo(sourceIndex, destination, destinationIndex, count);
	}
}

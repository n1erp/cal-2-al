using System;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

internal sealed class StringText : SourceText
{
	public override Encoding Encoding { get; }

	public string Source { get; }

	public override int Length => Source.Length;

	public override char this[int position] => Source[position];

	internal StringText(string source, Encoding encodingOpt)
	{
		DebugAssertHelper.Assert(source != null);
		Source = source;
		Encoding = encodingOpt;
	}

	public override string ToString(TextSpan span)
	{
		if (span.End > Source.Length)
		{
			throw new ArgumentOutOfRangeException("span");
		}
		if (span.Start == 0 && span.Length == Length)
		{
			return Source;
		}
		return Source.Substring(span.Start, span.Length);
	}

	public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		Source.CopyTo(sourceIndex, destination, destinationIndex, count);
	}
}

using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;

namespace Microsoft.CodeAnalysis.Text;

internal sealed class StringText : SourceText
{
	private readonly string _source;

	private readonly Encoding _encodingOpt;

	public override Encoding Encoding => _encodingOpt;

	public string Source => _source;

	public override int Length => _source.Length;

	public override char this[int position] => _source[position];

	internal StringText(string source, Encoding encodingOpt, ImmutableArray<byte> checksum = default(ImmutableArray<byte>), SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1, ImmutableArray<byte> embeddedTextBlob = default(ImmutableArray<byte>))
		: base(checksum, checksumAlgorithm, embeddedTextBlob)
	{
		_source = source;
		_encodingOpt = encodingOpt;
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

	public override void Write(TextWriter textWriter, TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (span.Start == 0 && span.End == Length)
		{
			textWriter.Write(Source);
		}
		else
		{
			base.Write(textWriter, span, cancellationToken);
		}
	}
}

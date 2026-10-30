using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Text;

internal abstract class SourceTextWriter : TextWriter
{
	public abstract SourceText ToSourceText();

	public static SourceTextWriter Create(Encoding encoding, SourceHashAlgorithm checksumAlgorithm, int length)
	{
		if (length < 40960)
		{
			return new StringTextWriter(encoding, checksumAlgorithm, length);
		}
		return new LargeTextWriter(encoding, checksumAlgorithm, length);
	}
}

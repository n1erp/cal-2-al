using System.Collections.Generic;
using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Text;

internal class SourceTextComparer : IEqualityComparer<SourceText>
{
	public static SourceTextComparer Instance = new SourceTextComparer();

	public bool Equals(SourceText x, SourceText y)
	{
		if (x == null)
		{
			return y == null;
		}
		if (y == null)
		{
			return false;
		}
		return x.ContentEquals(y);
	}

	public int GetHashCode(SourceText obj)
	{
		ImmutableArray<byte> checksum = obj.GetChecksum();
		int newKey = ((!checksum.IsDefault) ? Hash.CombineValues(checksum) : 0);
		int newKey2 = ((obj.Encoding != null) ? obj.Encoding.GetHashCode() : 0);
		return Hash.Combine(obj.Length, Hash.Combine(newKey, Hash.Combine(newKey2, obj.ChecksumAlgorithm.GetHashCode())));
	}
}

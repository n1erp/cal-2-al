using System.Collections.Immutable;
using System.Reflection;

namespace Microsoft.Cci;

internal interface IFileReference
{
	bool HasMetadata { get; }

	string FileName { get; }

	ImmutableArray<byte> GetHashValue(AssemblyHashAlgorithm algorithmId);
}

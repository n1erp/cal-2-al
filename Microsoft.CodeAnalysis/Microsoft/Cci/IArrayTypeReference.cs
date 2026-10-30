using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IArrayTypeReference : ITypeReference, IReference
{
	bool IsSZArray { get; }

	ImmutableArray<int> LowerBounds { get; }

	int Rank { get; }

	ImmutableArray<int> Sizes { get; }

	ITypeReference GetElementType(EmitContext context);
}

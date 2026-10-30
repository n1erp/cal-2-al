using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct MethodTypeInferenceResult(bool success, ImmutableArray<TypeSymbol> inferredTypeArguments)
{
	public readonly ImmutableArray<TypeSymbol> InferredTypeArguments = inferredTypeArguments;

	public readonly bool Success = success;
}

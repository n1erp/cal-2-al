using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface IErrorTypeSymbol : INamedTypeSymbol, ITypeSymbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	ImmutableArray<ISymbol> CandidateSymbols { get; }

	CandidateReason CandidateReason { get; }
}

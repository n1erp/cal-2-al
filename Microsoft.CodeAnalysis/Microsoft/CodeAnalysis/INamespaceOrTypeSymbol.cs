using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface INamespaceOrTypeSymbol : ISymbol, IEquatable<ISymbol>
{
	bool IsNamespace { get; }

	bool IsType { get; }

	ImmutableArray<ISymbol> GetMembers();

	ImmutableArray<ISymbol> GetMembers(string name);

	ImmutableArray<INamedTypeSymbol> GetTypeMembers();

	ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name);

	ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name, int arity);
}

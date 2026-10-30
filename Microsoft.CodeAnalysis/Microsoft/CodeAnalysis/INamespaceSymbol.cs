using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface INamespaceSymbol : INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	bool IsGlobalNamespace { get; }

	NamespaceKind NamespaceKind { get; }

	Compilation ContainingCompilation { get; }

	ImmutableArray<INamespaceSymbol> ConstituentNamespaces { get; }

	new IEnumerable<INamespaceOrTypeSymbol> GetMembers();

	new IEnumerable<INamespaceOrTypeSymbol> GetMembers(string name);

	IEnumerable<INamespaceSymbol> GetNamespaceMembers();
}

using System;

namespace Microsoft.CodeAnalysis;

public interface IAliasSymbol : ISymbol, IEquatable<ISymbol>
{
	INamespaceOrTypeSymbol Target { get; }
}

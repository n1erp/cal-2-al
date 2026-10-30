using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface IPointerTypeSymbol : ITypeSymbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	ITypeSymbol PointedAtType { get; }

	ImmutableArray<CustomModifier> CustomModifiers { get; }
}

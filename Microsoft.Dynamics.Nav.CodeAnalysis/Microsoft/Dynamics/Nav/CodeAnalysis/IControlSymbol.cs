using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IControlSymbol : ISymbolWithId, ISymbol, IEquatable<ISymbol>
{
	ControlKind ControlKind { get; }

	ITypeSymbol Type { get; }

	ImmutableArray<IControlSymbol> Controls { get; }

	IFieldSymbol RelatedFieldSymbol { get; }

	IPageTypeSymbol RelatedPartSymbol { get; }
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IEventSymbol : ISymbol, IEquatable<ISymbol>
{
	ImmutableArray<IParameterSymbol> Parameters { get; }

	ImmutableArray<IAttributeSymbol> Attributes { get; }
}

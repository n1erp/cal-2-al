using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal interface ISymbolWithAttributes : ISymbol, IEquatable<ISymbol>
{
	ImmutableArray<AttributeSymbol> Attributes { get; }
}

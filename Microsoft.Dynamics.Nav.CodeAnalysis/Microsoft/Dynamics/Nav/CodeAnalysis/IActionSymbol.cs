using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IActionSymbol : ISymbolWithId, ISymbol, IEquatable<ISymbol>
{
	ActionKind ActionKind { get; }

	ImmutableArray<IActionSymbol> Actions { get; }

	ImmutableArray<IMethodSymbol> Triggers { get; }
}

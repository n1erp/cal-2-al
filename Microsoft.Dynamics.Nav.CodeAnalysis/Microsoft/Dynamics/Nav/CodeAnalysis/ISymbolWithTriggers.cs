using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ISymbolWithTriggers : ISymbol, IEquatable<ISymbol>
{
	ImmutableArray<IMethodSymbol> Triggers { get; }

	TriggerTypeInfo GetTriggerTypeInfo(string name);
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IContainerSymbol : ISymbol, IEquatable<ISymbol>
{
	ImmutableArray<ISymbol> GetMembers();

	ImmutableArray<ISymbol> GetMembers(string name);
}

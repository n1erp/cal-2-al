using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IFieldNameListSymbol : ISymbol, IEquatable<ISymbol>
{
	ImmutableArray<IFieldSymbol> Fields { get; }
}

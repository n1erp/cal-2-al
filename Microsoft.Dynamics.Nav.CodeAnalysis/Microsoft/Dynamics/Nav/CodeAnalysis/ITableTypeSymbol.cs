using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ITableTypeSymbol : IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
	ImmutableArray<IKeySymbol> Keys { get; }

	ImmutableArray<IFieldSymbol> Fields { get; }

	ImmutableArray<IFieldGroupSymbol> FieldGroups { get; }
}

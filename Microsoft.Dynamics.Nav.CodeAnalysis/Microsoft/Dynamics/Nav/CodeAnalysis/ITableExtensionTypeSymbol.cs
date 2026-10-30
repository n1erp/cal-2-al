using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ITableExtensionTypeSymbol : IApplicationObjectExtensionTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
	ImmutableArray<IFieldSymbol> AddedFields { get; }

	ImmutableArray<IKeySymbol> AddedKeys { get; }
}

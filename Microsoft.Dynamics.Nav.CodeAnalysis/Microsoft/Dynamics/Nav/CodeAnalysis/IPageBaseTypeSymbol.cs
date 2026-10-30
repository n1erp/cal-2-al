using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IPageBaseTypeSymbol : IObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithTriggers
{
	PageTypeKind PageType { get; }

	ITableTypeSymbol RelatedTable { get; }

	ImmutableArray<IControlSymbol> FlattenedControls { get; }

	ImmutableArray<IActionSymbol> FlattenedActions { get; }

	ImmutableArray<IViewSymbol> FlattenedViews { get; }
}

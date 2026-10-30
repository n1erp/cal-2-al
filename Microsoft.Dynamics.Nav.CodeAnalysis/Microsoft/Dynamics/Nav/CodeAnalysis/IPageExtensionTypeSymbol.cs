using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IPageExtensionTypeSymbol : IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
	ImmutableArray<IActionSymbol> AddedActionsTopLevel { get; }

	ImmutableArray<IControlSymbol> AddedControlsTopLevel { get; }

	ImmutableArray<IActionSymbol> AddedActionsFlattened { get; }

	ImmutableArray<IControlSymbol> AddedControlsFlattened { get; }

	ImmutableArray<IViewSymbol> AddedViewsFlattened { get; }

	ImmutableArray<IActionSymbol> MovedActions { get; }

	ImmutableArray<IControlSymbol> MovedControls { get; }

	ImmutableArray<IViewSymbol> MovedViews { get; }

	ImmutableArray<IActionSymbol> ModifiedActions { get; }

	ImmutableArray<IViewSymbol> ModifiedViews { get; }

	ImmutableArray<IControlSymbol> ModifiedControls { get; }

	ImmutableArray<IChangeSymbol> MoveSymbols { get; }

	ImmutableArray<IChangeSymbol> ModifySymbols { get; }

	ImmutableArray<IChangeSymbol> AddSymbols { get; }
}

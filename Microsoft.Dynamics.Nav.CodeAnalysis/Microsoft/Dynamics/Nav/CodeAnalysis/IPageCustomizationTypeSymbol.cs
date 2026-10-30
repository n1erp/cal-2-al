using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IPageCustomizationTypeSymbol : IApplicationObjectExtensionTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
	ImmutableArray<IViewSymbol> AddedViews { get; }

	ImmutableArray<IActionSymbol> MovedActions { get; }

	ImmutableArray<IControlSymbol> MovedControls { get; }

	ImmutableArray<IViewSymbol> MovedViews { get; }

	ImmutableArray<IActionSymbol> ModifiedActions { get; }

	ImmutableArray<IControlSymbol> ModifiedControls { get; }

	ImmutableArray<IViewSymbol> ModifiedViews { get; }

	ImmutableArray<IChangeSymbol> MoveSymbols { get; }

	ImmutableArray<IChangeSymbol> ModifySymbols { get; }
}

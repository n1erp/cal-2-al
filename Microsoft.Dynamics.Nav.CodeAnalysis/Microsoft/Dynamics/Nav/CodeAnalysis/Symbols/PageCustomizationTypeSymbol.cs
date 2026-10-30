using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class PageCustomizationTypeSymbol : PageExtensionBaseTypeSymbol, IPageCustomizationTypeSymbol, IApplicationObjectExtensionTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
	public override SymbolKind Kind => SymbolKind.PageCustomization;

	public override NavTypeKind NavTypeKind => NavTypeKind.PageCustomization;

	public override SymbolCompilationScope CompilationScope => SymbolCompilationScope.Personalization;

	ImmutableArray<IActionSymbol> IPageCustomizationTypeSymbol.MovedActions => ImmutableArray<IActionSymbol>.CastUp(MovedActions);

	ImmutableArray<IControlSymbol> IPageCustomizationTypeSymbol.MovedControls => ImmutableArray<IControlSymbol>.CastUp(MovedControls);

	ImmutableArray<IViewSymbol> IPageCustomizationTypeSymbol.MovedViews => ImmutableArray<IViewSymbol>.CastUp(MovedViews);

	ImmutableArray<IActionSymbol> IPageCustomizationTypeSymbol.ModifiedActions => ImmutableArray<IActionSymbol>.CastUp(ModifiedActions);

	ImmutableArray<IControlSymbol> IPageCustomizationTypeSymbol.ModifiedControls => ImmutableArray<IControlSymbol>.CastUp(ModifiedControls);

	ImmutableArray<IViewSymbol> IPageCustomizationTypeSymbol.ModifiedViews => ImmutableArray<IViewSymbol>.CastUp(ModifiedViews);

	ImmutableArray<IChangeSymbol> IPageCustomizationTypeSymbol.MoveSymbols => ImmutableArray<IChangeSymbol>.CastUp(MoveSymbols);

	ImmutableArray<IChangeSymbol> IPageCustomizationTypeSymbol.ModifySymbols => ImmutableArray<IChangeSymbol>.CastUp(ModifySymbols);

	ImmutableArray<IViewSymbol> IPageCustomizationTypeSymbol.AddedViews => ImmutableArray<IViewSymbol>.CastUp(FlattenedViews);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitPageCustomizationType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitPageCustomizationType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitPageCustomizationType(this, a);
	}

	protected PageCustomizationTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
	}
}

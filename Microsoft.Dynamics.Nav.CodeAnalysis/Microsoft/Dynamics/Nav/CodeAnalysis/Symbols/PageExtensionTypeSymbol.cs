using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class PageExtensionTypeSymbol : PageExtensionBaseTypeSymbol, IPageExtensionTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
	public override SymbolKind Kind => SymbolKind.PageExtension;

	public override NavTypeKind NavTypeKind => NavTypeKind.PageExtension;

	public override string MetadataName => "PageExtension" + Id.ToString(CultureInfo.InvariantCulture);

	public ImmutableArray<ActionSymbol> AddedActionsTopLevel => LazyAddedActionsTree.Value;

	public ImmutableArray<ControlSymbol> AddedControlsTopLevel => LazyAddedControlsTree.Value;

	public ImmutableArray<ActionSymbol> FlattenedActions => FlattenedActionList.Value.FlattenedList;

	public ImmutableArray<ControlSymbol> FlattenedControls => FlattenedControlList.Value.FlattenedList;

	private Lazy<FlattenedMemberList<ControlSymbol>> FlattenedControlList => new Lazy<FlattenedMemberList<ControlSymbol>>(() => new FlattenedMemberList<ControlSymbol>(LazyAddedControlsTree.Value, (ControlSymbol a) => a.Controls));

	private Lazy<FlattenedMemberList<ActionSymbol>> FlattenedActionList => new Lazy<FlattenedMemberList<ActionSymbol>>(() => new FlattenedMemberList<ActionSymbol>(LazyAddedActionsTree.Value, (ActionSymbol a) => a.Actions));

	private Lazy<ImmutableArray<ControlSymbol>> LazyAddedControlsTree => new Lazy<ImmutableArray<ControlSymbol>>(() =>
	{
		ArrayBuilder<ControlSymbol> instance = ArrayBuilder<ControlSymbol>.GetInstance();
		foreach (Symbol member in GetMembers())
		{
			if (PageExtensionBaseTypeSymbol.IsChangeAddSymbol(member, SymbolKind.Control))
			{
				ImmutableArray<Symbol> addedSymbols = ((ChangeAddSymbol)member).AddedSymbols;
				for (int i = 0; i < addedSymbols.Length; i++)
				{
					instance.Add((ControlSymbol)addedSymbols[i]);
				}
			}
		}
		return instance.ToImmutableAndFree();
	});

	private Lazy<ImmutableArray<ActionSymbol>> LazyAddedActionsTree => new Lazy<ImmutableArray<ActionSymbol>>(() =>
	{
		ArrayBuilder<ActionSymbol> instance = ArrayBuilder<ActionSymbol>.GetInstance();
		foreach (Symbol member in GetMembers())
		{
			if (PageExtensionBaseTypeSymbol.IsChangeAddSymbol(member, SymbolKind.Action))
			{
				ImmutableArray<Symbol> addedSymbols = ((ChangeAddSymbol)member).AddedSymbols;
				for (int i = 0; i < addedSymbols.Length; i++)
				{
					instance.Add((ActionSymbol)addedSymbols[i]);
				}
			}
		}
		return instance.ToImmutableAndFree();
	});

	ImmutableArray<IActionSymbol> IPageExtensionTypeSymbol.AddedActionsTopLevel => ImmutableArray<IActionSymbol>.CastUp(AddedActionsTopLevel);

	ImmutableArray<IControlSymbol> IPageExtensionTypeSymbol.AddedControlsTopLevel => ImmutableArray<IControlSymbol>.CastUp(AddedControlsTopLevel);

	ImmutableArray<IActionSymbol> IPageExtensionTypeSymbol.AddedActionsFlattened => ImmutableArray<IActionSymbol>.CastUp(FlattenedActions);

	ImmutableArray<IControlSymbol> IPageExtensionTypeSymbol.AddedControlsFlattened => ImmutableArray<IControlSymbol>.CastUp(FlattenedControls);

	ImmutableArray<IViewSymbol> IPageExtensionTypeSymbol.AddedViewsFlattened => ImmutableArray<IViewSymbol>.CastUp(FlattenedViews);

	ImmutableArray<IActionSymbol> IPageExtensionTypeSymbol.MovedActions => ImmutableArray<IActionSymbol>.CastUp(MovedActions);

	ImmutableArray<IControlSymbol> IPageExtensionTypeSymbol.MovedControls => ImmutableArray<IControlSymbol>.CastUp(MovedControls);

	ImmutableArray<IViewSymbol> IPageExtensionTypeSymbol.MovedViews => ImmutableArray<IViewSymbol>.CastUp(MovedViews);

	ImmutableArray<IActionSymbol> IPageExtensionTypeSymbol.ModifiedActions => ImmutableArray<IActionSymbol>.CastUp(ModifiedActions);

	ImmutableArray<IControlSymbol> IPageExtensionTypeSymbol.ModifiedControls => ImmutableArray<IControlSymbol>.CastUp(ModifiedControls);

	ImmutableArray<IViewSymbol> IPageExtensionTypeSymbol.ModifiedViews => ImmutableArray<IViewSymbol>.CastUp(ModifiedViews);

	ImmutableArray<IChangeSymbol> IPageExtensionTypeSymbol.AddSymbols => ImmutableArray<IChangeSymbol>.CastUp(AddSymbols);

	ImmutableArray<IChangeSymbol> IPageExtensionTypeSymbol.MoveSymbols => ImmutableArray<IChangeSymbol>.CastUp(MoveSymbols);

	ImmutableArray<IChangeSymbol> IPageExtensionTypeSymbol.ModifySymbols => ImmutableArray<IChangeSymbol>.CastUp(ModifySymbols);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitPageExtensionType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitPageExtensionType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitPageExtensionType(this, a);
	}

	protected PageExtensionTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class PageExtensionBaseTypeSymbol : ApplicationObjectExtensionTypeSymbol
{
	public override TypeCategoryKind TypeCategory => TypeCategoryKind.ApplicationObject;

	internal override TypeSymbol BaseType => PageClassTypeSymbol.Instance;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.PageProperties;

	public ImmutableArray<IActionSymbol> MovedActions => LazyMovedActions.Value;

	public ImmutableArray<IControlSymbol> MovedControls => LazyMovedControls.Value;

	public ImmutableArray<IViewSymbol> MovedViews => LazyMovedViews.Value;

	public ImmutableArray<IActionSymbol> ModifiedActions => LazyModifiedActions.Value;

	public ImmutableArray<IControlSymbol> ModifiedControls => LazyModifiedControls.Value;

	public ImmutableArray<IViewSymbol> ModifiedViews => LazyModifiedViews.Value;

	internal ImmutableArray<SourceChangeAddSymbol> AddSymbols => LazyAddSymbols.Value;

	internal ImmutableArray<SourceChangeModifySymbol> ModifySymbols => LazyModifySymbols.Value;

	internal ImmutableArray<SourceChangeMoveSymbol> MoveSymbols => LazyMoveSymbols.Value;

	public ImmutableArray<ViewSymbol> FlattenedViews => LazyAddedViews.Value;

	private Lazy<ImmutableArray<ViewSymbol>> LazyAddedViews { get; }

	private Lazy<ImmutableArray<SourceChangeAddSymbol>> LazyAddSymbols { get; }

	private Lazy<ImmutableArray<SourceChangeModifySymbol>> LazyModifySymbols { get; }

	private Lazy<ImmutableArray<SourceChangeMoveSymbol>> LazyMoveSymbols { get; }

	private Lazy<ImmutableArray<IControlSymbol>> LazyModifiedControls { get; }

	private Lazy<ImmutableArray<IActionSymbol>> LazyModifiedActions { get; }

	private Lazy<ImmutableArray<IViewSymbol>> LazyModifiedViews { get; }

	private Lazy<ImmutableArray<IControlSymbol>> LazyMovedControls { get; }

	private Lazy<ImmutableArray<IActionSymbol>> LazyMovedActions { get; }

	private Lazy<ImmutableArray<IViewSymbol>> LazyMovedViews { get; }

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitPageExtensionBaseType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitPageExtensionBaseType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitPageExtensionBaseType(this, a);
	}

	protected PageExtensionBaseTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
		LazyAddedViews = new Lazy<ImmutableArray<ViewSymbol>>(LazyAddedViewsBody);
		LazyAddSymbols = new Lazy<ImmutableArray<SourceChangeAddSymbol>>(LazyAddSymbolsBody);
		LazyModifySymbols = new Lazy<ImmutableArray<SourceChangeModifySymbol>>(LazyModifySymbolsBody);
		LazyMoveSymbols = new Lazy<ImmutableArray<SourceChangeMoveSymbol>>(LazyMoveSymbolsBody);
		LazyModifiedControls = new Lazy<ImmutableArray<IControlSymbol>>(LazyModifiedControlsBody);
		LazyModifiedActions = new Lazy<ImmutableArray<IActionSymbol>>(LazyModifiedActionsBody);
		LazyModifiedViews = new Lazy<ImmutableArray<IViewSymbol>>(LazyModifiedViewsBody);
		LazyMovedControls = new Lazy<ImmutableArray<IControlSymbol>>(LazyMovedControlBody);
		LazyMovedActions = new Lazy<ImmutableArray<IActionSymbol>>(LazyMovedActionsBody);
		LazyMovedViews = new Lazy<ImmutableArray<IViewSymbol>>(LazyMovedViewsBody);
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetPageExtensionTriggerInfo(name);
	}

	protected static bool IsChangeAddSymbol(ISymbol symbol)
	{
		if (symbol.Kind == SymbolKind.Change)
		{
			return ((ChangeSymbol)symbol).ChangeKind.IsAdd();
		}
		return false;
	}

	protected static bool IsChangeModifySymbol(ISymbol symbol)
	{
		if (symbol.Kind == SymbolKind.Change)
		{
			return ((ChangeSymbol)symbol).ChangeKind.IsModify();
		}
		return false;
	}

	protected static bool IsChangeMoveSymbol(ISymbol symbol)
	{
		if (symbol.Kind == SymbolKind.Change)
		{
			return ((ChangeSymbol)symbol).ChangeKind.IsMove();
		}
		return false;
	}

	protected static bool IsChangeAddSymbol(ISymbol symbol, SymbolKind targetKind)
	{
		if (IsChangeAddSymbol(symbol))
		{
			return ((ChangeSymbol)symbol).ChangeTargetKind == targetKind;
		}
		return false;
	}

	protected static bool IsChangeModifySymbol(ISymbol symbol, SymbolKind targetKind)
	{
		if (IsChangeModifySymbol(symbol))
		{
			return ((ChangeSymbol)symbol).ChangeTargetKind == targetKind;
		}
		return false;
	}

	protected static bool IsChangeMoveSymbol(ISymbol symbol, SymbolKind targetKind)
	{
		if (IsChangeMoveSymbol(symbol))
		{
			return ((ChangeSymbol)symbol).ChangeTargetKind == targetKind;
		}
		return false;
	}

	private ImmutableArray<ViewSymbol> LazyAddedViewsBody()
	{
		ArrayBuilder<ViewSymbol> instance = ArrayBuilder<ViewSymbol>.GetInstance();
		foreach (Symbol member in GetMembers())
		{
			if (IsChangeAddSymbol(member, SymbolKind.View))
			{
				ImmutableArray<Symbol> addedSymbols = ((ChangeAddSymbol)member).AddedSymbols;
				for (int i = 0; i < addedSymbols.Length; i++)
				{
					instance.Add((ViewSymbol)addedSymbols[i]);
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	private ImmutableArray<SourceChangeAddSymbol> LazyAddSymbolsBody()
	{
		return GetMembers().WhereAsCastArray<ISymbol, SourceChangeAddSymbol>(IsChangeAddSymbol);
	}

	private ImmutableArray<SourceChangeModifySymbol> LazyModifySymbolsBody()
	{
		return GetMembers().WhereAsCastArray<ISymbol, SourceChangeModifySymbol>(IsChangeModifySymbol);
	}

	private ImmutableArray<SourceChangeMoveSymbol> LazyMoveSymbolsBody()
	{
		return GetMembers().WhereAsCastArray<ISymbol, SourceChangeMoveSymbol>(IsChangeMoveSymbol);
	}

	private ImmutableArray<IControlSymbol> LazyModifiedControlsBody()
	{
		return GetMembers().WhereAndSelectAsCastArray((Symbol x) => IsChangeModifySymbol(x, SymbolKind.Control), (ChangeModifySymbol x) => (IControlSymbol)x.Target);
	}

	private ImmutableArray<IActionSymbol> LazyModifiedActionsBody()
	{
		return GetMembers().WhereAndSelectAsCastArray((Symbol x) => IsChangeModifySymbol(x, SymbolKind.Action), (ChangeModifySymbol x) => (IActionSymbol)x.Target);
	}

	private ImmutableArray<IViewSymbol> LazyModifiedViewsBody()
	{
		return GetMembers().WhereAndSelectAsCastArray((Symbol x) => IsChangeModifySymbol(x, SymbolKind.View), (ChangeModifySymbol x) => (IViewSymbol)x.Target);
	}

	private ImmutableArray<IControlSymbol> LazyMovedControlBody()
	{
		ArrayBuilder<IControlSymbol> instance = ArrayBuilder<IControlSymbol>.GetInstance();
		foreach (Symbol member in GetMembers())
		{
			if (IsChangeMoveSymbol(member, SymbolKind.Control))
			{
				ImmutableArray<Symbol> movedSymbols = ((ChangeMoveSymbol)member).MovedSymbols;
				for (int i = 0; i < movedSymbols.Length; i++)
				{
					instance.Add((IControlSymbol)movedSymbols[i]);
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	private ImmutableArray<IActionSymbol> LazyMovedActionsBody()
	{
		ArrayBuilder<IActionSymbol> instance = ArrayBuilder<IActionSymbol>.GetInstance();
		foreach (Symbol member in GetMembers())
		{
			if (IsChangeMoveSymbol(member, SymbolKind.Action))
			{
				ImmutableArray<Symbol> movedSymbols = ((ChangeMoveSymbol)member).MovedSymbols;
				for (int i = 0; i < movedSymbols.Length; i++)
				{
					instance.Add((IActionSymbol)movedSymbols[i]);
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	private ImmutableArray<IViewSymbol> LazyMovedViewsBody()
	{
		ArrayBuilder<IViewSymbol> instance = ArrayBuilder<IViewSymbol>.GetInstance();
		foreach (Symbol member in GetMembers())
		{
			if (IsChangeMoveSymbol(member, SymbolKind.View))
			{
				ImmutableArray<Symbol> movedSymbols = ((ChangeMoveSymbol)member).MovedSymbols;
				for (int i = 0; i < movedSymbols.Length; i++)
				{
					instance.Add((IViewSymbol)movedSymbols[i]);
				}
			}
		}
		return instance.ToImmutableAndFree();
	}
}

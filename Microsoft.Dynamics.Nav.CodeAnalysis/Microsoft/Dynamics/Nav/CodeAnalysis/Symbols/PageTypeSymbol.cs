using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class PageTypeSymbol : ApplicationObjectTypeSymbol, IPageTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IPageBaseTypeSymbol, IObjectTypeSymbol, ISymbolWithTriggers, ISymbolWithTriggerEvents, IRootTypeSymbol
{
	private ImmutableArray<BuiltInTriggerEventSymbol> triggerEvents;

	private Dictionary<string, ImmutableArray<Symbol>> codeMembersByName;

	private PageReferenceTypeSymbol pageReferenceTypeSymbol;

	public override SymbolKind Kind => SymbolKind.Page;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.ApplicationObject;

	public override NavTypeKind NavTypeKind => NavTypeKind.Page;

	public override string MetadataName => NavTypeKind.Page.ToString() + Id.ToString(CultureInfo.InvariantCulture);

	public abstract PageTypeKind PageType { get; }

	public abstract TableTypeSymbol RelatedTable { get; }

	public override SymbolCompilationScope CompilationScope
	{
		get
		{
			if (RelatedTable == null)
			{
				return base.CompilationScope;
			}
			return (SymbolCompilationScope)Math.Max((int)base.CompilationScope, (int)RelatedTable.CompilationScope);
		}
	}

	public ImmutableArray<BuiltInTriggerEventSymbol> TriggerEvents
	{
		get
		{
			LazyGetTriggerEvents();
			return triggerEvents;
		}
	}

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.PageProperties;

	private Lazy<ImmutableArray<ControlSymbol>> FlattenedControlList { get; }

	public ImmutableArray<ControlSymbol> FlattenedControls => FlattenedControlList.Value;

	private Lazy<ImmutableArray<ActionSymbol>> FlattenedActionList { get; }

	public ImmutableArray<ActionSymbol> FlattenedActions => FlattenedActionList.Value;

	private Lazy<ImmutableArray<ActionSymbol>> FlattenedCueActionList { get; }

	public ImmutableArray<ActionSymbol> FlattenedCueActions => FlattenedCueActionList.Value;

	private Lazy<ImmutableArray<ActionSymbol>> FlattenedNonCueActionList { get; }

	public ImmutableArray<ActionSymbol> FlattenedNonCueActions => FlattenedNonCueActionList.Value;

	private Lazy<ImmutableArray<ViewSymbol>> FlattenedViewList { get; }

	public ImmutableArray<ViewSymbol> FlattenedViews => FlattenedViewList.Value;

	ITableTypeSymbol IPageBaseTypeSymbol.RelatedTable => RelatedTable;

	ImmutableArray<IControlSymbol> IPageBaseTypeSymbol.FlattenedControls => ImmutableArray<IControlSymbol>.CastUp(FlattenedControls);

	ImmutableArray<IActionSymbol> IPageBaseTypeSymbol.FlattenedActions => ImmutableArray<IActionSymbol>.CastUp(FlattenedActions);

	ImmutableArray<IViewSymbol> IPageBaseTypeSymbol.FlattenedViews => ImmutableArray<IViewSymbol>.CastUp(FlattenedViews);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitPageType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitPageType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitPageType(this, a);
	}

	protected PageTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
		FlattenedControlList = new Lazy<ImmutableArray<ControlSymbol>>(() => new FlattenedMemberList<ControlSymbol>(this.GetMembersOfKind<ControlSymbol>(SymbolKind.Control).ToImmutableArray(), (ControlSymbol a) => a.Controls).FlattenedList.ToImmutableArray());
		FlattenedNonCueActionList = new Lazy<ImmutableArray<ActionSymbol>>(GetNonCueActions);
		FlattenedCueActionList = new Lazy<ImmutableArray<ActionSymbol>>(GetCueActions);
		FlattenedActionList = new Lazy<ImmutableArray<ActionSymbol>>(GetAllActions);
		FlattenedViewList = new Lazy<ImmutableArray<ViewSymbol>>(GetAllViews);
	}

	internal PageReferenceTypeSymbol GetPageReference()
	{
		if (pageReferenceTypeSymbol == null)
		{
			Interlocked.CompareExchange(ref pageReferenceTypeSymbol, PageReferenceTypeSymbol.Create(this), null);
		}
		return pageReferenceTypeSymbol;
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetPageTriggerInfo(name);
	}

	private ImmutableArray<ActionSymbol> GetAllActions()
	{
		return FlattenedNonCueActions.Concat(FlattenedCueActions);
	}

	private ImmutableArray<ActionSymbol> GetNonCueActions()
	{
		return new FlattenedMemberList<ActionSymbol>(this.GetMembersOfKind<ActionSymbol>(SymbolKind.Action).ToImmutableArray(), (ActionSymbol a) => a.Actions).FlattenedList;
	}

	private ImmutableArray<ActionSymbol> GetCueActions()
	{
		return FlattenedControls.Where((ControlSymbol m) => m.ControlKind == ControlKind.CueGroup).SelectMany((ControlSymbol m) => m.Actions).ToImmutableArray();
	}

	private ImmutableArray<ViewSymbol> GetAllViews()
	{
		return this.GetMembersOfKind<ViewSymbol>(SymbolKind.View).ToImmutableArray();
	}

	private void LazyGetTriggerEvents()
	{
		if (!(triggerEvents != null))
		{
			ImmutableArray<BuiltInTriggerEventSymbol> value = TriggerEventDefinitions.PageTriggerEvents.Values.Select((TriggerEventTypeInfo x) => new BuiltInTriggerEventSymbol(this, x)).ToImmutableArrayOrEmpty();
			ImmutableInterlocked.InterlockedCompareExchange(ref triggerEvents, value, default);
		}
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return GetCodeMembers(ref codeMembersByName, name);
	}
}

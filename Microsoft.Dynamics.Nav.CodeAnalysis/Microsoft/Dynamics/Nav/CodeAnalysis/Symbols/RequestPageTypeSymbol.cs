using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class RequestPageTypeSymbol : ObjectTypeSymbol, IPageBaseTypeSymbol, IObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithTriggers
{
	private TestRequestPageTypeSymbol cachedTestRequestPage;

	public new ApplicationObjectTypeSymbol ContainingSymbol => (ApplicationObjectTypeSymbol)base.ContainingSymbol;

	public TypeSymbol Type => this;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override SymbolKind Kind => SymbolKind.RequestPage;

	public override string MetadataName => "RequestOptionsPage";

	public override string Name { get; }

	public override NavTypeKind NavTypeKind => NavTypeKind.RequestPage;

	public PageTypeKind PageType { get; }

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.RequestPage;

	private Lazy<ImmutableArray<ControlSymbol>> FlattenedControlList { get; }

	public ImmutableArray<ActionSymbol> FlattenedActions => FlattenedActionList.Value;

	private Lazy<ImmutableArray<ActionSymbol>> FlattenedActionList { get; }

	public ImmutableArray<ControlSymbol> FlattenedControls => FlattenedControlList.Value;

	public ImmutableArray<ViewSymbol> FlattenedViews => ImmutableArray<ViewSymbol>.Empty;

	public TableTypeSymbol RelatedTable
	{
		get
		{
			PropertySymbol property = GetProperty(PropertyKind.SourceTable);
			if ((object)property == null || property.Value == null)
			{
				return null;
			}
			return (TableTypeSymbol)property.Value;
		}
	}

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.RequestPageProperties;

	internal override TypeSymbol BaseType => RequestPageClassTypeSymbol.Instance;

	ITableTypeSymbol IPageBaseTypeSymbol.RelatedTable => RelatedTable;

	ImmutableArray<IControlSymbol> IPageBaseTypeSymbol.FlattenedControls => ImmutableArray<IControlSymbol>.CastUp(FlattenedControls);

	ImmutableArray<IActionSymbol> IPageBaseTypeSymbol.FlattenedActions => ImmutableArray<IActionSymbol>.CastUp(FlattenedActions);

	ImmutableArray<IViewSymbol> IPageBaseTypeSymbol.FlattenedViews => ImmutableArray<IViewSymbol>.CastUp(FlattenedViews);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitRequestPageType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitRequestPageType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitRequestPageType(this, a);
	}

	protected RequestPageTypeSymbol(ApplicationObjectTypeSymbol containingSymbol)
		: base(containingSymbol)
	{
		PageType = containingSymbol.GetTypeOfNestedRequestPage();
		Name = "RequestOptionsPage";
		FlattenedControlList = new Lazy<ImmutableArray<ControlSymbol>>(() => new FlattenedMemberList<ControlSymbol>(this.GetMembersOfKind<ControlSymbol>(SymbolKind.Control).ToImmutableArray(), (ControlSymbol a) => a.Controls).FlattenedList.ToImmutableArray());
		FlattenedActionList = new Lazy<ImmutableArray<ActionSymbol>>(() => new FlattenedMemberList<ActionSymbol>(this.GetMembersOfKind<ActionSymbol>(SymbolKind.Action).ToImmutableArray(), (ActionSymbol a) => a.Actions).FlattenedList.ToImmutableArray());
	}

	internal TestRequestPageTypeSymbol GetTestRequestPage()
	{
		if (cachedTestRequestPage == null)
		{
			Interlocked.CompareExchange(ref cachedTestRequestPage, TestRequestPageTypeSymbol.Create(ContainingModule, (ReportTypeSymbol)ContainingSymbol), null);
		}
		return cachedTestRequestPage;
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetPageTriggerInfo(name);
	}

	internal override ICodeEmitter<MemberDeclarationSyntax> GetEmitter(ModuleBuilder moduleBuilder)
	{
		return new ObjectCodeEmitter(new RequestPageCodeBuilder(this, moduleBuilder));
	}
}

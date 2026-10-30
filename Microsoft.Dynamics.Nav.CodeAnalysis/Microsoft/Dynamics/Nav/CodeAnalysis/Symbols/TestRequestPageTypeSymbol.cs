using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class TestRequestPageTypeSymbol : NavTypeSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>
{
	private ImmutableArray<Symbol> lazyMembers;

	internal override ModuleSymbol ContainingModule { get; }

	public override string Name => Target.Name;

	public int Id => Target.Id;

	public override NavTypeKind NavTypeKind => NavTypeKind.TestRequestPage;

	public override SymbolKind Kind => SymbolKind.TestRequestPage;

	public override TypeCategoryKind TypeCategory => Target.TypeCategory;

	public ReportTypeSymbol Target { get; }

	internal override TypeSymbol BaseType => NavCorLib.TestRequestPageType;

	protected override Symbol OriginalSymbolDefinition => (Symbol)(((object)Target.RequestPage) ?? ((object)Target));

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitTestRequestPageType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitTestRequestPageType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitTestRequestPageType(this, a);
	}

	internal static TestRequestPageTypeSymbol Create(ModuleSymbol containingModule, ReportTypeSymbol reportTypeSymbol)
	{
		return new TestRequestPageTypeSymbol(containingModule, reportTypeSymbol);
	}

	private TestRequestPageTypeSymbol(ModuleSymbol containingModule, ReportTypeSymbol reportTypeSymbol)
		: base(NavTypeKind.TestRequestPage)
	{
		ContainingModule = containingModule;
		Target = reportTypeSymbol;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableArray<Symbol> value = LazyGetMembers(Target);
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, value, default);
		return lazyMembers;
	}

	private ImmutableArray<Symbol> LazyGetMembers(ReportTypeSymbol reportTypeSymbol)
	{
		if (reportTypeSymbol.RequestPage == null)
		{
			return ImmutableArray<Symbol>.Empty;
		}
		using TestPageMembersBuilder testPageMembersBuilder = new TestPageMembersBuilder(ContainingModule, ContainingSymbol);
		RequestPageTypeSymbol requestPage = reportTypeSymbol.RequestPage;
		AddControls(requestPage, testPageMembersBuilder);
		AddActions(requestPage, testPageMembersBuilder);
		AddFiltersForDataItems(reportTypeSymbol, testPageMembersBuilder);
		return testPageMembersBuilder.ToImmutable();
	}

	private void AddActions(RequestPageTypeSymbol pageTypeSymbol, TestPageMembersBuilder builder)
	{
		foreach (ActionSymbol flattenedAction in pageTypeSymbol.FlattenedActions)
		{
			if (flattenedAction.ActionKind == ActionKind.Action)
			{
				builder.AddSymbol(new TestActionSymbol(this, flattenedAction));
			}
		}
	}

	private void AddControls(RequestPageTypeSymbol pageTypeSymbol, TestPageMembersBuilder builder)
	{
		foreach (ControlSymbol flattenedControl in pageTypeSymbol.FlattenedControls)
		{
			if (flattenedControl.ControlKind == ControlKind.Field || flattenedControl.ControlKind == ControlKind.Label)
			{
				builder.AddSymbol(new TestFieldSymbol(this, flattenedControl));
			}
		}
	}

	private void AddFiltersForDataItems(ReportTypeSymbol reportTypeSymbol, TestPageMembersBuilder builder)
	{
		for (int i = 0; i < reportTypeSymbol.FlattenedDataItems.Length; i++)
		{
			ReportDataItemSymbol reportDataItemSymbol = reportTypeSymbol.FlattenedDataItems[i];
			if (!IsDataItemIgnored(reportDataItemSymbol))
			{
				builder.AddSymbol(new TestFilterSymbol(this, i, reportDataItemSymbol.Name, reportTypeSymbol.RequestPage, reportDataItemSymbol.RelatedTable));
			}
		}
	}

	private static bool IsDataItemIgnored(ReportDataItemSymbol dataItem)
	{
		BoundIdentifierListPropertyValue boundPropertyValue = dataItem.GetBoundPropertyValue<BoundIdentifierListPropertyValue>(PropertyKind.RequestFilterFields);
		BoundTableViewPropertyValue boundPropertyValue2 = dataItem.GetBoundPropertyValue<BoundTableViewPropertyValue>(PropertyKind.DataItemTableView);
		int num = boundPropertyValue?.Value.Length ?? 0;
		if ((boundPropertyValue2?.Sorting.Length ?? 0) > 0 && num == 0)
		{
			return true;
		}
		return false;
	}
}

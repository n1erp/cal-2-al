using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class QueryObjectMembers : ApplicationObjectMembers<SourceQueryTypeSymbol>
{
	private ImmutableArray<Symbol> lazyCodeMembers;

	internal new QuerySyntax Declaration => (QuerySyntax)base.Declaration;

	public QueryObjectMembers(SourceQueryTypeSymbol containingSymbol, QuerySyntax declaration)
		: base(containingSymbol, (ApplicationObjectSyntax)declaration)
	{
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		if (lazyCodeMembers != default(ImmutableArray<Symbol>))
		{
			return lazyCodeMembers;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		try
		{
			instance.AddRange(ContainingSymbol.FlattenedColumns);
			instance.AddRange(ContainingSymbol.FlattenedFilters);
			instance.AddRange(ContainingSymbol.FlattenedDataItems);
			instance.AddRange(from symbol in GetMembers()
				where symbol.Kind != SymbolKind.QueryDataItem
				select symbol);
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyCodeMembers, instance.ToImmutable(), default);
			return lazyCodeMembers;
		}
		finally
		{
			instance.Free();
		}
	}

	protected override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetQueryTriggerInfo(name.ToUpperInvariant());
	}

	protected override void BuildDeclaredMembersCore(ArrayBuilder<Symbol> members, DiagnosticBag diagnostics)
	{
		members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, QueryReferenceTypeSymbol.Create(ContainingSymbol, isCurrQuery: true), "CurrQuery"));
		QueryElementsSyntax elements = Declaration.Elements;
		if (elements == null)
		{
			return;
		}
		_ = elements.DataItems;
		if (true)
		{
			foreach (QueryDataItemSyntax dataItem in Declaration.Elements.DataItems)
			{
				members.Add(SourceQueryDataItemSymbol.Create(ContainingSymbol, dataItem, 0));
			}
		}
	}

	protected override bool IsNameRestrictedForTriggers(string name)
	{
		return TriggerDefinitions.IsValidQueryTrigger(name);
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		ImmutableArray<Symbol> topLevelDataItems = GetMembers().WhereAsArray((Symbol m) => m.Kind == SymbolKind.QueryDataItem);
		VerifyQueryDoesNotHaveUnion(topLevelDataItems, diagnostics);
		VerifyQueryHasTopLevelDataItem(topLevelDataItems, diagnostics);
		VerifyQueryHasAtLeastOneColumn(diagnostics);
		VerifyQueryDoesNotContainDuplicateMembers(diagnostics);
		CheckAPIRestrictions(diagnostics);
	}

	private static void VerifyQueryDoesNotHaveUnion(ImmutableArray<Symbol> topLevelDataItems, DiagnosticBag diagnostics)
	{
		if (topLevelDataItems.Length > 1)
		{
			foreach (Symbol item in topLevelDataItems)
			{
				diagnostics.Add(ErrorCode.ERR_UnionsAreNotSupportedInQueries, item.GetLocation());
			}
		}
	}

	private void VerifyQueryDoesNotContainDuplicateMembers(DiagnosticBag diagnostics)
	{
		Func<Symbol, bool> isNonTriggerMethod = (Symbol symbol) => symbol.Kind == SymbolKind.Method && ((SourceMethodOrTriggerSymbol)symbol).MethodKind != MethodKind.Trigger;
		ObjectMembers<SourceQueryTypeSymbol>.CheckMembersForDuplicateNames((from m in GetMembers()
			where isNonTriggerMethod(m) || m.Kind == SymbolKind.GlobalVariable
			select m).Concat(ContainingSymbol.FlattenedDataItems).Concat(ContainingSymbol.FlattenedColumns).Concat(ContainingSymbol.FlattenedFilters), diagnostics, (Symbol m) => true, ErrorCode.ERR_DuplicateMemberName);
	}

	private void VerifyQueryHasAtLeastOneColumn(DiagnosticBag diagnostics)
	{
		if (ContainingSymbol.FlattenedColumns.Length == 0)
		{
			diagnostics.Add(ErrorCode.ERR_QueriesMustDefineAtLeastOneColumn, Declaration.Name.GetLocation());
		}
	}

	private void VerifyQueryHasTopLevelDataItem(ImmutableArray<Symbol> topLevelDataItems, DiagnosticBag diagnostics)
	{
		if (topLevelDataItems.Length == 0)
		{
			diagnostics.Add(ErrorCode.ERR_QueriesMustHaveTopLevelDataItem, Declaration.Name.GetLocation());
		}
	}

	private void CheckAPIRestrictions(DiagnosticBag diagnostics)
	{
		if (ContainingSymbol.QueryType == QueryTypeKind.API)
		{
			CheckColumnNamesForQueriesOfTypeAPI(diagnostics);
		}
	}

	private void CheckColumnNamesForQueriesOfTypeAPI(DiagnosticBag diagnostics)
	{
		foreach (QueryColumnSymbol flattenedColumn in ContainingSymbol.FlattenedColumns)
		{
			if (flattenedColumn.Kind == SymbolKind.QueryColumn && (string.IsNullOrEmpty(flattenedColumn.Name) || !flattenedColumn.Name.IsValidAlphaNumeric()))
			{
				diagnostics.Add(ErrorCode.ERR_ApiQueryColumnsShouldHaveAlphaNumericName, flattenedColumn.GetLocation());
			}
		}
	}
}

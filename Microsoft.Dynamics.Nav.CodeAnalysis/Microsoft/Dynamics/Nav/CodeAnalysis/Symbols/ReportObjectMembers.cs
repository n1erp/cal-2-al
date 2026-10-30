using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReportObjectMembers : ApplicationObjectMembers<SourceReportTypeSymbol>
{
	private readonly Lazy<ImmutableArray<SourceReportColumnSymbol>> flattenedColumnList;

	private readonly Lazy<ImmutableArray<ReportDataItemSymbol>> flattenedDataItemList;

	private readonly Lazy<ImmutableArray<VariableSymbol>> globalVariableList;

	private readonly Lazy<RequestPageTypeSymbol> requestPage;

	private ImmutableArray<Symbol> lazyCodeMembers;

	internal ImmutableArray<VariableSymbol> GlobalVariables => globalVariableList.Value;

	internal ImmutableArray<ReportDataItemSymbol> FlattenedDataItems => flattenedDataItemList.Value;

	internal ImmutableArray<SourceReportColumnSymbol> FlattenedColumns => flattenedColumnList.Value;

	internal RequestPageTypeSymbol RequestPage => requestPage.Value;

	internal new ReportSyntax Declaration => (ReportSyntax)base.Declaration;

	public ReportObjectMembers(SourceReportTypeSymbol containingSymbol, ApplicationObjectSyntax declaration)
		: base(containingSymbol, declaration)
	{
		ReportObjectMembers reportObjectMembers = this;
		flattenedDataItemList = new Lazy<ImmutableArray<ReportDataItemSymbol>>(() => new FlattenedMemberList<SourceReportDataItemSymbol>(containingSymbol.GetMembersOfKind<SourceReportDataItemSymbol>(SymbolKind.ReportDataItem).ToImmutableArray(), (SourceReportDataItemSymbol a) => a.DataItems).FlattenedList.CastArray<ReportDataItemSymbol>());
		flattenedColumnList = new Lazy<ImmutableArray<SourceReportColumnSymbol>>(() => containingSymbol.FlattenedDataItems.Cast<SourceReportDataItemSymbol>().SelectMany((SourceReportDataItemSymbol dataItem) => dataItem.Columns).ToImmutableArray());
		globalVariableList = new Lazy<ImmutableArray<VariableSymbol>>(() =>
		{
			ArrayBuilder<VariableSymbol> instance = ArrayBuilder<VariableSymbol>.GetInstance();
			try
			{
				instance.AddRange(reportObjectMembers.GetGlobalVariables());
				RequestPageTypeSymbol requestPageTypeSymbol = reportObjectMembers.GetMembers().FirstOrDefault((Symbol member) => member.Kind == SymbolKind.RequestPage) as RequestPageTypeSymbol;
				if (requestPageTypeSymbol != null && requestPageTypeSymbol.RelatedTable != null)
				{
					RecordTypeSymbol record = reportObjectMembers.ContainingSymbol.ContainingModule.GetRecord(requestPageTypeSymbol.RelatedTable, isTemporary: false);
					instance.Add(SynthesizedGlobalVariableSymbol.Create(containingSymbol, record, "Rec", isThis: false, hasImplicitWith: true));
					instance.Add(SynthesizedGlobalVariableSymbol.Create(containingSymbol, record, "xRec"));
				}
				return instance.ToImmutable();
			}
			finally
			{
				instance.Free();
			}
		});
		requestPage = new Lazy<RequestPageTypeSymbol>(() => (reportObjectMembers.GetMembers().FirstOrDefault((Symbol member) => member.Kind == SymbolKind.RequestPage) as RequestPageTypeSymbol) ?? new SynthesizedRequestPageTypeSymbol(containingSymbol));
	}

	protected override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetReportTriggerInfo(name.ToUpperInvariant());
	}

	protected override void BuildDeclaredMembersCore(ArrayBuilder<Symbol> members, DiagnosticBag diagnostics)
	{
		members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, ReportReferenceTypeSymbol.Create(ContainingSymbol, isCurrReport: true), "CurrReport"));
		ReportDataSetSectionSyntax dataSet = Declaration.DataSet;
		if (dataSet != null)
		{
			_ = dataSet.DataItems;
			if (true)
			{
				foreach (ReportDataItemSyntax dataItem in Declaration.DataSet.DataItems)
				{
					members.Add(SourceReportDataItemSymbol.Create(ContainingSymbol, dataItem, 0));
				}
			}
		}
		if (Declaration.RequestPage != null)
		{
			members.Add(new SourceRequestPageTypeSymbol(ContainingSymbol, Declaration.RequestPage));
		}
		ReportLabelsSectionSyntax labels = Declaration.Labels;
		if (labels == null)
		{
			return;
		}
		_ = labels.Labels;
		if (1 == 0)
		{
			return;
		}
		foreach (ReportLabelBaseSyntax label in Declaration.Labels.Labels)
		{
			if (label.Kind == SyntaxKind.ReportLabel)
			{
				members.Add(SourceReportLabelSymbol.Create(ContainingSymbol, (ReportLabelSyntax)label));
			}
			else
			{
				members.Add(SourceReportLabelMultilanguageSymbol.Create(ContainingSymbol, (ReportLabelMultilanguageSyntax)label));
			}
		}
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		ObjectMembers<SourceReportTypeSymbol>.CheckMembersForDuplicateNames(GetSymbolsWithGlobalVisibilityForNameChecking(), diagnostics, (Symbol m) => true, ErrorCode.ERR_DuplicateMemberName);
		ObjectMembers<SourceReportTypeSymbol>.CheckMembersForDuplicateNames((from m in GetMembers()
			where m.Kind == SymbolKind.ReportLabel
			select m).Concat(ContainingSymbol.FlattenedColumns), diagnostics, (Symbol s) => true, ErrorCode.ERR_DuplicateColumnOrLabelName);
		CheckMembersForInvalidNames(FlattenedColumns, diagnostics);
		PageMemberSymbolHelpers.CheckHelpLinkAndContextSensitiveHelpPropertyAtTheSameTime(ContainingSymbol.RequestPage, diagnostics);
	}

	private void CheckMembersForInvalidNames(IEnumerable<SourceReportColumnSymbol> columns, DiagnosticBag diagnostics)
	{
		foreach (SourceReportColumnSymbol column in columns)
		{
			if (!column.Name.IsValidClsIdentifier())
			{
				diagnostics.Add(ErrorCode.ERR_NameIsNotValidClsIdentifier, column.GetLocation(), column.Name);
			}
			CheckIfColumnNameReservedForInternalUse(column, columns, diagnostics);
		}
	}

	private static void CheckIfColumnNameReservedForInternalUse(SourceReportColumnSymbol column, IEnumerable<SourceReportColumnSymbol> columns, DiagnosticBag diagnostics)
	{
		if (!string.IsNullOrEmpty(column.Name) && column.Name.EndsWith("Format", StringComparison.OrdinalIgnoreCase))
		{
			string nameWithoutReservedNamesuffix = column.Name.Substring(0, column.Name.Length - "Format".Length);
			if (columns.Any((SourceReportColumnSymbol c) => SemanticFacts.IsSameName(c.Name, nameWithoutReservedNamesuffix)))
			{
				diagnostics.Add(ErrorCode.ERR_ColumnNameFormatClash, column.GetLocation(), column.Name, nameWithoutReservedNamesuffix);
			}
		}
	}

	protected override bool IsNameRestrictedForTriggers(string name)
	{
		if (!TriggerDefinitions.IsValidReportTrigger(name))
		{
			return TriggerDefinitions.IsValidReportDataItemTrigger(name);
		}
		return true;
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
			ImmutableArray<Symbol> members = GetMembers();
			instance.AddRange(ContainingSymbol.FlattenedDataItems);
			foreach (Symbol item in members)
			{
				if (item.Kind != SymbolKind.ReportDataItem)
				{
					instance.Add(item);
				}
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyCodeMembers, instance.ToImmutable(), default);
			return lazyCodeMembers;
		}
		finally
		{
			instance.Free();
		}
	}

	private IEnumerable<Symbol> GetSymbolsWithGlobalVisibilityForNameChecking()
	{
		return (from s in GetCodeMembers()
			where s.Kind != SymbolKind.ReportLabel
			select s).Concat(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, TypeSymbol.GetNavTypeSymbol(NavTypeKind.Integer), "Rec")).Concat(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, TypeSymbol.GetNavTypeSymbol(NavTypeKind.Integer), "xRec"));
	}
}

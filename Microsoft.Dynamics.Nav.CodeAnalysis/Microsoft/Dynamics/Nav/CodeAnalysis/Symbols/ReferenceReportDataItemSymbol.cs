using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class ReferenceReportDataItemSymbol : ReportDataItemSymbol
{
	private readonly Lazy<TableTypeSymbol> lazyRelatedTable;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	public override int Id { get; }

	public override string Name { get; }

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override TableTypeSymbol RelatedTable => lazyRelatedTable.Value;

	public ReferenceReportDataItemSymbol(Symbol containingSymbol, ReportDataItemDefinition dataItemDefinition)
	{
		ReferenceReportDataItemSymbol containingSymbol2 = this;
		Id = dataItemDefinition.Id ?? (-1);
		Name = dataItemDefinition.Name;
		ContainingSymbol = containingSymbol;
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, dataItemDefinition);
		lazyRelatedTable = ReferenceSymbolHelper.CreateLazy(() => ReferenceSymbolHelper.GetRelatedTableSymbol(containingSymbol2, dataItemDefinition.RelatedTable));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}
}

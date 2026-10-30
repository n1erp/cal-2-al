using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class ReferenceQueryDataItemSymbol : QueryDataItemSymbol
{
	private readonly Lazy<TableTypeSymbol> lazyRelatedTable;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	public override int Id { get; }

	public override string Name { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override TableTypeSymbol RelatedTable => lazyRelatedTable.Value;

	internal ReferenceQueryDataItemSymbol(Symbol containingSymbol, QueryDataItemDefinition dataItemDefinition)
	{
		ReferenceQueryDataItemSymbol referenceQueryDataItemSymbol = this;
		Id = dataItemDefinition.Id ?? (-1);
		Name = dataItemDefinition.Name;
		ContainingSymbol = containingSymbol;
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, dataItemDefinition);
		lazyRelatedTable = ReferenceSymbolHelper.CreateLazy(() => ReferenceSymbolHelper.GetRelatedTableSymbol(referenceQueryDataItemSymbol, dataItemDefinition.RelatedTable));
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referenceQueryDataItemSymbol.BuildMembers(dataItemDefinition));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	private ImmutableArray<Symbol> BuildMembers(QueryDataItemDefinition dataItemDefinition)
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(dataItemDefinition.DataItems.GetLengthSafe() + dataItemDefinition.Columns.GetLengthSafe() + dataItemDefinition.Filters.GetLengthSafe());
		try
		{
			if (dataItemDefinition.DataItems != null)
			{
				QueryDataItemDefinition[] dataItems = dataItemDefinition.DataItems;
				foreach (QueryDataItemDefinition dataItemDefinition2 in dataItems)
				{
					instance.Add(new ReferenceQueryDataItemSymbol(this, dataItemDefinition2));
				}
			}
			if (dataItemDefinition.Columns != null)
			{
				QueryColumnDefinition[] columns = dataItemDefinition.Columns;
				foreach (QueryColumnDefinition column in columns)
				{
					instance.Add(new ReferenceQueryColumnSymbol(this, column));
				}
			}
			if (dataItemDefinition.Filters != null)
			{
				QueryFilterDefinition[] filters = dataItemDefinition.Filters;
				foreach (QueryFilterDefinition filterDefinition in filters)
				{
					instance.Add(new ReferenceQueryFilterSymbol(this, filterDefinition));
				}
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}
}

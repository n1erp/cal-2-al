using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceQueryFilterSymbol : QueryFilterSymbol
{
	private readonly Lazy<FieldSymbol> lazyRelatedField;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private DiagnosticInfo useSiteDiagnostics;

	public override int Id { get; }

	public override string Name { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override FieldSymbol RelatedField => lazyRelatedField.Value;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public ReferenceQueryFilterSymbol(QueryDataItemSymbol containingSymbol, QueryFilterDefinition filterDefinition)
	{
		ReferenceQueryFilterSymbol referenceQueryFilterSymbol = this;
		ContainingSymbol = containingSymbol;
		Id = filterDefinition.Id ?? (-1);
		Name = filterDefinition.Name();
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, filterDefinition);
		lazyRelatedField = ReferenceSymbolHelper.CreateLazy(() => ReferenceQueryColumnSymbol.GetSourceFieldSymbol(filterDefinition, (QueryDataItemSymbol)referenceQueryFilterSymbol.ContainingSymbol, ref referenceQueryFilterSymbol.useSiteDiagnostics));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return Properties.CastArray<Symbol>();
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (!(lazyRelatedField.Value == null))
		{
			return null;
		}
		return useSiteDiagnostics;
	}
}

using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferenceParameterSymbol : ParameterSymbol
{
	public override string Name { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsParams => false;

	public override bool IsVar { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override int Ordinal { get; }

	public override TypeSymbol Type { get; }

	internal ReferenceParameterSymbol(Symbol containingSymbol, ParameterDefinition parameter, int ordinal)
	{
		ContainingSymbol = containingSymbol;
		Ordinal = ordinal;
		Name = parameter.Name();
		IsVar = parameter.IsVar ?? false;
		Type = SerializableSymbolModelConverter.ResolveType(containingSymbol.ContainingModule, this, parameter.GetTypeDefinition());
	}
}

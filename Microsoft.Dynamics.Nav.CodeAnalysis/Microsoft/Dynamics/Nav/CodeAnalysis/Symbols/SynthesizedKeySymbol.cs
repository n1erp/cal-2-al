using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SynthesizedKeySymbol : KeySymbol
{
	public override string Name { get; }

	public override ImmutableArray<FieldSymbol> Fields { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<PropertySymbol> Properties => ImmutableArray<PropertySymbol>.Empty;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public SynthesizedKeySymbol(Symbol containingSymbol, FieldSymbol field)
	{
		ContainingSymbol = containingSymbol;
		Fields = ImmutableArray.Create(field);
		Name = field.Name;
	}
}

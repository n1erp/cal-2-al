using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferenceAttributeArgumentSymbol : AttributeArgumentSymbol
{
	public override string Name { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Symbol ContainingSymbol { get; }

	public override TypeSymbol Type { get; }

	public override string ValueText { get; }

	public ReferenceAttributeArgumentSymbol(Symbol containingSymbol, TypeSymbol type, string name, string value)
	{
		ContainingSymbol = containingSymbol;
		ValueText = value;
		Type = type;
		Name = name;
	}
}

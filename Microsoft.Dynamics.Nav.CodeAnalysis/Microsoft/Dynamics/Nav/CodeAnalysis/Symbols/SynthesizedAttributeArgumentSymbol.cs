using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class SynthesizedAttributeArgumentSymbol : AttributeArgumentSymbol
{
	public override TypeSymbol Type { get; }

	public override string ValueText { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public SynthesizedAttributeArgumentSymbol(Symbol containingSymbol, TypeSymbol type, string valueText)
	{
		ContainingSymbol = containingSymbol;
		Type = type;
		ValueText = valueText;
	}
}

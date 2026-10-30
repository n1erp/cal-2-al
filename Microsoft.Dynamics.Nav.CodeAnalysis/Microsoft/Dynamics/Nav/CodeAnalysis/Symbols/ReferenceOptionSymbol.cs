using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferenceOptionSymbol : OptionSymbol
{
	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override int Ordinal { get; }

	public override TypeSymbol Type { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public static ReferenceOptionSymbol Create(Symbol owner, string name, int ordinal)
	{
		return new ReferenceOptionSymbol(owner, name, ordinal);
	}

	private ReferenceOptionSymbol(Symbol containingSymbol, string name, int ordinal)
	{
		ContainingSymbol = containingSymbol;
		Name = name;
		Ordinal = ordinal;
		Type = containingSymbol as TypeSymbol;
	}
}

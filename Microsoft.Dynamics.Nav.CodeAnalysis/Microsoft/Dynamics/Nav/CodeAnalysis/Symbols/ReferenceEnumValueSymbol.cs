using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferenceEnumValueSymbol : EnumValueSymbol
{
	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override int Ordinal { get; }

	public override TypeSymbol Type { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public static ReferenceEnumValueSymbol Create(Symbol owner, string name, int ordinal)
	{
		return new ReferenceEnumValueSymbol(owner, name, ordinal);
	}

	private ReferenceEnumValueSymbol(Symbol containingSymbol, string name, int ordinal)
	{
		ContainingSymbol = containingSymbol;
		Name = name;
		Ordinal = ordinal;
		ISymbol symbol = containingSymbol;
		if (symbol is IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbol)
		{
			symbol = applicationObjectExtensionTypeSymbol.Target;
		}
		Type = symbol as TypeSymbol;
	}
}

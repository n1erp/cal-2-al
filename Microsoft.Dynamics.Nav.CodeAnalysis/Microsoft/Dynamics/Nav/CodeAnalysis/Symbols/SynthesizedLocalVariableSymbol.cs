using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SynthesizedLocalVariableSymbol : VariableSymbol
{
	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<AttributeSymbol> Attributes => ImmutableArray<AttributeSymbol>.Empty;

	public override TypeSymbol Type { get; }

	public override VariableKind VariableKind => VariableKind.Synthesized;

	public override string Name { get; }

	public override SymbolKind Kind => SymbolKind.LocalVariable;

	public bool HasImplicitWith { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override bool IsImplicitlyDeclared => true;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public static SynthesizedLocalVariableSymbol Create(SourceMethodOrTriggerSymbol container, TypeSymbol type, string name, bool hasImplicitWith = false)
	{
		return new SynthesizedLocalVariableSymbol(container, type, name, hasImplicitWith);
	}

	private SynthesizedLocalVariableSymbol(SourceMethodOrTriggerSymbol container, TypeSymbol type, string name, bool hasImplicitWith)
	{
		ContainingSymbol = container;
		Type = type;
		Name = name;
		HasImplicitWith = hasImplicitWith;
	}
}

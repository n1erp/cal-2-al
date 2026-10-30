using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SynthesizedGlobalVariableSymbol : VariableSymbol
{
	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<AttributeSymbol> Attributes => ImmutableArray<AttributeSymbol>.Empty;

	public override VariableKind VariableKind => VariableKind.Synthesized;

	public override TypeSymbol Type { get; }

	public override string Name { get; }

	public override string MetadataName { get; }

	public bool IsThis { get; }

	public bool HasImplicitWith { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override bool IsImplicitlyDeclared { get; }

	public bool SuppressTarget { get; }

	public override Accessibility DeclaredAccessibility { get; }

	public static SynthesizedGlobalVariableSymbol Create(ObjectTypeSymbol container, TypeSymbol type, string name, bool isThis = false, bool hasImplicitWith = false, Accessibility declaredAccessibility = Accessibility.Private, bool isImplicitDeclared = true, bool suppressTarget = true, string metadataName = null)
	{
		return new SynthesizedGlobalVariableSymbol(container, type, name, isThis, hasImplicitWith, declaredAccessibility, isImplicitDeclared, suppressTarget, metadataName);
	}

	private SynthesizedGlobalVariableSymbol(ObjectTypeSymbol container, TypeSymbol type, string name, bool isThis, bool hasImplicitWith, Accessibility declaredAccessibility, bool isImplicitDeclared, bool suppressTarget, string metadataName)
	{
		ContainingSymbol = container;
		Type = type;
		Name = name;
		IsThis = isThis;
		HasImplicitWith = hasImplicitWith;
		DeclaredAccessibility = declaredAccessibility;
		IsImplicitlyDeclared = isImplicitDeclared;
		SuppressTarget = suppressTarget;
		MetadataName = metadataName ?? name;
	}
}

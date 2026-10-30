using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferenceEventSymbol : EventSymbol
{
	private readonly Lazy<ImmutableArray<ParameterSymbol>> lazyParameters;

	private readonly Lazy<ImmutableArray<AttributeSymbol>> lazyAttributes;

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; } = ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<Location> Locations { get; } = ImmutableArray<Location>.Empty;

	public override ImmutableArray<ParameterSymbol> Parameters => lazyParameters.Value;

	public override ImmutableArray<AttributeSymbol> Attributes => lazyAttributes.Value;

	internal ReferenceEventSymbol(Symbol containingSymbol, EventDefinition definition)
	{
		ContainingSymbol = containingSymbol;
		Name = definition.Name;
		lazyParameters = ReferenceSymbolHelper.BuildParametersLazy(this, definition);
		lazyAttributes = ReferenceSymbolHelper.BuildAttributesLazy(this, definition);
	}
}

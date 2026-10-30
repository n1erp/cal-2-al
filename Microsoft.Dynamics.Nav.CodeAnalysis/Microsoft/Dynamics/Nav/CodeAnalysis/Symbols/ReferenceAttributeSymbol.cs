using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class ReferenceAttributeSymbol : AttributeSymbol
{
	private readonly Lazy<ImmutableArray<AttributeArgumentSymbol>> lazyArguments;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override string Name { get; }

	public override Symbol ContainingSymbol { get; }

	internal override BoundAttribute Attribute => null;

	internal override ImmutableArray<AttributeArgumentSymbol> Arguments => lazyArguments.Value;

	internal override AttributeTypeInfo AttributeInfo { get; }

	internal ReferenceAttributeSymbol(Symbol containingSymbol, AttributeDefinition attributeDefinition)
	{
		ReferenceAttributeSymbol referenceAttributeSymbol = this;
		ContainingSymbol = containingSymbol;
		Name = attributeDefinition.Name;
		AttributeInfo = AttributeDefinitions.GetAttributeInfo(Name);
		lazyArguments = ReferenceSymbolHelper.CreateLazy(() => referenceAttributeSymbol.GetArgumentsLazy(attributeDefinition));
	}

	private ImmutableArray<AttributeArgumentSymbol> GetArgumentsLazy(AttributeDefinition attributeDefinition)
	{
		return attributeDefinition.Arguments.AsImmutableOrEmpty((Func<AttributeArgumentDefinition, AttributeArgumentSymbol>)((AttributeArgumentDefinition argument) => new ReferenceAttributeArgumentSymbol(this, null, string.Empty, argument.Value)));
	}
}

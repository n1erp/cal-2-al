using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class SynthesizedAttributeSymbol : AttributeSymbol
{
	public override Symbol ContainingSymbol { get; }

	internal override BoundAttribute Attribute { get; }

	internal override ImmutableArray<AttributeArgumentSymbol> Arguments { get; }

	internal override AttributeTypeInfo AttributeInfo { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public SynthesizedAttributeSymbol(Symbol containingSymbol, BoundAttribute attribute, ImmutableArray<AttributeArgumentSymbol> arguments, AttributeTypeInfo attributeInfo)
	{
		ContainingSymbol = containingSymbol;
		Attribute = attribute;
		Arguments = arguments;
		AttributeInfo = attributeInfo;
	}
}

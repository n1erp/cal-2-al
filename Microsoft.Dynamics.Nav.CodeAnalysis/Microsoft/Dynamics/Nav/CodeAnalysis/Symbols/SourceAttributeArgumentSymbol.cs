using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SourceAttributeArgumentSymbol : AttributeArgumentSymbol
{
	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public override TypeSymbol Type { get; }

	public override string ValueText { get; }

	public ApplicationObjectTypeSymbol ValueAsSymbol { get; }

	public SourceAttributeArgumentSymbol(Symbol containingSymbol, BoundExpression node)
	{
		ContainingSymbol = containingSymbol;
		Locations = ImmutableArray.Create(node.Syntax.Location);
		DeclaringSyntaxReferences = ImmutableArray.Create(node.Syntax.Parent.GetReference());
		if (node.Kind == BoundKind.Literal)
		{
			ValueText = node.ConstantValue.Value.ToString();
		}
		else if (node.Kind == BoundKind.OptionAccess)
		{
			ValueText = ((BoundOptionAccess)node).OptionSymbol.Name;
		}
		else if (node.Kind == BoundKind.ApplicationObjectAccess)
		{
			ValueAsSymbol = ((BoundApplicationObjectAccess)node).ApplicationObjectTypeSymbol;
		}
		Type = node.Type;
	}
}

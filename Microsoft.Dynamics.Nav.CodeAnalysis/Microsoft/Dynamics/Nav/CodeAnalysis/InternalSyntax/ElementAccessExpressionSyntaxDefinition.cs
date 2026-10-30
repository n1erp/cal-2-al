using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ElementAccessExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ElementAccessExpressionSyntaxDefinition> Instance = new Lazy<ElementAccessExpressionSyntaxDefinition>(() => new ElementAccessExpressionSyntaxDefinition());

	public ElementAccessExpressionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Expression",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = BracketedArgumentListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "ArgumentList",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

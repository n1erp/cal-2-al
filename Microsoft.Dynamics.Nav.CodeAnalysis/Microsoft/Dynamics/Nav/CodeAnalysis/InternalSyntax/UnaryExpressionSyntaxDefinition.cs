using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class UnaryExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<UnaryExpressionSyntaxDefinition> Instance = new Lazy<UnaryExpressionSyntaxDefinition>(() => new UnaryExpressionSyntaxDefinition());

	public UnaryExpressionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OperatorToken",
			Kinds = ImmutableArray.Create(SyntaxKind.PlusToken, SyntaxKind.MinusToken, SyntaxKind.NotKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Expression",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

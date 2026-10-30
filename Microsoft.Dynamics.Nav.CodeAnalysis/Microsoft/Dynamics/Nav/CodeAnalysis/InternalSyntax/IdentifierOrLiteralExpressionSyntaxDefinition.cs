using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class IdentifierOrLiteralExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<IdentifierOrLiteralExpressionSyntaxDefinition> Instance = new Lazy<IdentifierOrLiteralExpressionSyntaxDefinition>(() => new IdentifierOrLiteralExpressionSyntaxDefinition());

	public IdentifierOrLiteralExpressionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Identifier",
			Kinds = ImmutableArray.Create(SyntaxKind.IdentifierName, SyntaxKind.LiteralExpression)
		}));
	}
}

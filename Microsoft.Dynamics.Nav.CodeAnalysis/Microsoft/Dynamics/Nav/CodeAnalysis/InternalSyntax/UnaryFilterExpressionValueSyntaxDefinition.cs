using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class UnaryFilterExpressionValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<UnaryFilterExpressionValueSyntaxDefinition> Instance = new Lazy<UnaryFilterExpressionValueSyntaxDefinition>(() => new UnaryFilterExpressionValueSyntaxDefinition());

	public UnaryFilterExpressionValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "OperatorToken",
			Kinds = ImmutableArray.Create<SyntaxKind>(SyntaxKind.NotEqualsToken, SyntaxKind.EqualsToken, SyntaxKind.LessThanToken, SyntaxKind.LessThanEqualsToken, SyntaxKind.GreaterThanToken, SyntaxKind.GreaterThanEqualsToken, SyntaxKind.DotDotToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = IdentifierOrLiteralExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Identifier",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class BinaryExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<BinaryExpressionSyntaxDefinition> Instance = new Lazy<BinaryExpressionSyntaxDefinition>(() => new BinaryExpressionSyntaxDefinition());

	public BinaryExpressionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Left",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OperatorToken",
			Kinds = ImmutableArray.Create<SyntaxKind>(SyntaxKind.PlusToken, SyntaxKind.MinusToken, SyntaxKind.MultiplyToken, SyntaxKind.RDivToken, SyntaxKind.IDivToken, SyntaxKind.ModuloToken, SyntaxKind.EqualsToken, SyntaxKind.NotEqualsToken, SyntaxKind.GreaterThanToken, SyntaxKind.GreaterThanEqualsToken, SyntaxKind.LessThanToken, SyntaxKind.LessThanEqualsToken, SyntaxKind.OrKeyword, SyntaxKind.AndKeyword, SyntaxKind.XorKeyword, SyntaxKind.DotDotToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Right",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

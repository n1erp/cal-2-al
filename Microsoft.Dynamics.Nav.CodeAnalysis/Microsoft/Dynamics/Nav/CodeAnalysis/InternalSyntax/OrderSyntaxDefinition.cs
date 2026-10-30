using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class OrderSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<OrderSyntaxDefinition> Instance = new Lazy<OrderSyntaxDefinition>(() => new OrderSyntaxDefinition());

	public OrderSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OrderToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OrderKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenParenthesisToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenParenToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OrderBy",
			Kinds = ImmutableArray.Create(SyntaxKind.AscendingKeyword, SyntaxKind.DescendingKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "CloseParenthesisToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CloseParenToken)
		}));
	}
}

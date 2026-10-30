using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class OrderByPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<OrderByPropertyValueSyntaxDefinition> Instance = new Lazy<OrderByPropertyValueSyntaxDefinition>(() => new OrderByPropertyValueSyntaxDefinition());

	public OrderByPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = OrderByExpressionSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Order",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

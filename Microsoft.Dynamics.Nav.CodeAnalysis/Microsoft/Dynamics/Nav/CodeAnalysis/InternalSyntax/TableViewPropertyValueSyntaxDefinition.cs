using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class TableViewPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<TableViewPropertyValueSyntaxDefinition> Instance = new Lazy<TableViewPropertyValueSyntaxDefinition>(() => new TableViewPropertyValueSyntaxDefinition());

	public TableViewPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = SortingSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Sorting",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = OrderSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Order",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = WhereExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "TableFilter",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

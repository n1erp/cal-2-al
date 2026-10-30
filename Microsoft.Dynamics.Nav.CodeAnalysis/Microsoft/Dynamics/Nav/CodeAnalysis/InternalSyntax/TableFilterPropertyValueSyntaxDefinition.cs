using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class TableFilterPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<TableFilterPropertyValueSyntaxDefinition> Instance = new Lazy<TableFilterPropertyValueSyntaxDefinition>(() => new TableFilterPropertyValueSyntaxDefinition());

	public TableFilterPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = PropertyExpressionSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Conditions",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class TableRelationPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<TableRelationPropertyValueSyntaxDefinition> Instance = new Lazy<TableRelationPropertyValueSyntaxDefinition>(() => new TableRelationPropertyValueSyntaxDefinition());

	public TableRelationPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IfTableRelationSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "IfExpression",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = MemberAccessOrIdentifierPropertyValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "RelatedTableField",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = WhereExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "TableFilter",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = ElseTableRelationSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "ElseExpression",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

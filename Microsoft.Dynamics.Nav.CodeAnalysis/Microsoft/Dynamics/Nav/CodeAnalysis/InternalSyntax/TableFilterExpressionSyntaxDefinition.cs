using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class TableFilterExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<TableFilterExpressionSyntaxDefinition> Instance = new Lazy<TableFilterExpressionSyntaxDefinition>(() => new TableFilterExpressionSyntaxDefinition());

	public TableFilterExpressionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenParenthesisToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenParenToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = PropertyExpressionSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Conditions",
			Kinds = ImmutableArray.Create<SyntaxKind>(SyntaxKind.SimpleFieldExpression, SyntaxKind.FieldFilterExpression, SyntaxKind.FieldUpperLimitExpression, SyntaxKind.FieldUpperLimitFilterExpression, SyntaxKind.ConstExpression, SyntaxKind.FilterExpression)
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

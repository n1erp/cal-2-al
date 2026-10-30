using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class InvalidPropertyExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<InvalidPropertyExpressionSyntaxDefinition> Instance = new Lazy<InvalidPropertyExpressionSyntaxDefinition>(() => new InvalidPropertyExpressionSyntaxDefinition());

	public InvalidPropertyExpressionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "LeftHandSide",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "EqualsToken",
			Kinds = ImmutableArray.Create(SyntaxKind.EqualsToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "ConstOrFieldOrFilterKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.ConstFormulaKeyword, SyntaxKind.FilterFormulaKeyword, SyntaxKind.FieldFormulaKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "InvalidExpression",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

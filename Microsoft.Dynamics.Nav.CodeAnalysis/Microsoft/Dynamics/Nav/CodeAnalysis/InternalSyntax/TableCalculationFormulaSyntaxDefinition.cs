using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class TableCalculationFormulaSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<TableCalculationFormulaSyntaxDefinition> Instance = new Lazy<TableCalculationFormulaSyntaxDefinition>(() => new TableCalculationFormulaSyntaxDefinition());

	public TableCalculationFormulaSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "Sign",
			Kinds = ImmutableArray.Create(SyntaxKind.MinusToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "FormulaKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.ExistCalculationFormulaKeyword, SyntaxKind.CountCalculationFormulaKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenParenthesisToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenParenToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Table",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = WhereExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "WhereExpression",
			Kinds = ImmutableArray.Create<SyntaxKind>()
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

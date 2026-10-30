using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class FieldCalculationFormulaSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<FieldCalculationFormulaSyntaxDefinition> Instance = new Lazy<FieldCalculationFormulaSyntaxDefinition>(() => new FieldCalculationFormulaSyntaxDefinition());

	public FieldCalculationFormulaSyntaxDefinition()
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
			Kinds = ImmutableArray.Create<SyntaxKind>(SyntaxKind.SumCalculationFormulaKeyword, SyntaxKind.AverageCalculationFormulaKeyword, SyntaxKind.MinCalculationFormulaKeyword, SyntaxKind.MaxCalculationFormulaKeyword, SyntaxKind.LookupCalculationFormulaKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenParenthesisToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenParenToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = MemberAccessExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Field",
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

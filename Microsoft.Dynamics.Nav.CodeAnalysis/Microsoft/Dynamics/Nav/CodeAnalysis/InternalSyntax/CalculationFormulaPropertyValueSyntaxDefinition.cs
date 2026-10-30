using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class CalculationFormulaPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<CalculationFormulaPropertyValueSyntaxDefinition> Instance = new Lazy<CalculationFormulaPropertyValueSyntaxDefinition>(() => new CalculationFormulaPropertyValueSyntaxDefinition());

	public CalculationFormulaPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = TableCalculationFormulaSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = FieldCalculationFormulaSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}

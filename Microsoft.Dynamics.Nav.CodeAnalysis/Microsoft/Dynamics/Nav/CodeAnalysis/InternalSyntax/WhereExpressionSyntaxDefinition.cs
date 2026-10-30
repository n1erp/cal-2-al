using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class WhereExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<WhereExpressionSyntaxDefinition> Instance = new Lazy<WhereExpressionSyntaxDefinition>(() => new WhereExpressionSyntaxDefinition());

	public WhereExpressionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "WhereKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.WhereFormulaKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = TableFilterExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Filter",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class IfTableRelationSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<IfTableRelationSyntaxDefinition> Instance = new Lazy<IfTableRelationSyntaxDefinition>(() => new IfTableRelationSyntaxDefinition());

	public IfTableRelationSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "IfKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.IfKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = TableFilterExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "IfTableRelationCondition",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ElseTableRelationSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ElseTableRelationSyntaxDefinition> Instance = new Lazy<ElseTableRelationSyntaxDefinition>(() => new ElseTableRelationSyntaxDefinition());

	public ElseTableRelationSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ElseKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.ElseKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = TableRelationPropertyValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "ElseTableRelationCondition",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class QueryDataItemLinkExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<QueryDataItemLinkExpressionSyntaxDefinition> Instance = new Lazy<QueryDataItemLinkExpressionSyntaxDefinition>(() => new QueryDataItemLinkExpressionSyntaxDefinition());

	public QueryDataItemLinkExpressionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "SourceField",
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
			Node = MemberAccessExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "RelatedField",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

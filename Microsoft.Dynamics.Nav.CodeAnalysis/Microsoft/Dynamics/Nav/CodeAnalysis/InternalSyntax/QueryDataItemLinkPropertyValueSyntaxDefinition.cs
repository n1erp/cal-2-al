using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class QueryDataItemLinkPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<QueryDataItemLinkPropertyValueSyntaxDefinition> Instance = new Lazy<QueryDataItemLinkPropertyValueSyntaxDefinition>(() => new QueryDataItemLinkPropertyValueSyntaxDefinition());

	public QueryDataItemLinkPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = QueryDataItemLinkExpressionSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Joins",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

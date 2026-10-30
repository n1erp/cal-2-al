using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class QueryDataItemElementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<QueryDataItemElementSyntaxDefinition> Instance = new Lazy<QueryDataItemElementSyntaxDefinition>(() => new QueryDataItemElementSyntaxDefinition());

	public QueryDataItemElementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = QueryDataItemSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = QueryElementSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}

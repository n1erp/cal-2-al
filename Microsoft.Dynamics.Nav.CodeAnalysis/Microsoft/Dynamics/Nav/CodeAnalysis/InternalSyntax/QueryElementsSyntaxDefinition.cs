using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class QueryElementsSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<QueryElementsSyntaxDefinition> Instance = new Lazy<QueryElementsSyntaxDefinition>(() => new QueryElementsSyntaxDefinition());

	public QueryElementsSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "QueryElementsKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.QueryElementsKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBraceToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = QueryDataItemSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "DataItems",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "CloseBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CloseBraceToken)
		}));
	}
}

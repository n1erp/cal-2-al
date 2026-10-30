using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class PageViewListSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<PageViewListSyntaxDefinition> Instance = new Lazy<PageViewListSyntaxDefinition>(() => new PageViewListSyntaxDefinition());

	public PageViewListSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ViewsKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.ViewsKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBraceToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = PageViewSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Views",
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

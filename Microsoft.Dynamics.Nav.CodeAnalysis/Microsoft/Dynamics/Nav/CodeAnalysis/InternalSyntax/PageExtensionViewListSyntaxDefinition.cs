using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class PageExtensionViewListSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<PageExtensionViewListSyntaxDefinition> Instance = new Lazy<PageExtensionViewListSyntaxDefinition>(() => new PageExtensionViewListSyntaxDefinition());

	public PageExtensionViewListSyntaxDefinition()
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
			Node = ViewChangeBaseSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Changes",
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

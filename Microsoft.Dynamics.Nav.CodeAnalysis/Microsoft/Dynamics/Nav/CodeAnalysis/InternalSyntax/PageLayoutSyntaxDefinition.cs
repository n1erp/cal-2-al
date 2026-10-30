using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class PageLayoutSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<PageLayoutSyntaxDefinition> Instance = new Lazy<PageLayoutSyntaxDefinition>(() => new PageLayoutSyntaxDefinition());

	public PageLayoutSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "LayoutKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.PageLayoutKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBraceToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = PageAreaSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Areas",
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

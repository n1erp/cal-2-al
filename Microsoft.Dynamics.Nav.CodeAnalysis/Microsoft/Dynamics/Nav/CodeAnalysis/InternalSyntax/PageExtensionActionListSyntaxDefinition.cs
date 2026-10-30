using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class PageExtensionActionListSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<PageExtensionActionListSyntaxDefinition> Instance = new Lazy<PageExtensionActionListSyntaxDefinition>(() => new PageExtensionActionListSyntaxDefinition());

	public PageExtensionActionListSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ActionsKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.ActionsKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBraceToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = ActionChangeBaseSyntaxDefinition.Instance.Value,
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

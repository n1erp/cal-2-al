using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ViewAddChangeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ViewAddChangeSyntaxDefinition> Instance = new Lazy<ViewAddChangeSyntaxDefinition>(() => new ViewAddChangeSyntaxDefinition());

	public ViewAddChangeSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ChangeKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.AddFirstKeyword, SyntaxKind.AddLastKeyword, SyntaxKind.AddBeforeKeyword, SyntaxKind.AddAfterKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "OpenParenthesisToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenParenToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Anchor",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "CloseParenthesisToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CloseParenToken)
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

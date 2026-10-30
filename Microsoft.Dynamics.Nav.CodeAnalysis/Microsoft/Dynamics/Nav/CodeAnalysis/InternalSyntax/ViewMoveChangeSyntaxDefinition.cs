using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ViewMoveChangeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ViewMoveChangeSyntaxDefinition> Instance = new Lazy<ViewMoveChangeSyntaxDefinition>(() => new ViewMoveChangeSyntaxDefinition());

	public ViewMoveChangeSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ChangeKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.MoveFirstKeyword, SyntaxKind.MoveLastKeyword, SyntaxKind.MoveBeforeKeyword, SyntaxKind.MoveAfterKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
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
			Name = "SemicolonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.SemicolonToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Views",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "CloseParenthesisToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CloseParenToken)
		}));
	}
}

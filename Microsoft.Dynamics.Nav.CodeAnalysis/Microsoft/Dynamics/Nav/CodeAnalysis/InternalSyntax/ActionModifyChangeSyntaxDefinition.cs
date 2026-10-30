using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ActionModifyChangeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ActionModifyChangeSyntaxDefinition> Instance = new Lazy<ActionModifyChangeSyntaxDefinition>(() => new ActionModifyChangeSyntaxDefinition());

	public ActionModifyChangeSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ChangeKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.ModifyKeyword)
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
			IsOptional = false,
			Name = "Name",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
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
			Node = PropertyListSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "PropertyList",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = TriggerDeclarationSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Triggers",
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

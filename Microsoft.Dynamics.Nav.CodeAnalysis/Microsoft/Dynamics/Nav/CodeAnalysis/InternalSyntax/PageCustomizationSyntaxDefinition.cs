using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class PageCustomizationSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<PageCustomizationSyntaxDefinition> Instance = new Lazy<PageCustomizationSyntaxDefinition>(() => new PageCustomizationSyntaxDefinition());

	public PageCustomizationSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "PageCustomizationKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.PageCustomizationKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = ObjectIdSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "ObjectId",
			Kinds = ImmutableArray.Create<SyntaxKind>()
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
			Name = "CustomizesKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.CustomizesKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = ObjectNameOrIdSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "BaseObject",
			Kinds = ImmutableArray.Create<SyntaxKind>()
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
			IsOptional = true,
			Name = "PropertyList",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = PageExtensionLayoutSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Layout",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = PageExtensionActionListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Actions",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = PageExtensionViewListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Views",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = MemberSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Members",
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

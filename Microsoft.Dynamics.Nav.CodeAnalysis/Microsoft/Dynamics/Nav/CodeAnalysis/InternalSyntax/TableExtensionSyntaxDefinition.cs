using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class TableExtensionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<TableExtensionSyntaxDefinition> Instance = new Lazy<TableExtensionSyntaxDefinition>(() => new TableExtensionSyntaxDefinition());

	public TableExtensionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "TableExtensionKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.TableExtensionKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = ObjectIdSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
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
			Name = "ExtendsKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.ExtendsKeyword)
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
			IsOptional = false,
			Name = "PropertyList",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = FieldExtensionListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Fields",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = KeyListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Keys",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = FieldGroupExtensionListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "FieldGroups",
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

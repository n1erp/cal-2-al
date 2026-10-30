using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class TableSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<TableSyntaxDefinition> Instance = new Lazy<TableSyntaxDefinition>(() => new TableSyntaxDefinition());

	public TableSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "TableKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.TableKeyword)
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
			Node = FieldListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
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
			Node = FieldGroupListSyntaxDefinition.Instance.Value,
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

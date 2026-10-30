using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class FieldGroupListSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<FieldGroupListSyntaxDefinition> Instance = new Lazy<FieldGroupListSyntaxDefinition>(() => new FieldGroupListSyntaxDefinition());

	public FieldGroupListSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "FieldGroupsKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.FieldGroupsKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBraceToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = FieldGroupSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "FieldGroups",
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

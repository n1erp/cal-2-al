using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class FieldGroupExtensionListSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<FieldGroupExtensionListSyntaxDefinition> Instance = new Lazy<FieldGroupExtensionListSyntaxDefinition>(() => new FieldGroupExtensionListSyntaxDefinition());

	public FieldGroupExtensionListSyntaxDefinition()
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
			Node = FieldGroupChangeBaseSyntaxDefinition.Instance.Value,
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

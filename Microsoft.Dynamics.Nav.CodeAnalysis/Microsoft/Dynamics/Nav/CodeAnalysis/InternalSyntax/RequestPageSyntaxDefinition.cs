using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class RequestPageSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<RequestPageSyntaxDefinition> Instance = new Lazy<RequestPageSyntaxDefinition>(() => new RequestPageSyntaxDefinition());

	public RequestPageSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "RequestPageKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.RequestPageKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBraceToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Name",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = PropertyListSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "PropertyList",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = PageLayoutSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Layout",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = PageActionListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Actions",
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

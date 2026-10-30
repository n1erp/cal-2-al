using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class MemberAttributeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<MemberAttributeSyntaxDefinition> Instance = new Lazy<MemberAttributeSyntaxDefinition>(() => new MemberAttributeSyntaxDefinition());

	public MemberAttributeSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenBracketToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBracketToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Name",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = AttributeArgumentListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "ArgumentList",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "CloseBracketToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CloseBracketToken)
		}));
	}
}

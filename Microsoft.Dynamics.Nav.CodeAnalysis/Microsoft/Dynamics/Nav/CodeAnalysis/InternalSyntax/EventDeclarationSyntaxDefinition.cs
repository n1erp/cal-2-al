using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class EventDeclarationSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<EventDeclarationSyntaxDefinition> Instance = new Lazy<EventDeclarationSyntaxDefinition>(() => new EventDeclarationSyntaxDefinition());

	public EventDeclarationSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = MemberAttributeSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Attributes",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "EventKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.EventKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Name",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = ParameterListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "ParameterList",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "SemicolonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.SemicolonToken)
		}));
	}
}

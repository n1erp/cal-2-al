using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class EventTriggerDeclarationSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<EventTriggerDeclarationSyntaxDefinition> Instance = new Lazy<EventTriggerDeclarationSyntaxDefinition>(() => new EventTriggerDeclarationSyntaxDefinition());

	public EventTriggerDeclarationSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = MemberAttributeSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = true,
			Name = "Attributes",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "TriggerKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.TriggerKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Publisher",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ColonColonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.ColonColonToken)
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
			Node = ReturnValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "ReturnValue",
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
			Node = VarSectionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Variables",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = BlockSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Body",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

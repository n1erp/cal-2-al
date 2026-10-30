using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class MethodOrTriggerDeclarationSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<MethodOrTriggerDeclarationSyntaxDefinition> Instance = new Lazy<MethodOrTriggerDeclarationSyntaxDefinition>(() => new MethodOrTriggerDeclarationSyntaxDefinition());

	public MethodOrTriggerDeclarationSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = TriggerDeclarationSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = EventTriggerDeclarationSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = MethodDeclarationSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}

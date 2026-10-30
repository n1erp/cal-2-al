using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class MemberSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<MemberSyntaxDefinition> Instance = new Lazy<MemberSyntaxDefinition>(() => new MemberSyntaxDefinition());

	public MemberSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = VarSectionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = MethodOrTriggerDeclarationSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = EventDeclarationSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}

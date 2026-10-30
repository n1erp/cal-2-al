using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class MemberReferencePropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<MemberReferencePropertyValueSyntaxDefinition> Instance = new Lazy<MemberReferencePropertyValueSyntaxDefinition>(() => new MemberReferencePropertyValueSyntaxDefinition());

	public MemberReferencePropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class MethodReferenceAttributeArgumentSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<MethodReferenceAttributeArgumentSyntaxDefinition> Instance = new Lazy<MethodReferenceAttributeArgumentSyntaxDefinition>(() => new MethodReferenceAttributeArgumentSyntaxDefinition());

	public MethodReferenceAttributeArgumentSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = MemberAccessExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "MethodReference",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

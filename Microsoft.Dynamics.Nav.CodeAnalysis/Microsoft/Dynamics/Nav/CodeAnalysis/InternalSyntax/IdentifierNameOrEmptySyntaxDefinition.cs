using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class IdentifierNameOrEmptySyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<IdentifierNameOrEmptySyntaxDefinition> Instance = new Lazy<IdentifierNameOrEmptySyntaxDefinition>(() => new IdentifierNameOrEmptySyntaxDefinition());

	public IdentifierNameOrEmptySyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "IdentifierName",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "Empty",
			Kinds = ImmutableArray.Create(SyntaxKind.EmptyToken)
		}));
	}
}

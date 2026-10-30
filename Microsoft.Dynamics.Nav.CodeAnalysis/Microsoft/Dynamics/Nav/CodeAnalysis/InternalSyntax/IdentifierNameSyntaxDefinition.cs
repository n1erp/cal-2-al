using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class IdentifierNameSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<IdentifierNameSyntaxDefinition> Instance = new Lazy<IdentifierNameSyntaxDefinition>(() => new IdentifierNameSyntaxDefinition());

	public IdentifierNameSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Identifier",
			Kinds = ImmutableArray.Create(SyntaxKind.IdentifierToken)
		}));
	}
}

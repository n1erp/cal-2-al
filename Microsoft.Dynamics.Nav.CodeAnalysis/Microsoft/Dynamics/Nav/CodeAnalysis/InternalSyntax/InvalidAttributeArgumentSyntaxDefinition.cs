using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class InvalidAttributeArgumentSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<InvalidAttributeArgumentSyntaxDefinition> Instance = new Lazy<InvalidAttributeArgumentSyntaxDefinition>(() => new InvalidAttributeArgumentSyntaxDefinition());

	public InvalidAttributeArgumentSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "InvalidToken",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

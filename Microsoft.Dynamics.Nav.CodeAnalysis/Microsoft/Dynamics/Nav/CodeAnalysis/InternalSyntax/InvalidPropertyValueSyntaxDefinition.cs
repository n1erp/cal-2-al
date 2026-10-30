using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class InvalidPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<InvalidPropertyValueSyntaxDefinition> Instance = new Lazy<InvalidPropertyValueSyntaxDefinition>(() => new InvalidPropertyValueSyntaxDefinition());

	public InvalidPropertyValueSyntaxDefinition()
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

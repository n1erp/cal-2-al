using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class TimeLiteralValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<TimeLiteralValueSyntaxDefinition> Instance = new Lazy<TimeLiteralValueSyntaxDefinition>(() => new TimeLiteralValueSyntaxDefinition());

	public TimeLiteralValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create(SyntaxKind.TimeLiteralToken)
		}));
	}
}

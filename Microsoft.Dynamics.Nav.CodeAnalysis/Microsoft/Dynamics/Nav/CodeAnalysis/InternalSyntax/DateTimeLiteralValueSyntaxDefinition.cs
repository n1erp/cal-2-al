using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DateTimeLiteralValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<DateTimeLiteralValueSyntaxDefinition> Instance = new Lazy<DateTimeLiteralValueSyntaxDefinition>(() => new DateTimeLiteralValueSyntaxDefinition());

	public DateTimeLiteralValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create(SyntaxKind.DateTimeLiteralToken)
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DateLiteralValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<DateLiteralValueSyntaxDefinition> Instance = new Lazy<DateLiteralValueSyntaxDefinition>(() => new DateLiteralValueSyntaxDefinition());

	public DateLiteralValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create(SyntaxKind.DateLiteralToken)
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DateTimePropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<DateTimePropertyValueSyntaxDefinition> Instance = new Lazy<DateTimePropertyValueSyntaxDefinition>(() => new DateTimePropertyValueSyntaxDefinition());

	public DateTimePropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = DateTimeLiteralValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

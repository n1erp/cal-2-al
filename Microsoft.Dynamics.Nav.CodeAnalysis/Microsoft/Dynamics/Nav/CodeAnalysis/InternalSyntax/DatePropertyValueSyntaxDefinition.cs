using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DatePropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<DatePropertyValueSyntaxDefinition> Instance = new Lazy<DatePropertyValueSyntaxDefinition>(() => new DatePropertyValueSyntaxDefinition());

	public DatePropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = DateLiteralValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

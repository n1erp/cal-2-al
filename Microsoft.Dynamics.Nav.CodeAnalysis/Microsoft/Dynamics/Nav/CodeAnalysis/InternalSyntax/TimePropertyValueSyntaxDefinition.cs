using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class TimePropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<TimePropertyValueSyntaxDefinition> Instance = new Lazy<TimePropertyValueSyntaxDefinition>(() => new TimePropertyValueSyntaxDefinition());

	public TimePropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = TimeLiteralValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

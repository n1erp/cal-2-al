using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class OptionValuesPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<OptionValuesPropertyValueSyntaxDefinition> Instance = new Lazy<OptionValuesPropertyValueSyntaxDefinition>(() => new OptionValuesPropertyValueSyntaxDefinition());

	public OptionValuesPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = OptionValuesSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class OptionValuePropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<OptionValuePropertyValueSyntaxDefinition> Instance = new Lazy<OptionValuePropertyValueSyntaxDefinition>(() => new OptionValuePropertyValueSyntaxDefinition());

	public OptionValuePropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

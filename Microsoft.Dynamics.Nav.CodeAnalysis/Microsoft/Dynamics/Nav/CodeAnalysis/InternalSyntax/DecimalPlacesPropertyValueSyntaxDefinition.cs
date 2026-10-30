using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DecimalPlacesPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<DecimalPlacesPropertyValueSyntaxDefinition> Instance = new Lazy<DecimalPlacesPropertyValueSyntaxDefinition>(() => new DecimalPlacesPropertyValueSyntaxDefinition());

	public DecimalPlacesPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = DecimalPlacesSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class BooleanPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<BooleanPropertyValueSyntaxDefinition> Instance = new Lazy<BooleanPropertyValueSyntaxDefinition>(() => new BooleanPropertyValueSyntaxDefinition());

	public BooleanPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = BooleanLiteralValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

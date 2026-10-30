using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class EnumPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<EnumPropertyValueSyntaxDefinition> Instance = new Lazy<EnumPropertyValueSyntaxDefinition>(() => new EnumPropertyValueSyntaxDefinition());

	public EnumPropertyValueSyntaxDefinition()
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

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class OptionDataTypeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<OptionDataTypeSyntaxDefinition> Instance = new Lazy<OptionDataTypeSyntaxDefinition>(() => new OptionDataTypeSyntaxDefinition());

	public OptionDataTypeSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "TypeName",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = OptionValuesSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "OptionValues",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

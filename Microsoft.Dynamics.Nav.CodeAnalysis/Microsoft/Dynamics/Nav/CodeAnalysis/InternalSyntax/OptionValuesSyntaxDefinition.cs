using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class OptionValuesSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<OptionValuesSyntaxDefinition> Instance = new Lazy<OptionValuesSyntaxDefinition>(() => new OptionValuesSyntaxDefinition());

	public OptionValuesSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameOrEmptySyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Options",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

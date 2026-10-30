using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class SkippedTokensTriviaSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<SkippedTokensTriviaSyntaxDefinition> Instance = new Lazy<SkippedTokensTriviaSyntaxDefinition>(() => new SkippedTokensTriviaSyntaxDefinition());

	public SkippedTokensTriviaSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Tokens",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

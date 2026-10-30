using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class InvalidFilterExpressionValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<InvalidFilterExpressionValueSyntaxDefinition> Instance = new Lazy<InvalidFilterExpressionValueSyntaxDefinition>(() => new InvalidFilterExpressionValueSyntaxDefinition());

	public InvalidFilterExpressionValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "InvalidToken",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

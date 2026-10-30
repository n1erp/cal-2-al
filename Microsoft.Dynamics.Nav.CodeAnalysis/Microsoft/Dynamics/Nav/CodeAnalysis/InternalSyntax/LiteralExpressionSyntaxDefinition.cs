using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class LiteralExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<LiteralExpressionSyntaxDefinition> Instance = new Lazy<LiteralExpressionSyntaxDefinition>(() => new LiteralExpressionSyntaxDefinition());

	public LiteralExpressionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = LiteralValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Literal",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

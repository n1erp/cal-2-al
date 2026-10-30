using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class RangeFromFilterExpressionValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<RangeFromFilterExpressionValueSyntaxDefinition> Instance = new Lazy<RangeFromFilterExpressionValueSyntaxDefinition>(() => new RangeFromFilterExpressionValueSyntaxDefinition());

	public RangeFromFilterExpressionValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierOrLiteralExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Left",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OperatorToken",
			Kinds = ImmutableArray.Create(SyntaxKind.DotDotToken)
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class LiteralAttributeArgumentSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<LiteralAttributeArgumentSyntaxDefinition> Instance = new Lazy<LiteralAttributeArgumentSyntaxDefinition>(() => new LiteralAttributeArgumentSyntaxDefinition());

	public LiteralAttributeArgumentSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = LiteralExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Literal",
			Kinds = ImmutableArray.Create(SyntaxKind.BooleanLiteralValue, SyntaxKind.StringLiteralValue, SyntaxKind.Int32SignedLiteralValue)
		}));
	}
}

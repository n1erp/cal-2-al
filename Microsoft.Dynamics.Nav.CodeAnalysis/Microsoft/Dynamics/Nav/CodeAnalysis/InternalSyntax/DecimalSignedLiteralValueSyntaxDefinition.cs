using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DecimalSignedLiteralValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<DecimalSignedLiteralValueSyntaxDefinition> Instance = new Lazy<DecimalSignedLiteralValueSyntaxDefinition>(() => new DecimalSignedLiteralValueSyntaxDefinition());

	public DecimalSignedLiteralValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "Sign",
			Kinds = ImmutableArray.Create(SyntaxKind.MinusToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Number",
			Kinds = ImmutableArray.Create(SyntaxKind.DecimalLiteralToken)
		}));
	}
}

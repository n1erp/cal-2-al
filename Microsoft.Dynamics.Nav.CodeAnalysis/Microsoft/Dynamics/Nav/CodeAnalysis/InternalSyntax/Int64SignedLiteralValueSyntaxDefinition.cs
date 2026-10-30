using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class Int64SignedLiteralValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<Int64SignedLiteralValueSyntaxDefinition> Instance = new Lazy<Int64SignedLiteralValueSyntaxDefinition>(() => new Int64SignedLiteralValueSyntaxDefinition());

	public Int64SignedLiteralValueSyntaxDefinition()
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
			Kinds = ImmutableArray.Create(SyntaxKind.Int64LiteralToken)
		}));
	}
}

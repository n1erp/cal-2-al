using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class Int32SignedLiteralValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<Int32SignedLiteralValueSyntaxDefinition> Instance = new Lazy<Int32SignedLiteralValueSyntaxDefinition>(() => new Int32SignedLiteralValueSyntaxDefinition());

	public Int32SignedLiteralValueSyntaxDefinition()
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
			Kinds = ImmutableArray.Create(SyntaxKind.Int32LiteralToken)
		}));
	}
}

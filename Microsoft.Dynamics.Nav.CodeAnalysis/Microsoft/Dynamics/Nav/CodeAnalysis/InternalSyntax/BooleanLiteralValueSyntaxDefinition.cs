using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class BooleanLiteralValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<BooleanLiteralValueSyntaxDefinition> Instance = new Lazy<BooleanLiteralValueSyntaxDefinition>(() => new BooleanLiteralValueSyntaxDefinition());

	public BooleanLiteralValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create(SyntaxKind.TrueKeyword, SyntaxKind.FalseKeyword)
		}));
	}
}

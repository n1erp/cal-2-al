using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class StringLiteralValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<StringLiteralValueSyntaxDefinition> Instance = new Lazy<StringLiteralValueSyntaxDefinition>(() => new StringLiteralValueSyntaxDefinition());

	public StringLiteralValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create(SyntaxKind.StringLiteralToken)
		}));
	}
}

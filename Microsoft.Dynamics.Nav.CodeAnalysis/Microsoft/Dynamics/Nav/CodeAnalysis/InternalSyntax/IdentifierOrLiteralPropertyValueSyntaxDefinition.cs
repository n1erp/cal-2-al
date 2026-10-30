using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class IdentifierOrLiteralPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<IdentifierOrLiteralPropertyValueSyntaxDefinition> Instance = new Lazy<IdentifierOrLiteralPropertyValueSyntaxDefinition>(() => new IdentifierOrLiteralPropertyValueSyntaxDefinition());

	public IdentifierOrLiteralPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierOrLiteralExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Identifier",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

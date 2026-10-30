using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class CommaSeparatedIdentifierOrLiteralPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<CommaSeparatedIdentifierOrLiteralPropertyValueSyntaxDefinition> Instance = new Lazy<CommaSeparatedIdentifierOrLiteralPropertyValueSyntaxDefinition>(() => new CommaSeparatedIdentifierOrLiteralPropertyValueSyntaxDefinition());

	public CommaSeparatedIdentifierOrLiteralPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierOrLiteralExpressionSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Values",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

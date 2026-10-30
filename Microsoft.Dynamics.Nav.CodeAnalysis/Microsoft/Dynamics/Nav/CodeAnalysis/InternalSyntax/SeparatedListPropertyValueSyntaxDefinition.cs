using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class SeparatedListPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<SeparatedListPropertyValueSyntaxDefinition> Instance = new Lazy<SeparatedListPropertyValueSyntaxDefinition>(() => new SeparatedListPropertyValueSyntaxDefinition());

	public SeparatedListPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CommaSeparatedPropertyValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = CommaSeparatedStringsPropertyValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = CommaSeparatedIdentifierEqualsStringListPropertyValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = CommaSeparatedIdentifierOrLiteralPropertyValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}

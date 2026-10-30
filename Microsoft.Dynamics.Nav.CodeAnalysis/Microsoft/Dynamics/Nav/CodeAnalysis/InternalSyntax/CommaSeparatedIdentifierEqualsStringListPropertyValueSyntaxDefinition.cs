using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class CommaSeparatedIdentifierEqualsStringListPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<CommaSeparatedIdentifierEqualsStringListPropertyValueSyntaxDefinition> Instance = new Lazy<CommaSeparatedIdentifierEqualsStringListPropertyValueSyntaxDefinition>(() => new CommaSeparatedIdentifierEqualsStringListPropertyValueSyntaxDefinition());

	public CommaSeparatedIdentifierEqualsStringListPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CommaSeparatedIdentifierEqualsStringListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "List",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

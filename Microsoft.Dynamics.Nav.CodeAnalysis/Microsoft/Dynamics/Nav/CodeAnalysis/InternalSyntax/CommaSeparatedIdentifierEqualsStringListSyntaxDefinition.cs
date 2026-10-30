using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class CommaSeparatedIdentifierEqualsStringListSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<CommaSeparatedIdentifierEqualsStringListSyntaxDefinition> Instance = new Lazy<CommaSeparatedIdentifierEqualsStringListSyntaxDefinition>(() => new CommaSeparatedIdentifierEqualsStringListSyntaxDefinition());

	public CommaSeparatedIdentifierEqualsStringListSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierEqualsStringSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Values",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

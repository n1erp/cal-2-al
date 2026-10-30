using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class CommaSeparatedIdentifierEqualsLiteralListSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<CommaSeparatedIdentifierEqualsLiteralListSyntaxDefinition> Instance = new Lazy<CommaSeparatedIdentifierEqualsLiteralListSyntaxDefinition>(() => new CommaSeparatedIdentifierEqualsLiteralListSyntaxDefinition());

	public CommaSeparatedIdentifierEqualsLiteralListSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierEqualsLiteralSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Values",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

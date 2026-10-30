using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class EmptyStatementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<EmptyStatementSyntaxDefinition> Instance = new Lazy<EmptyStatementSyntaxDefinition>(() => new EmptyStatementSyntaxDefinition());

	public EmptyStatementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "SemicolonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.SemicolonToken)
		}));
	}
}

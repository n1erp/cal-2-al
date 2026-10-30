using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class BreakStatementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<BreakStatementSyntaxDefinition> Instance = new Lazy<BreakStatementSyntaxDefinition>(() => new BreakStatementSyntaxDefinition());

	public BreakStatementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "BreakKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.BreakKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "SemicolonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.SemicolonToken)
		}));
	}
}

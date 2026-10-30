using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class OrphanedElseStatementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<OrphanedElseStatementSyntaxDefinition> Instance = new Lazy<OrphanedElseStatementSyntaxDefinition>(() => new OrphanedElseStatementSyntaxDefinition());

	public OrphanedElseStatementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ElseKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.ElseKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = StatementSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "ElseStatement",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

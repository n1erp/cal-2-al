using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class AssertErrorStatementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<AssertErrorStatementSyntaxDefinition> Instance = new Lazy<AssertErrorStatementSyntaxDefinition>(() => new AssertErrorStatementSyntaxDefinition());

	public AssertErrorStatementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "AssertErrorKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.AssertErrorKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = StatementSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Statement",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class WithStatementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<WithStatementSyntaxDefinition> Instance = new Lazy<WithStatementSyntaxDefinition>(() => new WithStatementSyntaxDefinition());

	public WithStatementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "WithKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.WithKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "WithId",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "DoKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.DoKeyword)
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

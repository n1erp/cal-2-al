using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class RepeatStatementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<RepeatStatementSyntaxDefinition> Instance = new Lazy<RepeatStatementSyntaxDefinition>(() => new RepeatStatementSyntaxDefinition());

	public RepeatStatementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "RepeatKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.RepeatKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = StatementSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Statements",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "UntilKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.UntilKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Condition",
			Kinds = ImmutableArray.Create<SyntaxKind>()
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

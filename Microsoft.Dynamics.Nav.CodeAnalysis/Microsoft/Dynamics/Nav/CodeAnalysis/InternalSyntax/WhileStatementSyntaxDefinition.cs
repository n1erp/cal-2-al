using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class WhileStatementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<WhileStatementSyntaxDefinition> Instance = new Lazy<WhileStatementSyntaxDefinition>(() => new WhileStatementSyntaxDefinition());

	public WhileStatementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "WhileKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.WhileKeyword)
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

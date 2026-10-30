using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class IfStatementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<IfStatementSyntaxDefinition> Instance = new Lazy<IfStatementSyntaxDefinition>(() => new IfStatementSyntaxDefinition());

	public IfStatementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "IfKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.IfKeyword)
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
			Name = "ThenKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.ThenKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = StatementSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Statement",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "ElseKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.ElseKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = StatementSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "ElseStatement",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

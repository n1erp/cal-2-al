using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ForStatementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ForStatementSyntaxDefinition> Instance = new Lazy<ForStatementSyntaxDefinition>(() => new ForStatementSyntaxDefinition());

	public ForStatementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ForKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.ForKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "LoopVariable",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "AssignToken",
			Kinds = ImmutableArray.Create(SyntaxKind.AssignToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "InitialValue",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OperatorKeywordToken",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "EndValue",
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

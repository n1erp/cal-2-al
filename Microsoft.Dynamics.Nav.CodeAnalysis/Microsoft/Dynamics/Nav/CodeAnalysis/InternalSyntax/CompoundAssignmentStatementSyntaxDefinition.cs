using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class CompoundAssignmentStatementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<CompoundAssignmentStatementSyntaxDefinition> Instance = new Lazy<CompoundAssignmentStatementSyntaxDefinition>(() => new CompoundAssignmentStatementSyntaxDefinition());

	public CompoundAssignmentStatementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Target",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "AssignmentToken",
			Kinds = ImmutableArray.Create(SyntaxKind.AssignRDivToken, SyntaxKind.AssignPlusToken, SyntaxKind.AssignMinusToken, SyntaxKind.AssignMultiplyToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Source",
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

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class CaseStatementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<CaseStatementSyntaxDefinition> Instance = new Lazy<CaseStatementSyntaxDefinition>(() => new CaseStatementSyntaxDefinition());

	public CaseStatementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "CaseKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CaseKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Expression",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OfKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OfKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = CaseLineSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "CaseLines",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = CaseElseSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "CaseElse",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "EndKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.EndKeyword)
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

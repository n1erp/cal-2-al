using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class CaseElseSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<CaseElseSyntaxDefinition> Instance = new Lazy<CaseElseSyntaxDefinition>(() => new CaseElseSyntaxDefinition());

	public CaseElseSyntaxDefinition()
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
			IsList = true,
			IsOptional = true,
			Name = "ElseStatements",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

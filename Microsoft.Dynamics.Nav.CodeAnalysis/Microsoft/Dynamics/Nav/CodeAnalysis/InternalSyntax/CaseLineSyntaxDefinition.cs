using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class CaseLineSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<CaseLineSyntaxDefinition> Instance = new Lazy<CaseLineSyntaxDefinition>(() => new CaseLineSyntaxDefinition());

	public CaseLineSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Expressions",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ColonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.ColonToken)
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

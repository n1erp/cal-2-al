using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class InListExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<InListExpressionSyntaxDefinition> Instance = new Lazy<InListExpressionSyntaxDefinition>(() => new InListExpressionSyntaxDefinition());

	public InListExpressionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
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
			Name = "InKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.InKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = BracketedArgumentListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "ListExpression",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

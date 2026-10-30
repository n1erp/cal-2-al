using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class BracketedArgumentListSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<BracketedArgumentListSyntaxDefinition> Instance = new Lazy<BracketedArgumentListSyntaxDefinition>(() => new BracketedArgumentListSyntaxDefinition());

	public BracketedArgumentListSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenBracketToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBracketToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Arguments",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "CloseBracketToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CloseBracketToken)
		}));
	}
}

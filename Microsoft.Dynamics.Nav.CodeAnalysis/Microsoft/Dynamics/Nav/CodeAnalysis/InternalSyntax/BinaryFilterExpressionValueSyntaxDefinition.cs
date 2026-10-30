using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class BinaryFilterExpressionValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<BinaryFilterExpressionValueSyntaxDefinition> Instance = new Lazy<BinaryFilterExpressionValueSyntaxDefinition>(() => new BinaryFilterExpressionValueSyntaxDefinition());

	public BinaryFilterExpressionValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Left",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OperatorToken",
			Kinds = ImmutableArray.Create(SyntaxKind.AndFilterKeyword, SyntaxKind.OrFilterKeyword, SyntaxKind.DotDotToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Right",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

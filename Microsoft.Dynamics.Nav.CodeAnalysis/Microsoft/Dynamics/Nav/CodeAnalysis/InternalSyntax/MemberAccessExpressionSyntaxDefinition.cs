using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class MemberAccessExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<MemberAccessExpressionSyntaxDefinition> Instance = new Lazy<MemberAccessExpressionSyntaxDefinition>(() => new MemberAccessExpressionSyntaxDefinition());

	public MemberAccessExpressionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Expression",
			Kinds = ImmutableArray.Create(SyntaxKind.IdentifierName, SyntaxKind.MemberAccessExpression, SyntaxKind.InvocationExpression)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "DotToken",
			Kinds = ImmutableArray.Create(SyntaxKind.DotToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Name",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

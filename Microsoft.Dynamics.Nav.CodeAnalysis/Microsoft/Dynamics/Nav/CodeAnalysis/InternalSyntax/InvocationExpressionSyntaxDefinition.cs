using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class InvocationExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<InvocationExpressionSyntaxDefinition> Instance = new Lazy<InvocationExpressionSyntaxDefinition>(() => new InvocationExpressionSyntaxDefinition());

	public InvocationExpressionSyntaxDefinition()
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
			Node = ArgumentListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "ArgumentList",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

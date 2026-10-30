using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class AttributeArgumentListSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<AttributeArgumentListSyntaxDefinition> Instance = new Lazy<AttributeArgumentListSyntaxDefinition>(() => new AttributeArgumentListSyntaxDefinition());

	public AttributeArgumentListSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenParenthesisToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenParenToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = AttributeArgumentSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Arguments",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "CloseParenthesisToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CloseParenToken)
		}));
	}
}

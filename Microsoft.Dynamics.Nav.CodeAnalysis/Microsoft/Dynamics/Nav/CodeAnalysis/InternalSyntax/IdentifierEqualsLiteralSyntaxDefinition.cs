using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class IdentifierEqualsLiteralSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<IdentifierEqualsLiteralSyntaxDefinition> Instance = new Lazy<IdentifierEqualsLiteralSyntaxDefinition>(() => new IdentifierEqualsLiteralSyntaxDefinition());

	public IdentifierEqualsLiteralSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Identifier",
			Kinds = ImmutableArray.Create(SyntaxKind.IdentifierToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "EqualsToken",
			Kinds = ImmutableArray.Create(SyntaxKind.EqualsToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = LiteralValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Literal",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class IdentifierEqualsStringSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<IdentifierEqualsStringSyntaxDefinition> Instance = new Lazy<IdentifierEqualsStringSyntaxDefinition>(() => new IdentifierEqualsStringSyntaxDefinition());

	public IdentifierEqualsStringSyntaxDefinition()
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
			Node = StringLiteralValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "StringLiteral",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}

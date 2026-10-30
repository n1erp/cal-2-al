using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class MemberAccessOrIdentifierPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<MemberAccessOrIdentifierPropertyValueSyntaxDefinition> Instance = new Lazy<MemberAccessOrIdentifierPropertyValueSyntaxDefinition>(() => new MemberAccessOrIdentifierPropertyValueSyntaxDefinition());

	public MemberAccessOrIdentifierPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Identifier",
			Kinds = ImmutableArray.Create(SyntaxKind.IdentifierName, SyntaxKind.MemberAccessExpression)
		}));
	}
}

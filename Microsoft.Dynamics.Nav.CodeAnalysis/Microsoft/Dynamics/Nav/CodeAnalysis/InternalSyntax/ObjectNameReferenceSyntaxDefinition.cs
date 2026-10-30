using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ObjectNameReferenceSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ObjectNameReferenceSyntaxDefinition> Instance = new Lazy<ObjectNameReferenceSyntaxDefinition>(() => new ObjectNameReferenceSyntaxDefinition());

	public ObjectNameReferenceSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Identifier",
			Kinds = ImmutableArray.Create(SyntaxKind.IdentifierName)
		}));
	}
}

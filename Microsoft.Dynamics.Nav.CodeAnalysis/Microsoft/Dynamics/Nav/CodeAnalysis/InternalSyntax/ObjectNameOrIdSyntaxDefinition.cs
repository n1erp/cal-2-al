using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ObjectNameOrIdSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ObjectNameOrIdSyntaxDefinition> Instance = new Lazy<ObjectNameOrIdSyntaxDefinition>(() => new ObjectNameOrIdSyntaxDefinition());

	public ObjectNameOrIdSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Identifier",
			Kinds = ImmutableArray.Create(SyntaxKind.IdentifierName, SyntaxKind.ObjectId)
		}));
	}
}

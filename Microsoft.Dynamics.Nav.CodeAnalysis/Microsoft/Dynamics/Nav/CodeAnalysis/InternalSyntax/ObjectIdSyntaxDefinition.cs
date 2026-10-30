using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ObjectIdSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ObjectIdSyntaxDefinition> Instance = new Lazy<ObjectIdSyntaxDefinition>(() => new ObjectIdSyntaxDefinition());

	public ObjectIdSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create(SyntaxKind.Int32LiteralToken)
		}));
	}
}

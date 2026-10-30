using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class PropertyNameSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<PropertyNameSyntaxDefinition> Instance = new Lazy<PropertyNameSyntaxDefinition>(() => new PropertyNameSyntaxDefinition());

	public PropertyNameSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Identifier",
			Kinds = ImmutableArray.Create(SyntaxKind.IdentifierToken)
		}));
	}
}

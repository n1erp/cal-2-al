using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class PropertyListSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<PropertyListSyntaxDefinition> Instance = new Lazy<PropertyListSyntaxDefinition>(() => new PropertyListSyntaxDefinition());

	public PropertyListSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = PropertySyntaxOrEmptyDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Properties",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
